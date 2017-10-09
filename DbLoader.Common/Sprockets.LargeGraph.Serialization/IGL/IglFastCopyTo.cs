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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sprockets.LargeGraph.Serialization.IGL {
    /// <summary>
    ///     Declare that a copy of an array of primatives needs to be performed.
    /// </summary>
    [Serializable]
    public class IglFastCopyTo : IglTokenBase {
        public IglFastCopyTo(long index, long objectId, Array data) : base(index) {
            AssertNotNull(data, nameof(data));
            ObjectId = objectId;
            Blob = data;
        }

        public long ObjectId { get; set; }

        /// <summary>
        ///     Primatives only
        /// </summary>
        public Array Blob { get; set; }

        public object BlobToString() {
            using (var ms = new MemoryStream()) {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, Blob);
                ms.Position = 0;
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public override void Execute(IglTokenExecutionContext context) {
            Blob.CopyTo((Array) context.Storage[ObjectId], 0);
        }
    }
}