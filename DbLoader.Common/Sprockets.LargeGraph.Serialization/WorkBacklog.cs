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

namespace Sprockets.LargeGraph.Serialization {
    [Serializable]
    public class WorkBacklog<TContext, TData> {
        private readonly Stack<TData> _backlog = new Stack<TData>();

        public WorkBacklog(TContext context) {
            Context = context;
        }

        public TContext Context { get; }
        public bool HasWork => _backlog.Count > 0;

        public void AddWorkFor(TData obj) {
            _backlog.Push(obj);
        }

        public int Execute(Action<TContext, TData> work) {
            if (_backlog.Count == 0)
                return 0;

            var pop = _backlog.Pop();
            work(Context, pop);
            return _backlog.Count;
        }

        public void Clear() {
            _backlog.Clear();
        }

        public TData[] ToArray() {
            return _backlog.ToArray();
        }
    }
}