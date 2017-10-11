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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Scripting;
using Sprockets.Scripting.Hosts;
using Sprockets.Scripting.Types;

namespace Sprockets.Test {
    [TestClass]
    public class CSharpScriptingTests {
        [TestMethod]
        public void BasicCompileTest() {
            var bp = new BuildProject(ProgrammingLanguage.CSharp7);
            var host = new CSharpLanguageHost();

            var ns0 = bp.AddReference<ScriptableHost>();
            bp.AddReferences(AppDomain.CurrentDomain.GetAssemblies());
            bp.AddCodeFile(new CodeFile(ProgrammingLanguage.CSharp7,
                "program.cs",
                $"using {ns0};\r\n" +
                @"
                    using System;
                    using System.IO;
                    namespace TestApp
                    {
                        public class TestClass : ScriptableHost
                        {
                            public override void ExecuteCommand(string command, params object[] args)
                            {
                                var sw=(TextWriter)args[0];
                                sw.Write(command);
                            }
                        }
                    }
                "));
            var logs = host.Compile(bp, out var asm);
            foreach (var log in logs)
                Console.WriteLine(log.ToString());

            asm.AssertSuccess();

            var scriptableHost = (ScriptableHost) Activator.CreateInstance(asm.Result.GetType("TestApp.TestClass"));
            var sw = new StringWriter();
            scriptableHost.ExecuteCommand("test", sw);
            Assert.AreEqual("test", sw.ToString());
        }
    }
}