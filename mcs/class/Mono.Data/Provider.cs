//
// Mono.Data.Provider
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//  
//
// Copyright (C) Brian Ritchie, 2002
// 
//

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
using System;
using System.Data;
using System.Reflection;
using System.IO;

namespace Mono.Data
{
	public class Provider
	{
		private string name = null;
		private string connectionTypeName;
		private string adapterTypeName;
		private string commandTypeName;
		private Type connectionType;
		private Type adapterType;
		private Type commandType;
		private Assembly providerAssembly;
		private string assemblyName;
		private string description;
      
		public Provider(string _name, string _connection, 
			string _dataadapter, string _command, string _assembly,
			string _description)
		{
			name = _name;
			connectionTypeName = _connection;
			adapterTypeName = _dataadapter;
			assemblyName = _assembly;
			commandTypeName = _command;
			description = _description;
		}

		public Provider(string _name, Type _connection, Type _dataadapter, Type _command,
			string _description)
		{
			if (_connection == null) 
				throw new System.ArgumentNullException ("_connection");
			if (_dataadapter == null) 
				throw new System.ArgumentNullException ("_dataadapter");
			if (_command == null) 
				throw new System.ArgumentNullException ("_command");

			name = _name;
			connectionTypeName = _connection.FullName;
			adapterTypeName = _dataadapter.FullName;
			commandTypeName = _command.FullName;
			connectionType = _connection;
			adapterType = _dataadapter;
			commandType = _command;
			description = _description;
		}

		public string Name
		{
			get {return name;}
		}

		public string Description
		{
			get {return description;}
		}

		public Assembly ProviderAssembly {
			get {
				if (providerAssembly == null) {
					if (assemblyName.IndexOf(',') == -1) //try to load with a partial name if that's all we have
						providerAssembly = Assembly.LoadWithPartialName (assemblyName);
					else 
						providerAssembly = Assembly.Load (assemblyName);
				}

				return providerAssembly;
			}
		}

		public Type ConnectionType
		{
			get {
				if (connectionType == null) {
					connectionType = ProviderAssembly.GetType (connectionTypeName, false);
					if (connectionType == null) {
						throw new Exception (String.Format ("Unable to load type of connection class: {0} from assembly: {1}",
							connectionTypeName, assemblyName));
					}
				}
				return connectionType;
			}
		}

		public Type DataAdapterType
		{
			get {
				if (adapterType == null) {
					adapterType = ProviderAssembly.GetType (adapterTypeName, false);
					if (adapterType == null) {
						throw new Exception (String.Format ("Unable to load type of adapter class: {0} from assembly: {1}",
							adapterTypeName, assemblyName));
					}
				}
				return adapterType;
			}
		}

		public Type CommandType {
			get {
				if (commandType == null) {
					commandType = ProviderAssembly.GetType (commandTypeName, false);
					if (commandType == null) {
						throw new Exception (String.Format ("Unable to load type of command class: {0} from assembly: {1}",
							commandTypeName, assemblyName));
					}
				}
				return commandType;
			}
		}

		public IDbConnection CreateConnection()
		{
			object connObj = null;

			switch (Name) {
			case "System.Data.SqlClient":
				connObj = new System.Data.SqlClient.SqlConnection ();
				break;
			case "System.Data.Odbc":
				connObj = new System.Data.Odbc.OdbcConnection ();
				break;
			case "System.Data.OleDb":
				connObj = new System.Data.OleDb.OleDbConnection ();
				break;
			default:
				connObj = Activator.CreateInstance (ConnectionType);
				break;
			}

			if (connObj == null)
				throw new Exception (String.Format ("Unable to create instance of connection class: {0} from assembly: {1}",
					connectionTypeName, assemblyName));
			
			return (IDbConnection) connObj;
		}

		public IDbDataAdapter CreateDataAdapter()
		{
			object adapterObj = Activator.CreateInstance (DataAdapterType);
			if (adapterObj == null)
				throw new Exception (String.Format ("Unable to create instance of adapter class: {0} from assembly: {1}",
					adapterTypeName, assemblyName));

			return (IDbDataAdapter) adapterObj;
		}

		public IDbCommand CreateCommand()
		{
			object commandObj = Activator.CreateInstance (CommandType);
			if (commandObj == null)
				throw new Exception (String.Format ("Unable to create instance of command class: {0} from assembly: {1}",
					commandTypeName, assemblyName));

			return (IDbCommand) commandObj;
		}
	}
}
