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

namespace Sprockets.Core.DocumentIndexing.Types {
    /// <summary>
    ///     An extractor may extract out multiple document from a source, wherein
    ///     such documents may have their own citation details that need to be pushed
    ///     to the archiving database
    /// </summary>
    public class DocumentCitation {
        /// <summary>
        ///     Get or set the title of the document
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Get or set the publishers
        /// </summary>
        /// <remarks>
        ///     The publisher(s) of the document; there may be more then
        ///     one if the extraction was from an OCR
        /// </remarks>
        public string[] Publisher { get; set; }

        /// <summary>
        ///     Get or set any copyright notice
        /// </summary>
        public string[] Copyright { get; set; }

        /// <summary>
        ///     Get or set the langauge information
        /// </summary>
        public string[] Language { get; set; }

        /// <summary>
        ///     Get or set the document description
        /// </summary>
        public string Description { get; set; }
    }
}