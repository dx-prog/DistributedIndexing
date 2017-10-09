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

namespace Sprockets.Core.Disposables {
    /// <summary>
    ///     Untested
    /// </summary>
    public sealed class DisposableCerMayFailAction : DisposableAction {
        public DisposableCerMayFailAction(Action action) : base(action) {
            RuntimeHelpers.PrepareDelegate(action);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public override void Dispose() {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                base.Dispose();
            }
        }
    }
}