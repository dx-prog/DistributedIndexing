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

using System.Collections.Generic;
using Sprockets.DocumentIndexer.Lucene.Types;

namespace Sprockets.DocumentIndexer.Lucene {
    public class ScopeBuilder {
        public ScopeBuilder() {
            AllGroups = new List<ScopeSanitizerToken>();
            WorkingGroup = new Stack<ScopeSanitizerToken>();
            WorkingGroup.Push(new ScopeSanitizerToken(false));
        }

        public ScopeSanitizerToken LastestScopeSanitizer => WorkingGroup.Peek();
        public LinkedList<CodeSanitizerToken> LastestGroup => WorkingGroup.Peek().Tokens;
        public Stack<ScopeSanitizerToken> WorkingGroup { get; }

        public List<ScopeSanitizerToken> AllGroups { get; }

        public void BeginSubGroup() {
            var nextGroup = new ScopeSanitizerToken();
            AllGroups.Add(nextGroup);
            LastestGroup.AddLast(nextGroup);
            WorkingGroup.Push(nextGroup);
        }

        public void ExitSubGroup() {
            WorkingGroup.Pop();
        }
    }
}