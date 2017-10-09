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

namespace Sprockets.Core.Disposables {
    /// <summary>
    ///     Helper to avoid having to create IDisposable
    ///     classes used to perform limited operations.
    /// </summary>
    public class DisposableAction : IDisposable {
        private readonly Action _action;
        private int _executed;

        public DisposableAction(Action action) {
            _action = action ?? (() => { });
        }

        public virtual void Dispose() {
            if (Deactivate() > 1)
                return;

            _action();
        }


        /// <summary>
        ///     Disables the action without calling it
        /// </summary>
        /// <returns></returns>
        public int Deactivate() {
            return Interlocked.Increment(ref _executed);
        }
    }
}