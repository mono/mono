//------------------------------------------------------------------------------
// <copyright file="OdbcConnectionString.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Odbc {

    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    internal sealed class OdbcConnectionString : DbConnectionOptions {
        // instances of this class are intended to be immutable, i.e readonly
        // used by pooling classes so it is much easier to verify correctness
        // when not worried about the class being modified during execution

        private static class KEY {
            internal const string SaveFile                  = "savefile";
        }

        private readonly string _expandedConnectionString;        

        internal OdbcConnectionString(string connectionString, bool validate) : base(connectionString, null, true) {            
            if (!validate) {
                string filename = null;
                int position = 0;
                _expandedConnectionString = ExpandDataDirectories(ref filename, ref position);
            }
            if (validate || (null == _expandedConnectionString)) {
                // do not check string length if it was expanded because the final result may be shorter than the original
                if ((null != connectionString) && (ODBC32.MAX_CONNECTION_STRING_LENGTH < connectionString.Length)) { // MDAC 83536
                    throw ODBC.ConnectionStringTooLong();
                }
            }
        }

        protected internal override System.Security.PermissionSet CreatePermissionSet() {
            System.Security.PermissionSet permissionSet;
            if (ContainsKey(KEY.SaveFile)) {
                permissionSet = new NamedPermissionSet("FullTrust");
            }
            else {
                permissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
                permissionSet.AddPermission(new OdbcPermission(this));
            }
            return permissionSet;
        }

        protected internal override string Expand() {
            if (null != _expandedConnectionString) {
                return _expandedConnectionString;
            }
            else {
                return base.Expand();
            }
        }
    }
}
