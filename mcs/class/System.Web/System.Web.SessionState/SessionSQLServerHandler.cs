//
// System.Web.SessionState.SessionSQLServerHandler
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com), All rights reserved
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
#if !NET_2_0
using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Configuration;
using System.Collections.Specialized;
using System.Web.Configuration;

namespace System.Web.SessionState {

	internal class SessionSQLServerHandler : ISessionHandler
	{
		static Type cncType = null;
		IDbConnection cnc = null;
		SessionConfig config;
		string AppPath = String.Empty;
                
		const string defaultParamPrefix = ":";
		string paramPrefix;
		string selectCommandText = "SELECT timeout,staticobjectsdata,sessiondata FROM ASPStateTempSessions WHERE SessionID = :SessionID AND Expires > :Expires AND AppPath = :AppPath";
		IDbCommand selectCommand = null;
		string insertCommandText = "INSERT INTO ASPStateTempSessions (SessionId, AppPath, Created, expires, timeout, StaticObjectsData, SessionData)  VALUES (:SessionID, :AppPath, :Created, :Expires, :Timeout, :StaticObjectsData, :SessionData)";
		IDbCommand insertCommand = null;
		string updateCommandText = "UPDATE ASPStateTempSessions SET expires = :Expires, timeout = :Timeout, SessionData = :SessionData WHERE SessionId = :SessionID";
		IDbCommand updateCommand = null;
		string deleteCommandText = "DELETE FROM ASPStateTempSessions WHERE SessionId = :SessionID";
		IDbCommand deleteCommand = null;

		public void Dispose ()
		{
			if (cnc != null) {
				cnc.Close ();
				cnc = null;
			}
		}

		public void Init (SessionStateModule module, HttpApplication context,
				  SessionConfig config)
		{


			this.config = config;
			this.AppPath = context.Request.ApplicationPath;
			
			try {
				InitializeConnection ();
			} catch (Exception exc) {
				cnc = null;
				throw exc;
			}

			if (paramPrefix != defaultParamPrefix) {
				ReplaceParamPrefix (ref selectCommandText);
				ReplaceParamPrefix (ref insertCommandText);
				ReplaceParamPrefix (ref updateCommandText);
				ReplaceParamPrefix (ref deleteCommandText);
			}
		}
		
		void CreateNewConnection() 
		{
			string connectionTypeName;
			string providerAssemblyName;
			string cncString;
			GetConnectionData (out providerAssemblyName, out connectionTypeName, out cncString);
			if (cncType == null) {
				Assembly dbAssembly = Assembly.Load (providerAssemblyName);
				cncType = dbAssembly.GetType (connectionTypeName, true);
				if (!typeof (IDbConnection).IsAssignableFrom (cncType))
					throw new ApplicationException ("The type '" + cncType +
							"' does not implement IDB Connection.\n" +
							"Check 'DbConnectionType' in server.exe.config.");
			}

			cnc = (IDbConnection) Activator.CreateInstance (cncType);
			cnc.ConnectionString = cncString;
		}

		void ReplaceParamPrefix(ref string command)
		{
			command = command.Replace (defaultParamPrefix, paramPrefix);
		}

		public void UpdateHandler (HttpContext context, SessionStateModule module)
		{
			HttpSessionState session = context.Session;
			if (session == null || session.IsReadOnly)
				return;

			string id = session.SessionID;
			if (!session._abandoned) {
				SessionDictionary dict = session.SessionDictionary;
				UpdateSessionWithRetry (id, session.Timeout, dict);
			} else {
				DeleteSessionWithRetry (id);
			}
		}

		public HttpSessionState UpdateContext (HttpContext context, SessionStateModule module,
							bool required, bool read_only, ref bool isNew)
		{
			if (!required)
				return null;

			HttpSessionState session = null;
			string id = SessionId.Lookup (context.Request, config.CookieLess);

			if (id != null) {
				session = SelectSession (id, read_only);
				if (session != null)
					return session;
			}

			id = SessionId.Create ();
			session = new HttpSessionState (id, new SessionDictionary (),
					HttpApplicationFactory.ApplicationState.SessionObjects,
					config.Timeout,
					true, config.CookieLess, SessionStateMode.SQLServer, read_only);

			InsertSessionWithRetry (session, config.Timeout);
			isNew = true;
			return session;
		}

