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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sprockets.Core.DocumentIndexing.Host;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.Core.OperationalPatterns;

namespace Sprockets.Core.DocumentIndexing.Indexers {
    public class IndexerAgent {
        private readonly IIntermediateCache _cache;
        private readonly ExtractorHost _extractorHost;
        private readonly ISearchProvider _searchProvider;

        public IndexerAgent(
            ExtractorHost extractorHost,
            IIntermediateCache cache,
            ISearchProvider searchProvider) {
            _extractorHost = extractorHost;
            _cache = cache;
            _searchProvider = searchProvider;
        }

        public async Task<IEnumerable<SearchResult>> Search(CancellationToken token, TextSearch search) {
            return await Task.Run(
                () => _searchProvider.Search(search),
                token);
        }

        public async Task<List<TryOperationResult<string>>> IndexDocuments(CancellationToken token,
            IEnumerable<TextIndexingRequest> requests) {
            return await Task.Run(
                () => GetResults(requests),
                token);
        }

        private List<TryOperationResult<string>> GetResults(IEnumerable<TextIndexingRequest> requests) {
            var report = new List<TryOperationResult<string>>();
            using (_extractorHost.BeginServiceScope(out var extractor)) {
                foreach (var request in requests) {
                    report.Add(request.ExtractionResult);
                    try {
                        if (!extractor.CanExtract(request.Details.Culture,
                            request.Details.MimeType,
                            request.Details.Schema))
                            continue;

                        var text = extractor.ExtractText(request.Details, request.Content);
                        request.ExtractionResult.SetSuccess(_cache.Save(
                            request.RemoteSourceIdentity,
                            request.FriendlyName,
                            request.MimeType,
                            text));
                    }
                    catch (Exception ex) {
                        request.ExtractionResult.SetFailure(ex);
                    }
                }
            }

            return report;
        }
    }
}