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

using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprockets.Core.Security;

namespace Sprockets.Test.Security {
    [TestClass]
    public class RoleAuthorizationTests : IRoleAuthority {
        public readonly HashSet<string> Roles = new HashSet<string>();

        public bool IsInRole(string roleName) {
            return Roles.Contains(roleName);
        }

        [TestInitialize]
        public void Init() {
            Roles.Clear();
            RoleAuthorityPermission.RoleAuthority = this;
        }

        [TestMethod]
        [ExpectedException(typeof(SecurityException))]
        public void BlocksCallsTest() {
            AdminFunction(true);
        }

        [TestMethod]
        public void AllowsCallTest() {
            Roles.Add("admin");
            AdminFunction(false);
        }

        [RoleAuthorityPermission(SecurityAction.Demand, Roles = "admin")]
        private void AdminFunction(bool fail) {
            if (fail)
                Assert.Fail();
        }
    }
}