		public void Touch (string sessionId, int timeout)
		{
			HttpContext ctx = HttpContext.Current;
			if (ctx == null)
				return;
			
			HttpSessionState session = ctx.Session;
			if (session == null)
				return;
			
			UpdateSession (sessionId, timeout, session.SessionDictionary);
		}
		
		void GetConnectionData (out string providerAssembly,
				out string cncTypeName, out string cncString)
		{
			providerAssembly = null;
			cncTypeName = null;
			cncString = null;

			NameValueCollection config = ConfigurationSettings.AppSettings;
			if (config != null) {
				providerAssembly = config ["StateDBProviderAssembly"];
				cncTypeName = config ["StateDBConnectionType"];
				paramPrefix = config ["StateDBParamPrefix"];
			}

			cncString = this.config.SqlConnectionString;

			if (providerAssembly == null || providerAssembly == String.Empty)
				providerAssembly = "Npgsql.dll";

			if (cncTypeName == null || cncTypeName == String.Empty)
				cncTypeName = "Npgsql.NpgsqlConnection";

			if (cncString == null || cncString == String.Empty)
				cncString = "SERVER=127.0.0.1;USER ID=monostate;PASSWORD=monostate;dbname=monostate";

			if (paramPrefix == null || paramPrefix == String.Empty)
				paramPrefix = defaultParamPrefix;
		}

		IDataReader GetReader (string id)
		{
			((IDataParameter)selectCommand.Parameters["SessionID"]).Value = id;
			((IDataParameter)selectCommand.Parameters["Expires"]).Value = DateTime.Now;
			((IDataParameter)selectCommand.Parameters["AppPath"]).Value = this.AppPath;
			return selectCommand.ExecuteReader ();
		}

		IDataReader GetReaderWithRetry (string id)
		{
			try {
				return GetReader (id);
			} catch {
			}

			try {
				DisposeConnection();
			} catch {
			}

			InitializeConnection();
			return GetReader (id);
		}

		HttpSessionState SelectSession (string id, bool read_only)
		{
			HttpSessionState session = null;
			using (IDataReader reader = GetReaderWithRetry (id)) {
				if (!reader.Read ())
					return null;

				SessionDictionary dict; 
				HttpStaticObjectsCollection sobjs;
				int timeout;
				
				dict = SessionDictionary.FromByteArray (ReadBytes (reader, reader.FieldCount-1));
				sobjs = HttpStaticObjectsCollection.FromByteArray (ReadBytes (reader, reader.FieldCount-2));
				// try to support as many DBs/int types as possible
				timeout = Convert.ToInt32 (reader.GetValue (reader.FieldCount-3));
				
				session = new HttpSessionState (id, dict, sobjs, timeout, false, config.CookieLess,
						SessionStateMode.SQLServer, read_only);
				return session;
			}
		}

		void InsertSession (HttpSessionState session, int timeout)
		{
			((IDataParameter)insertCommand.Parameters["SessionID"]).Value = session.SessionID;
			((IDataParameter)insertCommand.Parameters["AppPath"]).Value = this.AppPath;
			((IDataParameter)insertCommand.Parameters["Created"]).Value = DateTime.Now;
			((IDataParameter)insertCommand.Parameters["Expires"]).Value = DateTime.Now.AddMinutes (timeout);
			((IDataParameter)insertCommand.Parameters["Timeout"]).Value = timeout;
			((IDataParameter)insertCommand.Parameters["StaticObjectsData"]).Value = session.StaticObjects.ToByteArray ();
			((IDataParameter)insertCommand.Parameters["SessionData"]).Value = session.SessionDictionary.ToByteArray ();
			insertCommand.ExecuteNonQuery ();
		}

		void InsertSessionWithRetry (HttpSessionState session, int timeout)
		{
			try {
				InsertSession (session, timeout);
				return;
			} catch {
			}

			try {
				DisposeConnection ();
			} catch {
			}

			InitializeConnection ();
			InsertSession (session, timeout);
		}

