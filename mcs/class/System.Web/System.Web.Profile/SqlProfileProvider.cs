//
// System.Web.UI.WebControls.SqlProfileProvider.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//  Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Globalization;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Security;
using System.Web.Util;

namespace System.Web.Profile
{
	public class SqlProfileProvider : ProfileProvider
	{
		ConnectionStringSettings connectionString;
		DbProviderFactory factory;

		string applicationName;
		bool schemaIsOk = false;

		public override int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_DeleteInactiveProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "ProfileAuthOptions", authenticationOption);
				AddParameter (command, "InactiveSinceDate", userInactiveSinceDate);
				DbParameter returnValue = AddParameter (command, null, ParameterDirection.ReturnValue, null);

				command.ExecuteNonQuery ();
				int retVal = GetReturnValue (returnValue);
				return retVal;
			}
		}

		public override int DeleteProfiles (ProfileInfoCollection profiles)
		{
			if (profiles == null)
				throw new ArgumentNullException ("prfoles");
			if (profiles.Count == 0)
				throw new ArgumentException ("prfoles");

			string [] usernames = new string [profiles.Count];

			int i = 0;
			foreach (ProfileInfo pi in profiles) {
				if (pi.UserName == null)
					throw new ArgumentNullException ("element in profiles collection is null");

				if (pi.UserName.Length == 0 || pi.UserName.Length > 256 || pi.UserName.IndexOf (',') != -1)
					throw new ArgumentException ("element in profiles collection in illegal format");

				usernames [i++] = pi.UserName;
			}

			return DeleteProfilesInternal (usernames);
		}

		public override int DeleteProfiles (string [] usernames)
		{
			if (usernames == null)
				throw new ArgumentNullException ("usernames");

			Hashtable users = new Hashtable ();
			foreach (string username in usernames) {
				if (username == null)
					throw new ArgumentNullException ("element in usernames array is null");

				if (username.Length == 0 || username.Length > 256 || username.IndexOf (',') != -1)
					throw new ArgumentException ("element in usernames array in illegal format");

				if (users.ContainsKey(username))
					throw new ArgumentException ("duplicate element in usernames array");

				users.Add (username, username);
			}

			return DeleteProfilesInternal (usernames);
		}

		int DeleteProfilesInternal (string [] usernames)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_DeleteProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "UserNames", string.Join (",", usernames));
				DbParameter returnValue = AddParameter (command, null, ParameterDirection.ReturnValue, null);

				command.ExecuteNonQuery ();
				int retVal = GetReturnValue (returnValue);
				return retVal;
			}
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption,
										      string usernameToMatch,
										      DateTime userInactiveSinceDate,
										      int pageIndex,
										      int pageSize,
										      out int totalRecords)
		{
			CheckParam ("usernameToMatch", usernameToMatch, 256);
			if (pageIndex < 0)
				throw new ArgumentException("pageIndex is less than zero");
			if (pageSize < 1)
				throw new ArgumentException ("pageIndex is less than one");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_GetProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "ProfileAuthOptions", authenticationOption);
				AddParameter (command, "PageIndex", pageIndex);
				AddParameter (command, "PageSize", pageSize);
				AddParameter (command, "UserNameToMatch", usernameToMatch);
				AddParameter (command, "InactiveSinceDate", userInactiveSinceDate);

				using (DbDataReader reader = command.ExecuteReader ()) {
					return BuildProfileInfoCollection (reader, out totalRecords);
				}
			}
		}

		public override ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption,
									      string usernameToMatch,
									      int pageIndex,
									      int pageSize,
									      out int totalRecords)
		{
			CheckParam ("usernameToMatch", usernameToMatch, 256);
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex is less than zero");
			if (pageSize < 1)
				throw new ArgumentException ("pageIndex is less than one");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_GetProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "ProfileAuthOptions", authenticationOption);
				AddParameter (command, "PageIndex", pageIndex);
				AddParameter (command, "PageSize", pageSize);
				AddParameter (command, "UserNameToMatch", usernameToMatch);
				AddParameter (command, "InactiveSinceDate", null);

				using (DbDataReader reader = command.ExecuteReader ()) {
					return BuildProfileInfoCollection (reader, out totalRecords);
				}
			}
		}

		public override ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption,
									      DateTime userInactiveSinceDate,
									      int pageIndex,
									      int pageSize,
									      out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex is less than zero");
			if (pageSize < 1)
				throw new ArgumentException ("pageIndex is less than one");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");
			
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_GetProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "ProfileAuthOptions", authenticationOption);
				AddParameter (command, "PageIndex", pageIndex);
				AddParameter (command, "PageSize", pageSize);
				AddParameter (command, "UserNameToMatch", null);
				AddParameter (command, "InactiveSinceDate", null);

				using (DbDataReader reader = command.ExecuteReader ()) {
					return BuildProfileInfoCollection (reader, out totalRecords);
				}
			}
		}

		public override ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption,
								      int pageIndex,
								      int pageSize,
								      out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex is less than zero");
			if (pageSize < 1)
				throw new ArgumentException ("pageIndex is less than one");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_GetProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "ProfileAuthOptions", authenticationOption);
				AddParameter (command, "PageIndex", pageIndex);
				AddParameter (command, "PageSize", pageSize);
				AddParameter (command, "UserNameToMatch", null);
				AddParameter (command, "InactiveSinceDate", null);

				using (DbDataReader reader = command.ExecuteReader ()) {
					return BuildProfileInfoCollection (reader, out totalRecords);
				}
			}
		}

		public override int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			using (DbConnection connection = CreateConnection ()) {

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_GetNumberOfInactiveProfiles";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "ProfileAuthOptions", authenticationOption);
				AddParameter (command, "InactiveSinceDate", userInactiveSinceDate);

				int returnValue = 0;
				using (DbDataReader reader = command.ExecuteReader ()) {
					if (reader.Read ())
						returnValue = reader.GetInt32 (0);
				}
				return returnValue;
			}
		}

		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext sc, SettingsPropertyCollection properties)
		{
			SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection ();

			if (properties.Count == 0)
				return settings;

			foreach (SettingsProperty property in properties) {
				if (property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
					if (property.PropertyType.IsPrimitive || property.PropertyType == typeof (String))
						property.SerializeAs = SettingsSerializeAs.String;
					else
						property.SerializeAs = SettingsSerializeAs.Xml;

				settings.Add (new SettingsPropertyValue (property));
			}

			string username = (string) sc ["UserName"];
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_GetProperties";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "UserName", username);
				AddParameter (command, "CurrentTimeUtc", DateTime.UtcNow);

				using (DbDataReader reader = command.ExecuteReader ()) {
					if (reader.Read ()) {
						string allnames = reader.GetString (0);
						string allvalues = reader.GetString (1);
						int binaryLen = (int) reader.GetBytes (2, 0, null, 0, 0);
						byte [] binaryvalues = new byte [binaryLen];
						reader.GetBytes (2, 0, binaryvalues, 0, binaryLen);

						DecodeProfileData (allnames, allvalues, binaryvalues, settings);
					}
				}
			}

			return settings;
		}
		
		public override void SetPropertyValues (SettingsContext sc, SettingsPropertyValueCollection properties)
		{
			string username = (string) sc ["UserName"];
			bool isAnonymous = !(bool) sc ["IsAuthenticated"];

			string names = String.Empty;
			string values = String.Empty;
			byte [] buf = null;

			EncodeProfileData (ref names, ref values, ref buf, properties, !isAnonymous);

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Profile_SetProperties";

				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "PropertyNames", names);
				AddParameter (command, "PropertyValuesString", values);
				AddParameter (command, "PropertyValuesBinary", buf);
				AddParameter (command, "UserName", username);
				AddParameter (command, "IsUserAnonymous", isAnonymous);
				AddParameter (command, "CurrentTimeUtc", DateTime.UtcNow);

				// Return value
				AddParameter (command, null, ParameterDirection.ReturnValue, null);

				command.ExecuteNonQuery ();
				return;
			}
		}

		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			base.Initialize (name, config);

			applicationName = GetStringConfigValue (config, "applicationName", "/");

			string connectionStringName = config ["connectionStringName"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			connectionString = WebConfigurationManager.ConnectionStrings [connectionStringName];
			factory = connectionString == null || String.IsNullOrEmpty (connectionString.ProviderName) ?
				System.Data.SqlClient.SqlClientFactory.Instance :
				ProvidersHelper.GetDbProviderFactory (connectionString.ProviderName);
		}

		public override string ApplicationName {
			get { return applicationName; }
			set { applicationName = value; }
		}

		DbConnection CreateConnection ()
		{
			if (!schemaIsOk && !(schemaIsOk = AspNetDBSchemaChecker.CheckMembershipSchemaVersion (factory, connectionString.ConnectionString, "profile", "1")))
				throw new ProviderException ("Incorrect ASP.NET DB Schema Version.");

			DbConnection connection = factory.CreateConnection ();
			connection.ConnectionString = connectionString.ConnectionString;

			connection.Open ();
			return connection;
		}

		DbParameter AddParameter (DbCommand command, string parameterName, object parameterValue)
		{
			return AddParameter (command, parameterName, ParameterDirection.Input, parameterValue);
		}

		DbParameter AddParameter (DbCommand command, string parameterName, ParameterDirection direction, object parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = direction;
			command.Parameters.Add (dbp);
			return dbp;
		}

		void CheckParam (string pName, string p, int length)
		{
			if (p == null)
				throw new ArgumentNullException (pName);
			if (p.Length == 0 || p.Length > length || p.IndexOf (',') != -1)
				throw new ArgumentException (String.Concat ("invalid format for ", pName));
		}
		
		static int GetReturnValue (DbParameter returnValue)
		{
			object value = returnValue.Value;
			return value is int ? (int) value : -1;
		}

		ProfileInfo ReadProfileInfo (DbDataReader reader)
		{
			ProfileInfo pi = null;
			try {
				string username = reader.GetString (0);
				bool anonymous = reader.GetBoolean (1);
				DateTime lastUpdate = reader.GetDateTime (2);
				DateTime lastActivity = reader.GetDateTime (3);
				int size = reader.GetInt32 (4);

				pi = new ProfileInfo (username, anonymous, lastActivity, lastUpdate, size);
			}
			catch {
			}

			return pi;
		}

		ProfileInfoCollection BuildProfileInfoCollection (DbDataReader reader, out int totalRecords)
		{
			ProfileInfoCollection pic = new ProfileInfoCollection ();
			while (reader.Read ()) {
				ProfileInfo pi = ReadProfileInfo (reader);
				if (pi != null)
					pic.Add (pi);
			}
			totalRecords = 0;
			if (reader.NextResult ()) {
				if (reader.Read ())
					totalRecords = reader.GetInt32 (0);
			}
			return pic;
		}

		string GetStringConfigValue (NameValueCollection config, string name, string def)
		{
			string retVal = def;
			string val = config [name];
			if (val != null)
				retVal = val;
			return retVal;
		}

		// Helper methods
		void DecodeProfileData (string allnames, string values, byte [] buf, SettingsPropertyValueCollection properties)
		{
			if (allnames == null || values == null || buf == null || properties == null)
				return;

			string [] names = allnames.Split (':');
			for (int i = 0; i < names.Length; i += 4) {
				string name = names [i];
				SettingsPropertyValue pp = properties [name];

				if (pp == null)
					continue;

				int pos = Int32.Parse (names [i + 2], Helpers.InvariantCulture);
				int len = Int32.Parse (names [i + 3], Helpers.InvariantCulture);

				if (len == -1 && !pp.Property.PropertyType.IsValueType) {
					pp.PropertyValue = null;
					pp.IsDirty = false;
					pp.Deserialized = true;
				}
				else if (names [i + 1] == "S" && pos >= 0 && len > 0 && values.Length >= pos + len) {
					pp.SerializedValue = values.Substring (pos, len);
				}
				else if (names [i + 1] == "B" && pos >= 0 && len > 0 && buf.Length >= pos + len) {
					byte [] buf2 = new byte [len];
					Buffer.BlockCopy (buf, pos, buf2, 0, len);
					pp.SerializedValue = buf2;
				}
			}
		}

		void EncodeProfileData (ref string allNames, ref string allValues, ref byte [] buf, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
		{
			StringBuilder names = new StringBuilder ();
			StringBuilder values = new StringBuilder ();
			MemoryStream stream = new MemoryStream ();

			try {
				foreach (SettingsPropertyValue pp in properties) {
					if (!userIsAuthenticated && !(bool) pp.Property.Attributes ["AllowAnonymous"])
						continue;

					if (!pp.IsDirty && pp.UsingDefaultValue)
						continue;

					int len = 0, pos = 0;
					string propValue = null;

					if (pp.Deserialized && pp.PropertyValue == null)
						len = -1;
					else {
						object sVal = pp.SerializedValue;

						if (sVal == null)
							len = -1;
						else if (sVal is string) {
							propValue = (string) sVal;
							len = propValue.Length;
							pos = values.Length;
						}
						else {
							byte [] b2 = (byte []) sVal;
							pos = (int) stream.Position;
							stream.Write (b2, 0, b2.Length);
							stream.Position = pos + b2.Length;
							len = b2.Length;
						}
					}

					names.Append (pp.Name + ":" + ((propValue != null) ? "S" : "B") + ":" + pos.ToString (Helpers.InvariantCulture) + ":" + len.ToString (Helpers.InvariantCulture) + ":");

					if (propValue != null)
						values.Append (propValue);
				}
				buf = stream.ToArray ();
			}
			finally {
				if (stream != null)
					stream.Close ();
			}

			allNames = names.ToString ();
			allValues = values.ToString ();
		}
	}
}

#endif
