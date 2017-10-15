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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.Linq.Mapping;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using Sprockets.Core.DocumentIndexing.Types;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Sprockets.DocumentIndexer.Lucene {
    public class LuceneCache : IImmediatecache, ISearchProvider {
        public enum MemoryModel {
            Ram,
            VRam,
            Disk
        }

        public const string FlexhSearch = "FLEX";
        public const string FullSearch = "FULL";
        public const string Simple = "";

        private readonly Directory _directory;
        private readonly LuceneDataProvider _provider;
        private readonly AsyncLocal<ISession<TextDocument>> _session = new AsyncLocal<ISession<TextDocument>>();
        private readonly IndexWriter _writer;

        public LuceneCache(MemoryModel model) {
            var version = Version.LUCENE_30;
            _directory = GetDirectory(model);
            _writer = new IndexWriter(_directory, new StandardAnalyzer(version), IndexWriter.MaxFieldLength.UNLIMITED);
            _provider = new LuceneDataProvider(_directory, _writer.Analyzer, version, _writer);
        }

        private ISession<TextDocument> ActiveSession => _session.Value;

        public IDisposable OpenCache() {
            return _session.Value = _provider.OpenSession<TextDocument>();
        }

        public string Save(string remoteSourceIdentity,
            string friendlyName,
            string originalMimeType,
            ExtractionPointDetail text) {
            var record = new TextDocument(remoteSourceIdentity, friendlyName, originalMimeType, text) {
                Id = Guid.NewGuid().ToString(),
                Created = DateTimeOffset.UtcNow
            };
            ActiveSession.Add(record);

            return record.Id;
        }

        public void Clear() {
            using (OpenCache()) {
                ActiveSession.DeleteAll();
            }
        }

        public string[] SupportQueryLanguages => new[] {FlexhSearch, FullSearch, Simple};

        public IEnumerable<SearchResult> Search(TextSearch search) {
            using (OpenCache()) {
                var parser = _provider.CreateQueryParser<TextDocument>("SearchText");
                parser.AllowLeadingWildcard = true;
                var query = new StringBuilder();
                var flexSearch = search.QueryLanguage == FlexhSearch;
                if (flexSearch || search.QueryLanguage == FullSearch) {
                    var actual = flexSearch ? LuceneQuerySanitizer.Sanitize(search.Content) : search.Content;
                    foreach (var match in _provider.AsQueryable<TextDocument>().Where(parser.Parse(actual))) {
                        var tmp = new SearchResult {
                            FriendlyName = match.FriendlyName,
                            HostName = Environment.MachineName,
                            LocalSourceIdentity = match.Id,
                            OriginalRemoteSourceIdentity = match.RemoteIdentity
                        };


                        tmp.AddStatistic(actual, "1");
                        tmp.AddStatistic(match.Id, match.SearchText);
                        yield return tmp;
                    }
                }
                else {
                    foreach (var searchResult in SimpleSearch(search, query, parser))
                        yield return searchResult;
                }
            }
        }

        private IEnumerable<SearchResult> SimpleSearch(TextSearch search,
            StringBuilder query,
            FieldMappingQueryParser<TextDocument> parser) {
            var args = search.Content.Split(' ').Distinct().ToArray();
            foreach (var arg in args) {
                if (query.Length > 0)
                    query.Append(" AND ");
                query.AppendFormat("{0}", QueryParser.Escape(arg));
            }
            foreach (var match in _provider.AsQueryable<TextDocument>().Where(parser.Parse(query.ToString()))) {
                var tmp = new SearchResult {
                    FriendlyName = match.FriendlyName,
                    HostName = Environment.MachineName,
                    LocalSourceIdentity = match.Id,
                    OriginalRemoteSourceIdentity = match.RemoteIdentity
                };
                foreach (var arg in args)
                    tmp.AddStatistic(arg, args.Count(verb => match.SearchText.Contains(verb)).ToString());

                tmp.AddStatistic(match.Id, match.SearchText);
                yield return tmp;
            }
        }

        private Directory GetDirectory(MemoryModel model) {
            var dropFolder = Path.Combine(Path.GetTempPath(), "indexcache", "lucene");
            System.IO.Directory.CreateDirectory(dropFolder);
            switch (model) {
                case MemoryModel.Ram:
                    return new RAMDirectory();
                case MemoryModel.VRam:
                    return new MMapDirectory(new DirectoryInfo(dropFolder));
                case MemoryModel.Disk:
                    return new SimpleFSDirectory(new DirectoryInfo(dropFolder));
            }

            throw new NotImplementedException();
        }


        public class TextDocument {
            public TextDocument(
                string remoteIdentity,
                string friendlyName,
                string originalMimeType,
                ExtractionPointDetail details) {
                SearchText = details.Segment;
                SegmentId = details.Sid;
                RemoteIdentity = remoteIdentity;
                FriendlyName = friendlyName;
                MimeType = (originalMimeType ?? "text/unknown").ToUpper();
            }

            public TextDocument() {
            }

            [Field(IndexMode.NotAnalyzed, Store = StoreMode.Yes)]
            public string Id { get; set; }

            [Field(IndexMode.NotAnalyzed, Store = StoreMode.Yes)]
            public string RemoteIdentity { get; set; }

            [Field(IndexMode.Analyzed, Store = StoreMode.Yes)]
            public string FriendlyName { get; set; }

            [Field(IndexMode.NotAnalyzed, Store = StoreMode.Yes)]
            public string MimeType { get; set; }


            [Field(IndexMode.Analyzed, Store = StoreMode.Yes)]
            public string CitationInformation { get; set; }

            [Field(IndexMode.Analyzed, Store = StoreMode.Yes)]
            public string SearchText { get; set; }

            [NumericField]
            public int SegmentId { get; }

            public DateTimeOffset Created { get; set; }
        }
    }
}