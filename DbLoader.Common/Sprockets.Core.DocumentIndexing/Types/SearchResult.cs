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

using System.Collections.Generic;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class SearchResult {
        /// <summary>
        ///     Get or set the name of the machine that hosts the content
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        ///     Get or set the identity passed to the the indexer from the
        ///     provider of the document; this may be a file name, or a URI
        /// </summary>
        public string OriginalRemoteSourceIdentity { get; set; }

        /// <summary>
        ///     Get or set the friendly name for the document
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     Get or set the identity used to retrieve a specific text documents content
        ///     from the text repository
        /// </summary>
        public string LocalSourceIdentity { get; set; }

        public Dictionary<string, List<string>> Statistics { get; } = new Dictionary<string, List<string>>();

        public void AddStatistic(string namedStatistic, string tuple) {
            if (!Statistics.TryGetValue(namedStatistic, out var results))
                Statistics[namedStatistic] = results = new List<string>();

            results.Add(tuple);
        }
    }
}