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
using System.Linq;
using Sprockets.DocumentIndexer.Lucene.Types;
using Sprockets.Lexer;

namespace Sprockets.DocumentIndexer.Lucene {
    public class LuceneQuerySanitizer {
        private static readonly char[] Repeatables = {
            '\"',
            '(',
            ')'
        };

         static string SanitizePhase(LexerCursor inputCursor) {
            var elements = new List<LexCapture>();
            RemoveDisallowedRepeats(inputCursor, elements);


            var scopebuilder = new ScopeBuilder();
            StructureSyntax(elements, scopebuilder);
            scopebuilder.LastestScopeSanitizer.Sanitize();
            var ret= scopebuilder.LastestScopeSanitizer.ToString().Trim();
            return string.IsNullOrWhiteSpace(ret) ? "*" : ret;
        }

        private static void StructureSyntax(List<LexCapture> elements, ScopeBuilder scopebuilder) {
            foreach (var element in elements)
                switch (element.Type.Name) {
                    case LuceneQueryParser.KeywordParenthesesOpen:
                        scopebuilder.BeginSubGroup();
                        break;
                    case LuceneQueryParser.KeywordParenthesesClose:
                        scopebuilder.ExitSubGroup();
                        break;
                    case LuceneQueryParser.KeywordQuoteContent:
                        switch (scopebuilder.LastestGroup.LastOrDefault()) {
                            case StringSanitizerToken stringToken:
                                var stringGroup = new StringGroupSanitizerToken();
                                scopebuilder.LastestGroup.Last.Value = stringGroup;
                                stringGroup.Tokens.Add(stringToken);
                                stringGroup.Tokens.Add(new StringSanitizerToken(element.Value));
                                break;
                            case StringGroupSanitizerToken group:
                                group.Tokens.Add(new StringSanitizerToken(element.Value));
                                break;
                            default:
                                scopebuilder.LastestGroup.AddLast(new StringSanitizerToken(element.Value));
                                break;
                        }

                        break;
                    case LuceneQueryParser.KeywordOperator:
                        switch (element.Value) {
                            case "AND":
                            case "OR":
                                scopebuilder.LastestGroup.AddLast(new BinaryOperatorSanitizerToken(element.Value));
                                break;
                            case "NOT":
                                scopebuilder.LastestGroup.AddLast(new UnaryOperatorSanitizerToken("!"));
                                break;
                            case "+":
                            case "-":
                            case "!":
                                scopebuilder.LastestGroup.AddLast(new UnaryOperatorSanitizerToken(element.Value));
                                break;
                            default:
                                throw new InvalidOperationException();
                        }

                        break;
                    case LuceneQueryParser.KeywordOperand:
                        scopebuilder.LastestGroup.AddLast(new OperandSanitizerToken(element.Value));
                        break;
                    case LuceneQueryParser.KeywordWhiteSpace:
                        scopebuilder.LastestGroup.AddLast(new DelimiterSanitizerToken());
                        break;
                }
        }

        private static void RemoveDisallowedRepeats(LexerCursor inputCursor, List<LexCapture> elements) {
            LexCapture lastOperator = null;
            foreach (var element in inputCursor.GetVitalCapture()) {
                if (element.Type.Name == null)
                    continue;

                element.Value = (element.Value ?? "").Trim();
                if (string.IsNullOrWhiteSpace(element.Value))
                    element.Value = " ";

                if (IsDisallowedRepeats(elements, lastOperator, element))
                    continue;

                elements.Add(element);
                switch (element.Type.Name) {
                    case LuceneQueryParser.KeywordParenthesesClose:
                    case LuceneQueryParser.KeywordParenthesesOpen:
                    case LuceneQueryParser.KeywordQuoteContent:
                    case LuceneQueryParser.KeywordOperand:
                        lastOperator = null;
                        break;
                    case LuceneQueryParser.KeywordOperator:
                        lastOperator = element;
                        break;
                    // ReSharper disable once RedundantEmptySwitchSection
                    default:
                        break;
                }
            }
        }

        private static bool IsDisallowedRepeats(List<LexCapture> elements,
            LexCapture lastOperator,
            LexCapture element) {
            if (lastOperator?.Value == element.Value && lastOperator?.Type?.Name == element.Type.Name)
                return true;

            if (elements.LastOrDefault()?.Value != element.Value)
                return false;

            if (element.Value?.Length != 1)
                return true;

            return !Repeatables.Contains(element.Value[0]);
        }

        public static string Sanitize(string unsanitizedInput) {
            var input = unsanitizedInput;
            while (true)
            { 
                var segments = LuceneQueryParser.ParseQuery(input);
                var phaseResult = SanitizePhase(segments);

                if (phaseResult == input)
                    return phaseResult;

                input = phaseResult;
            } 

        }
    }
}