/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/overview/*'/>
	public sealed class FbSecurity : FbService
	{
		#region Properties

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/property[@name="UsersDbPath"]/*'/>
		public string UsersDbPath
		{
			get
			{
				byte[] buffer = this.QueryService(
					new byte[] { IscCodes.isc_info_svc_user_dbpath });
				System.Collections.ArrayList info = this.ParseQueryInfo(buffer);

				return info.Count != 0 ? (string)info[0] : null;
			}
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/constructor[@name="FbSecurity"]/*'/>
		public FbSecurity() : base()
		{
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/method[@name="AddUser(FbUserData)"]/*'/>
		public void AddUser(FbUserData user)
		{
			if (user.UserName != null && user.UserName.Length == 0)
			{
				throw new InvalidOperationException("Invalid user name.");
			}
			if (user.UserPassword != null && user.UserPassword.Length == 0)
			{
				throw new InvalidOperationException("Invalid user password.");
			}

			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_add_user);

			this.StartSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);
			this.StartSpb.Append(IscCodes.isc_spb_sec_password, user.UserPassword);

			if (user.FirstName != null && user.FirstName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_firstname, user.FirstName);
			}

			if (user.MiddleName != null && user.MiddleName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_middlename, user.MiddleName);
			}

			if (user.LastName != null && user.LastName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_lastname, user.LastName);
			}

			if (user.UserID != 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_userid, user.UserID);
			}

			if (user.GroupID != 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_groupid, user.GroupID);
			}

			if (user.GroupName != null && user.GroupName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_groupname, user.GroupName);
			}

			if (user.RoleName != null && user.RoleName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/method[@name="DeleteUser(FbUserData)"]/*'/>
		public void DeleteUser(FbUserData user)
		{
			if (user.UserName != null && user.UserName.Length == 0)
			{
				throw new InvalidOperationException("Invalid user name.");
			}

			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_delete_user);

			this.StartSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);

			if (user.RoleName != null && user.RoleName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/method[@name="ModifyUser(FbUserData)"]/*'/>
		public void ModifyUser(FbUserData user)
		{
			if (user.UserName != null && user.UserName.Length == 0)
			{
				throw new InvalidOperationException("Invalid user name.");
			}
			if (user.UserPassword != null && user.UserPassword.Length == 0)
			{
				throw new InvalidOperationException("Invalid user password.");
			}

			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_modify_user);
			this.StartSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);

			if (user.UserPassword != null && user.UserPassword.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_password, user.UserPassword);
			}

			if (user.FirstName != null && user.FirstName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_firstname, user.FirstName);
			}

			if (user.MiddleName != null && user.MiddleName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_middlename, user.MiddleName);
			}

			if (user.LastName != null && user.LastName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_lastname, user.LastName);
			}

			this.StartSpb.Append(IscCodes.isc_spb_sec_userid, user.UserID);
			this.StartSpb.Append(IscCodes.isc_spb_sec_groupid, user.GroupID);

			if (user.GroupName != null && user.GroupName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sec_groupname, user.GroupName);
			}

			if (user.RoleName != null && user.RoleName.Length > 0)
			{
				this.StartSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/method[@name="DisplayUser(System.String)"]/*'/>
		public FbUserData DisplayUser(string userName)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_display_user);
			this.StartSpb.Append(IscCodes.isc_spb_sec_username, userName);

			// Start execution
			this.StartTask();

			byte[] buffer = this.QueryService(
				new byte[] { IscCodes.isc_info_svc_get_users });

			System.Collections.ArrayList info = base.ParseQueryInfo(buffer);

			this.Close();

			if (info.Count == 0)
			{
				return null;
			}

			FbUserData[] users = (FbUserData[])info[0];

			return (users != null && users.Length > 0) ? users[0] : null;
		}

		/// <include file='Doc/en_EN/FbSecurity.xml' path='doc/class[@name="FbSecurity"]/method[@name="DisplayUsers"]/*'/>
		public FbUserData[] DisplayUsers()
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_display_user);

			// Start execution
			this.StartTask();

			byte[] buffer = this.QueryService(
				new byte[] { IscCodes.isc_info_svc_get_users });

			System.Collections.ArrayList info = base.ParseQueryInfo(buffer);

			this.Close();

			if (info.Count == 0)
			{
				return null;
			}

			return (FbUserData[])info[0];
		}

		#endregion
	}
}
