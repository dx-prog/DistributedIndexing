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
using System.Reflection;
using System.Runtime.Serialization;
using Sprockets.Core.Reflection;

namespace Sprockets.LargeGraph.Serialization.IGL {
    /// <summary>
    ///     Declares that special initialization of a type is required through ISerializable semantics
    /// </summary>
    [Serializable]
    public class IglSpecialSerialization : IglTokenBase {
        public IglSpecialSerialization(long index,
            long objectId,
            IglRegisterType objectType,
            Dictionary<string, long> data) : base(index) {
            ObjectId = objectId;
            Blob = data;
            ObjectType = objectType;
        }

        public long ObjectId { get; protected set; }

        /// <summary>
        ///     Dictonary of Serializable keys and values (As IGL Token IDs)
        /// </summary>
        public Dictionary<string, long> Blob { get; protected set; }

        public IglRegisterType ObjectType { get; protected set; }

        public override void Execute(IglTokenExecutionContext context) {
            var objType = context.TypeMap[ObjectType.Index];
            var streamingContext = new StreamingContext(StreamingContextStates.Other);
            var info = new SerializationInfo(objType, new FormatterConverter());

            foreach (var kv in Blob) {
                var nameInfo = kv.Key;
                var valueInfo = context.Storage[kv.Value];
                info.AddValue(nameInfo, valueInfo);
            }

            var ctor = objType.GetBestConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                typeof(SerializationInfo),
                typeof(StreamingContext));
            if (null == ctor)
                throw new EntryPointNotFoundException();

            var rootObject = context.Storage[ObjectId];
            ctor.Invoke(rootObject,
                new object[] {
                    info,
                    streamingContext
                });

            if (rootObject is IDeserializationCallback callback)
                callback.OnDeserialization(info);
        }
    }
}