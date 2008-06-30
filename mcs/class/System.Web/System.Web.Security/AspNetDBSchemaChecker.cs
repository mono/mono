//
// System.Web.Security.SqlMembershipProvider
//
// Authors:
//  Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005,2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Configuration.Provider;

namespace System.Web.Security
{
	internal static class AspNetDBSchemaChecker
	{
		static DbConnection CreateConnection (DbProviderFactory factory, string connStr)
		{
			DbConnection connection = factory.CreateConnection ();
			connection.ConnectionString = connStr;

			connection.Open ();
			return connection;
		}

		public static bool CheckMembershipSchemaVersion (DbProviderFactory factory, string connStr, string feature, string compatibleVersion)
		{
			using (DbConnection connection = CreateConnection (factory, connStr)) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_CheckSchemaVersion";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (factory, command, "@Feature", ParameterDirection.Input, feature);
				AddParameter (factory, command, "@CompatibleSchemaVersion", ParameterDirection.Input, compatibleVersion);
				DbParameter returnValue = AddParameter (factory, command, "@ReturnVal", ParameterDirection.ReturnValue, null);

				try {
					command.ExecuteNonQuery ();
				}
				catch (Exception) {
					throw new ProviderException ("ASP.NET Membership schema not installed.");
				}

				if ((int) (returnValue.Value ?? -1) == 0)
					return true;

				return false;
			}
		}

		static DbParameter AddParameter (DbProviderFactory factory, DbCommand command, string parameterName, ParameterDirection direction, object parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = direction;
			command.Parameters.Add (dbp);
			return dbp;
		}

	}
}
#endif