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
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    /// <summary>
    ///     Tab deliminated file extractor
    /// </summary>
    public class DefaultTdfExtractor : IExtractor {
        public DefaultTdfExtractor(IServiceProvider provider) {
            Provider = provider;
        }

        public IServiceProvider Provider { get; }

        public bool CanExtract(CultureInfo culture, string mimeType, string schema) {
            return
                string.Equals("text/tab-separated-values", mimeType, StringComparison.OrdinalIgnoreCase);
        }

        public ExtractionResult ExtractText(IndexingRequestDetails details, Stream stream) {
            using (var reader = new StreamReader(stream, details.Encoding, false, 16, true)) {
                var rows = new List<string>();
                var row = string.Empty;
                while ((row = reader.ReadLine()) != null) {
                    var entry = string.Join("\r\n", row.Split('\t').AsEnumerable().Reverse());
                    rows.Add(entry);
                }


                var returnResult = new ExtractionResult(details);

                returnResult.GenerateSegments(rows, null);
                returnResult.AnnotateSegments();
                return returnResult;
            }
        }

    }
}