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
using System.Collections;
using System.Globalization;
using System.IO;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    public class DefaultHtmlExtractor : IExtractor {
        public int MaxNodeDepth { get; set; } = 256;
        public bool FullDocumentCapture { get; set; }
        public bool CanExtract(CultureInfo culture, string mimeType, string schema) {
            return
                string.Equals("text/htm", mimeType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals("text/html", mimeType, StringComparison.OrdinalIgnoreCase);
        }

        public ExtractionResult ExtractText(IndexingRequestDetails details, Stream stream) {
            using (var reader = new StreamReader(stream, details.Encoding, false, 16, true)) {
                var config = Configuration.Default.WithDefaultLoader();

                var document = new HtmlParser(config).Parse(reader.ReadToEnd());

                // using degrapher because AngleSharp uses recursion
                var returnResult = new ExtractionResult(details);
                if (FullDocumentCapture) {
                    returnResult.GenerateSegments(document.TextContent, HtmlDegrapher);
                }
                else {
                    returnResult.GenerateSegments(document, HtmlDegrapher);
                }
                returnResult.AnnotateSegments();

                return returnResult;
            }
        }


        public static IEnumerator HtmlDegrapher(IObjectDegrapher caller, object arg) {
            switch (arg) {
                case ICharacterData text:

                    yield return text.Data;

                    break;

                case INode element:

                    foreach (var node in element.ChildNodes)
                        yield return node;

                    break;
            }
        }
    }
}