/***********************************************************************************
 * Copyright 2017  David Garcia
 *      
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * *********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Sprockets.Core.Disposables;
using Sprockets.Graph.Contracts;

namespace Sprockets.Graph {
    public class NodeSearchContext<T> : IDisposable {
        private readonly Stack<IGraphNode<T>> _cursor = new Stack<IGraphNode<T>>();

        private readonly DisposableStopwatch _watch;

        public NodeSearchContext(IGraphNode<T> start,
            GraphNodeVisitationControl<T> visitTracker,
            NodeSearch<T> predicate) {
            Settings = visitTracker;
            StartingPoint = start;
            MoveTo(start);
            _watch = new DisposableStopwatch();

            SearchBehavior = predicate;
        }

        public NodeSearch<T> SearchBehavior { get; protected set; }

        public TimeSpan RunTime => _watch.Elapsed;

        public GraphNodeVisitationControl<T> Settings { get; }
        public IGraphNode<T> StartingPoint { get; }
        public bool AllowWanderingThisCycle { get; set; }
        public IGraphNode<T> Cursor => _cursor.Count == 0 ? null : _cursor.Peek();
        public ICollection<IGraphNode<T>> CurrentPeers => Cursor?.Peers;
        public int CurrentDepth => _cursor.Count;

        /// <summary>
        ///     True if all immediate peers have been visited already
        /// </summary>
        /// <returns></returns>
        public bool? IsExhausted => Cursor?.Peers?.All(Settings.HasVisited);

        /// <summary>
        ///     Get a value indicated if the current node is consisted
        ///     visisted (or blocked)
        /// </summary>
        public bool IsVisited => Settings.HasVisited(Cursor);

        /// <summary>
        ///     Gets or sets a value suggesting to the algorithms that
        ///     a node should not be visited
        /// </summary>
        /// <remarks>Once set to true, it cannot be unset</remarks>
        public bool IsBlocked {
            get => Settings.IsPathBlocked(Cursor);
            set {
                if (value)
                    Settings.MarkBlocked(Cursor);
            }
        }

        public IGraphNode<T> Last { get; private set; }
        public double Freedom => GetDegreesFreedom(Cursor);

        public bool Closed { get; private set; }


        public void Dispose() {
            _watch?.Dispose();
        }

        /// <summary>
        ///     If a node is marked as visisted, this will cause such mark
        ///     to be forgetten for this context
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool ForgetVisit(IGraphNode<T> n) {
            return Settings.ForgetVisit(n);
        }

        /// <summary>
        ///     Mark a node as having been visited
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool MarkVisit(IGraphNode<T> n) {
            return Settings.MarkVisit(n);
        }

        /// <summary>
        ///     Causes the context to unwind
        /// </summary>
        public void UnwindWhile(NodeSearch<T> predicate = null) {
            predicate = predicate ?? ((ctx, w) => true);
            while (_cursor.Count > 0) {
                var last = _cursor.Pop();
                if (!predicate(this, last))
                    break;
            }

            if (_cursor.Count == 0)
                AllowWanderingThisCycle = false;
        }

        /// <summary>
        ///     Forgets the exact path between the starting point and the current
        ///     node; Move Back will cause going back to the starting point.
        /// </summary>
        public void ClearWalkbackHistory() {
            var current = Cursor;
            while (_cursor.Count > 1)
                _cursor.Pop();

            MoveTo(current);
        }

        /// <summary>
        ///     Move the cursor to the specified node
        /// </summary>
        /// <param name="n"></param>
        public void MoveTo(IGraphNode<T> n) {
            Last = Cursor;
            _cursor.Push(n);
        }

        /// <summary>
        ///     Move back to the previous cursor position
        /// </summary>
        public void MoveBack() {
            _cursor.Pop();
        }

        /// <summary>
        ///     Check if the current node has been visited before
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool HasVisistedAlready(IGraphNode<T> n) {
            return Settings.HasVisited(n);
        }


        public bool MoveToMostFree(IGraphNode<T> wanderPoint, int degrees = 1) {
            var next = CurrentPeers.OrderByDescending(n => GetDegreesFreedom(n, degrees)).First();
            if (next == Cursor)
                return false;

            MoveTo(next);
            return true;
        }

        public bool IsNodeExhausted(IGraphNode<T> wanderPoint) {
            return wanderPoint.Peers?.All(Settings.HasVisited) == true;
        }

        /// <summary>
        ///     Returns a geometrically scaled value indicating how large the unvisited network
        ///     is beyond the specified <see cref="graphNode" />
        /// </summary>
        /// <param name="graphNode"></param>
        /// <param name="degrees">postive to bias towards more open networks</param>
        /// <returns></returns>
        public double GetDegreesFreedom(IGraphNode<T> graphNode, int degrees = 1) {
            var tmp = CountOpenPeers(graphNode);
            var pow = 1;
            var peers = graphNode.Peers.ToList();
            var count = Math.Abs(degrees);
            var sign = Math.Sign(degrees);
            if (sign == 0)
                sign = 1;
            while (count-- > 0) {
                var next = Math.Pow(peers.Sum(c => CountOpenPeers(c)), pow * sign);
                pow++;
                peers = peers.SelectMany(p => p.Peers).ToList();
                tmp += next;
            }

            return tmp;
        }

        public void Exit() {
            Closed = true;
        }

        private double CountOpenPeers(IGraphNode<T> graphNode) {
            var count = graphNode.Peers.Count() + double.Epsilon;
            var ret = graphNode.Peers.Count(n => !Settings.HasVisited(n)) / count;
            return ret;
        }
    }
}