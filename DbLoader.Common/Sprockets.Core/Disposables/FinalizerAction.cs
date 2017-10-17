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
using System.Runtime.ConstrainedExecution;

namespace Sprockets.Core.Disposables {
    public class FinalizerAction : CriticalFinalizerObject, IDisposableAction {
        private readonly DisposableAction _action;

        public FinalizerAction(Action action) {
            _action = new DisposableAction(action);
        }

        public FinalizerAction(DisposableAction action) {
            _action = action;
        }

        public void Dispose() {
            _action?.Dispose();
            GC.SuppressFinalize(this);
        }

        public int Deactivate() {
            return (_action?.Deactivate()).GetValueOrDefault(-1);
        }

        ~FinalizerAction() {
            Dispose();
        }
    }
}