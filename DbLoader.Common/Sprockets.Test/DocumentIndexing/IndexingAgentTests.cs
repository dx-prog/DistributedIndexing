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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Core.DocumentIndexing.Extractors;
using Sprockets.Core.DocumentIndexing.Host;
using Sprockets.Core.DocumentIndexing.Indexers;
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Test.DocumentIndexing
{
    [TestClass]
    public class IndexingAgentTests
    {
        [TestMethod]
        public void BasicAgentTest() {

            var host = new ExtractorHost();
            host.RegisterScopedExtractor<PassthroughExtractor>();
            host.RegisterScopedExtractor<DefaultHtmlExtractor>();
            host.RegisterScopedExtractor(p=>new DefaultXmlExtractor(p) {
            });
            host.Initialize();
            var cache = new DefaultIntermediateCacheProvider();
            cache.Clear();
            var agent = new IndexerAgent(host, cache, cache);



            var agentWorker=agent.IndexDocuments(CancellationToken.None, GetTestFiles());
            agentWorker.GetAwaiter().GetResult();

            var resultsWorker=( agent.Search(CancellationToken.None,
                new TextSearch(CultureInfo.InvariantCulture,
                    "REGEX",
                    "land")));
         
            var results = resultsWorker.GetAwaiter().GetResult().ToArray();
            Assert.IsTrue(results.Length>0);
        }

        private IEnumerable<TextIndexingRequest> GetTestFiles() {
            return Directory.GetFiles("C:\\testDocSource\\",
                "*.xml",
                SearchOption.AllDirectories).Take(10).Select(fullFileName => new TextIndexingRequest(
                null,
                fullFileName,
                "text file",
                IndexingRequestDetails.Create<DefaultXmlExtractor>(
                    CultureInfo.InvariantCulture,Encoding.ASCII, "text/xml",string.Empty),
                (r)=>File.OpenRead(fullFileName)
            ));
        }
    }
}
