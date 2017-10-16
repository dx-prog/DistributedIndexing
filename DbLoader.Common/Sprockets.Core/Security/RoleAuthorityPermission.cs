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
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;

namespace Sprockets.Core.Security {
    public class RoleAuthorityPermission : IPermission {
        private const string Roles = nameof(Roles);
        public static readonly RoleAuthorityPermission Empty = new RoleAuthorityPermission();
        private readonly HashSet<string> _roles = new HashSet<string>();

        public RoleAuthorityPermission() {
        }

        public static IRoleAuthority RoleAuthority { get; set; } = new PrincipalRoleAuthority();
        public RoleAuthorityPermission(bool allManditory, params string[] roles) {
            foreach (var role in roles)
                _roles.Add(role);

            AllManditory = allManditory;
        }

        public RoleAuthorityPermission(IEnumerable<string> roles, bool allManditory) {
            foreach (var role in roles)
                _roles.Add(role);

            AllManditory = allManditory;
        }

        public bool AllManditory { get; private set; }

        /// <summary>
        ///     Not Implement right now
        /// </summary>
        /// <param name="e"></param>
        public void FromXml(SecurityElement e) {
            var rolesContainer = e.SearchForChildByTag(Roles);

            AllManditory = e.Attribute(nameof(AllManditory)) == "1";
            if (rolesContainer == null)
                return;

            foreach (var entry in rolesContainer.Attributes.Keys.OfType<string>()) {
                var attributeValue = e.Attribute(entry);
                if (attributeValue == "1" || attributeValue == "true")
                    _roles.Add(entry);
            }
        }

        public SecurityElement ToXml() {
            var ret = new SecurityElement(nameof(RoleAuthorityPermission));
            ret.AddAttribute(nameof(AllManditory), AllManditory ? "1" : "0");
            var roles = new SecurityElement(Roles);
            foreach (var role in _roles)
                roles.AddAttribute(role, "1");

            ret.AddChild(roles);
            return ret;
        }

        public IPermission Copy() {
            var ret = new RoleAuthorityPermission(_roles, AllManditory);

            return ret;
        }


        public void Demand() {
            if (_roles.Count == 0)
                return;

            var principal = RoleAuthority ?? new PrincipalRoleAuthority(Thread.CurrentPrincipal);
            if (AllManditory) {
                if (!_roles.All(role => principal.IsInRole(role)))
                    throw new SecurityException();
            }
            else if (!_roles.Any(role => principal.IsInRole(role))) {
                throw new SecurityException();
            }
        }

        public IPermission Intersect(IPermission target) {
            if (target is RoleAuthorityPermission cap)
                return new RoleAuthorityPermission(
                    cap._roles.Intersect(_roles),
                    AllManditory
                );

            return Empty;
        }

        public bool IsSubsetOf(IPermission target) {
            if (target is RoleAuthorityPermission cap)
                return cap._roles.Intersect(_roles).Any();

            return false;
        }

        public IPermission Union(IPermission target) {
            if (target is RoleAuthorityPermission cap)
                return new RoleAuthorityPermission(
                    cap._roles.Union(_roles),
                    AllManditory
                );

            return Empty;
        }

        public static void DemandForUser(params object[] roles) {
            new RoleAuthorityPermission(roles.Select(Convert.ToString), false).Demand();
        }
    }
}