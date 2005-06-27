//
// Mono.Data.ProviderFactory
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
using System.Runtime.Remoting;
using System.Configuration;
using System.Xml;
using System.Collections.Specialized;

namespace Mono.Data
{
	public class ProviderFactory
	{
		private static ProviderCollection providers;

		static ProviderFactory()
		{
			providers=(ProviderCollection) ConfigurationSettings.GetConfig("mono.data/providers");
			if (providers==null)
				providers=new ProviderCollection();
		}

		static public ProviderCollection Providers
		{
			get
			{
				return providers;
			}
		}

		static public IDbConnection CreateConnectionFromConfig(string Setting)
		{
			return CreateConnection(ConfigurationSettings.AppSettings[Setting]);
		}

		static public IDbConnection CreateConnection(string ConnectionString)
		{
			string[] ConnectionAttributes=ConnectionString.Split(new Char[1] { ';' }); 
			string ProviderName=null;
			string NewConnectionString="";
			foreach (string s in ConnectionAttributes)
			{
				string[] AttributeParts=s.Split(new Char[1] { '=' });
				if (AttributeParts[0].ToLower().Trim()=="factory")
					ProviderName=AttributeParts[1].Trim();
				else 
					NewConnectionString+=";"+s;
			}
			NewConnectionString=NewConnectionString.Remove(0,1);
			return CreateConnection(ProviderName, NewConnectionString);
		}

		static public IDbConnection CreateConnection(string ProviderName, string ConnectionString)
		{
			Provider provider=providers[ProviderName];
			IDbConnection conn=provider.CreateConnection();
			conn.ConnectionString=ConnectionString;
			return conn;
		}

		static public IDbCommand CreateStoredProc(IDbConnection Conn, string CommandName)
		{
			IDbCommand cmd=Conn.CreateCommand();
			cmd.CommandText=CommandName;
			cmd.CommandType=CommandType.StoredProcedure;
			return cmd;
		}

		static public IDbDataAdapter CreateDataAdapter(IDbCommand SelectCommand)
		{
			Provider provider=providers.FindByCommandType(SelectCommand.GetType());
			IDbDataAdapter adapter=provider.CreateDataAdapter();
			adapter.SelectCommand=SelectCommand;
			return adapter;
		}

		static public IDbDataAdapter CreateDataAdapter(string ProviderName)
		{
			Provider provider=providers[ProviderName];
			IDbDataAdapter adapter=provider.CreateDataAdapter();
			return adapter;
		}

		static public IDbDataAdapter CreateDataAdapter(IDbConnection Conn, string SelectCommand)
		{
			IDbCommand cmd=Conn.CreateCommand();
			cmd.CommandText=SelectCommand;
			return CreateDataAdapter(cmd);
		}

		static public IDbCommand CreateCommand(string ProviderName)
		{
			Provider provider=providers[ProviderName];
			return provider.CreateCommand();
		}

		static public IDbCommand CreateCommand(IDbConnection Conn)
		{
			return Conn.CreateCommand();
		}

	}
}
