using System;
using System.Security.Principal;

namespace MonoTests.Common {
    public class FooUser : IPrincipal {
        public IIdentity Identity {
            get;
        }

        public bool IsInRole(string role) {
            return false;
        }
    }
}