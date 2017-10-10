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
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    public class DefaultJsonTextExtractor : ITextExtractor {
        public string ExtractText(IndexingRequestDetails details, TextReader reader) {
            var obj = JsonConvert.DeserializeObject(reader.ReadToEnd());

            var degrapher = new SimpleDegrapher {CustomerEnumerator = JsonDegrapher};
            degrapher.LoadObject(obj);
            if (degrapher.PumpFor(TimeSpan.FromSeconds(1)))
                throw new SerializationException();

            return string.Join(Environment.NewLine, degrapher.KnowledgeBase.SelectMany(x => x).OfType<string>());
        }

        public static IEnumerator JsonDegrapher(IObjectDegrapher caller, object arg) {
            switch (arg) {
                case JToken array:
                    foreach (var entry in array.Children())
                        yield return entry;

                    break;

                case string _:
                    yield return arg;

                    break;
            }
        }
    }
}