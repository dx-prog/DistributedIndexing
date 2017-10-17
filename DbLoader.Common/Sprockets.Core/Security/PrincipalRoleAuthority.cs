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
using System.Security.Principal;
using System.Threading;

namespace Sprockets.Core.Security {
    public class PrincipalRoleAuthority : IRoleAuthority {
        private readonly Func<IPrincipal> _source;

        /// <summary>
        /// </summary>
        /// <param name="source">Null will result in using Thread.CurrentPrincipal</param>
        public PrincipalRoleAuthority(IPrincipal source = null) {
            _source = () => source ?? Thread.CurrentPrincipal;
        }

        public bool IsInRole(string roleName) {
            return _source().IsInRole(roleName);
        }
    }
}