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
#if NET_2_0
	[Obsolete("ProviderFactory in assembly Mono.Data has been made obsolete by DbProviderFactories in assembly System.Data.")]
#endif
	public class ProviderFactory
	{
		private static ProviderCollection providers;

		static ProviderFactory ()
		{
			providers = (ProviderCollection) ConfigurationSettings.GetConfig ("mono.data/providers");
			if (providers == null) {
				providers = new ProviderCollection ();
				// warn the developer or administrator that the provider list is empty
				System.Diagnostics.Debug.Listeners.Add (new System.Diagnostics.TextWriterTraceListener (Console.Out));
				System.Diagnostics.Debug.WriteLine ("No providers found. Did you set up a mono.data/providers area in your app.config or in machine.config?");
			}

		}

		static public ProviderCollection Providers
		{
			get {
				return providers;
			}
		}

		static public IDbConnection CreateConnectionFromConfig (string Setting)
		{
			if (Setting == null) 
				throw new System.ArgumentNullException ("Setting");

			return CreateConnection (ConfigurationSettings.AppSettings [Setting]);
		}

		static public IDbConnection CreateConnection(string ConnectionString)
		{
			if (ConnectionString == null) 
				throw new System.ArgumentNullException ("ConnectionString");

			string [] ConnectionAttributes = ConnectionString.Split (new Char [1] { ';' }); 
			string ProviderName = null;
			string NewConnectionString = "";
			foreach (string s in ConnectionAttributes) {
				string [] AttributeParts = s.Split (new Char [1] { '=' });
				if (AttributeParts [0].ToLower ().Trim () == "factory")
					ProviderName = AttributeParts [1].Trim ();
				else 
					NewConnectionString += ";" + s;
			}
			NewConnectionString = NewConnectionString.Remove (0, 1); // remove the initial semicolon
			if (ProviderName == null) 
				throw new System.ArgumentException ("The connection string must contain a 'factory=Provider.Class' token", "ConnectionString");
			return CreateConnection (ProviderName, NewConnectionString);
		}

		static public IDbConnection CreateConnection(string ProviderName, string ConnectionString)
		{
			if (ProviderName == null) 
				throw new System.ArgumentNullException("ProviderName");
			if (ConnectionString == null) 
				throw new System.ArgumentNullException ("ConnectionString");

			Provider provider = providers [ProviderName];

			if (provider == null)
				throw new ArgumentException ("ProviderName", "The specified provider does not exist");
			
			IDbConnection conn = provider.CreateConnection ();
			conn.ConnectionString = ConnectionString;
			return conn;
		}

		static public IDbCommand CreateStoredProc (IDbConnection Conn, string CommandName)
		{
			if (Conn == null) 
				throw new System.ArgumentNullException ("Conn");
			if (CommandName == null) 
				throw new System.ArgumentNullException ("CommandName");

			IDbCommand cmd = Conn.CreateCommand ();
			cmd.CommandText = CommandName;
			cmd.CommandType = CommandType.StoredProcedure;
			return cmd;
		}

		static public IDbDataAdapter CreateDataAdapter (IDbCommand SelectCommand)
		{
			if (SelectCommand == null) 
				throw new System.ArgumentNullException("SelectCommand");

			Provider provider = providers.FindByCommandType (SelectCommand.GetType ());
			IDbDataAdapter adapter = provider.CreateDataAdapter ();
			adapter.SelectCommand = SelectCommand;
			return adapter;
		}

		static public IDbDataAdapter CreateDataAdapter (string ProviderName)
		{
			if (ProviderName == null) 
				throw new System.ArgumentNullException("ProviderName");

			Provider provider = providers [ProviderName];
			IDbDataAdapter adapter = provider.CreateDataAdapter ();
			return adapter;
		}

		static public IDbDataAdapter CreateDataAdapter (IDbConnection Conn, string SelectCommand)
		{
			if (Conn == null) 
				throw new System.ArgumentNullException ("Conn");
			if (SelectCommand == null) 
				throw new System.ArgumentNullException("SelectCommand");

			IDbCommand cmd = Conn.CreateCommand ();
			cmd.CommandText = SelectCommand;
			return CreateDataAdapter (cmd);
		}

		static public IDbCommand CreateCommand (string ProviderName)
		{
			if (ProviderName == null) 
				throw new System.ArgumentNullException("ProviderName");

			Provider provider = providers [ProviderName];
			return provider.CreateCommand ();
		}

		static public IDbCommand CreateCommand (IDbConnection Conn)
		{
			if (Conn == null) 
				throw new System.ArgumentNullException("Conn");

			return Conn.CreateCommand ();
		}

	}
}
