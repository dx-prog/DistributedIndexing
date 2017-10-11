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

namespace Sprockets.Core.DocumentIndexing.Types {
    /// <summary>
    ///     Intermediate cache is only responsible for storing content until it pulled into the
    ///     final database
    /// </summary>
    public interface IIntermediateCache {
        void MarkAsIndex(string fileId);
        IEnumerable<TextIndexingRequest> GetReadyFiles();
        string Save(string remoteSourceIdentity, string friendlyName, string originalMimeType, string text);
    }
}