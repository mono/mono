//
// System.Web.Compilation.SessionStateItemCollection
//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2009-2010 Novell, Inc (http://novell.com/)
//

// Code based on samples from MSDN
//
// Database schema found in ../ASPState.sql
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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace System.Web.SessionState 
{
	sealed class SessionSQLServerHandler : SessionStateStoreProviderBase
	{
		static readonly string defaultDbFactoryTypeName = "Mono.Data.Sqlite.SqliteFactory, Mono.Data.Sqlite, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
		
		SessionStateSection sessionConfig;
		string connectionString;
		Type providerFactoryType;
		DbProviderFactory providerFactory;
		int sqlCommandTimeout;
		
		DbProviderFactory ProviderFactory {
			get {
				if (providerFactory == null) {
					try {
						providerFactory = Activator.CreateInstance (providerFactoryType) as DbProviderFactory;
					} catch (Exception ex) {
						throw new ProviderException ("Failure to create database factory instance.", ex);
					}
				}

				return providerFactory;
			}
		}
		
		public string ApplicationName {
			get; private set;
		}
		
		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			if (String.IsNullOrEmpty (name))
				name = "SessionSQLServerHandler";

			if (String.IsNullOrEmpty (config["description"])) {
				config.Remove ("description");
				config.Add ("description", "Mono SQL Session Store Provider");
			}
			ApplicationName = HostingEnvironment.ApplicationVirtualPath;
			
			base.Initialize(name, config);
			sessionConfig = WebConfigurationManager.GetWebApplicationSection ("system.web/sessionState") as SessionStateSection;
			connectionString = sessionConfig.SqlConnectionString;
			string dbProviderName;
			
			if (String.IsNullOrEmpty (connectionString) || String.Compare (connectionString, SessionStateSection.DefaultSqlConnectionString, StringComparison.Ordinal) == 0) {
				connectionString = "Data Source=|DataDirectory|/ASPState.sqlite;Version=3";
				dbProviderName = defaultDbFactoryTypeName;
			} else {
				string[] parts = connectionString.Split (';');
				var newCS = new List <string> ();
				dbProviderName = null;
				bool allowDb = sessionConfig.AllowCustomSqlDatabase;
				
				foreach (string p in parts) {
					if (p.Trim ().Length == 0)
						continue;
					
					if (p.StartsWith ("DbProviderName", StringComparison.OrdinalIgnoreCase)) {
						int idx = p.IndexOf ('=');
						if (idx < 0)
							throw new ProviderException ("Invalid format for the 'DbProviderName' connection string parameter. Expected 'DbProviderName = value'.");

						dbProviderName = p.Substring (idx + 1);
						continue;
					}

					if (!allowDb) {
						string tmp = p.Trim ();
						if (tmp.StartsWith ("database", StringComparison.OrdinalIgnoreCase) ||
						    tmp.StartsWith ("initial catalog", StringComparison.OrdinalIgnoreCase))
							throw new ProviderException ("Specifying a custom database is not allowed. Set the allowCustomSqlDatabase attribute of the <system.web/sessionState> section to 'true' in order to use a custom database name.");
					}
					
					newCS.Add (p);
				}

				connectionString = String.Join (";", newCS.ToArray ());
				if (String.IsNullOrEmpty (dbProviderName))
					dbProviderName = defaultDbFactoryTypeName;

				
			}

			Exception typeException = null;
			
			try {	
				providerFactoryType = Type.GetType (dbProviderName, true);
			} catch (Exception ex) {
				typeException = ex;
				providerFactoryType = null;
			}

			if (providerFactoryType == null)
				throw new ProviderException ("Unable to find database provider factory type.", typeException);

			sqlCommandTimeout = (int)sessionConfig.SqlCommandTimeout.TotalSeconds;
		}

		public override void Dispose ()
		{
		}

		public override bool SetItemExpireCallback (SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}

		public override void SetAndReleaseItemExclusive (HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)                                           
		{
			DbCommand cmd;
			DbCommand deleteCmd = null;
			string sessItems = Serialize((SessionStateItemCollection)item.Items);
			DbProviderFactory factory = ProviderFactory;
			string appName = ApplicationName;
			DbConnection conn = CreateConnection (factory);
			DateTime now = DateTime.Now;			
			DbParameterCollection parameters;
			
			if (newItem) {	
				deleteCmd = CreateCommand (factory, conn, "DELETE FROM Sessions WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND Expires < @Expires");
				parameters = deleteCmd.Parameters;				

				parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
				parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));
				parameters.Add (CreateParameter <DateTime> (factory, "@Expires", now));

				cmd = CreateCommand (factory, conn, "INSERT INTO Sessions (SessionId, ApplicationName, Created, Expires, LockDate, LockId, Timeout, Locked, SessionItems, Flags) Values (@SessionId, @ApplicationName, @Created, @Expires, @LockDate, @LockId , @Timeout, @Locked, @SessionItems, @Flags)");
				parameters = cmd.Parameters;
				
				parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
				parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));
				parameters.Add (CreateParameter <DateTime> (factory, "@Created", now));
				parameters.Add (CreateParameter <DateTime> (factory, "@Expires", now.AddMinutes ((double)item.Timeout)));
				parameters.Add (CreateParameter <DateTime> (factory, "@LockDate", now));
				parameters.Add (CreateParameter <int> (factory, "@LockId", 0));
				parameters.Add (CreateParameter <int> (factory, "@Timeout", item.Timeout));
				parameters.Add (CreateParameter <bool> (factory, "@Locked", false));
				parameters.Add (CreateParameter <string> (factory, "@SessionItems", sessItems));
				parameters.Add (CreateParameter <int> (factory, "@Flags", 0));
			} else {
				cmd = CreateCommand (factory, conn, "UPDATE Sessions SET Expires = @Expires, SessionItems = @SessionItems, Locked = @Locked WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND LockId = @LockId");
				parameters = cmd.Parameters;

				parameters.Add (CreateParameter <DateTime> (factory, "@Expires", now.AddMinutes ((double)item.Timeout)));
				parameters.Add (CreateParameter <string> (factory, "@SessionItems", sessItems));
				parameters.Add (CreateParameter <bool> (factory, "@Locked", false));
				parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
				parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));
				parameters.Add (CreateParameter <int> (factory, "@Lockid", (int)lockId));
			}

			try
			{
				conn.Open();
				if (deleteCmd != null)
					deleteCmd.ExecuteNonQuery();

				cmd.ExecuteNonQuery();
			} catch (Exception ex) {
				throw new ProviderException ("Failure storing session item in database.", ex);
			} finally {
				conn.Close();
			}
		}

		public override SessionStateStoreData GetItem (HttpContext context, string id, out bool locked, out TimeSpan lockAge,
							       out object lockId, out SessionStateActions actionFlags)
		{
			return GetSessionStoreItem (false, context, id, out locked, out lockAge, out lockId, out actionFlags);
		}

		public override SessionStateStoreData GetItemExclusive (HttpContext context, string id, out bool locked,out TimeSpan lockAge,
								       out object lockId, out SessionStateActions actionFlags)
		{
			return GetSessionStoreItem (true, context, id, out locked, out lockAge, out lockId, out actionFlags);
		}

		private SessionStateStoreData GetSessionStoreItem (bool lockRecord, HttpContext context, string id, out bool locked,
								   out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{
			SessionStateStoreData item = null;
			lockAge = TimeSpan.Zero;
			lockId = null;
			locked = false;
			actionFlags = 0;

			DbProviderFactory factory = ProviderFactory;
			DbConnection conn = CreateConnection (factory);
			string appName = ApplicationName;
			DbCommand cmd = null;
			DbDataReader reader = null;
			DbParameterCollection parameters;
			DateTime expires;
			string serializedItems = String.Empty;
			bool foundRecord = false;
			bool deleteData = false;
			int timeout = 0;
			DateTime now = DateTime.Now;

			try {
				conn.Open();
				if (lockRecord) {
					cmd = CreateCommand (factory, conn, "UPDATE Sessions SET Locked = @Locked, LockDate = @LockDate WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND Expires > @Expires");
					parameters = cmd.Parameters;
					
					parameters.Add (CreateParameter <bool> (factory, "@Locked", true));
					parameters.Add (CreateParameter <DateTime> (factory, "@LockDate", now));
					parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
					parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));
					parameters.Add (CreateParameter <DateTime> (factory, "@Expires", now));

					if (cmd.ExecuteNonQuery() == 0)
						locked = true;             
					else
						locked = false;
				}

				cmd = CreateCommand (factory, conn, "SELECT Expires, SessionItems, LockId, LockDate, Flags, Timeout FROM Sessions WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName");
				parameters = cmd.Parameters;

				parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
				parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));
				
				reader = cmd.ExecuteReader (CommandBehavior.SingleRow);
				while (reader.Read()) {
					expires = reader.GetDateTime (reader.GetOrdinal ("Expires"));

					if (expires < now) {
						locked = false;
						deleteData = true;
					} else
						foundRecord = true;

					serializedItems = reader.GetString (reader.GetOrdinal ("SessionItems"));
					lockId = reader.GetInt32 (reader.GetOrdinal ("LockId"));
					lockAge = now.Subtract (reader.GetDateTime (reader.GetOrdinal ("LockDate")));
					actionFlags = (SessionStateActions) reader.GetInt32 (reader.GetOrdinal ("Flags"));
					timeout = reader.GetInt32 (reader.GetOrdinal ("Timeout"));
				}
				reader.Close();

				if (deleteData) {
					cmd = CreateCommand (factory, conn, "DELETE FROM Sessions WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName");
					parameters = cmd.Parameters;

					parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
					parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));

					cmd.ExecuteNonQuery();
				}

				if (!foundRecord)
					locked = false;

				if (foundRecord && !locked) {
					lockId = (int)lockId + 1;

					cmd = CreateCommand (factory, conn, "UPDATE Sessions SET LockId = @LockId, Flags = 0 WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName");
					parameters = cmd.Parameters;

					parameters.Add (CreateParameter <int> (factory, "@LockId", (int)lockId));
					parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
					parameters.Add (CreateParameter <string> (factory, "@ApplicationName", appName, 255));
					
					cmd.ExecuteNonQuery();

					if (actionFlags == SessionStateActions.InitializeItem)
						item = CreateNewStoreData (context, (int)sessionConfig.Timeout.TotalMinutes);
					else
						item = Deserialize (context, serializedItems, timeout);
				}
			} catch (Exception ex) {
				throw new ProviderException ("Unable to retrieve session item from database.", ex);
			} finally {
				if (reader != null)
					reader.Close ();
				
				conn.Close();
			} 

			return item;
		}

		string Serialize (SessionStateItemCollection items)
		{
#if NET_4_0
			GZipStream gzip = null;
#endif
			Stream output;
			MemoryStream ms = null;
			BinaryWriter writer = null;
			
			try {
				ms = new MemoryStream ();
#if NET_4_0
				if (sessionConfig.CompressionEnabled)
					output = gzip = new GZipStream (ms, CompressionMode.Compress, true);
				else
#endif
					output = ms;
				writer = new BinaryWriter (output);

				if (items != null)
					items.Serialize (writer);
#if NET_4_0
				if (gzip != null)
					gzip.Close ();
#endif
				writer.Close ();
				return Convert.ToBase64String (ms.ToArray ());
			} finally {
#if NET_4_0
				if (writer != null)
					writer.Dispose ();
				if (gzip != null)
					gzip.Dispose ();
#else
				if (writer != null)
					((IDisposable)writer).Dispose ();
#endif
				if (ms != null)
					ms.Dispose ();
			}
		}

		SessionStateStoreData Deserialize (HttpContext context, string serializedItems, int timeout)
		{
			MemoryStream ms = null;
			Stream input;
			BinaryReader reader = null;
#if NET_4_0
			GZipStream gzip = null;
#endif
			try {
				ms = new MemoryStream (Convert.FromBase64String (serializedItems));
				var sessionItems = new SessionStateItemCollection ();

				if (ms.Length > 0) {
#if NET_4_0
					if (sessionConfig.CompressionEnabled)
						input = gzip = new GZipStream (ms, CompressionMode.Decompress, true);
					else
#endif
						input = ms;
					
					reader = new BinaryReader (input);
					sessionItems = SessionStateItemCollection.Deserialize (reader);
#if NET_4_0
					if (gzip != null)
						gzip.Close ();
#endif
					reader.Close ();
				}

				return new SessionStateStoreData (sessionItems, SessionStateUtility.GetSessionStaticObjects (context), timeout);
			} finally {
#if NET_4_0
				if (reader != null)
					reader.Dispose ();
				if (gzip != null)
					gzip.Dispose ();
#else
				if (reader != null)
					((IDisposable)reader).Dispose ();
#endif
				if (ms != null)
					ms.Dispose ();
			}
		}

		public override void ReleaseItemExclusive (HttpContext context, string id, object lockId)
		{
			DbProviderFactory factory = ProviderFactory;
			DbConnection conn = CreateConnection (factory);
			DbCommand cmd = CreateCommand (factory, conn,
						       "UPDATE Sessions SET Locked = 0, Expires = @Expires WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND LockId = @LockId");

			DbParameterCollection parameters = cmd.Parameters;
			
			parameters.Add (CreateParameter <DateTime> (factory, "@Expires", DateTime.Now.AddMinutes(sessionConfig.Timeout.TotalMinutes)));
			parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
			parameters.Add (CreateParameter <string> (factory, "@ApplicationName", ApplicationName, 255));
			parameters.Add (CreateParameter <int> (factory, "@LockId", (int)lockId));

			try {
				conn.Open ();
				cmd.ExecuteNonQuery ();
			} catch (Exception ex) {
				throw new ProviderException ("Error releasing item in database.", ex);
			} finally {
				conn.Close();
			}      
		}

		public override void RemoveItem (HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			DbProviderFactory factory = ProviderFactory;
			DbConnection conn = CreateConnection (factory);
			DbCommand cmd = CreateCommand (factory, conn,
						       "DELETE FROM Sessions WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName AND LockId = @LockId");

			DbParameterCollection parameters = cmd.Parameters;
			parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
			parameters.Add (CreateParameter <string> (factory, "@ApplicationName", ApplicationName, 255));
			parameters.Add (CreateParameter <int> (factory, "@LockId", (int)lockId));

			try {
				conn.Open ();
				cmd.ExecuteNonQuery ();
			} catch (Exception ex) {
				throw new ProviderException ("Error removing item from database.", ex);
			} finally {
				conn.Close();
			} 
		}

		public override void CreateUninitializedItem (HttpContext context, string id, int timeout)
		{
			DbProviderFactory factory = ProviderFactory;
			DbConnection conn = CreateConnection (factory);
			DbCommand cmd = CreateCommand (factory, conn,
						       "INSERT INTO Sessions (SessionId, ApplicationName, Created, Expires, LockDate, LockId, Timeout, Locked, SessionItems, Flags) Values (@SessionId, @ApplicationName, @Created, @Expires, @LockDate, @LockId , @Timeout, @Locked, @SessionItems, @Flags)");

			DateTime now = DateTime.Now;
			DbParameterCollection parameters = cmd.Parameters;
			parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
			parameters.Add (CreateParameter <string> (factory, "@ApplicationName", ApplicationName, 255));
			parameters.Add (CreateParameter <DateTime> (factory, "@Created", now));
			parameters.Add (CreateParameter <DateTime> (factory, "@Expires", now.AddMinutes ((double)timeout)));
			parameters.Add (CreateParameter <DateTime> (factory, "@LockDate", now));
			parameters.Add (CreateParameter <int> (factory, "@LockId", 0));
			parameters.Add (CreateParameter <int> (factory, "@Timeout", timeout));
			parameters.Add (CreateParameter <bool> (factory, "@Locked", false));
			parameters.Add (CreateParameter <string> (factory, "@SessionItems", String.Empty));
			parameters.Add (CreateParameter <int> (factory, "@Flags", 1));
				
			try {
				conn.Open ();
				cmd.ExecuteNonQuery ();
			} catch (Exception ex) {
				throw new ProviderException ("Error creating uninitialized session item in the database.", ex);
			} finally {
				conn.Close();
			}
		}

		public override SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout)
		{
			return new SessionStateStoreData (new SessionStateItemCollection (), SessionStateUtility.GetSessionStaticObjects (context), timeout);
		}

		public override void ResetItemTimeout (HttpContext context, string id)
		{
			DbProviderFactory factory = ProviderFactory;
			DbConnection conn = CreateConnection (factory);
			DbCommand cmd = CreateCommand (factory, conn,
						       "UPDATE Sessions SET Expires = @Expires WHERE SessionId = @SessionId AND ApplicationName = @ApplicationName");

			DbParameterCollection parameters = cmd.Parameters;
			parameters.Add (CreateParameter <DateTime> (factory, "@Expires", DateTime.Now.AddMinutes (sessionConfig.Timeout.TotalMinutes)));
			parameters.Add (CreateParameter <string> (factory, "@SessionId", id, 80));
			parameters.Add (CreateParameter <string> (factory, "@ApplicationName", ApplicationName, 255));

			try {
				conn.Open ();
				cmd.ExecuteNonQuery ();
			} catch (Exception ex) {
				throw new ProviderException ("Error resetting session item timeout in the database.", ex);
			} finally {
				conn.Close();
			}
		}

		public override void InitializeRequest (HttpContext context)
		{
		}

		public override void EndRequest(HttpContext context)
		{
		}

		DbConnection CreateConnection (DbProviderFactory factory)
		{
			DbConnection conn = factory.CreateConnection ();
			conn.ConnectionString = connectionString;

			return conn;
		}

		DbCommand CreateCommand (DbProviderFactory factory, DbConnection conn, string commandText)
		{
			DbCommand cmd = factory.CreateCommand ();
			cmd.CommandTimeout = sqlCommandTimeout;
			cmd.Connection = conn;
			cmd.CommandText = commandText;

			return cmd;
		}
		
		DbParameter CreateParameter <ValueType> (DbProviderFactory factory, string name, ValueType value)
		{
			return CreateParameter <ValueType> (factory, name, value, -1);
		}
		
		DbParameter CreateParameter <ValueType> (DbProviderFactory factory, string name, ValueType value, int size)
		{
			DbParameter param = factory.CreateParameter ();
			param.ParameterName = name;
			Type vt = typeof (ValueType);
			
			if (vt == typeof (string))
				param.DbType = DbType.String;
			else if (vt == typeof (int))
				param.DbType = DbType.Int32;
			else if (vt == typeof (bool))
				param.DbType = DbType.Boolean;
			else if (vt == typeof (DateTime))
				param.DbType = DbType.DateTime;

			if (size > -1)
				param.Size = size;
			
			param.Value = value;

			return param;
		}
		
	}
}
#endif
