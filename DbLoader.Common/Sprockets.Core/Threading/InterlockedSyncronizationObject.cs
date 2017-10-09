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

namespace Sprockets.Core.Threading {
    /// <summary>
    ///     A managed critical section
    /// </summary>
    [Serializable]
    public class InterlockedSyncronizationObject {
        private static int _globalIdCounter;
        [NonSerialized] private long _currentLock = -1;

        [NonSerialized] private long _lockCount;

        public InterlockedSyncronizationObject() {
            Id = Interlocked.Increment(ref _globalIdCounter);
        }

        public int Id { get; }


        /// <summary>
        ///     Acquire a new lock ID
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected internal bool AcquireLock(out long token) {
            var ret = false;
            var id = Thread.CurrentThread.ManagedThreadId;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                token = -1;
                if (-1 == Interlocked.CompareExchange(ref _currentLock, id, -1)) {
                    token = id;
                    Interlocked.Increment(ref _lockCount);
                    ret = true;
                }
                else if (id == Interlocked.CompareExchange(ref _currentLock, id, id)) {
                    // seperate branch so can verify via code coverage
                    token = id;
                    Interlocked.Increment(ref _lockCount);
                    ret = true;
                }
                if (ret)
                    Thread.BeginCriticalRegion();
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected internal bool ReleaseLock(ref long token, bool assertIfFail = true) {
            var ret = false;
            var id = Thread.CurrentThread.ManagedThreadId;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                // check if we are same thread
                if (id == Interlocked.CompareExchange(ref _currentLock, id, id)) {
                    if (token != id)
                        throw new InvalidOperationException("E0: cross thread release");

                    if (Interlocked.Decrement(ref _lockCount) == 0) {
                        Thread.EndCriticalRegion();
                        // if the logic herein is correct, this should never happen
                        if (Interlocked.CompareExchange(ref _currentLock, -1, id) != id)
                            throw new InvalidOperationException();

                        ret = true;
                    }
                }

                if (ret == false && assertIfFail)
                    throw new InvalidOperationException($"E1: cross thread release {id} from {token}");
            }

            return ret;
        }
    }
}