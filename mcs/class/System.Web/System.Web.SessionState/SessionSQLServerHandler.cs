//
// System.Web.SessionState.SessionSQLServerHandler
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com), All rights reserved
//

using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Configuration;
using System.Collections.Specialized;

namespace System.Web.SessionState {

	internal class SessionSQLServerHandler : ISessionHandler
	{
		const string CookieName = "ASPSESSION";
		const int DefTimeout = 600;

		private Type cncType = null;
		private IDbConnection cnc = null;
		private SessionConfig config;

		public void Dispose ()
		{
			if (cnc != null) {
				cnc.Close ();
				cnc = null;
			}
		}

		public void Init (HttpApplication context, SessionConfig config)
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
		}

		public void UpdateHandler (HttpContext context, SessionStateModule module)
		{
			if (context.Session == null)
				return;

			string id = context.Session.SessionID;
			SessionDictionary dict = context.Session.SessionDictionary;

			UpdateSession (id, dict);
		}

		public bool UpdateContext (HttpContext context, SessionStateModule module)
		{
			HttpSessionState session = null;
			string id = GetId (context);

			if (id != null) {
				session = SelectSession (id);
				if (session != null) {
					context.SetSession (session);
					return false;
				}
			}

			id = SessionId.Create (module.Rng);
			session = new HttpSessionState (id, new SessionDictionary (),
					new HttpStaticObjectsCollection (), config.Timeout, true,
					false, SessionStateMode.SQLServer, false);

			InsertSession (session, config.Timeout);
			context.SetSession (session);
			context.Session.IsNewSession = true;
			context.Response.AppendCookie (new HttpCookie (CookieName, id));

			return true;
		}

		private void GetConnectionData (out string providerAssembly,
				out string cncTypeName, out string cncString)
		{
			providerAssembly = null;
			cncTypeName = null;
			cncString = null;

			NameValueCollection config = ConfigurationSettings.AppSettings as NameValueCollection;
			if (config != null) {
				foreach (string s in config.Keys) {
					if (0 == String.Compare ("StateDBProviderAssembly", s, true)) {
						providerAssembly = config [s];
					} else if (0 == String.Compare ("StateDBConnectionType", s, true)) {
						cncTypeName = config [s];
					}
				}
			}

			cncString = this.config.SqlConnectionString;

			if (providerAssembly == null || providerAssembly == String.Empty)
				providerAssembly = "Npgsql.dll";

			if (cncTypeName == null || cncTypeName == String.Empty)
				cncTypeName = "Npgsql.NpgsqlConnection";

			if (cncString == null || cncString == String.Empty)
				cncString = "SERVER=127.0.0.1;USER ID=monostate;PASSWORD=monostate;dbname=monostate";
		}

		private HttpSessionState SelectSession (string id)
		{
			HttpSessionState session = null;
			IDbCommand command = cnc.CreateCommand();
			IDataReader reader;

			string select = "SELECT * from aspstatetempsessions WHERE SessionID = :SessionID";

			command.CommandText = select;

			command.Parameters.Add (CreateParam (command, DbType.String, ":SessionID", id));

			try {
				reader = command.ExecuteReader ();

				if (!reader.Read ())
					return null;

				SessionDictionary dict; 
				HttpStaticObjectsCollection sobjs;
				
				dict = SessionDictionary.FromByteArray (ReadBytes (reader, reader.FieldCount-1));
				sobjs = HttpStaticObjectsCollection.FromByteArray (ReadBytes (reader, reader.FieldCount-2));
				
				session = new HttpSessionState (id, dict, sobjs, 100, true, false,
						SessionStateMode.SQLServer, false);
				return session;
			} catch {
				throw;
			}
		}

		private void InsertSession (HttpSessionState session, int timeout)
		{
			IDbCommand command = cnc.CreateCommand ();
			IDataParameterCollection param;

			string insert = "INSERT INTO ASPStateTempSessions VALUES " +
			"(:SessionID, :Created, :Expires, :Timeout, :StaticObjectsData, :SessionData)";

			command.CommandText = insert;

			param = command.Parameters;
			param.Add (CreateParam (command, DbType.String, ":SessionID", session.SessionID));
			param.Add (CreateParam (command, DbType.DateTime, ":Created", DateTime.Now));
			param.Add (CreateParam (command, DbType.DateTime, ":Expires", Tommorow ()));
			param.Add (CreateParam (command, DbType.Int32, ":Timeout", timeout));
			param.Add (CreateParam (command, DbType.Binary, ":StaticObjectsData",
						   session.StaticObjects.ToByteArray ()));
			param.Add (CreateParam (command, DbType.Binary, ":SessionData",
						   session.SessionDictionary.ToByteArray ()));

			command.ExecuteNonQuery ();
		}

		private void UpdateSession (string id, SessionDictionary dict)
		{
			IDbCommand command = cnc.CreateCommand ();
			IDataParameterCollection param;

			string update = "UPDATE ASPStateTempSessions SET " +
			"SessionData = :SessionData WHERE SessionId = :SessionID";

			command.CommandText = update;

			param = command.Parameters;
			param.Add (CreateParam (command, DbType.String, ":SessionID", id));
			param.Add (CreateParam (command, DbType.Binary, ":SessionData",
								dict.ToByteArray ()));

			command.ExecuteNonQuery ();
		}

		private IDataParameter CreateParam (IDbCommand command, DbType type,
				string name, object value)
		{
			IDataParameter result = command.CreateParameter ();
			result.DbType = type;
			result.ParameterName = name;
			result.Value = value;
			return result;
		}

		private DateTime Tommorow ()
		{
			return DateTime.Now.AddDays (1);
		}

		private string GetId (HttpContext context)
		{
			if (!config.CookieLess &&
					context.Request.Cookies [CookieName] != null)
				return context.Request.Cookies [CookieName].Value;

			return null;
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

