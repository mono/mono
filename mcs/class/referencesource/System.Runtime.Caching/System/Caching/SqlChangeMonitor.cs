// <copyright file="SqlChangeMonitor.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Data.SqlClient;
using System.Globalization;
namespace System.Runtime.Caching {
    public sealed class SqlChangeMonitor : ChangeMonitor {
        private String _uniqueId;
        private SqlDependency _sqlDependency;
 
        public override String UniqueId { get { return _uniqueId; } }
 
        private SqlChangeMonitor() {} // hide default .ctor
 
        public SqlChangeMonitor(SqlDependency dependency) {
            if (dependency == null) {
                throw new ArgumentNullException("dependency");
            }
            bool dispose = true;
            try {
                _sqlDependency = dependency;
                _sqlDependency.OnChange += new OnChangeEventHandler(OnDependencyChanged);
                _uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                dispose = false;
            }
            finally {
                InitializationComplete();
                if (dispose) {
                    Dispose();
                }
            }
        }
 
        protected override void Dispose(bool disposing) {}
 
        private void OnDependencyChanged(Object sender, SqlNotificationEventArgs e) {
            OnChanged(null);
        }
    }
}
