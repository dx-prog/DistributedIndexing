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
using System.Xml.Linq;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    public class DefaultXmlExtractor : IExtractor {
        public DefaultXmlExtractor(IServiceProvider provider) {
            Provider = provider;
        }

        public IServiceProvider Provider { get; }
        public List<IExtractorExtension<XDocument>> Extensions { get; } = new List<IExtractorExtension<XDocument>>();

        public bool CanExtract(CultureInfo culture, string mimeType, string schema) {
            return
                string.Equals("text/xml", mimeType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals("application/xml", mimeType, StringComparison.OrdinalIgnoreCase);
        }

        public ExtractionResult ExtractText(IndexingRequestDetails details, Stream stream) {
            using (var reader = new StreamReader(stream, details.Encoding, false, 16, true)) {
                var doc = XDocument.Load(reader);

                var returnResult = new ExtractionResult(details);
                returnResult.DocumentStructure.CustomerEnumerator = SimpleDegrapher.XElementDegrapher;

                if (Extensions.Count == 0)
                    returnResult.DocumentStructure.LoadObject(doc);
                foreach (var ext in Extensions)
                    if (ext.TryProcess(returnResult, doc))
                        break;

                returnResult.AnnotateSegments();
                return returnResult;
            }
        }
    }
}