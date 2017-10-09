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
using System.Reflection;

namespace Sprockets.LargeGraph.Serialization.IGL {
    [Serializable]
    public class IglSetField : IglTokenBase {
        public IglSetField(long index, long objectId, string fieldName, long valueId) : base(index) {
            ValueId = valueId;
            FieldName = fieldName;
            ObjectId = objectId;
        }

        public long ObjectId { get; protected set; }
        public string FieldName { get; protected set; }
        public long ValueId { get; protected set; }

        public override void Execute(IglTokenExecutionContext context) {
            var instance = context.Storage[ObjectId];
            var value = context.Storage[ValueId];
            var intanceType = instance.GetType();

            if (intanceType.Name.Contains("Inverted")) {
            }
            var fi = intanceType.GetField(FieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            fi.SetValue(instance, value);
        }
    }
}