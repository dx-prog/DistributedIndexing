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
using Sprockets.Core.DocumentIndexing.Extractors;
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Core.DocumentIndexing.Indexers {
    public abstract class TextFileIndexer : ITextDocumentIndexer {
        private static readonly IndexingRequestDetails[] StandardTextFormats;

        static TextFileIndexer() {
            StandardTextFormats = new[] {
                IndexingRequestDetails.Create<PassthroughTextExtractor>(CultureInfo.InvariantCulture,
                    "text/plain",
                    string.Empty),
                IndexingRequestDetails.Create<DefaultXmlTextExtractor>(CultureInfo.InvariantCulture,
                    "text/xml",
                    string.Empty),
                IndexingRequestDetails.Create<DefaultHtmlTextExtractor>(CultureInfo.InvariantCulture,
                    "text/html",
                    string.Empty),
                IndexingRequestDetails.Create<DefaultJsonTextExtractor>(CultureInfo.InvariantCulture,
                    "text/json",
                    string.Empty)
            };
        }

        public virtual string Name => "SIMPLE TEXT";
        public virtual Version Version => new Version(0, 1);

        public virtual string[] QueryLangauges => new[] {"PLAIN"};

        public virtual IndexingRequestDetails[] SupportedFormats => StandardTextFormats;

        public abstract void Search<TDocumentType>(TextSearch search);

        public abstract void IndexDocuments<TDocumentType>(IEnumerable<TextIndexingRequest> source);
    }
}