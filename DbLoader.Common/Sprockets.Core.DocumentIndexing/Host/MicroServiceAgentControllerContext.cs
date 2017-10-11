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
using System.Threading;
using Sprockets.Threading;

namespace Sprockets.Core.DocumentIndexing.Host {
    public class MicroServiceAgentControllerContext : ThreadWorkSource {
        private CancellationTokenSource _src;

        public MicroServiceAgentControllerContext(Guid id,
            Type agentType,
            MicroServiceAgentControllerContext parentContext = null,
            CancellationTokenSource src = null) {
            Id = id;
            _src = src ?? new CancellationTokenSource();
            Type = agentType;
            ParentContext = parentContext;
            Token = _src.Token;
        }

        public MicroServiceAgentControllerContext ParentContext { get; }

        /// <summary>
        ///     The CLR type that is used as the underlying service agent
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     The unique ID for this context across all systems in the sprocket farm
        /// </summary>
        public Guid Id { get; set; }

        public bool IsRunning => _src.IsCancellationRequested == false && ParentContext?.IsRunning != false;
        public CancellationToken Token { get; }

        /// <summary>
        ///     Causes the service to stop
        /// </summary>
        public void Stop() {
            _src.Cancel();
        }

        /// <summary>
        ///     Causes the service to stop
        /// </summary>
        public void RestartStart() {
            Stop();
            _src?.Dispose();
            _src = new CancellationTokenSource();
        }
    }
}