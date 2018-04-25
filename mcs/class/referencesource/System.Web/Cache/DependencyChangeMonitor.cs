// <copyright file="DependencyChangeMonitor.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
#if USE_MEMORY_CACHE
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;
using System.Web.Util;

namespace System.Web.Caching {
    internal sealed class DependencyChangeMonitor: ChangeMonitor, ICacheDependencyChanged {
        private CacheDependency _dependency;
        
        public override string UniqueId { 
            get {
                return _dependency.GetUniqueID();
            }
        }

        internal DependencyChangeMonitor(CacheDependency dependency) {
            if (dependency == null) {
                throw new ArgumentNullException("dependency");
            }
            if (!dependency.Use()) {
                throw new InvalidOperationException(
                    SR.GetString(SR.Cache_dependency_used_more_that_once));
            }
            _dependency = dependency;
            _dependency.SetCacheDependencyChanged(this);
            if (_dependency.HasChanged) {
                OnChanged(null);
            }
            InitializationComplete();
        }

        void ICacheDependencyChanged.DependencyChanged(Object sender, EventArgs e) {
            OnChanged(null);
        }

        [SuppressMessage("Microsoft.Usage", "CA2215:DisposeMethodsShouldCallBaseClassDispose", Justification="suppressed because fxcop is suffering from despotism")]
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_dependency != null) {
                    _dependency.DisposeInternal();
                }
            }
        }
    }
}
#endif
