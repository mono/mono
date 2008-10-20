//
// $Id: PgRoleProvider.cs 12 2007-10-17 17:22:43Z dna $
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Hosting;
using System.Web.Security;

using Mono.Data.Sqlite;

namespace System.Web.Security
{
	internal class SqliteRoleProvider : RoleProvider
	{
		const string m_RolesTableName = "Roles";
		const string m_UserInRolesTableName = "UsersInRoles";
		string m_ConnectionString = string.Empty;

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
				name = Properties.Resources.RoleProviderDefaultName;

			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", Properties.Resources.RoleProviderDefaultDescription);
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
		/// System.Web.Security.RoleProvider properties.
		/// </summary>
		#region System.Web.Security.RoleProvider properties
		string m_ApplicationName = string.Empty;

		public override string ApplicationName
		{
			get { return m_ApplicationName; }
			set { m_ApplicationName = value; }
		}
		#endregion

		/// <summary>
		/// System.Web.Security.RoleProvider methods.
		/// </summary>
		#region System.Web.Security.RoleProvider methods

		/// <summary>
		/// RoleProvider.AddUsersToRoles
		/// </summary>
		public override void AddUsersToRoles(string[] userNames, string[] roleNames)
		{
			foreach (string rolename in roleNames)
			{
				if (!RoleExists(rolename))
				{
					throw new ProviderException(string.Format(Properties.Resources.ErrRoleNotExist, rolename));
				}
			}

			foreach (string username in userNames)
			{
				foreach (string rolename in roleNames)
				{
					if (IsUserInRole(username, rolename))
					{
						throw new ProviderException(string.Format(Properties.Resources.ErrUserAlreadyInRole, username, rolename));
					}
				}
			}

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("INSERT INTO \"{0}\" (\"Username\", \"Rolename\", \"ApplicationName\") Values (@Username, @Rolename, @ApplicationName)", m_UserInRolesTableName);

					AddParameter (dbCommand, "@Username");
					AddParameter (dbCommand, "@Rolename");
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					SqliteTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (dbTrans = dbConn.BeginTransaction())
						{
							foreach (string username in userNames)
							{
								foreach (string rolename in roleNames)
								{
									dbCommand.Parameters["@Username"].Value = username;
									dbCommand.Parameters["@Rolename"].Value = rolename;
									dbCommand.ExecuteNonQuery();
								}
							}
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

		/// <summary>
		/// RoleProvider.CreateRole
		/// </summary>
		public override void CreateRole(string roleName)
		{
			if (RoleExists(roleName))
			{
				throw new ProviderException(string.Format(Properties.Resources.ErrRoleAlreadyExist, roleName));
			}

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("INSERT INTO \"{0}\" (\"Rolename\", \"ApplicationName\") Values (@Rolename, @ApplicationName)", m_RolesTableName);

					AddParameter (dbCommand, "@Rolename", roleName);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

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
		/// RoleProvider.DeleteRole
		/// </summary>
		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			if (!RoleExists(roleName))
			{
				throw new ProviderException(string.Format(Properties.Resources.ErrRoleNotExist, roleName));
			}

			if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0)
			{
				throw new ProviderException(Properties.Resources.ErrCantDeletePopulatedRole);
			}

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("DELETE FROM \"{0}\" WHERE \"Rolename\" = @Rolename AND \"ApplicationName\" = @ApplicationName", m_RolesTableName);

					AddParameter (dbCommand, "@Rolename", roleName);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					SqliteTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (dbTrans = dbConn.BeginTransaction())
						{
							dbCommand.ExecuteNonQuery();

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

			return true;
		}

		/// <summary>
		/// RoleProvider.FindUsersInRole
		/// </summary>
		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			List<string> userList = new List<string>();

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT \"Username\" FROM \"{0}\" WHERE \"Username\" LIKE @Username AND \"Rolename\" = @Rolename AND \"ApplicationName\" = @ApplicationName ORDER BY \"Username\" ASC", m_UserInRolesTableName);

					AddParameter (dbCommand, "@Username", usernameToMatch);
					AddParameter (dbCommand, "@Rolename", roleName);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (SqliteDataReader reader = dbCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								while (reader.Read())
								{
									userList.Add(reader.GetString(0));
								}
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

			return userList.ToArray();
		}

		/// <summary>
		/// RoleProvider.GetAllRoles
		/// </summary>
		public override string[] GetAllRoles()
		{
			List<string> rolesList = new List<string>();

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT \"Rolename\" FROM \"{0}\" WHERE \"ApplicationName\" = @ApplicationName ORDER BY \"Rolename\" ASC", m_RolesTableName);

					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (SqliteDataReader reader = dbCommand.ExecuteReader())
						{
							while (reader.Read())
							{
								rolesList.Add(reader.GetString(0));
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

			return rolesList.ToArray();
		}

		/// <summary>
		/// RoleProvider.GetRolesForUser
		/// </summary>
		public override string[] GetRolesForUser(string username)
		{
			List<string> rolesList = new List<string>();

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT \"Rolename\" FROM \"{0}\" WHERE \"Username\" = @Username AND \"ApplicationName\" = @ApplicationName ORDER BY \"Rolename\" ASC", m_UserInRolesTableName);

					AddParameter (dbCommand, "@Username", username);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (SqliteDataReader reader = dbCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								while (reader.Read())
								{
									rolesList.Add(reader.GetString(0));
								}
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

			return rolesList.ToArray();
		}

		/// <summary>
		/// RoleProvider.GetUsersInRole
		/// </summary>
		public override string[] GetUsersInRole(string roleName)
		{
			List<string> userList = new List<string>();

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT \"Username\" FROM \"{0}\" WHERE \"Rolename\" = @Rolename AND \"ApplicationName\" = @ApplicationName ORDER BY \"Username\" ASC", m_UserInRolesTableName);

					AddParameter (dbCommand, "@Rolename", roleName);
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (SqliteDataReader reader = dbCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								while (reader.Read())
								{
									userList.Add(reader.GetString(0));
								}
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

			return userList.ToArray();
		}

		/// <summary>
		/// RoleProvider.IsUserInRole
		/// </summary>
		public override bool IsUserInRole(string userName, string roleName)
		{
			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT COUNT(*) FROM \"{0}\" WHERE \"Username\" = @Username AND \"Rolename\" = @Rolename AND \"ApplicationName\" = @ApplicationName", m_UserInRolesTableName);

					AddParameter (dbCommand, "@Username", userName);
					AddParameter (dbCommand, "@Rolename", roleName);
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
		/// RoleProvider.RemoveUsersFromRoles
		/// </summary>
		public override void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
		{
			foreach (string rolename in roleNames)
			{
				if (!RoleExists(rolename))
				{
					throw new ProviderException(string.Format(Properties.Resources.ErrRoleNotExist, rolename));
				}
			}

			foreach (string username in userNames)
			{
				foreach (string rolename in roleNames)
				{
					if (!IsUserInRole(username, rolename))
					{
						throw new ProviderException(string.Format(Properties.Resources.ErrUserIsNotInRole, username, rolename));
					}
				}
			}

			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("DELETE FROM \"{0}\" WHERE \"Username\" = @Username AND \"Rolename\" = @Rolename AND \"ApplicationName\" = @ApplicationName", m_UserInRolesTableName);

					AddParameter (dbCommand, "@Username");
					AddParameter (dbCommand, "@Rolename");
					AddParameter (dbCommand, "@ApplicationName", m_ApplicationName);

					SqliteTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						using (dbTrans = dbConn.BeginTransaction())
						{
							foreach (string username in userNames)
							{
								foreach (string rolename in roleNames)
								{
									dbCommand.Parameters["@Username"].Value = username;
									dbCommand.Parameters["@Rolename"].Value = rolename;
									dbCommand.ExecuteNonQuery();
								}
							}
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

		/// <summary>
		/// RoleProvider.RoleExists
		/// </summary>
		public override bool RoleExists(string roleName)
		{
			using (SqliteConnection dbConn = new SqliteConnection(m_ConnectionString))
			{
				using (SqliteCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format("SELECT COUNT(*) FROM \"{0}\" WHERE \"Rolename\" = @Rolename AND \"ApplicationName\" = @ApplicationName", m_RolesTableName);

					AddParameter (dbCommand, "@Rolename", roleName);
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
		#endregion

		#region private methods
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
