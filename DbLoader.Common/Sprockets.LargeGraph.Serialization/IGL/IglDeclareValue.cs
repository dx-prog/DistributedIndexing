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

namespace Sprockets.LargeGraph.Serialization.IGL {
    /// <summary>
    ///     Declares that a value exists; the value may be array of primatives
    /// </summary>
    [Serializable]
    public class IglDeclareValue : IglTokenBase {
        public IglDeclareValue(long index, IglRegisterType type, object value) : base(index) {
            TypeId = type?.Index;
            Value = value;
        }

        public object Value { get; protected set; }

        public long?TypeId { get; protected set; }

        public override void Execute(IglTokenExecutionContext context) {
            context.Storage[Index] = Value;
        }
    }
}