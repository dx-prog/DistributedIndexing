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
using System.Linq;

namespace Sprockets.DocumentIndexer.Lucene.Types {
    public class ScopeSanitizerToken : OperandSanitizerToken {
        private bool _showParen;

        public ScopeSanitizerToken(bool showParen = true) : base("") {
            _showParen = showParen;
        }

        public LinkedList<CodeSanitizerToken> Tokens { get; } = new LinkedList<CodeSanitizerToken>();

        public override string Value =>
            (_showParen ? "(" : "") + string.Concat(Tokens).Trim() + (_showParen ? ")" : "");


        public override void Sanitize() {
            if (Tokens.Count == 0) {
                _showParen = false;
                MarkInvalid();
                return;
            }
            // TODO: fix for performance; need a way to count how much internal 
            // presentable content contained to avoid string allocations
            var content = string.Concat(Tokens);
            if (string.IsNullOrWhiteSpace(content)) {
                _showParen = false;
                MarkInvalid();
                return;
            }

            var input = Tokens;

            // TODO: USE INFEX TO POSTFIX INSTEAD OF PHASE PASS?
            ProcessUnary(input, out var tmp, "NOT");
            ProcessUnary(tmp, out tmp, "!");
            ProcessUnary(tmp, out tmp, "-");
            ProcessUnary(tmp, out tmp, "+");
            ProcessBinary(tmp, out tmp, "AND");
            ProcessBinary(tmp, out tmp, "OR");
            foreach (var n in tmp)
                n.Sanitize();

            Tokens.Clear();
            foreach (var token in tmp.AsEnumerable())
                Tokens.AddLast(token);
        }

        private static void ProcessUnary(LinkedList<CodeSanitizerToken> input,
            out LinkedList<CodeSanitizerToken> output,
            string key) {
            output = new LinkedList<CodeSanitizerToken>();
            for (var n = input.First; n != null; n = n.Next) {
                if (n.Value is DelimiterSanitizerToken) {
                    if (null != GobbleDelimiterSanitizerToken(n))
                        output.Last?.Value?.IncreasePadRight();
                    continue;
                }

                if (n.Value is UnaryOperatorSanitizerToken unary && unary.OriginalValue == key) {
                    var next = GobbleDelimiterSanitizerToken(n);
                    if (next == null)
                        continue;

                    unary.Argument = next.Value;


                    output.AddLast(unary);
                    n = next;
                }
                else {
                    output.AddLast(n.Value);
                }
            }
        }

        private static LinkedListNode<CodeSanitizerToken> GobbleDelimiterSanitizerToken(
            LinkedListNode<CodeSanitizerToken> linkedListNode) {
            linkedListNode = linkedListNode.Next;
            while (linkedListNode != null)
                if (linkedListNode.Value is DelimiterSanitizerToken)
                    linkedListNode = linkedListNode.Next;
                else
                    break;

            return linkedListNode;
        }

        private static void ProcessBinary(LinkedList<CodeSanitizerToken> input,
            out LinkedList<CodeSanitizerToken> output,
            string key) {
            output = new LinkedList<CodeSanitizerToken>();

            for (var n = input.First; n != null; n = n.Next)
                switch (n.Value) {
                    case DelimiterSanitizerToken _:
                        if (null != GobbleDelimiterSanitizerToken(n))
                            output.Last?.Value?.IncreasePadRight();
                        continue;
                    case BinaryOperatorSanitizerToken binary when binary.OriginalValue == key:
                        var next = GobbleDelimiterSanitizerToken(n);
                        var lastNode = output.Last;
                        if (next == null)
                            continue;
                        if (lastNode == null)
                            continue;

                        binary.Left = lastNode.Value;
                        binary.Right = next.Value;

                        output.RemoveLast();
                        output.AddLast(binary);
                        n = next;
                        break;
                    default:
                        output.AddLast(n.Value);
                        break;
                }
        }
    }
}