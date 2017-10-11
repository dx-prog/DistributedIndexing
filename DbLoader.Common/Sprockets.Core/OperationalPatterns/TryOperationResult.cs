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
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace Sprockets.Core.OperationalPatterns {
    /// <summary>
    ///     Used for functions that operate as a CER, or
    ///     generally need to always return even under
    ///     exceptional scenarios.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TryOperationResult<T> {
        private readonly ManualResetEvent _hasSet = new ManualResetEvent(false);
        private T _result;
        public Exception Failure { get; private set; }
        public bool IsSet => _hasSet.WaitOne(0);

        public T Result {
            get {
                if (IsSet == false)
                    return default(T);

                if (Failure != null)
                    throw new InvalidOperationException();

                return _result;
            }
        }

        public bool IsSuccess { get; private set; }

        /// <summary>
        ///     Waits for this result to be udpated
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public bool WaitForChange(int milliseconds = 0) {
            return _hasSet.WaitOne(milliseconds);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public bool SetSuccess(T success) {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                lock (_hasSet) {
                    if (!IsSet) {
                        IsSuccess = true;
                        _result = success;
                        _hasSet.Set();
                    }
                }
            }
            return IsSet;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public bool SetFailure(Exception failure) {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                lock (_hasSet) {
                    if (!IsSet) {
                        Failure = failure;
                        _hasSet.Set();
                    }
                }
            }
            return IsSet;
        }

        public void AssertSuccess() {
            if (IsSet == false)
                throw new InvalidOperationException();

            if (Failure != null)
                throw new AggregateException(Failure);

            if (!IsSuccess)
                throw new InvalidOperationException();
        }

        public static TryOperationResult<T> Run(Func<T> work) {
            var result = new TryOperationResult<T>();
            try {
                result.SetSuccess(work());
            }
            catch (Exception ex) {
                result.SetFailure(ex);
            }
            return result;
        }

        public static TryOperationResult<T> SuccessFrom(T i) {
            var result = new TryOperationResult<T>();
            result.SetSuccess(i);
            return result;
        }
    }
}