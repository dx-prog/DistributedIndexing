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
using Sprockets.Graph.Contracts;

namespace Sprockets.Graph {
    public class GraphNode<T> : IGraphNode<T> {
        public GraphNode(double x, double y) {
            Id = Tuple.Create(x, y);
        }

        public GraphNode(IComparable id = null) {
            Id = id ?? DateTime.Now;
        }

        public int CompareTo(IGraphNode<T> other) {
            return Id.CompareTo(other.Id);
        }

        public void UnjoinFrom(IGraphNode<T> other) {
            Peers.Remove(other);
            other.Peers.Remove(this);
        }

        public void JoinTo(IGraphNode<T> other) {
            Peers.Add(other);
            other.Peers.Add(this);
        }

        public T Value { get; set; }
        public IComparable Id { get; }

        public ICollection<IGraphNode<T>> Peers { get; } = new HashSet<IGraphNode<T>>();

        public IEnumerable<IGraphNode<T>> Search(GraphNodeVisitationControl<T> visitationControl,
            NodeSearch<T> predicate) {
            var explorer = visitationControl ?? new GraphNodeVisitationControl<T>();
        
            var previousPredicate = predicate;
            predicate = (ctx, wanderPoint) => {
                var pos = ctx.Cursor;
                var ret = previousPredicate(ctx, wanderPoint);
                explorer.MarkVisit(pos, explorer.CreateVisitationMark(pos));
                return ret;
            };
            var context = new NodeSearchContext<T>(this, explorer, predicate);
            try {
                while (context.Cursor != null) {
                    if (context.RunTime > explorer.Timeout)
                        yield break;

                    if (context.CurrentDepth > explorer.MaxVisitDepth)
                        yield break;
                    // by default wandering around the center point
                    context.AllowWanderingThisCycle = explorer.AllowWandering;
                    // check the current node before moving outward
                    if (predicate(context, context.Cursor))
                        yield return context.Cursor;
                    if (context.Closed)
                        yield break;
                    // the predicate can opt out of wandering
                    if (!context.AllowWanderingThisCycle)
                        continue;

                    if (context.CurrentDepth == 0)
                        break;

                    var pos = context.Cursor;
                    foreach (var n in explorer.GetWanderingApproach(context, pos)) {
                       if(context.SearchBehavior(context, n))
                        yield return context.Cursor;

                        if (context.Closed)
                            yield break;
                        if (!context.AllowWanderingThisCycle)
                            break;
                        if (context.CurrentDepth == 0)
                            break;
                    }
                }
            }
            finally {
                context.Dispose();
            }
        }

        public override string ToString() {
            return $"{Id}={Value}";
        }

        public static void CrossJoin(params GraphNode<T>[]nodes) {
            foreach (var x in nodes)
            foreach (var y in nodes) {
                if (x == y)
                    continue;

                x.JoinTo(y);
            }
        }
    }
}