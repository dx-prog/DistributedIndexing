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

namespace Sprockets.Scripting.Types
{
    /// <summary>
    /// Descripts programming langauges that can be used
    /// </summary>
    public class ProgrammingLanguage
    {
        public ProgrammingLanguage(Version version, string name, string standard)
        {
            Version = version;
            Name = name;
            Standard = standard;
        }

        public string Standard { get; }
        public Version Version { get; }
        public string Name { get; }


        public static readonly ProgrammingLanguage CSharp5 = new ProgrammingLanguage(
            Version.Parse("5.0"),
            "CSHARP",
            "ROSYLN"
        );

        public static readonly ProgrammingLanguage CSharp6 = new ProgrammingLanguage(
            Version.Parse("6.0"),
            "CSHARP",
            "ROSYLN"
        );

        public static readonly ProgrammingLanguage CSharp7 = new ProgrammingLanguage(
            Version.Parse("7.0"),
            "CSHARP",
            "ROSYLN"
        );
    }
}
