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

namespace Sprockets.Core.IO {
    /// <summary>
    ///     Used to bound directory paths to a common root
    /// </summary>
    public class FilePathSanitizer {
        public static string AssureLeadingVirtualPathSlash(string prefix) {
            prefix = prefix.Replace("\\", "/");
            if (prefix.StartsWith("/"))
                return prefix;

            return "/" + prefix;
        }

        public static string AssureLeadingLocalPathSlash(string prefix) {
            prefix = prefix.Replace("\\", "/");
            if (prefix.StartsWith("/"))
                return prefix;

            return "/" + prefix;
        }

        public static string AssureTrailingPathSlash(string path) {
            path = CleanBadSlashes(path);
            if (path.EndsWith("\\"))
                return path;

            return path + "\\";
        }

        /// <summary>
        ///     Make sure all the dots and double dots are remove
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fullPathResolver"></param>
        /// <returns>return true if the navigation dots are invalid</returns>
        public static bool RemoveNavigationDots(ref string filePath, Func<string, string> fullPathResolver = null) {
            fullPathResolver = fullPathResolver ?? Path.GetFullPath;
            var parts = new Stack<string>();
            var segments = filePath.Split('\\', '/').Where(x => !string.IsNullOrEmpty(x));
            foreach (var segment in segments)
                switch (segment) {
                    case ".":
                        continue;
                    case "..":
                        if (parts.Count == 0)
                            return true;

                        parts.Pop();
                        continue;
                    default:
                        parts.Push(segment);
                        break;
                }

            filePath = fullPathResolver(string.Join("\\", parts.ToArray().Reverse()));
            return false;
        }

        public static string CleanBadSlashes(string subDir, string preferedDelimeter = "\\") {
            return string.Join(preferedDelimeter, subDir.Split('\\', '/').Where(x => !string.IsNullOrEmpty(x)));
        }

        public static string CreateCleanlyJoinedFilePath(string root,
            string subDir,
            Func<string, string> fullPathResolver = null) {
            fullPathResolver = fullPathResolver ?? Path.GetFullPath;

            root = fullPathResolver(CleanBadSlashes(root));
            subDir = CleanBadSlashes(subDir);

            var combined = fullPathResolver(Path.Combine(root, subDir));

            if (RemoveNavigationDots(ref combined, fullPathResolver))
                return null;

            // we may get a folder path that does not end in a slash
            // and if two folders overlap in name, we can get a false match
            // as we only want to return the file path if the 
            // requested file path is equal or suborinate to the
            // "root" path
            return (combined + "\\").StartsWith(root + "\\") ? combined : null;
        }

        public static string ToClassName(string filePath) {
            return CleanBadSlashes(filePath, "_");
        }
    }
}