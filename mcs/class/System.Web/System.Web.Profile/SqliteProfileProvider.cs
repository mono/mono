//
// $Id: PgProfileProvider.cs 36 2007-11-24 09:44:42Z dna $
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
// Copyright © 2006, 2007 Nauck IT KG		http://www.nauck-it.de
//
// Author:
//	Daniel Nauck		<d.nauck(at)nauck-it.de>
//
// Adapted to Sqlite by Marek Habersack <mhabersack@novell.com>
//

#if NET_2_0
using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Web.Hosting;
using System.Web.Util;

using Mono.Data.Sqlite;

namespace System.Web.Profile
{
	internal class SqliteProfileProvider : ProfileProvider
	{
		const string m_ProfilesTableName = "Profiles";
		const string m_ProfileDataTableName = "ProfileData";
		string m_ConnectionString = string.Empty;

		SerializationHelper m_serializationHelper = new SerializationHelper();

		DbParameter AddParameter (DbCommand command, string parameterName)
                {
                        return AddParameter (command, parameterName, null);
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
		
		/// <summary>
		/// System.Configuration.Provider.ProviderBase.Initialize Method
		/// </summary>
		public override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("Config", Properties.Resources.ErrArgumentNull);

			if (string.IsNullOrEmpty(name))
				name = Properties.Resources.ProfileProviderDefaultName;

			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", Properties.Resources.ProfileProviderDefaultDescription);
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			m_ApplicationName = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

			// Get connection string.
			string connStrName = config["connectionStringName"];

			if (string.IsNullOrEmpty(connStrName))
			{
				throw new ArgumentOutOfRangeException("ConnectionStringName", Properties.Resources.ErrArgumentNullOrEmpty);
			}
			else
			{
				ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connStrName];

				if (ConnectionStringSettings == null || string.IsNullOrEmpty(ConnectionStringSettings.ConnectionString.Trim()))
				{
					throw new ProviderException(Properties.Resources.ErrConnectionStringNullOrEmpty);
				}

				m_ConnectionString = ConnectionStringSettings.ConnectionString;
			}
		}

		/// <summary>
		/// System.Web.Profile.ProfileProvider properties.
		/// </summary>
		#region System.Web.Security.ProfileProvider properties
		string m_ApplicationName = string.Empty;

		public override string ApplicationName
		{
			get { return m_ApplicationName; }
			set { m_ApplicationName = value; }
		}
		#endregion

		/// <summary>
		/// System.Web.Profile.ProfileProvider methods.
		/// </summary>
		#region System.Web.Security.ProfileProvider methods

