//
// DbProvider.cs
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.Gui.GtkSharp 
{
	using System;
	using System.Data;

	public class DbProvider 
	{
		string key;  // unique key to identify this provider - SYBASE
		string name; // description of provider - Sybase SQL Server
		string assembly; // assembly file - Mono.Data.SybaseClient
		string connectionClass; // xxxConnection class 
		string adapterClass; // xxxAdapter class
		// the class that implements IDbConnection
		// - Mono.Data.SybaseClient.SybaseConnection
		
		bool internalProvider; // true = exists in System.Data.dll
		                       // false = provider is external and
		                       // must be loaded dynamically

		public string Key {
			get {
				return key;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public string Assembly {
			get {
				return assembly;
			}
		}

		public string ConnectionClass {
			get {
				return connectionClass;
			}
		}

		public string AdapterClass {
			get {
				return adapterClass;
			}
		}

		public bool InternalProvider {
			get {
				return internalProvider;
			}
		}

		public DbProvider(string key, string name, string assembly,
				string connectionClass, string adapterClass, 
				bool internalProvider) 
		{
			this.key = key;
			this.name = name;
			this.assembly = assembly;
			this.connectionClass = connectionClass;
			this.adapterClass = adapterClass;
			this.internalProvider = internalProvider;
		}
	}
}
