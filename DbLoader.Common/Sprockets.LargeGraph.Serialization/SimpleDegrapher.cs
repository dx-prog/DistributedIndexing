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
using System.Xml.Linq;
using Sprockets.Core.Collection;
using Sprockets.Core.Disposables;
using Sprockets.Core.Reflection;

namespace Sprockets.LargeGraph.Serialization {
    /// <summary>
    ///     This class is used to help degraph complex nest objects
    /// </summary>
    public class SimpleDegrapher : IObjectDegrapher {
        private readonly WorkBacklog<SimpleDegrapher, object> _backlog;
        private readonly TwoWayLongMap<object> _dataMap = new TwoWayLongMap<object>();
        private long _ids;

        public SimpleDegrapher() {
            _backlog = new WorkBacklog<SimpleDegrapher, object>(this);
        }

        public Func<IObjectDegrapher, object, IEnumerator> CustomerEnumerator { get; set; }

        public List<object[]> KnowledgeBase { get; } = new List<object[]>();

        public void Reset(long?newIdStartPoint = null) {
            KnowledgeBase.Clear();
            _dataMap.Clear();
            _backlog.Clear();
            if (newIdStartPoint.HasValue)
                _ids = newIdStartPoint.Value;
        }

        public bool LoadObject(object obj) {
            if (obj == null)
                return _backlog.HasWork;

            if (!_dataMap.TryGetOrAdd(ref _ids,Tuple.Create(obj), out var actualId))
                return _backlog.HasWork;


            var extractions = Extract(obj);
            foreach (var entry in extractions)
                _backlog.AddWorkFor(entry);

            return _backlog.HasWork;
        }

        public bool Pump() {
            var backLog = _backlog.ToArray();
            KnowledgeBase.Add(backLog);
            _backlog.Clear();
            foreach (var entry in backLog)
                LoadObject(entry);

            return _backlog.HasWork;
        }

        public bool PumpFor(TimeSpan fromSeconds) {
            using (var watch = new DisposableStopwatch()) {
                while (Pump())
                    if (watch.Elapsed > fromSeconds)
                        break;
            }

            return _backlog.HasWork;
        }


        public static IEnumerator GenericDegrapher(IObjectDegrapher caller, object arg) {
            var type = arg.GetType().GetElementTypeOfEnumerable();
            if (type == null) {
                yield return arg;

                yield break;
            }

            var items = (IEnumerable) arg;
            foreach (var child in items.Cast<object>())
                yield return child;
        }

        public static IEnumerator XElementDegrapher(IObjectDegrapher caller, object arg) {
            switch (arg) {
                case XAttribute att:
                    yield return att.Name;
                    yield return att.Value;

                    break;
                case XElement element:
                    if (element.HasElements == false)
                        yield return element.Value;

                    foreach (var child in element.Attributes())
                        yield return child;
                    foreach (var child in element.Elements())
                        yield return child;

                    break;
                case XName name:
                    yield return name.LocalName;
                    yield return name.NamespaceName;

                    break;
                case XDocument document:
                    yield return document.Root;

                    break;
                case string _:
                    yield return arg;

                    break;
            }
        }


        protected virtual IEnumerable<object> Extract(object obj) {
            if (obj == null)
                return new object[0];

            var callback = CustomerEnumerator ?? GenericDegrapher;

            var enumerator = callback(this, obj);

            if (enumerator == null)
                return new object[0];

            var returnObject = new List<object>();


            while (enumerator.MoveNext())
                if (!_dataMap.TryGetId(Tuple.Create(enumerator.Current), out _))
                    returnObject.Add(enumerator.Current);

            return returnObject;
        }
    }
}