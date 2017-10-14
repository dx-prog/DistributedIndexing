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

using System.Text.RegularExpressions;

namespace Sprockets.Lexer {
    public class LexCapture {
        private readonly int _forcePosition;
        private string _forceValue;

        public LexCapture(int cursorPosition) {
            _forcePosition = cursorPosition;
        }

        /// <summary>
        ///     Get or set the depth of the stack in a nesting context
        /// </summary>
        public int StackPosition { get; internal set; }

        /// <summary>
        ///     Get or set the nesting context identity
        /// </summary>
        public object StackId { get; set; }

        /// <summary>
        ///     The position of the match
        /// </summary>
        public int CursorPositon => (Match?.Index).GetValueOrDefault(_forcePosition);

        public LexToken Type { get; set; }

        /// <summary>
        ///     Get or set the information aquired due to a regular expression match. This may be
        ///     null if the match was faked to force correct the syntax.
        /// </summary>
        public Match Match { get; set; }

        public int Length => Match.Length;

        /// <summary>
        ///     Get or set the value to return;
        /// </summary>
        /// <remarks>Unless the value is expressly set, the value to return is that of Match.Value</remarks>
        public string Value {
            get => _forceValue ?? Match.Value;
            set => _forceValue = value;
        }

        public static implicit operator bool(LexCapture capture) {
            return capture.Match.Success;
        }
    }
}