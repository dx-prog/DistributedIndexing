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

using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.LargeGraph.Serialization;

namespace Sprockets.Test.DegrapherTesting {
    [TestClass]
    public class TreeOrderTests {
        [TestMethod]
        public void CanDegraphXElementBySteps() {
            var xmlDegrapher = new TreeOrderDegrapher {CustomerEnumerator = SimpleDegrapher.XElementDegrapher};

            var a = XElement.Parse(@"


            <root>
                <a>            
                    <aa>
                    </aa>
                    <ab>
                    </ab>
                    <ac>
                    </ac>
                </a>
                <b>            
                    <ba>
                    </ba>
                    <bb>
                    </bb>
                    <bc>
                    </bc>
                </b>
                <c>
                    <ca>
                    </ca>
                    <cb>
                    </cb>
                    <cc>
                    </cc>
                </c>
            </root>
            ");
            xmlDegrapher.LoadObject(a);


            var stringContent =
                string.Join(".", xmlDegrapher.KnowledgeBase.SelectMany(m => m).OfType<XElement>().Select(x => x.Name));

            Assert.AreEqual("root.a.aa.ab.ac.b.ba.bb.bc.c.ca.cb.cc", stringContent);
        }
    }
}