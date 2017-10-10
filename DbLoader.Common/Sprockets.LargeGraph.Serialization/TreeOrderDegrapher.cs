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
using Sprockets.Core.Collection;

namespace Sprockets.LargeGraph.Serialization {
    /// <summary>
    ///     This class is used to help degraph complex nest objects into an a structure
    ///     represents the loging left-to-right ordering of the graph/tree
    /// </summary>
    public class TreeOrderDegrapher : IObjectDegrapher {
        private readonly TwoWayLongMap<object> _dataMap = new TwoWayLongMap<object>();
        private long _ids;

        public Func<TreeOrderDegrapher, object, IEnumerator> CustomerEnumerator { get; set; }

        public List<object[]> KnowledgeBase { get; } = new List<object[]>();

        public void Reset(long? newIdStartPoint = null) {
            KnowledgeBase.Clear();
            _dataMap.Clear();

            if (newIdStartPoint.HasValue)
                _ids = newIdStartPoint.Value;
        }

        public bool LoadObject(object obj) {
            var workLog = new Stack<IEnumerator<object>>();
            var currentObj = obj;
            var loopId = 0;
            var depthId = 0;
            var occuranceId = 0;
            var mapping = new Dictionary<TreeOrderIndex, object>();
            var skip = new object();
            while (null != currentObj || workLog.Count > 0) {
                if (currentObj == skip)
                    goto _skipPoint;

                long? finalRefId = null;
                var isValeuType = currentObj is ValueType;
                if (currentObj != null && (_dataMap.TryGetOrAdd(ref _ids, Tuple.Create(currentObj), out var refId)
                                           || isValeuType)) {
                    var scope = Extract(currentObj);
                    workLog.Push(scope);
                    depthId++;
                    if (!isValeuType)
                        finalRefId = refId;
                }

                mapping[new TreeOrderIndex(loopId, depthId, occuranceId, finalRefId)] = currentObj;
                _skipPoint:
                if (workLog.Count == 0)
                    break;

                if (workLog.Peek().MoveNext()) {
                    var tmp = workLog.Peek().Current;
                    currentObj = tmp;
                    occuranceId++;
                }
                else {
                    workLog.Pop();
                    currentObj = skip;
                    depthId--;
                }
                loopId++;
            }

            KnowledgeBase.AddRange(mapping.Select(kv =>
                new[] {kv.Key, kv.Value}));

            return false;
        }


        protected virtual IEnumerator<object> Extract(object obj) {
            if (obj == null)
                yield break;


            var callback = CustomerEnumerator ?? SimpleDegrapher.GenericDegrapher;

            var enumerator = callback(this, obj);

            if (enumerator == null)
                yield break;

            while (enumerator.MoveNext())
                if (!_dataMap.TryGetId(Tuple.Create(enumerator.Current), out _))
                    yield return enumerator.Current;
        }

        public class TreeOrderIndex {
            public TreeOrderIndex(int loopId, int depthId, int occurrenceId, long? finalRefId) {
                LoopId = loopId;
                DepthId = depthId;
                OccurrenceId = occurrenceId;
                FinalRefId = finalRefId;
            }

            public int DepthId { get; }
            public int OccurrenceId { get; }
            public int LoopId { get; }
            public long? FinalRefId { get; }
        }
    }
}