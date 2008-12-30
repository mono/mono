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
#if NET_2_0
	[Obsolete("ProviderFactory in assembly Mono.Data has been made obsolete by DbProviderFactories in assembly System.Data.")]
#endif
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
		private string parameterprefix;
		private string commandBuilderTypeName = String.Empty;
		private Type commandBuilderType;

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

		public Provider(string _name, string _connection, 
			string _dataadapter, string _command, string _assembly,
			string _description, string _parameterprefix, string _commandbuilder)
		{
			name = _name;
			connectionTypeName = _connection;
			adapterTypeName = _dataadapter;
			assemblyName = _assembly;
			commandTypeName = _command;
			description = _description;

			switch(_parameterprefix) {
			case "colon":
				parameterprefix = ":"; // named parameter prefixed by a semicolon
				break;
			case "at":
				parameterprefix = "@"; // named parameter prefixed by an at symbol
				break;
			case "questionmark":
				parameterprefix = "?"; // postional parameter noted by the question mark
				break;
			}

			commandBuilderTypeName = _commandbuilder;
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

		public string ParameterPrefix 
		{
			get {return parameterprefix;}
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

		public Type CommandBuilderType {
			get {
				if (commandBuilderType == null) {
					if (commandBuilderTypeName.Equals(String.Empty))
						throw new Exception("Provider does not have CommandBuilder type defined.");
					commandBuilderType = ProviderAssembly.GetType (commandBuilderTypeName, false);
					if (commandBuilderType == null) {
						throw new Exception (String.Format ("Unable to load type of command class: {0} from assembly: {1}",
							commandBuilderTypeName, assemblyName));
					}
				}
				return commandBuilderType;
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

		public object CreateCommandBuilder(IDbDataAdapter adapter) 
		{
			if (adapter == null) 
				throw new System.ArgumentNullException ("adapter");

			object obj = (object) adapter;
			if (!DataAdapterType.ToString ().Equals (obj.ToString ()))
				throw new System.ArgumentException ("adapter not part of this provider.");
				
			if (commandBuilderTypeName.Equals (String.Empty))
				throw new Exception ("Provider does not have CommandBuilder type defined.");
			
			object[] parms = new object [] { obj };
			object commandBuilderObj = Activator.CreateInstance (CommandBuilderType, parms);
			if (commandBuilderObj == null)
				throw new Exception (String.Format ("Unable to create instance of command builder class: {0} from assembly: {1}",
					commandBuilderTypeName, assemblyName));

			return commandBuilderObj;
		}
	}
}

