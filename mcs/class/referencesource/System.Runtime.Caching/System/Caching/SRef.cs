// <copyright file="SRef.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace System.Runtime.Caching {
    internal class SRef {
#if !MONO
        private static Type s_type = Type.GetType("System.SizedReference", true, false);
        private Object _sizedRef;
#endif

        internal SRef(Object target) {
#if !MONO
            _sizedRef = Activator.CreateInstance(s_type,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    new object[] { target },
                    null);
#endif
        }

        internal long ApproximateSize {
            [SecuritySafeCritical]
            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            get {
#if MONO
                // TODO: .net uses System.SizedReference which contains approximate size after Gen 2 collection
                return 16;
#else
                object o = s_type.InvokeMember("ApproximateSize",
                                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                                               null, // binder
                                               _sizedRef, // target
                                               null, // args
                                               CultureInfo.InvariantCulture);
                return (long)o;
#endif
            }
        }

        // 
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Grandfathered suppression from original caching code checkin")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void Dispose() {
#if !MONO
            s_type.InvokeMember("Dispose",
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null, // binder
                                _sizedRef, // target
                                null, // args
                                CultureInfo.InvariantCulture);
#endif
        }
    }

    internal class SRefMultiple {
        private SRef[] _srefs;
        private long[] _sizes;  // Getting SRef size in the debugger is extremely tedious so we keep the last read value here

        internal SRefMultiple(object[] targets) {
            _srefs = new SRef[targets.Length];
            _sizes = new long[targets.Length];
            for (int i = 0; i < targets.Length; i++) {
                _srefs[i] = new SRef(targets[i]);
            }
        }

        internal long ApproximateSize {
            get {
                long size = 0;
                for (int i = 0; i < _srefs.Length; i++) {
                    size += (_sizes[i] = _srefs[i].ApproximateSize);
                }
                return size;
            }
        }

        internal void Dispose() {
            foreach (SRef s in _srefs) {
                s.Dispose();
            }
        }
    }

    internal class GCHandleRef<T> : IDisposable
    where T : class, IDisposable {
        GCHandle _handle;
        T _t;

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public GCHandleRef(T t) {
            _handle = GCHandle.Alloc(t);
        }

        public T Target {
            [SecuritySafeCritical]
            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            get {
                try { 
                    T t = (T)_handle.Target;
                    if (t != null) {
                        return t;
                    }
                }
                catch (InvalidOperationException) {
                    // use the normal reference instead of throwing an exception when _handle is already freed
                }
                return _t;
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public void Dispose() {
            Target.Dispose();
            // Safe to call Dispose more than once but not thread-safe
            if (_handle.IsAllocated) {
                // We must free the GC handle to avoid leaks.
                // However after _handle is freed we no longer have access to its Target
                // which will cause AVs and various race conditions under stress.
                // We revert to using normal references after disposing the GC handle
                _t = (T)_handle.Target;
                _handle.Free();
            }
        }
    }
}
