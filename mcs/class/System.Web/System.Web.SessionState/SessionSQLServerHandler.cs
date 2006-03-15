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
		private static Type cncType = null;
		private IDbConnection cnc = null;
#if NET_2_0
		private SessionStateSection config;
#else
		private SessionConfig config;
#endif
                
		const string defaultParamPrefix = ":";
		string paramPrefix;
		string selectCommand = "SELECT * FROM ASPStateTempSessions WHERE SessionID = :SessionID";
		string insertCommand = "INSERT INTO ASPStateTempSessions VALUES (:SessionID, :Created, :Expires, :Timeout, :StaticObjectsData, :SessionData)";
		string updateCommand = "UPDATE ASPStateTempSessions SET SessionData = :SessionData WHERE SessionId = :SessionID";
		string deleteCommand = "DELETE FROM ASPStateTempSessions WHERE SessionId = :SessionID";

		public void Dispose ()
		{
			if (cnc != null) {
				cnc.Close ();
				cnc = null;
			}
		}

		public void Init (SessionStateModule module, HttpApplication context,
#if NET_2_0
				  SessionStateSection config
#else
				  SessionConfig config
#endif
				  )
		{
			string connectionTypeName;
			string providerAssemblyName;
			string cncString;

			this.config = config;

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
			try {
				cnc.Open ();
			} catch (Exception exc) {
				cnc = null;
				throw exc;
			}

			if (paramPrefix != defaultParamPrefix) {
				ReplaceParamPrefix (ref selectCommand);
				ReplaceParamPrefix (ref insertCommand);
				ReplaceParamPrefix (ref updateCommand);
				ReplaceParamPrefix (ref deleteCommand);
			}
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
				UpdateSessionWithRetry (id, dict);
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

			id = SessionId.Create (module.Rng);
			session = new HttpSessionState (id, new SessionDictionary (),
					HttpApplicationFactory.ApplicationState.SessionObjects,
#if NET_2_0
					(int)config.Timeout.TotalMinutes,
#else
					config.Timeout,
#endif
					true, config.CookieLess, SessionStateMode.SQLServer, read_only);

			InsertSessionWithRetry (session,
#if NET_2_0
				       (int)config.Timeout.TotalMinutes
#else
				       config.Timeout
#endif
				       );
			isNew = true;
			return session;
		}

		private void GetConnectionData (out string providerAssembly,
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
			IDbCommand command = null;
			command = cnc.CreateCommand();
			command.CommandText = selectCommand;
			command.Parameters.Add (CreateParam (command, DbType.String, "SessionID", id));
			return command.ExecuteReader ();
		}

		IDataReader GetReaderWithRetry (string id)
		{
			try {
				return GetReader (id);
			} catch {
			}

			try {
				cnc.Close ();
			} catch {
			}

			cnc.Open ();
			return GetReader (id);
		}

		private HttpSessionState SelectSession (string id, bool read_only)
		{
			HttpSessionState session = null;
			using (IDataReader reader = GetReaderWithRetry (id)) {
				if (!reader.Read ())
					return null;

				SessionDictionary dict; 
				HttpStaticObjectsCollection sobjs;
				
				dict = SessionDictionary.FromByteArray (ReadBytes (reader, reader.FieldCount-1));
				sobjs = HttpStaticObjectsCollection.FromByteArray (ReadBytes (reader, reader.FieldCount-2));
				
				session = new HttpSessionState (id, dict, sobjs, 100, false, config.CookieLess,
						SessionStateMode.SQLServer, read_only);
				return session;
			}
		}

		void InsertSession (HttpSessionState session, int timeout)
		{
			IDbCommand command = cnc.CreateCommand ();
			IDataParameterCollection param;

			command.CommandText = insertCommand;

			param = command.Parameters;
			param.Add (CreateParam (command, DbType.String, "SessionID", session.SessionID));
			param.Add (CreateParam (command, DbType.DateTime, "Created", DateTime.Now));
			param.Add (CreateParam (command, DbType.DateTime, "Expires", Tommorow ()));
			param.Add (CreateParam (command, DbType.Int32, "Timeout", timeout));
			param.Add (CreateParam (command, DbType.Binary, "StaticObjectsData",
						   session.StaticObjects.ToByteArray ()));
			param.Add (CreateParam (command, DbType.Binary, "SessionData",
						   session.SessionDictionary.ToByteArray ()));

			command.ExecuteNonQuery ();
		}

		void InsertSessionWithRetry (HttpSessionState session, int timeout)
		{
			try {
				InsertSession (session, timeout);
				return;
			} catch {
			}

			try {
				cnc.Close ();
			} catch {
			}

			cnc.Open ();
			InsertSession (session, timeout);
		}

		void UpdateSession (string id, SessionDictionary dict)
		{
			IDbCommand command = cnc.CreateCommand ();
			IDataParameterCollection param;

			command.CommandText = updateCommand;

			param = command.Parameters;
			param.Add (CreateParam (command, DbType.String, "SessionID", id));
			param.Add (CreateParam (command, DbType.Binary, "SessionData",
								dict.ToByteArray ()));

			command.ExecuteNonQuery ();
		}

		void UpdateSessionWithRetry (string id, SessionDictionary dict)
		{
			try {
				UpdateSession (id, dict);
				return;
			} catch {
			}

			try {
				cnc.Close ();
			} catch {
			}

			cnc.Open ();
			UpdateSession (id, dict);
		}

		void DeleteSession (string id)
		{
			IDbCommand command = cnc.CreateCommand ();
			IDataParameterCollection param;

			command.CommandText = deleteCommand;
			param = command.Parameters;
			param.Add (CreateParam (command, DbType.String, "SessionID", id));
			command.ExecuteNonQuery ();
		}

		void DeleteSessionWithRetry (string id)
		{
			try {
				DeleteSession (id);
				return;
			} catch {
			}

			try {
				cnc.Close ();
			} catch {
			}

			cnc.Open ();
			DeleteSession (id);
		}

		private IDataParameter CreateParam (IDbCommand command, DbType type,
				string name, object value)
		{
			IDataParameter result = command.CreateParameter ();
			result.DbType = type;
			result.ParameterName = paramPrefix + name;
			result.Value = value;
			return result;
		}

		private DateTime Tommorow ()
		{
			return DateTime.Now.AddDays (1);
		}

		private byte [] ReadBytes (IDataReader reader, int index)
		{
			int len = (int) reader.GetBytes (reader.FieldCount-1, 0, null, 0, 0);
			byte [] data = new byte [len];
			reader.GetBytes (index, 0, data, 0, len);
			return data;
		}
	}
}

