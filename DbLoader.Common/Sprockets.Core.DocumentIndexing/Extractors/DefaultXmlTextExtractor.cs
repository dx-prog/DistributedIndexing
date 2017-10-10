﻿/***********************************************************************************
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Extractors {
    public class DefaultXmlTextExtractor : ITextExtractor {
        public string ExtractText(IndexingRequestDetails details, TextReader reader) {
            var doc = XDocument.Load(reader);

            var degrapher = new SimpleDegrapher();
            degrapher.LoadObject(doc);
            if (degrapher.PumpFor(TimeSpan.FromSeconds(1)))
                throw new SerializationException();

            return string.Join(Environment.NewLine, degrapher.KnowledgeBase.SelectMany(x => x).OfType<string>());
        }
    }
}