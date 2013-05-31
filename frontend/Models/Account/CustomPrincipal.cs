using System.Security.Principal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using Localactors.entities;



public class CustomPrincipal : IPrincipal {
        public bool IsInRole(string role) {
            localactors ent = new localactors();
            var user = ent.users.FirstOrDefault(x => x.UserName == Identity.Name && x.Enabled );

            role = role.ToLower();
            string userrole = user.Role.ToLower();

            if (!string.IsNullOrEmpty(userrole) &&  role.Contains(userrole))
            {
                return true;
            }

            return false;
        }

        public IIdentity Identity { get; private set; }

        public CustomPrincipal(CustomIdentity identity)
        {
            Identity = identity;
        }
    }
