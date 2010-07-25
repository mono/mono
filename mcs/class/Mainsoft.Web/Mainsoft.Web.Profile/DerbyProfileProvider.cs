//
// Mainsoft.Web.Profile.DerbyProfileProvider
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
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
using System.Data.OleDb;
using System.Data.Common;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Web.Profile;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Text;
using System.IO;

using Mainsoft.Web.Security;
using System.Configuration.Provider;

namespace Mainsoft.Web.Profile
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// <para>Manages storage of profile information for an ASP.NET application in a Derby database.</para>
	/// </summary>
	public class DerbyProfileProvider : ProfileProvider
	{
		ConnectionStringSettings _connectionString;
		string _applicationName = string.Empty;
		bool _schemaChecked = false;
		DerbyUnloadManager.DerbyShutDownPolicy _shutDownPolicy = DerbyUnloadManager.DerbyShutDownPolicy.Default;

		public DerbyProfileProvider ()
		{
		}

		public override string ApplicationName
		{
			get { return _applicationName; }
			set { _applicationName = value; }
		}

		DbConnection CreateConnection ()
		{
			if (!_schemaChecked) {
				DerbyDBSchema.CheckSchema (_connectionString.ConnectionString);
				_schemaChecked = true;

				DerbyUnloadManager.RegisterUnloadHandler (_connectionString.ConnectionString, _shutDownPolicy);
			}

			OleDbConnection connection = new OleDbConnection (_connectionString.ConnectionString);
			connection.Open ();
			return connection;
		}
		
		public override int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			using (DbConnection connection = CreateConnection ()) {
				return DerbyProfileHelper.Profile_DeleteInactiveProfiles (connection, ApplicationName, (int) authenticationOption, userInactiveSinceDate);
			}
		}

		public override int DeleteProfiles (ProfileInfoCollection profiles)
		{
			if (profiles == null)
				throw new ArgumentNullException ("profiles");
			if (profiles.Count == 0)
				throw new ArgumentException ("profiles");

			string [] usernames = new string [profiles.Count];

			int i = 0;
			foreach (ProfileInfo pi in profiles) {
				if (pi.UserName == null)
					throw new ArgumentNullException ("element in profiles collection is null");

				if (pi.UserName.Length == 0 || pi.UserName.Length > 256 || pi.UserName.IndexOf (",") != -1)
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

				if (username.Length == 0 || username.Length > 256 || username.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");

				if (users.ContainsKey (username))
					throw new ArgumentException ("duplicate element in usernames array");

				users.Add (username, username);
			}

			return DeleteProfilesInternal (usernames);
		}

		private int DeleteProfilesInternal (string[] usernames)
		{
			using (DbConnection connection = CreateConnection ()) {
				return DerbyProfileHelper.Profile_DeleteProfiles (connection, ApplicationName, usernames);
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
				throw new ArgumentException ("pageIndex is less than zero");
			if (pageSize < 1)
				throw new ArgumentException ("pageIndex is less than one");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			DbDataReader reader = null;
			using (DbConnection connection = CreateConnection ()) {
				totalRecords = DerbyProfileHelper.Profile_GetInactiveProfiles (connection, ApplicationName, (int) authenticationOption, pageIndex, pageSize, usernameToMatch, userInactiveSinceDate, out reader);

				using (reader) {
					return BuildProfileInfoCollection (reader, pageIndex, pageSize, out totalRecords);
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

			DbDataReader reader = null;
			using (DbConnection connection = CreateConnection ()) {
				totalRecords = DerbyProfileHelper.Profile_GetProfiles (connection, ApplicationName, (int) authenticationOption, pageIndex, pageSize, usernameToMatch, out reader);

				using (reader) {
					return BuildProfileInfoCollection (reader, pageIndex, pageSize, out totalRecords);
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

			DbDataReader reader = null;
			using (DbConnection connection = CreateConnection ()) {
				totalRecords = DerbyProfileHelper.Profile_GetInactiveProfiles (
					connection, ApplicationName, (int) authenticationOption, 
					pageIndex, pageSize, null, userInactiveSinceDate, out reader);

				using (reader) {
					return BuildProfileInfoCollection (reader, pageIndex, pageSize, out totalRecords);
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

			DbDataReader reader = null;
			using (DbConnection connection = CreateConnection ()) {
				totalRecords = DerbyProfileHelper.Profile_GetProfiles (
					connection, ApplicationName, (int) authenticationOption, 
					pageIndex, pageSize, null, out reader);

				using (reader) {
					return BuildProfileInfoCollection (reader, pageIndex, pageSize, out totalRecords);
				}
			}
		}

		public override int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			using (DbConnection connection = CreateConnection ()) {
				return DerbyProfileHelper.Profile_GetNumberOfInactiveProfiles (
					connection, ApplicationName, (int) authenticationOption, userInactiveSinceDate);
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

			DbDataReader reader;
			using (DbConnection connection = CreateConnection ()) {
				DerbyProfileHelper.Profile_GetProperties (connection, ApplicationName, username, DateTime.UtcNow, out reader);
				if (reader != null) {
					using (reader) {
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
			}
			return settings;
		}

		public override void SetPropertyValues (SettingsContext sc, SettingsPropertyValueCollection properties)
		{
			string username = (string) sc ["UserName"];
			bool authenticated = (bool) sc ["IsAuthenticated"];

			string names = String.Empty;
			string values = String.Empty;
			byte [] buf = null;

			EncodeProfileData (ref names, ref values, ref buf, properties, authenticated);

			using (DbConnection connection = CreateConnection ()) {
					DerbyProfileHelper.Profile_SetProperties (
					connection, _applicationName, names, values, 
					buf, username, authenticated, DateTime.UtcNow);
			}
		}

		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			if (string.IsNullOrEmpty (name))
				name = "DerbyProfileProvider";

			if (string.IsNullOrEmpty (config ["description"])) {
				config.Remove ("description");
				config.Add ("description", "Derby profile provider");
			}
			base.Initialize (name, config);

			_applicationName = GetStringConfigValue (config, "applicationName", "/");

			ProfileSection profileSection = (ProfileSection) WebConfigurationManager.GetSection ("system.web/profile");
			string connectionStringName = config ["connectionStringName"];
			_connectionString = WebConfigurationManager.ConnectionStrings [connectionStringName];
			if (_connectionString == null)
				throw new ProviderException (String.Format ("The connection name '{0}' was not found in the applications configuration or the connection string is empty.", connectionStringName));

			string shutdown = config ["shutdown"];
			if (!String.IsNullOrEmpty (shutdown))
				_shutDownPolicy = (DerbyUnloadManager.DerbyShutDownPolicy) Enum.Parse (typeof (DerbyUnloadManager.DerbyShutDownPolicy), shutdown, true);
		}

		private ProfileInfoCollection BuildProfileInfoCollection (DbDataReader reader, int pageIndex, int pageSize, out int totalRecords)
		{
			int num_read = 0;
			int num_added = 0;
			int num_to_skip = pageIndex * pageSize;
			ProfileInfoCollection pic = new ProfileInfoCollection ();

			while (reader.Read ()) {
				if (num_read >= num_to_skip && num_added < pageSize) {
					ProfileInfo pi = ReadProfileInfo (reader);
					if (pi != null) {
						pic.Add (pi);
						num_added++;
					}
				}
				num_read++;
			}
			totalRecords = num_read;
			return pic;
		}

		private ProfileInfo ReadProfileInfo (DbDataReader reader)
		{
			string username = reader.GetString (0);
			bool anonymous = reader.GetInt32 (1) > 0;
			DateTime lastUpdate = reader.GetDateTime (2);
			DateTime lastActivity = reader.GetDateTime (3);
			int size = reader.GetInt32 (4);

			return new ProfileInfo (username, anonymous, lastActivity, lastUpdate, size);
		}

		// Helper methods
		private void DecodeProfileData (string allnames, string values, byte [] buf, SettingsPropertyValueCollection properties)
		{
			if (allnames == null || values == null || buf == null || properties == null)
				return;

			string [] names = allnames.Split (':');
			for (int i = 0; i < names.Length; i += 4) {
				string name = names [i];
				SettingsPropertyValue pp = properties [name];

				if (pp == null)
					continue;

				int pos = Int32.Parse (names [i + 2], CultureInfo.InvariantCulture);
				int len = Int32.Parse (names [i + 3], CultureInfo.InvariantCulture);

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

		private void EncodeProfileData (ref string allNames, ref string allValues, ref byte [] buf, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
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

					names.Append (pp.Name + ":" + ((propValue != null) ? "S" : "B") + ":" + pos.ToString (CultureInfo.InvariantCulture) + ":" + len.ToString (CultureInfo.InvariantCulture) + ":");

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

		string GetStringConfigValue (NameValueCollection config, string name, string def)
		{
			string rv = def;
			string val = config [name];
			if (val != null)
				rv = val;
			return rv;
		}

		void CheckParam (string pName, string p, int length)
		{
			if (p == null)
				throw new ArgumentNullException (pName);
			if (p.Length == 0 || p.Length > length || p.IndexOf (",") != -1)
				throw new ArgumentException (String.Format ("invalid format for {0}", pName));
		}
		

	}
}

#endif
