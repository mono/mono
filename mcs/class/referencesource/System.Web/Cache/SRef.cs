using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Web;

namespace System.Web.Caching {

#if MONO 
    // At least on Mono, the SizedReference createinstance bombs.  In order to get
    // this to roll an approximate size is good enough (Marshal.SizeOf).
    // We will need to revisit.
    internal class SRef
    { 
        private long _lastReportedSize;
        private WeakReference _ref;

        internal SRef(object target)
        {
            this._ref = new WeakReference(target, false);
        }

        internal long ApproximateSize
        {
            [PermissionSet(SecurityAction.Assert, Unrestricted = true)] get
            {
                this._lastReportedSize = (this._ref.IsAlive) ? (long)Marshal.SizeOf(this._ref.Target.GetType()) : 0;
                return this._lastReportedSize;
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void Dispose()
        {
        
        }
    }

#else 
    internal class SRef {
        private static Type s_type = Type.GetType("System.SizedReference", true, false);
        private Object _sizedRef;
        private long _lastReportedSize; // This helps tremendously when looking at large dumps
        
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
                return _lastReportedSize = (long) o;
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
#endif

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
