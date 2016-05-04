using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Caching {
    internal class SRef {
        private static Type s_type = Type.GetType("System.SizedReference", true, false);
        private Object _sizedRef;
        
        internal SRef(Object target) {
            _sizedRef = HttpRuntime.CreateNonPublicInstance(s_type, new object[] {target});
        }
        
        internal long ApproximateSize {
            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            get {
                object o = s_type.InvokeMember("ApproximateSize",
                                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, 
                                               null, // binder
                                               _sizedRef, // target
                                               null, // args
                                               CultureInfo.InvariantCulture);
                return (long) o;
            }
        }
        
        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal void Dispose() {
            s_type.InvokeMember("Dispose",
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, 
                                null, // binder
                                _sizedRef, // target
                                null, // args
                                CultureInfo.InvariantCulture);
        }
    }

    internal class SRefMultiple {
        private List<SRef> _srefs = new List<SRef>();

        internal void AddSRefTarget(Object o) {
            _srefs.Add(new SRef(o));
        }

        internal long ApproximateSize {
            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            get {
                return _srefs.Sum(s => s.ApproximateSize);
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void Dispose() {
            foreach (SRef s in _srefs) {
                s.Dispose();
            }
        }
    }
}
