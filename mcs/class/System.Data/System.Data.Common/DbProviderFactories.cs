//
// System.Data.Common.DbProviderFactories.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Threading;
using System.Reflection;
using System.Collections;
using System.Configuration;

namespace System.Data.Common {
	public static class DbProviderFactories
	{
		private static object configEntries; // DataSet

		internal const string CONFIG_SECTION_NAME        = "system.data";
		internal const string CONFIG_SEC_TABLE_NAME      = "DbProviderFactories";

		#region Methods

		public static DbProviderFactory GetFactory (DataRow providerRow)
		{
			string assemblyType = (string) providerRow ["AssemblyQualifiedName"];
#if TARGET_JVM // case insensitive GetType is not supported
			Type type = Type.GetType (assemblyType, false);
#else
			Type type = Type.GetType (assemblyType, false, true);
#endif
			if (type != null && type.IsSubclassOf (typeof (DbProviderFactory))) {
				// Provider factories are singletons with Instance field having
				// the sole instance
				FieldInfo field = type.GetField ("Instance", BindingFlags.Public |
					BindingFlags.Static);
				if (field != null) {
					return field.GetValue (null) as DbProviderFactory;
				}
			}

			throw new ConfigurationErrorsException("Failed to find or load the registered .Net Framework Data Provider.");
		}

		public static DbProviderFactory GetFactory (string providerInvariantName)
		{
			if (providerInvariantName == null)
				throw new ArgumentNullException ("providerInvariantName");

			DataTable table = GetFactoryClasses ();
			if (table != null) {
				DataRow row = table.Rows.Find (providerInvariantName);
				if (row != null)
					return GetFactory (row);
			}

			throw new ConfigurationErrorsException (String.Format("Failed to find or load the registered .Net Framework Data Provider '{0}'.", providerInvariantName));
		}
		
#if NET_4_5
		public static DbProviderFactory GetFactory (DbConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			return connection.DbProviderFactory;
		}
#endif

		public static DataTable GetFactoryClasses ()
		{
				DataSet ds = GetConfigEntries ();
				DataTable table = ds != null ? ds.Tables [CONFIG_SEC_TABLE_NAME] : null;
				if (table != null)
					table = table.Copy (); // avoid modifications by user
				return table;
		}

		#endregion // Methods

		#region Internal Methods

		internal static DataSet GetConfigEntries ()
		{
			if (configEntries != null)
				return configEntries as DataSet;

			DataSet ds = (DataSet) ConfigurationManager.GetSection (CONFIG_SECTION_NAME);
			Interlocked.CompareExchange (ref configEntries, ds, null);
			return configEntries as DataSet;
		}

		#endregion Internal Methods
	}
}