		void UpdateSession (string id, int timeout, SessionDictionary dict)
		{
			((IDataParameter)updateCommand.Parameters["SessionID"]).Value = id;
			((IDataParameter)updateCommand.Parameters["Expires"]).Value = DateTime.Now.AddMinutes (timeout);
			((IDataParameter)updateCommand.Parameters["Timeout"]).Value = timeout;
			((IDataParameter)updateCommand.Parameters["SessionData"]).Value = dict.ToByteArray ();
			
			updateCommand.ExecuteNonQuery ();
		}

		void UpdateSessionWithRetry (string id, int timeout, SessionDictionary dict)
		{
			try {
				UpdateSession (id, timeout, dict);
				return;
			} catch {
			}

			try {
				DisposeConnection ();
			} catch {
			}

			InitializeConnection ();
			UpdateSession (id, timeout, dict);
		}

		void DeleteSession (string id)
		{
			
			((IDataParameter)deleteCommand.Parameters["SessionID"]).Value = id;
			deleteCommand.ExecuteNonQuery ();
		}

		void DeleteSessionWithRetry (string id)
		{
			try {
				DeleteSession (id);
				return;
			} catch {
			}

			try {
				DisposeConnection ();
			} catch {
			}

			InitializeConnection ();
			DeleteSession (id);
		}

		void InitializeConnection()
		{
			if (cnc == null)
				CreateNewConnection();
			cnc.Open ();
			selectCommand = cnc.CreateCommand ();
			selectCommand.CommandText = selectCommandText;
			selectCommand.Parameters.Add (CreateParam (selectCommand, DbType.String, "SessionID", String.Empty));
			selectCommand.Parameters.Add (CreateParam (selectCommand, DbType.DateTime, "Expires", DateTime.MinValue ));
			selectCommand.Parameters.Add (CreateParam (selectCommand, DbType.String, "AppPath", String.Empty));
			selectCommand.Prepare ();
			
			insertCommand = cnc.CreateCommand ();
			insertCommand.CommandText = insertCommandText;
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.String, "SessionID", String.Empty));
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.String, "AppPath", String.Empty));
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.DateTime, "Created", DateTime.MinValue));
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.DateTime, "Expires", DateTime.MinValue));
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.Int32, "Timeout", 0));
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.Binary, "StaticObjectsData",new byte[0] ));
			insertCommand.Parameters.Add (CreateParam (insertCommand, DbType.Binary, "SessionData",
						   new byte[0]));
			insertCommand.Prepare();
			
			updateCommand = cnc.CreateCommand ();
			updateCommand.CommandText = updateCommandText;
			updateCommand.Parameters.Add (CreateParam (updateCommand, DbType.String, "SessionID", String.Empty));
			updateCommand.Parameters.Add (CreateParam (updateCommand, DbType.DateTime, "Expires", DateTime.MinValue));
			updateCommand.Parameters.Add (CreateParam (updateCommand, DbType.Int32, "Timeout", 0));
			updateCommand.Parameters.Add (CreateParam (updateCommand, DbType.Binary, "SessionData",
								new byte[0]));
			updateCommand.Prepare();
			
			deleteCommand = cnc.CreateCommand ();
			deleteCommand.CommandText = deleteCommandText;
			deleteCommand.Parameters.Add (CreateParam (deleteCommand, DbType.String, "SessionID", String.Empty));
			deleteCommand.Prepare();
		}
		
		void DisposeConnection()
		{
			selectCommand.Dispose();
			insertCommand.Dispose();
			updateCommand.Dispose();
			deleteCommand.Dispose();
			cnc.Close();
		}
		
		IDataParameter CreateParam (IDbCommand command, DbType type,
				string name, object value)
		{
			IDataParameter result = command.CreateParameter ();
			result.DbType = type;
			result.ParameterName = paramPrefix + name;
			result.Value = value;
			return result;
		}

		byte [] ReadBytes (IDataReader reader, int index)
		{
			int len = (int) reader.GetBytes (index, 0, null, 0, 0);
			byte [] data = new byte [len];
			reader.GetBytes (index, 0, data, 0, len);
			return data;
		}
	}
}
#endif
