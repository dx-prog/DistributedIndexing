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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sprockets.Graph.Contracts;

namespace Sprockets.Graph {
    /// <summary>
    ///     Controls how visiting a graph is performed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GraphNodeVisitationControl<T> {
        public GraphNodeVisitationControl() {
            Timeout = TimeSpan.FromMilliseconds(100);
            MaxVisitDepth = short.MaxValue;
        }

        public ConcurrentDictionary<IGraphNode<T>, IComparable> History { get; } =
            new ConcurrentDictionary<IGraphNode<T>, IComparable>();

        /// <summary>
        ///     Get or set the amount of time visiting is allowed
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        ///     Get or set the maximum depth of visitation
        /// </summary>
        public int MaxVisitDepth { get; set; }

        /// <summary>
        ///     Get or set the operation that controls the handedness of the wandering
        /// </summary>
        public Func<IGraphNode<T>, IComparable> WandingOption { get; set; }

        /// <summary>
        ///     Get or set the flag that determines if search uses the Wandering approach by default on
        ///     each cycle
        /// </summary>
        public bool AllowWandering { get; set; } = true;

        public virtual bool IsMarked(IGraphNode<T> pos) {
            if (History.TryGetValue(pos, out var flag))
                return CreateVisitationMark(pos).CompareTo(flag) == 0;

            return false;
        }

        public virtual IEnumerable<IGraphNode<T>>
            GetWanderingApproach(NodeSearchContext<T> context, IGraphNode<T> node) {
            return node.Peers.OrderByDescending(Options);
        }

        public virtual bool TryGetVisitMark(IGraphNode<T> graphNode, out IComparable mark) {
            return History.TryGetValue(graphNode, out mark);
        }

        /// <summary>
        ///     A node that is marked as blocked cannot not be modified
        /// </summary>
        /// <param name="graphNode"></param>
        /// <returns></returns>
        public virtual bool ForgetVisit(IGraphNode<T> graphNode) {
            return !IsPathBlocked(graphNode) && History.TryRemove(graphNode, out _);
        }

        public virtual bool MarkVisit(IGraphNode<T> graphNode, IComparable mark = null) {
            return History.TryAdd(graphNode, mark ?? CreateVisitationMark(graphNode));
        }

        public virtual bool MarkBlocked(IGraphNode<T> graphNode) {
            return History.TryAdd(graphNode, CreateBlockPathMark(graphNode));
        }

        public virtual bool HasVisited(IGraphNode<T> graphNode) {
            return History.TryGetValue(graphNode, out var mark) &&
                   (mark.CompareTo(CreateVisitationMark(graphNode)) == 0 ||
                    mark.CompareTo(CreateBlockPathMark(graphNode)) == 0);
        }

        public virtual bool IsPathBlocked(IGraphNode<T> graphNode) {
            return History.TryGetValue(graphNode, out var mark) && mark.CompareTo(CreateBlockPathMark(graphNode)) == 0;
        }

        protected internal virtual IComparable CreateVisitationMark(IGraphNode<T> pos) {
            return 0;
        }

        protected internal virtual IComparable CreateBlockPathMark(IGraphNode<T> pos) {
            return 1;
        }

        private IComparable Options(IGraphNode<T> arg) {
            if (null != WandingOption)
                return WandingOption(arg);

            if (arg.Value is IComparable ret)
                return ret;

            return arg.Id;
        }
    }
}