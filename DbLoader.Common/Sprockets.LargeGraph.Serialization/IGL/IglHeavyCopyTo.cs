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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sprockets.Core.Reflection;

namespace Sprockets.LargeGraph.Serialization.IGL {
    /// <summary>
    ///     Declare that a heavy copy of large objects is required; this applies
    ///     to arrays of non-primatives, or arrays of arrays
    /// </summary>
    [Serializable]
    public class IglHeavyCopyTo : IglTokenBase {
        public IglHeavyCopyTo(long index, long objectId, long[] data) : base(index) {
            ObjectId = objectId;
            Blob = data;
        }

        public long ObjectId { get; set; }
        public long[] Blob { get; set; }

        public override void Execute(IglTokenExecutionContext context) {
            var src = Blob.Select(id => context.Storage[id]).ToArray();
            var dst = context.Storage[ObjectId];

            if (dst is Array arrayDest) {
                src.CopyTo(arrayDest, 0);
            }
            else {
                var destinationElementType = dst.GetType().GetElementTypeOfEnumerable();
                var argType = typeof(IEnumerable<>).MakeGenericType(destinationElementType);
                var ctor = dst.GetType().GetBestConstructor(argType);
                if (ctor != null) {
                    var tmpArray = Array.CreateInstance(destinationElementType, src.Length);
                    src.CopyTo(tmpArray, 0);
                    ctor.Invoke(dst, new object[] {tmpArray});
                    return;
                }

                if (dst is IList list)
                    foreach (var element in src)
                        list.Add(element);
                else
                    throw new InvalidOperationException();
            }
        }
    }
}