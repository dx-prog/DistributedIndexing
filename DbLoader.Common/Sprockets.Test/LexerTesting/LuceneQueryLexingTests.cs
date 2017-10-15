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
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.DocumentIndexer.Lucene;
using Sprockets.Lexer;
using Version = Lucene.Net.Util.Version;

namespace Sprockets.Test.LexerTesting {
    [TestClass]
    public class LuceneQueryLexingTests {
        [TestMethod]
        public void IdealCase_SingleQuoteExtraction() {
            TestQuery("\"quote\"", "\"quote\"");
        }

        [TestMethod]
        public void IdealCase_TwoQuoteExtraction() {
            TestQuery("\"quote1\" \"quote2\"", "\"quote1\" \"quote2\"");
        }

        [TestMethod]
        public void IdealCase_SingleQuoteFuzzyRealNumber() {
            TestQuery("\"quote\"~.1", "\"quote\"~.1");
        }

        [TestMethod]
        public void IdealCase_SingleQuoteFuzzyIntegerNumber() {
            TestQuery("\"quote\"~1", "\"quote\"~1");
        }

        [TestMethod]
        public void IdealCase_TwoQuoteFuzzyRealNumber() {
            TestQuery("\"quote\"~.1 \"quote\"~.2", "\"quote\"~.1 \"quote\"~.2");
        }

        [TestMethod]
        public void IdealCase_TwoQuoteFuzzyIntegerNumber() {
            TestQuery("\"quote\"~1 \"quote\"~.2", "\"quote\"~1 \"quote\"~.2");
        }

        [TestMethod]
        public void IdealCase_LogicAndStylingMaintainedOnQueryRewrite_001()
        {
            TestQuery("(a OR c)", "(a OR c)");
        }

        [TestMethod]
        public void IdealCase_LogicAndStylingMaintainedOnQueryRewrite_002()
        {
            TestQuery("(a OR c) AND d)", "(a OR c) AND d");
        }

        [TestMethod]
        public void IdealCase_LogicAndStylingMaintainedOnQueryRewrite_004()
        {
            TestQuery("((a OR c) AND d) AND E OR (g AND h OR i)", "((a OR c) AND d) AND E OR (g AND h OR i)");
        }
        [TestMethod]
        public void IdealCase_SingleParentheticalGroup() {
            TestQuery("(a c)", "(a c)");
        }

        [TestMethod]
        public void IdealCase_SingleNestParentheticalGroup() {
            TestQuery("(a (b) z)", "(a (b) z)");
        }

        [TestMethod]
        public void IdealCase_DoubleNestParentheticalGroup() {
            TestQuery("(a (b (c) d) z)", "(a (b (c) d) z)");
        }

        [TestMethod]
        public void IdealCase_DoubleSingleNestParentheticalGroup() {
            TestQuery("(a (b c) (d e) z)", "(a (b c) (d e) z)");
        }

        [TestMethod]
        public void BadCase_ForceClosureOfQuote() {
            TestQuery("test \"quote hello test", "test \"quote hello test\"");
        }

        [TestMethod]
        public void BadCase_ForceClosureOfOneMissingParen() {
            TestQuery("(a (b) c", "(a (b) c)");
        }

        [TestMethod]
        public void BadCase_ForceClosureOfTwoMissingParen() {
            TestQuery("(a (b c", "(a (b c))");
        }

        [TestMethod]
        public void BadCase_CleanOrphanedParentheses() {
            TestQuery("a ) b ) c )", "a b c");
        }


        [TestMethod]
        public void BadCase_CleanInvalidLeftBinaryOperator() {
            TestQuery("a AND", "a");
        }

        [TestMethod]
        public void BadCase_CleanInvalidRightBinaryOperator() {
            TestQuery("a AND", "a");
        }


        [TestMethod]
        public void BadCase_CleanInvalidUnivaryOperator1() {
            TestQuery("a +", "a");
        }

        [TestMethod]
        public void BadCase_CleanInvalidUnivaryOperator2() {
            TestQuery("+", "*");
        }

        [TestMethod]
        public void BadCase_CleansUnexpectedBackslashAtEnd() {
            TestQuery("test \"quote\\", "test \"quote\"");
        }

        [TestMethod]
        public void BadCase_CleansUnexpectedBadEscape() {
            TestQuery("test \"quote\\3\"", "test \"quote\"");
        }

        [TestMethod]
        public void BadCase_CleansDuplicateAndOperator() {
            TestQuery("a AND AND b", "a AND b");
        }

        [TestMethod]
        public void BadCase_CleansDuplicateOrOperator() {
            TestQuery("a OR OR b", "a OR b");
        }

        [TestMethod]
        public void BadCase_CleansDuplicateNotOperator() {
            TestQuery("a NOT NOT b", "a !b");
        }

        [TestMethod]
        public void BadCase_CleansNonCompatibleNestedUnary()
        {
            TestQuery("!+5", "!(+5)");
        }
        [TestMethod]
        public void BadCase_FuzzGenerated_001()
        {
            TestQuery("OROR1 ANDAND1OR(  +1var+)varvar ANDAND) 1!var+varOR", "OROR1 ANDAND1OR(+1var)varvar ANDAND 1!var+varOR");
        }

        [TestMethod]
        public void BadCase_FuzzGenerated_002()
        {
            TestQuery("OR !)(! +11ORORAND) !OR(ORvarOR AND+OR()+OROR", "!(!(+11ORORAND)) (ORvarOR +OROR)");
        }

        [TestMethod]
        public void BadCase_FuzzGenerated_003()
        {
            TestQuery("+(1 !(varvarOR(+OR!AND1AND)1varAND))",
                "+(1 !(varvarOR(!AND1AND)1varAND))");
        }

        


        [TestMethod]
        public void FuzzTest() {
            string[] input = {
                "var",
                " ",
                "1",
                "!",
                "+",
                "AND",
                "OR",
                "(",
                ")"
            };
            for (var i = 0; i < 100; i++)
                TestQuery(CreateFuzz(30, input), null);
        }

        private string CreateFuzz(int length, params string[] input) {
            var rnd = new Random((int) DateTime.Now.Ticks);
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
                sb.Append(input[rnd.Next() % input.Length]);

            return sb.ToString();
        }

        private static LexerCursor TestQuery(string input, string expectOutput) {
            try {
     
                var actual = LuceneQuerySanitizer.Sanitize(input);
                Console.WriteLine("Unsantized Input: {0}", input);
                Console.WriteLine("Expected Output: {0}", expectOutput);
                Console.WriteLine("Actual Output: {0}", actual);
        

                try {
                    var p = new QueryParser(Version.LUCENE_30, "FIELD", new StandardAnalyzer(Version.LUCENE_29));
                    p.AllowLeadingWildcard = true;

                    p.Parse(actual);
                }
                catch (Exception x) {
                    Console.WriteLine(x.Message);
                    Assert.Fail("QUERY CORRECTION FAILED");
                }
                if (expectOutput != null)
                {

                    Assert.AreEqual(expectOutput, actual);
                }

                return null;
            }
            catch (Exception x) {

                Console.WriteLine(input);
                throw;
            }
        }
    }
}