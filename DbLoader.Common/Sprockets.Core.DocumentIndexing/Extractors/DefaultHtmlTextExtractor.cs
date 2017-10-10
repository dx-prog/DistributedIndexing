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
using System.IO;
using System.Runtime.Serialization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    public class DefaultHtmlTextExtractor : ITextExtractor {
        public int MaxNodeDepth { get; set; } = 256;

        public string ExtractText(IndexingRequestDetails details, TextReader reader) {
            var config = Configuration.Default.WithDefaultLoader();

            var document = new HtmlParser(config).Parse(reader.ReadToEnd());

            // using degrapher because AngleSharp uses recursion
            var degrapher = new SimpleDegrapher {CustomerEnumerator = HtmlDegrapher};
            degrapher.LoadObject(document);
            if (degrapher.PumpFor(TimeSpan.FromSeconds(1)))
                throw new SerializationException();

            // MaxNodeDepth test, might stack overflow
            if (degrapher.KnowledgeBase.Count > MaxNodeDepth)
                throw new SerializationException();

            return
                document.Head.TextContent + Environment.NewLine +
                document.Body.TextContent;
        }


        public static IEnumerator HtmlDegrapher(SimpleDegrapher caller, object arg) {
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