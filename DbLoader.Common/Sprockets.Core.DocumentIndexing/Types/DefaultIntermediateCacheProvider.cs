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
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Sprockets.Core.IO;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class DefaultIntermediateCacheProvider : IIntermediateCache, ISearchProvider {
        private readonly string _dropFolder;


        public DefaultIntermediateCacheProvider() {
            _dropFolder = Path.Combine(Path.GetTempPath(), "indexcache");
            Directory.CreateDirectory(_dropFolder);
        }

        public string Save(string remoteSourceIdentity,
            string friendlyName,
            string originalMimeType,
            ExtractionResult.ExtractionPointDetail text) {
            var dropFile = "";
            while (string.IsNullOrEmpty(dropFile) || File.Exists(dropFile))
                dropFile = Path.Combine(_dropFolder, DateTime.Now.Ticks + "." + text.Index + ".idx");

            var idxIdentity =
                CreateCacheEntry(remoteSourceIdentity, friendlyName, originalMimeType, text.Line, dropFile);

            return idxIdentity;
        }

        public IEnumerable<TextIndexingRequest> GetReadyFiles() {
            var finalPath = FilePathSanitizer.AssureTrailingPathSlash(_dropFolder);
            foreach (var file in Directory.EnumerateFiles(finalPath, "*.idx")) {
                // the index file is marked as indexed by the cache consumer
                if ((File.GetAttributes(file) & FileAttributes.ReadOnly) != 0)
                    continue;

                var infoFile = GetInfoFileName(file);
                if (!File.Exists(infoFile))
                    continue;

                // the info file needs to be marked as readonly to make sure
                // the index file is ready
                if ((File.GetAttributes(infoFile) & FileAttributes.ReadOnly) == 0)
                    continue;

                var info = XDocument.Load(infoFile);
                var ret = new TextIndexingRequest(
                    info.Root.Attribute("LSI").Value,
                    info.Root.Attribute("RSI").Value,
                    info.Root.Attribute("FriendlyName").Value,
                    new IndexingRequestDetails(CultureInfo.InvariantCulture,
                        Encoding.Unicode,
                        info.Root.Attribute("OriginalMimeType").Value,
                        string.Empty,
                        string.Empty),
                    p => File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read)
                );
                yield return ret;
            }
        }

        public void MarkAsIndex(string fileId) {
            var finalPath = FilePathSanitizer.CreateCleanlyJoinedFilePath(_dropFolder,
                fileId + ".idx");
            if (!File.Exists(finalPath))
                throw new FileNotFoundException(fileId);


            File.SetAttributes(finalPath, FileAttributes.ReadOnly);
        }

        public IEnumerable<SearchResult> Search(TextSearch search) {
            var matchers = GetQuery(search);

            foreach (var file in GetReadyFiles())
                using (var sr = new StreamReader(file.Content)) {
                    var str = sr.ReadToEnd();
                    var results = new SearchResult {
                        FriendlyName = file.FriendlyName,
                        HostName = Environment.MachineName,
                        LocalSourceIdentity = file.LocalSourceIdentity,
                        OriginalRemoteSourceIdentity = file.RemoteSourceIdentity
                    };
                    var anySuccess = false;
                    foreach (var matcher in matchers) {
                        var match = matcher.Match(str);
                        if (!match.Success)
                            continue;

                        anySuccess = true;
                        while (match.Success) {
                            results.AddStatistic("GROUPS", string.Join(",", match.Index, match.Length));
                            match = match.NextMatch();
                        }
                    }

                    if (anySuccess)
                        yield return results;
                }
        }

        public void Clear() {
            var finalPath = FilePathSanitizer.AssureTrailingPathSlash(_dropFolder);
            foreach (var file in Directory.EnumerateFiles(finalPath, "*.idx")) {
                File.SetAttributes(file, FileAttributes.Normal);
                File.SetAttributes(GetInfoFileName(file), FileAttributes.Normal);

                File.Delete(file);
                File.Delete(GetInfoFileName(file));
            }
        }

        private static string CreateCacheEntry(string remoteSourceIdentity,
            string friendlyName,
            string originalMimeType,
            string text,
            string dropFile) {
            var idxIdentity = Path.GetFileNameWithoutExtension(dropFile);
            File.WriteAllText(dropFile, text);
            var e = new XElement("IndexInformation");
            var now = DateTimeOffset.UtcNow;
            e.SetAttributeValue("LSI", idxIdentity);
            e.SetAttributeValue("RSI", remoteSourceIdentity);
            e.SetAttributeValue("FriendlyName", friendlyName);
            e.SetAttributeValue("OriginalMimeType", originalMimeType);

            e.SetAttributeValue("CreatedDtoUtc", now.ToString());
            e.SetAttributeValue("CreatedLocal", now.DateTime.ToLocalTime());
            using (var sha = new SHA256Managed()) {
                e.SetAttributeValue("Md5Hash",
                    Convert.ToBase64String(sha.ComputeHash(Encoding.Unicode.GetBytes(text))));
            }
            File.WriteAllText(GetInfoFileName(dropFile), e.ToString(SaveOptions.None));
            File.SetAttributes(GetInfoFileName(dropFile), FileAttributes.ReadOnly);
            return idxIdentity;
        }

        private static List<Regex> GetQuery(TextSearch search) {
            var matchers = new List<Regex>();
            var language = (search.QueryLanguage ?? "PLAIN").ToUpperInvariant();
            var query = search.Content;
            switch (language) {
                case "REGEX":
                    AddFullRegex(matchers, query);
                    break;
                case "PLAIN":
                    AddEscapedRegex(matchers, query);
                    break;
                case "REGEX+XML":
                    var markup = XElement.Parse(search.Content);

                    foreach (var pattern in markup.Descendants("regex"))
                        if (pattern.Attribute("escape").Value == "1")
                            AddEscapedRegex(matchers, pattern.Value);
                        else
                            AddFullRegex(matchers, pattern.Value);

                    break;
                default:
                    throw new NotSupportedException(search.QueryLanguage);
            }

            return matchers;
        }

        private static void AddEscapedRegex(List<Regex> matchers, string query) {
            matchers.Add(new Regex(Regex.Escape(query),
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline));
        }

        private static void AddFullRegex(List<Regex> matchers, string query) {
            matchers.Add(new Regex(query, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline));
        }

        private static string GetInfoFileName(string dropFile) {
            return dropFile + ".info";
        }
    }
}