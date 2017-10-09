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

namespace Sprockets.LargeGraph.Serialization.IGL {
    [Serializable]
    public abstract class IglTokenBase {
        protected IglTokenBase(long index) {
            Index = index;
        }

        public long Index { get; set; }


        public abstract void Execute(IglTokenExecutionContext context);

        protected void AssertNotNull(object obj, string name) {
            if (null == obj)
                throw new ArgumentException(name, name);
        }

        public class IglTokenExecutionContext {
            public IglTokenExecutionContext(
                IglScriptBody scriptBody) {
                Script = scriptBody;
            }

            public IglScriptBody Script { get; }

            public Dictionary<long, Type> TypeMap { get; } = new Dictionary<long, Type>();
            public Dictionary<long, object> Storage { get; } = new Dictionary<long, object>();
        }
    }
}