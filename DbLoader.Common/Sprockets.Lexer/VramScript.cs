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

namespace Sprockets.Lexer {
    public class VramScript {
        public static LexerCursor ParseEquation(string input) {
            var cursor = new LexerCursor {
                Input = input
            };

            while (cursor.Peek("$", @".", false)) {
                // LOOP EXIT
                if (cursor.EOF)
                    break;

                cursor.Ignore(@"\s+");
                ExtractNumber(cursor);
                while (ExtractVariable(cursor)) {
                    cursor.Ignore(@"\s+");
                    cursor.PassThrough("MEMBER", @"\.");
                    // LOOP EXIT
                    if (cursor.EOF)
                        break;
                }

                cursor.Ignore(@"\s+");
                ExtractQuoteString(cursor);

                cursor.Ignore(@"\s+");
                var parenthDepthCounter = 0;
                while (!cursor.EOF) {
                    cursor.TryPush("(", ref parenthDepthCounter);
                    cursor.Ignore(@"\s+");
                    if (cursor.TryMatch(","))
                        continue;

                    if (cursor.TryMatch("++"))
                        continue;
                    if (cursor.TryMatch("--"))
                        continue;
                    if (ExtractVariable(cursor))
                        continue;
                    if (ExtractNumber(cursor))
                        continue;


                    if (cursor.TryMatch("*"))
                        continue;
                    if (cursor.TryMatch("\\"))
                        continue;
                    if (cursor.TryMatch("%"))
                        continue;


                    if (cursor.TryMatch("+"))
                        continue;
                    if (cursor.TryMatch("-"))
                        continue;

                    if (cursor.TryMatch("==="))
                        continue;

                    if (cursor.TryMatch("<="))
                        continue;
                    if (cursor.TryMatch(">="))
                        continue;
                    if (cursor.TryMatch("<"))
                        continue;
                    if (cursor.TryMatch(">"))
                        continue;
                    if (cursor.TryMatch("=="))
                        continue;

                    if (cursor.TryPop(")", ref parenthDepthCounter))
                        break;

                    cursor.PassThrough(".");
                }
            }

            return cursor;
        }

        public static void ExtractQuoteString(LexerCursor cursor) {
            var closed = false;
            while (cursor.TryMatch("QUOTE:START", "\"")) {
                cursor.PassThrough("ESCAPE", @"\\.");

                if (cursor.TryMatch("QUOTE:END", @"""")) {
                    closed = true;
                    break;
                }

                cursor.PassThrough("TEXT_RUN", @"[^\\\""]+");
                // LOOP EXIT
                if (cursor.EOF)
                    break;
            }

            if (closed == false)
                cursor.FakeMatch("QUOTE:END", @"""");
        }

        private static bool ExtractVariable(LexerCursor cursor) {
            return cursor.TryMatch("ARG", @"\w([\d\w])*");
        }

        private static bool ExtractNumber(LexerCursor cursor) {
            return cursor.TryMatch("NUMBER", @"\d+(\.\d+)?");
        }
    }
}