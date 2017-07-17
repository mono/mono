// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Data.SqlClient;
using System.Reflection;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Text;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb
{
    public sealed class OleDbFactory : DbProviderFactory { }
}

namespace System.Data.Odbc
{
    partial class OdbcFactory
    {        
        public override CodeAccessPermission CreatePermission(PermissionState state) =>
            new OdbcPermission(state);
    }
}

namespace System.Data.SqlClient
{
    partial class SqlClientFactory
    {
        public override bool CanCreateDataSourceEnumerator => true;

        public override DbDataSourceEnumerator CreateDataSourceEnumerator() => 
            SqlDataSourceEnumerator.Instance;

		public override CodeAccessPermission CreatePermission (PermissionState state) =>
			new SqlClientPermission(state);
    }

    partial class SqlParameter
    {
        public SqlParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
        {
            if (parameterName == null)
                parameterName = string.Empty;

            _isNull = isNullable;
            Value = value;
            Scale = scale;
            Size = size;
            Precision = precision;
            SqlDbType = dbType;
            Direction = direction;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
        }

        [MonoTODO]
        public string UdtTypeName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }

    partial class SqlException
    {
        private const string DEF_MESSAGE = "SQL Exception has occured.";

        public override string Message {
			get {
				if (Errors.Count == 0)
					return base.Message;
				StringBuilder result = new StringBuilder ();
				if (base.Message != DEF_MESSAGE) {
					result.Append (base.Message);
					result.Append ("\n");
				}
				for (int i = 0; i < Errors.Count -1; i++) {
					result.Append (Errors [i].Message);
					result.Append ("\n");
				}
				result.Append (Errors [Errors.Count - 1].Message);
				return result.ToString ();
			}
		}
    }

    partial class SqlCommand
    {
        public bool NotificationAutoEnlist => Notification != null;

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteNonQuery() => BeginExecuteNonQuery(null, null);

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteXmlReader() => BeginExecuteXmlReader(null, null);

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader() =>
            BeginExecuteReader(CommandBehavior.Default, null, null);

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject) =>
            BeginExecuteReader(CommandBehavior.Default, callback, stateObject);

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior) =>
            BeginExecuteReader(behavior, callback, stateObject);

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(CommandBehavior behavior) =>
            BeginExecuteReader(behavior, null, null);
    }
}

namespace Microsoft.SqlServer.Server
{
    partial class SqlMetaData
    {
		public SqlMetaData (string name, SqlDbType dbType, Type userDefinedType) :
            this (name, dbType, -1, 0, 0, 0, System.Data.SqlTypes.SqlCompareOptions.None, userDefinedType)
		{
		}
    }
}
