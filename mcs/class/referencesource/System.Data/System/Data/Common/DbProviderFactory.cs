//------------------------------------------------------------------------------
// <copyright file="DbProviderFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Common {

    public abstract class DbProviderFactory { // V1.2.3300

        protected DbProviderFactory() {
        }
        
        virtual public bool CanCreateDataSourceEnumerator {
            get { 
                return false;
            }
        }

        public virtual DbCommand CreateCommand() {
            return null;
        }

        public virtual DbCommandBuilder CreateCommandBuilder() {
            return null;
        }

        public virtual DbConnection CreateConnection() {
            return null;
        }

        public virtual DbConnectionStringBuilder CreateConnectionStringBuilder() {
            return null;
        }

        public virtual DbDataAdapter CreateDataAdapter() {
            return null;
        }

        public virtual DbParameter CreateParameter() {
            return null;
        }

        public virtual CodeAccessPermission CreatePermission(PermissionState state) {
            return null;
        }

        public virtual DbDataSourceEnumerator CreateDataSourceEnumerator() {
            return null;
        }
    }
}

