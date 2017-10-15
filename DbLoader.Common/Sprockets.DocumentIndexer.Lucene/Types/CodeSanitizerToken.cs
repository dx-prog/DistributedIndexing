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

namespace Sprockets.DocumentIndexer.Lucene.Types {
    /// <summary>
    ///     This is the basic unit of storage for a token that may need to be sanitized
    /// </summary>
    public class CodeSanitizerToken {
        public readonly string OriginalValue;
        private bool _invalid;

        public CodeSanitizerToken(string value) {
            OriginalValue = value ?? "";
        }

        public virtual int Priority => 100;
        public int PadLeft { get; private set; }
        public int PadRight { get; private set; }

        public virtual string Value => OriginalValue;

        public override string ToString() {
            if (_invalid)
                return "";

            var value = Value ?? "";
            var prefix = value.StartsWith(" ") ? "" : "".PadLeft(PadLeft, ' ');
            var suffix = value.EndsWith(" ") ? "" : "".PadLeft(PadRight, ' ');
            return prefix + value + suffix;
        }

        public virtual void Sanitize() {
        }

        public void MarkInvalid() {
            _invalid = true;
        }

        public void IncreasePadLeft() {
            if (PadLeft == 0)
                PadLeft++;
        }

        public void IncreasePadRight() {
            if (PadRight == 0)
                PadRight++;
        }
    }

    public class OperatorSanitizerToken : CodeSanitizerToken {
        public OperatorSanitizerToken(string value) : base(value) {
        }
    }
}