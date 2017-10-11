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
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Graph;

namespace Sprockets.Test {
    [TestClass]
    public class GraphNodeTests {
        [TestMethod]
        public void TestWander() {
            var x = new GraphNode<int>();
            var y = CreateBranch(x);
            var z = CreateBranch(y);

            var count = z.Search(new GraphNodeVisitationControl<int> {
                        Timeout = TimeSpan.FromMinutes(Debugger.IsAttached ? 1 : 0.1)
                    },
                    (context, wander) => {
                        context.Settings.WandingOption = n => n.Value;
                        var freedom = Math.Abs(context.Freedom);
                        Console.WriteLine("[{0},{1}] ===== {2}", freedom, context.Cursor, wander);
                        if (x.Value == wander.Value) {
                            context.MoveTo(wander);
                            context.Exit();
                            return true;
                        }

                        if (x.Value == context.Cursor.Value) {
                            context.Exit();
                            return true;
                        }
                        // test if we are at the center
                        if (context.Cursor == wander)
                            return false;
                        if (Math.Abs(context.Freedom) < double.Epsilon)
                            return false;
                        // test if all peers have been visited
                        if (context.IsExhausted == true) {
                            context.IsBlocked = true;
                            context.MoveBack();
                            return false;
                        }

                        // try to navigate to node with lowest value


                        if (x.Value != context.Cursor.Value)
                            context.MoveToMostFree(wander, 2);


                        return true;
                    })
                .ToArray();

            Assert.AreEqual(3, count.Length);
            Assert.IsTrue(count.Contains(x));
        }

        private static GraphNode<int> CreateBranch(GraphNode<int> a) {
            var b = new GraphNode<int> {Value = a.Value + 1};
            var c = new GraphNode<int> {Value = b.Value + 1};
            var d = new GraphNode<int> {Value = c.Value + 1};
            var e = new GraphNode<int> {Value = d.Value + 1};

            GraphNode<int>.CrossJoin(a, b, c, d, e);
            return e;
        }
    }
}