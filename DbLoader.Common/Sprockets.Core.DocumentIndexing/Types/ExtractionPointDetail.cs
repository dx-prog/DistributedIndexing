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
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class ExtractionPointDetail {
        /// <summary>
        /// </summary>
        public DocumentCitation Citation { get; set; }

        /// <summary>
        ///     Get or set the location where the segment may be found out
        /// </summary>
        public string DocumentLocation { get; set; }

        /// <summary>
        ///     Get or set the segment of text to put into the archiving database
        /// </summary>
        /// <remarks>The segment may be the content of a line, paragraph, page, section, chapter, book, etc.</remarks>
        public string Segment { get; set; }

        /// <summary>
        ///     Get or set the segment id
        /// </summary>
        public int Sid { get; set; }

        /// <summary>
        ///     Get information regarding the internal data structure from the original source
        /// </summary>
        public List<TreeOrderDegrapher.TreeOrderIndex> MappingPoint { get; internal set; }
    }
}