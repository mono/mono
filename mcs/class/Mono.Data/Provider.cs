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

		public Type ConnectionType
		{
			get 
			{
				if (connectionType==null)
				{
					connectionType=Type.GetType(connectionTypeName+","+assemblyName);
				}
				return connectionType;
			}
		}

		public Type DataAdapterType
		{
			get 
			{
				if (adapterType==null)
				{
					adapterType=Type.GetType(adapterTypeName+","+assemblyName);
				}
				return adapterType;
			}
		}

		public Type CommandType
		{
			get 
			{
				if (commandType==null)
				{
					commandType=Type.GetType(commandTypeName+","+assemblyName);
				}
				return commandType;
			}
		}

		public IDbConnection CreateConnection()
		{
			return (IDbConnection) Activator.CreateInstance(ConnectionType);
		}

		public IDbDataAdapter CreateDataAdapter()
		{
			return (IDbDataAdapter) Activator.CreateInstance(DataAdapterType);
		}

		public IDbCommand CreateCommand()
		{
			return (IDbCommand) Activator.CreateInstance(CommandType);
		}
	}
}
