//------------------------------------------------------------------------------
// <copyright file="SqlClientFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Data.Sql;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient {

    public sealed class SqlClientFactory : DbProviderFactory, IServiceProvider {

        public static readonly SqlClientFactory Instance = new SqlClientFactory();

        private SqlClientFactory() {
        }

        public override bool CanCreateDataSourceEnumerator {
            get { 
                return true;
            }
        }

        public override DbCommand CreateCommand() {
            return new SqlCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder() {
            return new SqlCommandBuilder();
        }

        public override DbConnection CreateConnection() {
            return new SqlConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder() {
            return new SqlConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter() {
            return new SqlDataAdapter();
        }

        public override DbParameter CreateParameter() {
            return new SqlParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state) {
            return new SqlClientPermission(state);
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator() {
            return SqlDataSourceEnumerator.Instance;
        }

        /// <summary>
        /// Extension mechanism for additional services; currently the only service
        /// supported is the DbProviderServices
        /// </summary>
        /// <returns>requested service provider or null.</returns>
        object IServiceProvider.GetService(Type serviceType) {
            object result = null;
            if (serviceType == GreenMethods.SystemDataCommonDbProviderServices_Type) {
                result = GreenMethods.SystemDataSqlClientSqlProviderServices_Instance();
            }
            return result;
        }
    }
}

