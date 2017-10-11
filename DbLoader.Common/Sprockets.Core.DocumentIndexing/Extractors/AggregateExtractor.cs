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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    public class AggregateExtractor : IExtractor {
        public AggregateExtractor(IEnumerable<IExtractor> extractors) {
            Extractors = new List<IExtractor>(extractors);
        }

        public virtual IEnumerable<IExtractor> Extractors { get; }

        public bool CanExtract(CultureInfo culture, string mimeType, string schema) {
            return Extractors.Any(e => e.CanExtract(culture, mimeType, schema));
        }

        public string ExtractText(IndexingRequestDetails details, Stream reader) {
            var available = Extractors.Where(e => e.CanExtract(details.Culture, details.MimeType, details.Schema))
                .ToArray();

            // first choice
            var choice = available.FirstOrDefault(e => e.GetType().AssemblyQualifiedName == details.Handler);
            if (choice != null)
                return choice.ExtractText(details, reader);

            // second choice
            choice = available.FirstOrDefault(e => e.GetType().FullName == details.Handler);
            if (choice != null)
                return choice.ExtractText(details, reader);

            // third choice
            choice = available.FirstOrDefault(e => e.GetType().Name == details.Handler);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (choice != null)
                return choice.ExtractText(details, reader);

            // last choice
            return available[0].ExtractText(details, reader);
        }
    }
}