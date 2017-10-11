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
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sprockets.Core.OperationalPatterns;
using Sprockets.Scripting.Types;

namespace Sprockets.Scripting.Hosts {
    public class CSharpLanguageHost : ICompilerService {
        public ProgrammingLanguage Language => new ProgrammingLanguage(
            Version.Parse("5.0"),
            "CSHARP",
            "ROSYLN"
        );

        public List<Diagnostic> Compile(BuildProject project, out TryOperationResult<Assembly> assembly) {
            var ret = new List<Diagnostic>();
            assembly = new TryOperationResult<Assembly>();
            try {
                var assemblyName = project.ProjectName ?? Path.GetRandomFileName();
                var references = new Dictionary<string, MetadataReference>();
                foreach (var reference in project.References) {
                    var asmRef = MetadataReference.CreateFromFile(reference);
                    references[asmRef.FilePath] = asmRef;
                }


                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    project.Files.Select(code => CSharpSyntaxTree.ParseText(code.Content)).ToArray(),
                    references.Values,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream()) {
                    var result = compilation.Emit(ms);

                    ret.AddRange(result.Diagnostics);
                    if (result.Success) {
                        ms.Position = 0;
                        assembly.SetSuccess(Assembly.Load(ms.ToArray()));
                    }
                }
            }
            catch (Exception ex) {
                assembly.SetFailure(ex);
            }

            return ret;
        }
    }
}