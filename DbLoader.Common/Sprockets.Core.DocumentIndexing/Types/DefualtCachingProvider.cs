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
using System.Xml.Linq;
using Sprockets.Core.IO;

namespace Sprockets.Core.DocumentIndexing.Types {
    public class DefualtCachingProvider : IIntermediateCache {


        private readonly string _dropFolder;

        public DefualtCachingProvider() {
            _dropFolder = Path.Combine(Path.GetTempPath(), "indexcache");
            Directory.CreateDirectory(_dropFolder);
        }

        public string Save(string remoteSourceIdentity, string friendlyName, string text) {
            var dropFile = "";
            while (string.IsNullOrEmpty(dropFile) || File.Exists(dropFile))
                dropFile = Path.Combine(_dropFolder, DateTime.Now.Ticks + ".idx");

            var idxIdentity = Path.GetFileNameWithoutExtension(dropFile);
            File.WriteAllText(dropFile, text);
            var e = new XElement("IndexInformation");
            var now = DateTimeOffset.UtcNow;
            e.SetAttributeValue("RSI", remoteSourceIdentity);
            e.SetAttributeValue("FriendlyName", friendlyName);
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

        public IEnumerable<TextIndexingRequest> GetReadyFiles() {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(_dropFolder, "*.idx"))) {
                // the index file is marked as indexed in the final database
                // if it readonly
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
                    info.Root.Attribute("RSI").Value,
                    info.Root.Attribute("FriendlyName").Value,
                    new IndexingRequestDetails(CultureInfo.InvariantCulture,
                        Encoding.Unicode,
                        "text/plain",
                        string.Empty,
                        string.Empty),
                    p => File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None)
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

        private static string GetInfoFileName(string dropFile) {
            return dropFile + ".info";
        }
    }
}