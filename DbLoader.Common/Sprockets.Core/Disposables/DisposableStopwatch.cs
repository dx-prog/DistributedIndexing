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
using System.Diagnostics;

namespace Sprockets.Core.Disposables {
    /// <summary>
    ///     Class used to measure performance around an execution block
    /// </summary>
    public sealed class DisposableStopwatch : IDisposable {
        private readonly DisposableAction _callback;
        private readonly Stopwatch _sw;

        public DisposableStopwatch(Action<TimeSpan> onDispose = null) {
            _sw = new Stopwatch();
            onDispose = onDispose ?? (t => { });
            _callback = new DisposableAction(() => onDispose(_sw.Elapsed));
            _sw.Start();
        }

        public TimeSpan Elapsed => _sw.Elapsed;

        public void Dispose() {
            _callback?.Dispose();
        }

        public IDisposable PauseStopwatch() {
            _sw.Stop();
            return new DisposableAction(() => _sw.Stop());
        }
    }
}