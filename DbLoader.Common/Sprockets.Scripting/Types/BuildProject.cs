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
using System.Reflection;

namespace Sprockets.Scripting.Types {
    public class BuildProject : ILanguageHost {
        private readonly List<CodeFile> _codeFile = new List<CodeFile>();
        private readonly List<string> _references = new List<string>();

        public BuildProject(ProgrammingLanguage langauge) {
            Language = langauge;
        }

        public IReadOnlyCollection<CodeFile> Files => _codeFile.AsReadOnly();
        public IReadOnlyCollection<string> References => _references.AsReadOnly();
        public ProgrammingLanguage Language { get; }
        public string ProjectName { get; set; }
        public void AddReference(string file) {
            _references.Add(file);
        }

        public string AddReference<T>() {
            var type = typeof(T);
            var asm = type.Assembly;
            AddReference(asm.Location);
            return type.Namespace;
        }
        public void AddCodeFile(CodeFile file) {
            if (file.Language.Name != Language.Name)
                throw new ArgumentException();

            _codeFile.Add(file);
        }

        public void AddReferences(Assembly[] assemblies) {
            foreach (var file in assemblies) {
                _references.Add(file.Location);
            }
        }
    }
}