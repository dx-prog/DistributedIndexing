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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sprockets.Lexer {
    /// <summary>
    ///     This class looks horrible, however, it is designed to make writing parsers easier.
    /// </summary>
    public class LexerCursor {
        // ReSharper disable once InconsistentNaming
        public bool EOF => Position == Input.Length;

        public string Input { get; set; }
        public int Position { get; set; }
        public List<LexCapture> Captures { get; } = new List<LexCapture>();
        public Stack<object> MatchStack { get; } = new Stack<object>();

        public LexCapture LastMatch => Captures.LastOrDefault();
        public ConcurrentDictionary<string, Regex> Patterns { get; } = new ConcurrentDictionary<string, Regex>();
        public int MaxStack { get; set; } = 1000;

        /// <summary>
        ///     Try to pop out of a nested parsing section
        /// </summary>
        /// <param name="name">Name of the token to try to match</param>
        /// <param name="pattern">The pattern to match</param>
        /// <param name="matchCounter"></param>
        /// <param name="advance">true if the cursor is to advance forward on a match</param>
        /// <param name="autoAnchor">true if the \G anchor is automatically inserted</param>
        /// <returns>true if scope is fully exited</returns>
        public bool TryPop(
            string name,
            string pattern,
            ref int matchCounter,
            bool advance = true,
            bool autoAnchor = true) {
            var matched = TryMatch(name, pattern, advance, autoAnchor);
            if (matched == false)
                return MatchStack.Count == 0;

            MatchPop();
            matchCounter--;
            return MatchStack.Count == 0;
        }

        /// <summary>
        ///     Try to pop out of a nested parsing section
        /// </summary>
        /// <param name="nameEscape">Name and Pattern of the token to try to match</param>
        /// <param name="matchCounter"></param>
        /// <param name="advance">true if the cursor is to advance forward on a match</param>
        /// <param name="autoAnchor">true if the \G anchor is automatically inserted</param>
        /// <returns>true if scope is fully exited</returns>
        public bool TryPop(string nameEscape, ref int matchCounter, bool advance = true, bool autoAnchor = true) {
            var matched = TryMatch(nameEscape, advance, autoAnchor);
            if (matched == false)
                return MatchStack.Count == 0;

            MatchPop();
            matchCounter--;
            return MatchStack.Count == 0;
        }

        /// <summary>
        ///     Push into the scope of a nested parsing section
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pattern"></param>
        /// <param name="matchCounter"></param>
        /// <param name="advance"></param>
        /// <param name="autoAnchor"></param>
        /// <returns></returns>
        public bool TryPush(string name,
            string pattern,
            ref int matchCounter,
            bool advance = true,
            bool autoAnchor = true) {
            var matched = TryMatch(name, pattern, advance, autoAnchor);
            if (matched == false)
                return MatchStack.Count > 0;

            MatchPush();
            matchCounter++;
            return MatchStack.Count > 0;
        }

        /// <summary>
        ///     Push into the scope of a nested parsing section
        /// </summary>
        /// <param name="nameEscape"></param>
        /// <param name="matchCounter"></param>
        /// <param name="advance"></param>
        /// <param name="autoAnchor"></param>
        /// <returns></returns>
        public bool TryPush(string nameEscape, ref int matchCounter, bool advance = true, bool autoAnchor = true) {
            var matched = TryMatch(nameEscape, advance, autoAnchor);
            if (matched == false)
                return MatchStack.Count > 0;

            MatchPush();
            matchCounter++;
            return MatchStack.Count > 0;
        }

        /// <summary>
        ///     Look ahead without advancing by default
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pattern"></param>
        /// <param name="advance"></param>
        /// <param name="autoAnchor"></param>
        /// <returns></returns>
        public bool Peek(string name, string pattern, bool advance = false, bool autoAnchor = true) {
            return Execute(name, pattern, advance, autoAnchor);
        }

        /// <summary>
        ///     Capture and advance if match found
        /// </summary>
        /// <param name="nameEscape"></param>
        /// <param name="advance"></param>
        /// <param name="autoAnchor"></param>
        /// <returns></returns>
        public bool TryMatch(string nameEscape, bool advance = true, bool autoAnchor = true) {
            var ret = Execute(nameEscape, Regex.Escape(nameEscape), advance, autoAnchor);
            if (advance && ret)
                Position += LastMatch.Length;
            return ret;
        }

        /// <summary>
        ///     Capture and advance if match found
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pattern"></param>
        /// <param name="advance"></param>
        /// <param name="autoAnchor"></param>
        /// <returns></returns>
        public bool TryMatch(string name, string pattern, bool advance = true, bool autoAnchor = true) {
            var ret = Execute(name, pattern, advance, autoAnchor);
            if (advance && ret)
                Position += LastMatch.Length;

            return ret;
        }

        /// <summary>
        ///     Marks a match if found as ignored
        /// </summary>
        /// <param name="pattern"></param>
        public void Ignore(string pattern) {
            TryMatch(null, pattern);
        }

        /// <summary>
        ///     Wrap "TryMatch" to prevent people from trying to use return value; named passthrough to indicate purpose.
        /// </summary>
        /// <param name="namePattern"></param>
        public void PassThrough(string namePattern) {
            TryMatch(namePattern);
        }

        /// <summary>
        ///     Wrap "TryMatch" to prevent people from trying to use return value; named passthrough to indicate purpose.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pattern"></param>
        public void PassThrough(string name, string pattern) {
            TryMatch(name, pattern);
        }

        public IEnumerable<string> GetVitalCaptureValues() {
            return Captures.Where(c =>
                    c.Type.Advancing &&
                    !string.IsNullOrEmpty(c.Type.Name))
                .Select(c => c.Value);
        }

        public IEnumerable<LexCapture> GetVitalCapture() {
            return Captures.Where(c =>
                    c.Type.Advancing &&
                    !string.IsNullOrEmpty(c.Type.Name))
                .Select(c => c);
        }

        /// <summary>
        ///     Used for corrective parsing
        /// </summary>
        /// <param name="name"></param>
        /// <param name="forcedValue"></param>
        /// <param name="advanceBy"></param>
        public void FakeMatch(string name, string forcedValue, int advanceBy = 1, bool pop = false) {
            AddCaptureToCaptureHistory(new LexCapture(Position) {
                Match = null,
                Type = new LexToken(name, Regex.Escape(forcedValue), advanceBy > 0),
                Value = forcedValue
            });
            if (!EOF)
                Position += advanceBy;
            if (pop)
                MatchPop();
        }

        protected LexCapture Execute(string name, string pattern, bool advance = true, bool autoAnchor = true) {
            if (Captures.Count > MaxStack)
                throw new InvalidOperationException();

            if (autoAnchor)
                pattern = "\\G" + pattern;
            var regex = Patterns.GetOrAdd(pattern, p => new Regex(p, RegexOptions.Compiled));

            var regMatch = regex.Match(Input, Position);


            var matchResult = new LexCapture(Position) {
                Match = regMatch,
                StackPosition = MatchStack.Count,
                StackId = CurrentStackId(),
                Type = new LexToken(name, pattern, advance)
            };

            if (matchResult.Match?.Success == true)
                AddCaptureToCaptureHistory(matchResult);
            return matchResult;
        }

        private void AddCaptureToCaptureHistory(LexCapture matchResult) {
            Captures.Add(matchResult);
            if (Captures.Count > MaxStack)
                throw new InvalidOperationException();
        }

        private object CurrentStackId() {
            return MatchStack.Count == 0 ? null : MatchStack.Peek();
        }

        private void MatchPush() {
            var objId = new object();
            MatchStack.Push(objId);
            LastMatch.StackPosition = MatchStack.Count;
            LastMatch.StackId = objId;
        }

        private void MatchPop() {
            if (MatchStack.Count == 0) {
                LastMatch.StackPosition = -1;
                LastMatch.StackId = null;
                return;
            }

            LastMatch.StackPosition = MatchStack.Count;
            LastMatch.StackId = MatchStack.Pop();
        }
    }
}