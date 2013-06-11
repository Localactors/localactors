using System.Security.Principal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Security.Cryptography;
using Localactors.entities;



public class CustomPrincipal : IPrincipal {
        public bool IsInRole(string role) {

            string username = Identity.Name;
            string userrole = null;
            if(username == null || !Identity.IsAuthenticated) {
                return false;
            }

            //cached
            user user = HttpContext.Current.Cache.Get(username) as user;
            if(user!=null) {
                userrole = user.Role.ToLower();
                if (!string.IsNullOrEmpty(userrole) && role.Contains(userrole))
                {
                    return true;
                }
                return false;
            }

            //not cached
            localactors ent = new localactors();
            user = ent.users.FirstOrDefault(x => x.UserName == username && x.Enabled);
            if (user != null) {
                HttpContext.Current.Cache.Insert(
                    username, 
                    user, 
                    null, 
                    DateTime.Now.AddMinutes(10), 
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);

                role = role.ToLower();
                userrole = user.Role.ToLower();

                if (!string.IsNullOrEmpty(userrole) && role.Contains(userrole)) {
                    return true;
                }
            }

            return false;
        }

        public IIdentity Identity { get; private set; }

        public CustomPrincipal(CustomIdentity identity)
        {
            Identity = identity;
        }
    }
