﻿/***********************************************************************************
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

using Sprockets.Lexer;

namespace Sprockets.DocumentIndexer.Lucene {
    public class LuceneQueryParser {
        public const string KeywordOperator = "OPERATOR";
        public const string KeywordParenthesesOpen = "PAREN:OPEN";
        public const string KeywordParenthesesClose = "PAREN:CLOSE";
        public const string KeywordQuoteContent = "QUOTE";
        public const string KeywordOperand = "OPERAND";
        public const string KeywordWhiteSpace = "WHITESPACE";


        public static LexerCursor ParseQuery(string input) {
            var cursor = new LexerCursor {
                Input = input,
                MaxStack = 256
            };

            var lastPosition = -1;
            while (cursor.Peek("$", @".", false)) {

                // LOOP EXIT
                if (cursor.EOF)
                    break;

                if (ExtractContent(cursor))
                    continue;
                // ignore closing parentheses if not in the context of group
                cursor.Ignore(@"\)");
                var closuresRequired = 0;
                while (!cursor.EOF) {
                    lastPosition = GobbleUnsupported(lastPosition, cursor);
                    if (!cursor.TryPush(KeywordParenthesesOpen, @"\(", ref closuresRequired))
                        break;


                    if (ExtractContent(cursor))
                        continue;

                    var exitPop = cursor.TryPop(KeywordParenthesesClose, @"\)", ref closuresRequired);

                    if (exitPop)
                        break;
                }

                while (closuresRequired > 0) {
                    closuresRequired--;
                    cursor.FakeMatch(KeywordParenthesesClose, @")", pop: true);
                }

            }

            return cursor;
        }

        private static int GobbleUnsupported(int lastPosition, LexerCursor cursor) {
            if (lastPosition != cursor.Position)
                lastPosition = cursor.Position;
            else {
                cursor.TryMatch(null, ".");
            }
            return lastPosition;
        }


        public static void ExtractQuoteString(LexerCursor cursor) {
            bool? closed = null;
            while (!cursor.EOF) {
                if (cursor.Peek(null, "\""))
                    if (null == closed) {
                        closed = false;
                        cursor.TryMatch(KeywordQuoteContent, "\"");
                    }
                    else if (cursor.TryMatch(KeywordQuoteContent, @"([""](\~\d{1,4}(\z|(?=\s)))?)")) {
                        closed = true;
                        break;
                    }

                if (closed == null)
                    break;

                if (cursor.TryMatch(KeywordQuoteContent, @"\\[\""]"))
                    continue;

                // tag unexpected backslashes to be ignored
                if (cursor.TryMatch(null, @"(\\$|\\.)"))
                    continue;
                if (cursor.TryMatch(KeywordQuoteContent, @"[^\\\""]+"))
                    continue;
                // LOOP EXIT
                if (cursor.EOF)
                    break;

            }

            if (closed == false)
                cursor.FakeMatch(KeywordQuoteContent, @"""", pop: true);
        }

        private static bool ExtractContent(LexerCursor cursor) {
            cursor.PassThrough(KeywordWhiteSpace, @"\s+");
            ExtractQuoteString(cursor);

            if (cursor.TryMatch(KeywordOperator, "\\+") ||
                cursor.TryMatch(KeywordOperator, "\\-") ||
                cursor.TryMatch(KeywordOperator, "\\!") ||
                cursor.TryMatch(KeywordOperator, "NOT[\\s$]"))
                return true;
            if (ExtractOperator(cursor) || ExtractSearchTerm(cursor))
                return true;

            return false;
        }

        private static bool ExtractOperator(LexerCursor cursor) {
            return cursor.TryMatch(KeywordOperator, @"(\bOR\b|\bAND\b)");
        }

        private static bool ExtractSearchTerm(LexerCursor cursor) {
            return cursor.TryMatch(KeywordOperand, @"(?<var>[\w\.\*\?]+)(\~0*\.\d{1,4}(\z|(?=\s)))?");
        }
    }
}