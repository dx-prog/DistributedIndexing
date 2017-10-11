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
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sprockets.Core.DocumentIndexing.Extractors;
using Sprockets.Core.DocumentIndexing.Types;

namespace Sprockets.Core.DocumentIndexing.Host {
    public class ExtractorHost {
        private readonly List<Type> _knownExtractorTypes = new List<Type>();

        public ExtractorHost() {
            Services.TryAddScoped(p => new ExtractorList(_knownExtractorTypes, p));
        }

        public IServiceCollection Services { get; } = new ServiceCollection();

        public ServiceProvider Provider { get; private set; }

        public object GetService(Type serviceType) {
            return Provider.GetService(serviceType);
        }

        public void RegisterScopedExtractor<T>(T instance) where T : IExtractor {
            Services.TryAddScoped(typeof(T), p => instance);
            _knownExtractorTypes.Add(typeof(T));
        }

        public void RegisterScopedExtractor<T>() where T : IExtractor {
            Services.TryAddScoped(typeof(T));
            _knownExtractorTypes.Add(typeof(T));
        }

        public void RegisterExtractors(Assembly assemblies) {
            foreach (var extractorType in assemblies.GetExportedTypes()
                .Where(t => typeof(IIndexServicePlugin).IsAssignableFrom(t))) {
                var usage = extractorType.GetCustomAttribute<ServiceUsageAttribute>() ?? new ServiceUsageAttribute();

                if (typeof(IExtractor).IsAssignableFrom(extractorType)) {
                    Services.AddScoped(extractorType);

                    _knownExtractorTypes.Add(extractorType);
                }
                else if (usage.Singleton) {
                    Services.TryAddSingleton(extractorType);
                }
                else {
                    Services.TryAddScoped(extractorType);
                }
            }
        }

        public void Initialize() {
            Provider = Services.BuildServiceProvider();
        }


        public IServiceScope BeginServiceScope(out AggregateExtractor extractor) {
            var scope = Provider.CreateScope();
            extractor = new AggregateExtractor(scope.ServiceProvider.GetService<ExtractorList>());
            return scope;
        }
    }
}