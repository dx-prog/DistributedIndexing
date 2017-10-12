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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sprockets.Graph;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Types {
    /// <summary>
    ///     This class exists to make code changes easier at a later point; my hunch is that
    ///     simply returning one large string from an extraction would disadvantage the design
    ///     later, but I also suspect using a standard data type like an array or dictionary
    ///     would not contain enough information. The graph node is highly flexible.
    /// </summary>
    public class ExtractionResult {
        private IndexingRequestDetails _originalDetails;

        public ExtractionResult(IndexingRequestDetails originalDetails, DocumentGraphNode extractions) {
            _originalDetails = originalDetails;
            Extractions = extractions;
        }

        public DocumentGraphNode Extractions { get; set; }

        public class ExtractionPointDetail {
            public string Line { get; set; }
            public int Index { get; set; }
            public string MappingPoint { get; set; }
        }


        public class DocumentGraphNode : GraphNode<ExtractionPointDetail> {
            public IEnumerable<DocumentGraphNode> Degraph() {
                var degrapher = new TreeOrderDegrapher {
                    CustomerEnumerator = GraphNodeDegrapher
                };
                degrapher.LoadObject(this);

                return degrapher.KnowledgeBase.SelectMany(n => n).OfType<DocumentGraphNode>();
            }

            public static IEnumerator GraphNodeDegrapher(IObjectDegrapher caller, object arg) {
                if (arg is DocumentGraphNode node) {
                    yield return node.Id;
                    yield return node.Value;
                    yield return node.Value.Line;
                    yield return node.Value.Index;
                    yield return node.Value.MappingPoint;

                    foreach (var peer in node.Peers)
                        yield return peer;

                    yield break;
                }

                yield return arg;
            }

            public static DocumentGraphNode Create(TreeOrderDegrapher degrapher) {
                DocumentGraphNode root = null;
                var index = 0;
                foreach (var line in degrapher.KnowledgeBase.SelectMany(n => n).OfType<string>()) {
                    var node = new DocumentGraphNode {
                        Value = new ExtractionPointDetail {
                            Line = line,
                            Index = index++,
                            MappingPoint = string.Join(";", degrapher.Mappings[line])
                        }
                    };
                    if (null == root) {
                        root = node;
                    }
                    else {
                        root.JoinTo(node);
                        root = node;
                    }
                }

                return root;
            }
        }
    }
}