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
      
		public Provider(string _name, string _connection, 
			string _dataadapter, string _command, string _assembly)
		{
			name = _name;
			connectionTypeName = _connection;
			adapterTypeName = _dataadapter;
			assemblyName = _assembly;
			commandTypeName = _command;
		}

		public Provider(string _name, Type _connection, Type _dataadapter, Type _command)
		{
			name = _name;
			connectionTypeName = _connection.FullName;
			adapterTypeName = _dataadapter.FullName;
			commandTypeName = _command.FullName;
			connectionType = _connection;
			adapterType = _dataadapter;
			commandType = _command;
		}

		public string Name
		{
			get {return name;}
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
