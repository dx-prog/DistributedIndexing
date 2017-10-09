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
using System.Runtime.Serialization;

namespace Sprockets.LargeGraph.Serialization.IGL {
    /// <summary>
    ///     Used to declare that an object is to exist; on execution may allocate
    ///     an unitialized object
    /// </summary>
    [Serializable]
    public class IglDeclareObject : IglTokenBase {
        /// <summary>
        /// </summary>
        /// <param name="index">Token index within the IGL script</param>
        /// <param name="type">The IGL-TYPE</param>
        /// <param name="arrayDetails">Lengths and ranks</param>
        public IglDeclareObject(long index, IglRegisterType type, params Tuple<int, int>[]arrayDetails) : base(index) {
            if (arrayDetails == null || arrayDetails?.Length == 0 ||
                arrayDetails.Length == 1 && arrayDetails[0] == null) {
                arrayDetails = new Tuple<int, int>[0];
                ;
            }
            ArrayDetails = arrayDetails;
            TypeInformation = type;
        }

        public IglRegisterType TypeInformation { get; set; }

        /// <summary>
        ///     Lengths, and ranks
        /// </summary>
        public Tuple<int, int>[] ArrayDetails { get; set; }

        /// <summary>
        ///     The type id as it relates to the overall script
        /// </summary>
        public long TypeId => TypeInformation.Index;

        public override void Execute(IglTokenExecutionContext context) {
            var type = context.TypeMap[TypeId];
            if (ArrayDetails.Length == 0)
                context.Storage[Index] = FormatterServices.GetUninitializedObject(type);
            else if (ArrayDetails.Length > 1)
                throw new NotSupportedException();
            else
                context.Storage[Index] = Array.CreateInstance(type.GetElementType(), ArrayDetails[0].Item1);
        }
    }
}