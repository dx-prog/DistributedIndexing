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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Test {
    [TestClass]
    public class IglScriptBuilderTests {
        [TestMethod]
        public void CanCreateFromInstance() {
            var expected = CreateClassOfPrimatives(2);
            var actual = TestScripting(expected);


            Assert.AreEqual(expected.IntValue, actual.IntValue);
            Assert.AreEqual(expected.LongValue, actual.LongValue);
            Assert.AreEqual(expected.StringValue, actual.StringValue);
        }

        [TestMethod]
        public void CanCreateFromArray() {
            var expected = new[] {
                CreateClassOfPrimatives(1),
                CreateClassOfPrimatives(2)
            };
            var actual = TestScripting(expected);


            Assert.AreEqual(expected.Length, actual.Length);
            QuickCheck(expected, actual);
        }

        [TestMethod]
        public void CanCreateFromHashSet() {
            var expected = new HashSet<SimplePrimatives> {
                CreateClassOfPrimatives(1),
                CreateClassOfPrimatives(2)
            };
            var actual = TestScripting(expected);


            QuickCheck(expected, actual);
        }


        [TestMethod]
        public void CanCreateFromList() {
            var expected = new List<SimplePrimatives> {
                CreateClassOfPrimatives(1),
                CreateClassOfPrimatives(2)
            };
            var actual = TestScripting(expected);


            Assert.AreEqual(expected.Count, actual.Count);
            QuickCheck(expected, actual);
        }


        [TestMethod]
        public void CanCreateFromLinkList() {
            var expected = new LinkedList<SimplePrimatives>();
            expected.AddLast(CreateClassOfPrimatives(1));
            expected.AddLast(CreateClassOfPrimatives(2));
            expected.AddLast(CreateClassOfPrimatives(4));
            expected.AddLast(CreateClassOfPrimatives(8));
            var actual = TestScripting(expected);


            Assert.AreEqual(expected.Count, actual.Count);
            QuickCheck(expected, actual);
        }

        [TestMethod]
        public void CanCreateFromDictionary() {
            var expected = new Dictionary<int, SimplePrimatives> {
                [1] = CreateClassOfPrimatives(1),
                [2] = CreateClassOfPrimatives(2)
            };
            var actual = TestScripting(expected);


            Assert.AreEqual(expected.Count, actual.Count);
            QuickCheck(expected, actual);
        }

        [TestMethod]
        public void CanCreateFromArrayOfArrayPrimative() {
            var expected = new[] {
                new byte[] {1, 2, 3, 4},
                new byte[] {5, 6, 7, 8},
                new byte[] {9, 10, 11, 21}
            };
            var actual = TestScripting(expected);


            Assert.AreEqual(
                string.Join(".", expected.SelectMany(x => x)),
                string.Join(".", actual.SelectMany(x => x)));
        }


        [TestMethod]
        public void CanCreateFromArrayOfArrayNonPrimative() {
            var expected = new[] {
                new[] {CreateClassOfPrimatives(1), CreateClassOfPrimatives(2)},
                new[] {CreateClassOfPrimatives(3), CreateClassOfPrimatives(4)},
                new[] {CreateClassOfPrimatives(5), CreateClassOfPrimatives(6)}
            };
            var actual = TestScripting(expected);


            QuickCheck(expected.SelectMany(x => x), actual.SelectMany(y => y));
        }

        private static void QuickCheck<T>(IEnumerable<T> expected, IEnumerable<T> actual) {
            var a = string.Join(".", expected);
            var b = string.Join(".", actual);

            Console.WriteLine("A===================================");
            Console.WriteLine(a);
            Console.WriteLine("B===================================");
            Console.WriteLine(b);
            Console.WriteLine("===================================");
            Assert.AreEqual(a,
                b
            );
        }

        private T TestScripting<T>(T input) {
            var script = new IglScriptBody();
            var builder = new IglScriptBuilder(script);
            var objId = builder.RegisterRoot(
                input,
                out _);
            while (builder.HasPressure) {
                builder.Pump();
                Console.WriteLine(builder.TokenCount);
            }

            Console.WriteLine("=============");
            builder.Report(Console.Out);
            script.Execute(out var map);
            return (T) map[objId];
        }


        private static SimplePrimatives CreateClassOfPrimatives(int counter) {
            var expected = new SimplePrimatives {
                IntValue = 1 * counter,
                LongValue = 2 * counter,
                StringValue = "hello world" + counter
            };
            return expected;
        }

        [Serializable]
        public class SimplePrimatives {
            public int IntValue { get; set; }
            public int LongValue { get; set; }
            public string StringValue { get; set; }

            public override string ToString() {
                return Tuple.Create(IntValue, LongValue, StringValue).ToString();
            }
        }

        [Serializable]
        public class SimpleNode {
            public SimplePrimatives SimpleNodeA { get; set; }
            public SimplePrimatives SimpleNodeB { get; set; }
        }

        [Serializable]
        public class ComplexNode<TValueType> {
            public ComplexNode<TValueType> Next { get; set; }
            public TValueType Value { get; set; }
            public ComplexNode<TValueType> Pevious { get; set; }
        }


        [Serializable]
        public class GraphNode<TValueType> {
            public HashSet<GraphNode<TValueType>> Peers { get; set; } = new HashSet<GraphNode<TValueType>>();
            public TValueType Value { get; set; }

            public void MapTo(params GraphNode<TValueType>[] nodes) {
                foreach (var n in nodes) {
                    Peers.Add(n);
                    n.Peers.Add(this);
                }
            }
        }

        [Serializable]
        public class SimpleReadonly {
            public SimpleReadonly(int v) {
                Value = v;
            }

            public int Value { get; }
        }
    }
}