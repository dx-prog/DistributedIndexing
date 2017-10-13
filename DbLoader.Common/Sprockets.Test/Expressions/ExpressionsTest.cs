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
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.DocumentIndexer.Lucene;
using Sprockets.Expressions;

namespace Sprockets.Test.Expressions {
    [TestClass]
    public class ExpressionsTest {
        [TestMethod]
        public void SearchTextDocumentTest() {
            var predicate = new PredicateGroup<LuceneCache.TextDocument>();

            predicate.Add(f => f.SearchText.StartsWith("keyword"));

            var data = new[] {
                new LuceneCache.TextDocument("", "", "", new ExtractionPointDetail()) {
                    SearchText = "keyword"
                },

                new LuceneCache.TextDocument("", "", "", new ExtractionPointDetail()) {
                    SearchText = "non-keyword"
                }
            };

            var result = data.AsQueryable().Where(predicate.Combine(Expression.OrElse)).ToArray();
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void SimplyArraySearchTest() {
            var predicate = new PredicateGroup<int>();

            predicate.Add(f => f == 1);
            predicate.Add(f => f == 3);
            predicate.Add(f => f == 5);

            var data = new[] {1, 7, 6, 3, 8, 9, 4, 2};

            var result = data.AsQueryable().Where(predicate.Combine(Expression.OrElse)).ToArray();
            Assert.IsTrue(result.Contains(1));
            Assert.IsTrue(result.Contains(3));
            Assert.IsFalse(result.Contains(2));
            Assert.AreEqual(2, result.Length);
        }
    }
}