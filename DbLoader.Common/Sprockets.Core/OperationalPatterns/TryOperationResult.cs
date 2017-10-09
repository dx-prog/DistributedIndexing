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
        private int _isSet;
        private T _result;

        public Exception Failure { get; private set; }
        public bool IsSet => _isSet == 1;

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

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public bool SetSuccess(T success, out bool isNotSet) {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                AssertNotSet(out isNotSet);
                if (isNotSet) {
                    IsSuccess = true;
                    _result = success;
                }
            }
            return isNotSet;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public bool SetFailure(Exception failure, out bool isNotSet) {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                AssertNotSet(out isNotSet);
                if (isNotSet)
                    Failure = failure;
            }
            return isNotSet;
        }

        public void AssertSuccess() {
            if (IsSet == false)
                throw new InvalidOperationException();

            if (Failure != null)
                throw new AggregateException(Failure);

            if (!IsSuccess)
                throw new InvalidOperationException();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void AssertNotSet(out bool isNotSet) {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                isNotSet = Interlocked.Exchange(ref _isSet, 1) == 0;
            }
        }
    }
}