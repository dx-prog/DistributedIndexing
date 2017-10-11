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
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Core.DocumentIndexing.Host {
    public class ExtractorList : IEnumerable<IExtractor> {
        private readonly IServiceProvider _provider;
        private readonly List<Type> _types;

        public ExtractorList(List<Type> types, IServiceProvider provider) {
            _types = new List<Type>(types);
            _provider = provider;
        }

        public IEnumerator<IExtractor> GetEnumerator() {
            foreach (var type in _types)
                yield return (IExtractor) _provider.GetService(type);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}