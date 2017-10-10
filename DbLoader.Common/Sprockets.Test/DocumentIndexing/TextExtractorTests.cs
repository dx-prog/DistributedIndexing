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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Core.DocumentIndexing.Extractors;
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Test.DocumentIndexing {
    [TestClass]
    public class TextExtractorTests {
        [TestMethod]
        public void HtmlExtractorTest() {
            var extractor = new DefaultHtmlTextExtractor();

            var finalHtml = extractor.ExtractText(
                IndexingRequestDetails.Create<DefaultHtmlTextExtractor>(CultureInfo.InvariantCulture,
                    "text/html",
                    string.Empty),
                new StringReader(
                    @"<html>
                        <head>
                        </head>
                       <body>
                        <p>1 <i> 2 </i> <b>3</b> </p>_<span>4</span>
                        </body>
                    </html>")
            );
            Assert.IsTrue(finalHtml.Contains("1"));
            Assert.IsTrue(finalHtml.Contains("2"));
            Assert.IsTrue(finalHtml.Contains("3"));
            Assert.IsTrue(finalHtml.Contains("_"));
            Assert.IsTrue(finalHtml.Contains("4"));
        }
    }
}