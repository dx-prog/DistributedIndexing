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

using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Core.DocumentIndexing.Extractors;
using Sprockets.Core.DocumentIndexing.Host;
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Test.DocumentIndexing {
    [TestClass]
    public class TextExtractorTests {
        public Stream GetTestObjectStream() {
            return new MemoryStream(Encoding.Unicode.GetBytes(
                @"<html>
                        <head>
                        </head>
                       <body>
                        <p>1 <i> 2 </i> <b>3</b> </p>_<span>4</span>
                        </body>
                    </html>"
            ));
        }

        [TestMethod]
        public void ExtractorHostTestTextPlain() {
            var host = new ExtractorHost();
            host.RegisterScopedExtractor<PassthroughExtractor>();
            host.RegisterScopedExtractor<DefaultHtmlExtractor>();
            host.Initialize();
            using (host.BeginServiceScope(out var extractor)) {
                var detailsForHtml = new IndexingRequestDetails(CultureInfo.InvariantCulture,
                    Encoding.Unicode,
                    "text/plain",
                    string.Empty,
                    string.Empty);
                var finalHtml = extractor.ExtractText(
                    detailsForHtml,
                    GetTestObjectStream()
                );
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c=>c.Segment.Contains("1")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("2")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("3")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("_")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("4")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("html")));
            }
        }

        [TestMethod]
        public void ExtractorHostTestTextHtml() {
            var host = new ExtractorHost();
            host.RegisterScopedExtractor<PassthroughExtractor>();
            host.RegisterScopedExtractor<DefaultHtmlExtractor>();
            host.Initialize();
            using (host.BeginServiceScope(out var extractor)) {
                var detailsForHtml = new IndexingRequestDetails(CultureInfo.InvariantCulture,
                    Encoding.Unicode,
                    "text/html",
                    string.Empty,
                    string.Empty);
                var finalHtml = extractor.ExtractText(
                    detailsForHtml,
                    GetTestObjectStream()
                );
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("1")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("2")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("3")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("_")));
                Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("4")));
            }
        }

        [TestMethod]
        public void HtmlExtractorTest() {
            var extractor = new DefaultHtmlExtractor();
            var details = IndexingRequestDetails.Create<DefaultHtmlExtractor>(
                CultureInfo.InvariantCulture,
                Encoding.Unicode,
                "text/html",
                string.Empty);
            var finalHtml = extractor.ExtractText(details, GetTestObjectStream());
            Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("1")));
            Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("2")));
            Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("3")));
            Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("_")));
            Assert.IsTrue(finalHtml.ExtractionPointDetails.Any(c => c.Segment.Contains("4")));
        }
    }
}