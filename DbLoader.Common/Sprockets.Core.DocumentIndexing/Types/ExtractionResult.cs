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
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom.Html;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class ExtractionResult {
        public ExtractionResult(
            IndexingRequestDetails originalDetails) {
            Details = originalDetails;
        }

        public IndexingRequestDetails Details { get; }
        public List<ExtractionPointDetail> ExtractionPointDetails { get; } = new List<ExtractionPointDetail>();
        public TreeOrderDegrapher DocumentStructure { get; } = new TreeOrderDegrapher();


        public void AnnotateSegments() {
            for (var i = 0; i < ExtractionPointDetails.Count; i++) {
                var line = ExtractionPointDetails[i].Segment;
                ExtractionPointDetails[i].Sid = i;
                if (DocumentStructure.Mappings.TryGetValue(line, out var mappings))
                    ExtractionPointDetails[i].MappingPoint = mappings;
            }
        }

        public void GenerateSegments(object document, Func<IObjectDegrapher, object, IEnumerator> htmlDegrapher, DocumentCitation citation=null) {
            DocumentStructure.CustomerEnumerator = htmlDegrapher;
            DocumentStructure.LoadObject(document);
            ExtractionPointDetails.Clear();

            foreach (var objectString in DocumentStructure.KnowledgeBase.SelectMany(obj => obj).OfType<string>()) {
                ExtractionPointDetails.Add(
                    new ExtractionPointDetail {
                         Segment = objectString,
                         Citation= citation
                    });
            }
        }
    }
}