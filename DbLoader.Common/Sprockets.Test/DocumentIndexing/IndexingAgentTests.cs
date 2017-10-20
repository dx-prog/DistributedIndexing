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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Core.DocumentIndexing.CacheProviders;
using Sprockets.Core.DocumentIndexing.Extractors;
using Sprockets.Core.DocumentIndexing.Host;
using Sprockets.Core.DocumentIndexing.Indexers;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.DocumentIndexer.Lucene;

namespace Sprockets.Test.DocumentIndexing {
    [TestClass]
    public class IndexingAgentTests {
        [TestMethod]
        public void BasicXmlAgentTest() {
            var content = GetXmlFiles();
            var keywordTest = "land";
            ExecuteExtractiontest(content, keywordTest);
        }

        [TestMethod]
        public void BasicTdfTest() {
            var content = GetTdfFiles();
            var keywordTest = "earth";
            ExecuteExtractiontest(content, keywordTest, new LuceneCache(LuceneCache.MemoryModel.Disk));
        }

        private static void ExecuteExtractiontest(IEnumerable<TextIndexingRequest> content,
            string keywordTest,
            ITextCache cache = null) {
            var host = new ExtractorHost();
            host.RegisterScopedExtractor<PassthroughExtractor>();

            host.RegisterScopedExtractor<DefaultTdfExtractor>();
            // Testing instance approaching
            host.RegisterScopedExtractor(new DefaultHtmlExtractor());
            // Testing factory approach
            host.RegisterScopedExtractor(p => new DefaultXmlExtractor(p));
            host.Initialize();
            cache = cache ?? new DefaultIntermediateCacheProvider();
            cache.Clear();
            var agent = new IndexerAgent(host, cache, (ISearchProvider) cache);


            var agentWorker = agent.IndexDocuments(CancellationToken.None, content);
            agentWorker.GetAwaiter().GetResult();

            var resultsWorker = agent.Search(CancellationToken.None,
                new TextSearch(CultureInfo.InvariantCulture,
                    "REGEX",
                    keywordTest));

            var results = resultsWorker.GetAwaiter().GetResult().ToArray();
            Console.WriteLine(results.Length);
            Assert.IsTrue(results.Length > 0);
        }

        private IEnumerable<TextIndexingRequest> GetXmlFiles() {
            return Directory.GetFiles("C:\\testDocSource\\",
                    "*.xml",
                    SearchOption.AllDirectories)
                .Take(10)
                .Select(fullFileName => new TextIndexingRequest(
                    null,
                    fullFileName,
                    "text file",
                    "",
                    IndexingRequestDetails.Create<DefaultXmlExtractor>(
                        CultureInfo.InvariantCulture,
                        Encoding.ASCII,
                        "text/xml",
                        string.Empty),
                    r => File.OpenRead(fullFileName)
                ));
        }

        private IEnumerable<TextIndexingRequest> GetTdfFiles() {
            return Directory.GetFiles("C:\\testDocSource\\",
                    "*.txt",
                    SearchOption.AllDirectories)
                .Take(10)
                .Select(fullFileName => new TextIndexingRequest(
                    null,
                    fullFileName,
                    "text file",
                    "",
                    IndexingRequestDetails.Create<DefaultTdfExtractor>(
                        CultureInfo.InvariantCulture,
                        Encoding.ASCII,
                        "text/tab-separated-values",
                        string.Empty),
                    r => File.OpenRead(fullFileName)
                ));
        }
    }
}