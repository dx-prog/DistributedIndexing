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
using System.Text;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class IndexingRequestDetails {
        public IndexingRequestDetails(
            CultureInfo culture,
            Encoding encoding,
            string mimeType,
            string schema,
            string handler) {
            Schema = schema;
            Culture = culture;
            MimeType = mimeType;
            Handler = handler;
            Encoding = encoding;
        }

        public string Handler { get; set; }

        public string MimeType { get; }

        public string Schema { get; }

        public Encoding Encoding { get; }
        public CultureInfo Culture { get; }


        public static IndexingRequestDetails Create<T>(CultureInfo culture,
            Encoding encoding,
            string mimeType,
            string schema)
            where T : IExtractor {
            return new IndexingRequestDetails(culture, encoding, mimeType, schema, typeof(T).AssemblyQualifiedName);
        }
    }
}