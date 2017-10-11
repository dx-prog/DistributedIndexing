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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sprockets.Core.OperationalPatterns;

namespace Sprockets.Threading {
    /// <summary>
    ///     A thread sink pulls work in from any number of sources, dispatches them on any
    /// </summary>
    public class ThreadWorkSink : IThreadWorkSource, IDisposable {
        // ReSharper disable once InconsistentNaming
        private static readonly ThreadLocal<ThreadWorkSink> _currentWorker =
            new ThreadLocal<ThreadWorkSink>(() => new ThreadWorkSink());

        private readonly AutoResetEvent _exitControl;

        private readonly ConcurrentDictionary<IThreadWorkSource, object> _sources =
            new ConcurrentDictionary<IThreadWorkSource, object>();

        private ThreadWorkSink() {
            _exitControl = new AutoResetEvent(false);
            ;
        }

        public static ThreadWorkSink CurrentWorker => _currentWorker.Value;

        /// <summary>
        ///     Get or set the value indicate the number of additional threads to help process requests on the pump.
        ///     If set to 0, the wink will not drain. If the sink is set to 1, it will drain on the thread on which
        ///     the the pump was created. If set to more then 1, the thread that finally executes is undetermined
        /// </summary>
        public int MaxConcurrent { get; set; } = 1;

        public bool Suspended { get; private set; }

        public IEnumerable<IThreadWorkSource> Sources => _sources.Keys;

        public void Dispose() {
            Shutdown();
            CurrentWorker._sources.TryRemove(this, out _);
        }

        public bool TryGetWork(out DispatchedWork work) {
            foreach (var source in Sources)
                if (source.TryGetWork(out work))
                    return true;

            work = null;
            return false;
        }

        public TryOperationResult<object> PostWork(CancellationToken token, DispatchedWork callback) {
            var source = GetViableSource();
            return source.PostWork(token, callback);
        }

        public TryOperationResult<object> SendWork(CancellationToken token, DispatchedWork callback) {
            var source = GetViableSource();
            return source.SendWork(token, callback);
        }

        public void Shutdown() {
            _exitControl.Set();
        }

        public bool WaitForExit(int millisecondsWaitTime = 100) {
            return _exitControl.WaitOne(millisecondsWaitTime);
        }

        public void InstallPump(CancellationToken exitToken) {
            do {
                var numberOfWorkers = MaxConcurrent;
                if (numberOfWorkers == 0) {
                    Suspended = true;

                    continue;
                }

                Suspended = false;


                if (numberOfWorkers < 0)
                    numberOfWorkers = 128;

                if (numberOfWorkers == 1)
                    ExecuteSynchroniously(exitToken);
                else if (numberOfWorkers > 1)
                    ExecuteInDistributedMode(exitToken, numberOfWorkers);
            } while (false == WaitForExit(10));
        }

        public void RegisterSource(ThreadWorkSource source) {
            _sources[source] = Thread.CurrentThread;
            source.Registered.Set();
        }

        private void ExecuteInDistributedMode(CancellationToken exitToken, int numberOfWorkers) {
            Parallel.For(0,
                numberOfWorkers,
                new ParallelOptions {
                    CancellationToken = exitToken,
                    MaxDegreeOfParallelism = numberOfWorkers
                },
                i => { ExecuteSynchroniously(exitToken); });
        }

        private void ExecuteSynchroniously(CancellationToken exitToken) {
            foreach (var source in Sources) {
                if (!source.TryGetWork(out var work))
                    continue;

                try {
                    work(source, exitToken);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception) {
                }
                if (exitToken.IsCancellationRequested)
                    return;
            }
        }

        private IThreadWorkSource GetViableSource() {
            var source = Sources.OrderBy(ignored => new Random().Next()).FirstOrDefault() ?? new ThreadWorkSource();
            return source;
        }
    }
}