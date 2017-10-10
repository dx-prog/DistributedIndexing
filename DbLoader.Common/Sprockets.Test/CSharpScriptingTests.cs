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