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
using System.IO;
using Sprockets.Core.OperationalPatterns;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class TextIndexingRequest : IndexingRequest {
        private readonly Func<TextIndexingRequest, Stream> _stream;

        public TextIndexingRequest(
            string localSourceIdentity,
            string remoteSourceIdentity,
            string friendlyName,
            IndexingRequestDetails details,
            Stream content) : base(details) {
            _stream = ignored => content;
            RemoteSourceIdentity = remoteSourceIdentity;
            LocalSourceIdentity = localSourceIdentity;
            FriendlyName = friendlyName;
        }

        public TextIndexingRequest(
            string localSourceIdentity,
            string remoteSourceIdentity,
            string friendlyName,
            IndexingRequestDetails details,
            Func<TextIndexingRequest, Stream> content) : base(details) {
            _stream = content;
            RemoteSourceIdentity = remoteSourceIdentity;
            LocalSourceIdentity = localSourceIdentity;
            FriendlyName = friendlyName;
        }

        public string FriendlyName { get; }

        public string LocalSourceIdentity { get; }
        public string RemoteSourceIdentity { get; }

        public Stream Content => _stream(this);

        public TryOperationResult<string> ExtractionResult { get; } = new TryOperationResult<string>();
        public string MimeType => Details.MimeType;
    }
}