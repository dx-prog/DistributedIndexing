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

namespace Sprockets.Core.Collection {
    /// <summary>
    ///     This class is used for tracking relationships betweeen two
    ///     sets of data that have a 1:1 identity
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    [Serializable]
    public class TwoWayMap<TObject, TIndex> : IEnumerable<Tuple<TIndex, TObject>> {
        private readonly Dictionary<TObject, TIndex> _backward = new Dictionary<TObject, TIndex>();
        private readonly Dictionary<TIndex, TObject> _forward = new Dictionary<TIndex, TObject>();

        public IEnumerator<Tuple<TIndex, TObject>> GetEnumerator() {
            return _forward.Select(kv => Tuple.Create(kv.Key, kv.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public TIndex Map(TIndex id, TObject obj) {
            _forward[id] = obj;
            _backward[obj] = id;
            return id;
        }

        public bool TryGetObject(TIndex id, out TObject obj) {
            return _forward.TryGetValue(id, out obj);
        }

        public bool TryGetId(TObject obj, out TIndex id) {
            return _backward.TryGetValue(obj, out id);
        }

        public TIndex GetId(TObject instance) {
            if (TryGetId(instance, out var id))
                return id;

            throw new KeyNotFoundException();
        }
    }
}