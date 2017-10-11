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
using System.Threading.Tasks;
using Sprockets.Threading;

namespace Sprockets.Core.DocumentIndexing.Host {
    /// <summary>
    ///     This class controls either an in-memory agent, or an out-of-process agent
    /// </summary>
    public class MicroServiceAgentController {
        public MicroServiceAgentController(MicroServiceAgentControllerContext context) {
            Context = context;
        }

        public MicroServiceAgentControllerContext Context { get; }

        public MicroServiceAgentControllerContext CreateWorker() {
            var childContext = new MicroServiceAgentControllerContext(
                Guid.NewGuid(),
                Context.Type,
                Context);

            Task.Run(() => {
                    var sink = ThreadWorkSink.CurrentWorker;
                    sink.RegisterSource(childContext);
                    sink.InstallPump(childContext.Token);
                    return childContext;
                },
                childContext.Token);

            return childContext;
        }
    }
}