		/// <summary>
		/// ProfileProvider.DeleteInactiveProfiles
		/// </summary>
		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new Exception("DeleteInactiveProfiles: The method or operation is not implemented.");
		}

		public override int DeleteProfiles(string[] usernames)
		{
			throw new Exception("DeleteProfiles1: The method or operation is not implemented.");
		}

		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			throw new Exception("DeleteProfiles2: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception("FindInactiveProfilesByUserName: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception("FindProfilesByUserName: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception("GetAllInactiveProfiles: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception("GetAllProfiles: The method or operation is not implemented.");
		}

		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new Exception("GetNumberOfInactiveProfiles: The method or operation is not implemented.");
		}
		#endregion

		/// <summary>
		/// System.Configuration.SettingsProvider methods.
		/// </summary>
		#region System.Web.Security.SettingsProvider methods

		/// <summary>
		/// 
		/// </summary>
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			SettingsPropertyValueCollection result = new SettingsPropertyValueCollection();
			string username = (string)context["UserName"];
			bool isAuthenticated = (bool)context["IsAuthenticated"];
			Dictionary<string, object> databaseResult = new Dictionary<string, object>();

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT \"Name\", \"ValueString\", \"ValueBinary\" FROM \"{0}\" WHERE \"Profile\" = (SELECT \"pId\" FROM \"{1}\" WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName AND \"IsAnonymous\" = @IsAuthenticated)", m_ProfileDataTableName, m_ProfilesTableName);

					AddParameter (dbCommand, "@Username", username);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);
					AddParameter (dbCommand, "@IsAuthenticated", !isAuthenticated);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (SqliteDataReader reader = dbCommand.ExecuteReader())
						{
							while (reader.Read())
							{
								object resultData = null;
								if(!reader.IsDBNull(1))
									resultData = reader.GetValue(1);
								else if(!reader.IsDBNull(2))
									resultData = reader.GetValue(2);

								databaseResult.Add(reader.GetString(0), resultData);
							}
						}
					}
					catch (SqliteException e)
					{
						Trace.WriteLine(e.ToString());
						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbConn != null)
							dbConn.Close();
					}
				}
			}

			foreach (SettingsProperty item in collection)
			{
				if (item.SerializeAs == SettingsSerializeAs.ProviderSpecific)
				{
					if (item.PropertyType.IsPrimitive || item.PropertyType.Equals(typeof(string)))
						item.SerializeAs = SettingsSerializeAs.String;
					else
						item.SerializeAs = SettingsSerializeAs.Xml;
				}

				SettingsPropertyValue itemValue = new SettingsPropertyValue(item);

				if ((databaseResult.ContainsKey(item.Name)) && (databaseResult[item.Name] != null))
				{
					if(item.SerializeAs == SettingsSerializeAs.String)
						itemValue.PropertyValue = m_serializationHelper.DeserializeFromBase64((string)databaseResult[item.Name]);
					
					else if (item.SerializeAs == SettingsSerializeAs.Xml)
						itemValue.PropertyValue = m_serializationHelper.DeserializeFromXml((string)databaseResult[item.Name]);

					else if (item.SerializeAs == SettingsSerializeAs.Binary)
						itemValue.PropertyValue = m_serializationHelper.DeserializeFromBinary((byte[])databaseResult[item.Name]);
				}
				itemValue.IsDirty = false;				
				result.Add(itemValue);
			}

			UpdateActivityDates(username, isAuthenticated, true);

			return result;
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			string username = (string)context["UserName"];
			bool isAuthenticated = (bool)context["IsAuthenticated"];

			if (collection.Count < 1)
				return;

			if (!ProfileExists(username))
				CreateProfileForUser(username, isAuthenticated);

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand deleteCommand = dbConn.CreateCommand(),
					insertCommand = dbConn.CreateCommand())
				{
					deleteCommand.CommandText = string.Format("DELETE FROM \"{0}\" WHERE \"Name\" = @Name AND \"Profile\" = (SELECT \"pId\" FROM \"{1}\" WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName AND \"IsAnonymous\" = @IsAuthenticated)", m_ProfileDataTableName, m_ProfilesTableName);

					AddParameter (deleteCommand, "@Name");
					AddParameter (deleteCommand, "@Username", username);
					AddParameter (deleteCommand, "@ApplicationName", m_ApplicationName);
					AddParameter (deleteCommand, "@IsAuthenticated", !isAuthenticated);


					insertCommand.CommandText = string.Format("INSERT INTO \"{0}\" (\"pId\", \"Profile\", \"Name\", \"ValueString\", \"ValueBinary\") VALUES (@pId, (SELECT \"pId\" FROM \"{1}\" WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName AND \"IsAnonymous\" = @IsAuthenticated), @Name, @ValueString, @ValueBinary)", m_ProfileDataTableName, m_ProfilesTableName);

					AddParameter (insertCommand, "@pId");
					AddParameter (insertCommand, "@Name");
					AddParameter (insertCommand, "@ValueString");
					insertCommand.Parameters["@ValueString"].IsNullable = true;
					AddParameter (insertCommand, "@ValueBinary");
					insertCommand.Parameters["@ValueBinary"].IsNullable = true;
					AddParameter (insertCommand, "@Username", username);
					AddParameter (insertCommand, "@ApplicationName", m_ApplicationName);
					AddParameter (insertCommand, "@IsAuthenticated", !isAuthenticated);

					SqliteTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						deleteCommand.Prepare();
						insertCommand.Prepare();

						using (dbTrans = dbConn.BeginTransaction())
						{

							foreach (SettingsPropertyValue item in collection)
							{
								if (!item.IsDirty)
									continue;

								deleteCommand.Parameters["@Name"].Value = item.Name;

								insertCommand.Parameters["@pId"].Value = Guid.NewGuid().ToString();
								insertCommand.Parameters["@Name"].Value = item.Name;

								if (item.Property.SerializeAs == SettingsSerializeAs.String)
								{
									insertCommand.Parameters["@ValueString"].Value = m_serializationHelper.SerializeToBase64(item.PropertyValue);
									insertCommand.Parameters["@ValueBinary"].Value = DBNull.Value; //new byte[0];//DBNull.Value;
								}
								else if (item.Property.SerializeAs == SettingsSerializeAs.Xml)
								{
									item.SerializedValue = m_serializationHelper.SerializeToXml(item.PropertyValue);
									insertCommand.Parameters["@ValueString"].Value = item.SerializedValue;
									insertCommand.Parameters["@ValueBinary"].Value = DBNull.Value; //new byte[0];//DBNull.Value;
								}
								else if (item.Property.SerializeAs == SettingsSerializeAs.Binary)
								{
									item.SerializedValue = m_serializationHelper.SerializeToBinary(item.PropertyValue);
									insertCommand.Parameters["@ValueString"].Value = DBNull.Value; //string.Empty;//DBNull.Value;
									insertCommand.Parameters["@ValueBinary"].Value = item.SerializedValue;
								}

								deleteCommand.ExecuteNonQuery();
								insertCommand.ExecuteNonQuery();
							}

							UpdateActivityDates(username, isAuthenticated, false);

							// Attempt to commit the transaction
							dbTrans.Commit();
						}
					}
					catch (SqliteException e)
					{
						Trace.WriteLine(e.ToString());

						try
						{
							// Attempt to roll back the transaction
							Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
							dbTrans.Rollback();
						}
						catch (SqliteException re)
						{
							// Rollback failed
							Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
							Trace.WriteLine(re.ToString());
						}

						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}
		#endregion

		#region private methods
		/// <summary>
		/// Create a empty user profile
		/// </summary>
		/// <param name="username"></param>
		/// <param name="isAuthenticated"></param>
		void CreateProfileForUser(string username, bool isAuthenticated)
		{
			if (ProfileExists(username))
			{
				throw new ProviderException(string.Format(Properties.Resources.ErrProfileAlreadyExist, username));
			}

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("INSERT INTO \"{0}\" (\"pId\", \"Username\", \"ApplicationName\", \"IsAnonymous\", \"LastActivityDate\", \"LastUpdatedDate\") Values (@pId, @Username, @ApplicationName, @IsAuthenticated, @LastActivityDate, @LastUpdatedDate)", m_ProfilesTableName);

					AddParameter (dbCommand, "@pId", Guid.NewGuid().ToString());
					AddParameter (dbCommand, "@Username", username);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);
					AddParameter (dbCommand, "@IsAuthenticated", !isAuthenticated);
					AddParameter (dbCommand, "@LastActivityDate", DateTime.Now);
					AddParameter (dbCommand, "@LastUpdatedDate", DateTime.Now);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						dbCommand.ExecuteNonQuery();
					}
					catch (SqliteException e)
					{
						Trace.WriteLine(e.ToString());
						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}


		bool ProfileExists(string username)
		{
			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT COUNT(*) FROM \"{0}\" WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName", m_ProfilesTableName);

					AddParameter (dbCommand, "@Username", username);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						int numRecs = 0;
						Int32.TryParse(dbCommand.ExecuteScalar().ToString(), out numRecs);

						if (numRecs > 0)
							return true;
					}
					catch (SqliteException e)
					{
						Trace.WriteLine(e.ToString());
						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbConn != null)
							dbConn.Close();
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Updates the LastActivityDate and LastUpdatedDate values when profile properties are accessed by the
		/// GetPropertyValues and SetPropertyValues methods.
		/// Passing true as the activityOnly parameter will update only the LastActivityDate.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="isAuthenticated"></param>
		/// <param name="activityOnly"></param>
		void UpdateActivityDates(string username, bool isAuthenticated, bool activityOnly)
		{
			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					if (activityOnly)
					{
						dbCommand.CommandText = string.Format("UPDATE \"{0}\" SET \"LastActivityDate\" = @LastActivityDate WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName AND \"IsAnonymous\" = @IsAuthenticated", m_ProfilesTableName);

						AddParameter (dbCommand, "@LastActivityDate", DateTime.Now);
					}
					else
					{
						dbCommand.CommandText = string.Format("UPDATE \"{0}\" SET \"LastActivityDate\" = @LastActivityDate, \"LastUpdatedDate\" = @LastUpdatedDate WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName AND \"IsAnonymous\" = @IsAuthenticated", m_ProfilesTableName);

						AddParameter (dbCommand, "@LastActivityDate", DateTime.Now);
						AddParameter (dbCommand, "@LastUpdatedDate", DateTime.Now);
					}
					
					AddParameter (dbCommand, "@Username", username);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);
					AddParameter (dbCommand, "@IsAuthenticated", !isAuthenticated);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						dbCommand.ExecuteNonQuery();
					}
					catch (SqliteException e)
					{
						Trace.WriteLine(e.ToString());
						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}

		/// <summary>
		/// A helper function to retrieve config values from the configuration file.
		/// </summary>
		/// <param name="configValue"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		string GetConfigValue(string configValue, string defaultValue)
		{
			if (string.IsNullOrEmpty(configValue))
				return defaultValue;

			return configValue;
		}
		#endregion
	}
}
#endif
