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
using System.Collections.Concurrent;
using System.Threading;
using Sprockets.Core.OperationalPatterns;

namespace Sprockets.Threading {
    /// <summary>
    ///     Call used to feed work into the tthread work sink
    /// </summary>
    public class ThreadWorkSource : IThreadWorkSource {
        private readonly ConcurrentQueue<DispatchedWork> _workers =
            new ConcurrentQueue<DispatchedWork>();

        public AutoResetEvent Registered { get; } = new AutoResetEvent(true);

        public bool TryGetWork(out DispatchedWork work) {
            return _workers.TryDequeue(out work);
        }

        /// <summary>
        ///     Post work for to be executed on another thread without regard to when it returns
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns>An operation result that can be waited on</returns>
        public TryOperationResult<object> PostWork(CancellationToken token, DispatchedWork callback) {
            var result = new TryOperationResult<object>();
            _workers.Enqueue((source, token2) => {
                try {
                    result.SetSuccess(ExecuteCallback(token, callback, source, token2).Result);
                }
                catch (Exception ex) {
                    result.SetFailure(ex);
                }
                return result;
            });
            return result;
        }

        /// <summary>
        ///     Blocks there current thread until results are available
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public TryOperationResult<object> SendWork(
            CancellationToken token,
            DispatchedWork callback) {
            var result = PostWork(token, callback);
            result.WaitForChange(-1);
            return result;
        }

        public TryOperationResult<object> PostWork(
            CancellationToken token,
            Func<object> callback) {
            return PostWork(token,
                (s, f) => TryOperationResult<object>.Run(callback));
        }

        public TryOperationResult<object> SendWork(
            CancellationToken token,
            Func<object> callback) {
            return SendWork(token,
                (s, f) => TryOperationResult<object>.Run(callback));
        }

        private static TryOperationResult<object> ExecuteCallback(CancellationToken token,
            DispatchedWork callback,
            IThreadWorkSource s,
            CancellationToken token2) {
            return callback(s, CancellationTokenSource.CreateLinkedTokenSource(token2, token).Token);
        }
    }
}