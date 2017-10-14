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
using System.Text;
using System.Threading.Tasks;
using Sprockets.Lexer;

namespace Sprockets.DocumentIndexer.Lucene
{
    public class LuceneQuerySanitizer
    {
        private static readonly char [] Repeatables=new char[] {
            '\"',
            '(',
            ')'
        };
        public static string Sanitize(LexerCursor inputCursor) {
            var elements = new List<LexCapture>();
            RemoveDisallowedRepeats(inputCursor, elements);

            return string.Concat(elements.Select(e=>e.Value)).Trim();
        }

        private static void RemoveDisallowedRepeats(LexerCursor inputCursor, List<LexCapture> elements) {
            foreach (var element in inputCursor.GetVitalCapture()) {
                element.Value = (element.Value ?? "").Trim();
                if (string.IsNullOrWhiteSpace(element.Value))
                    element.Value = " ";

                // Skip over repeat search terms OR operators, UNLESS
                // those terms or operators are in the whitelist
                if (elements.LastOrDefault()?.Value == element.Value) {
                    if (element.Value.Length != 1 || !Repeatables.Contains(element.Value[0])) {
                        continue;
                    }
                }
                elements.Add(element);
            }
        }
    }
}
