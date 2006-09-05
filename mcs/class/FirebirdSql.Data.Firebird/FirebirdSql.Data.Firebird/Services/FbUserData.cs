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

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbUserData"]/overview/*'/>
	public sealed class FbUserData
	{
		#region Fields

		private string	userName;
		private string	firstName;
		private string	lastName;
		private string	middleName;
		private string	userPassword;
		private string	groupName;
		private string	roleName;
		private int		userID;
		private int		groupID;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="UserName"]/*'/>
		public string UserName
		{
			get { return this.userName; }
			set
			{
				if (value == null)
				{
					throw new InvalidOperationException("The user name cannot be null.");
				}
				if (value.Length > 31)
				{
					throw new InvalidOperationException("The user name cannot have more than 31 characters.");
				}

				this.userName = value;
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="UserPassword"]/*'/>
		public string UserPassword
		{
			get { return this.userPassword; }
			set
			{
				if (value == null)
				{
					throw new InvalidOperationException("The user password cannot be null.");
				}
				if (value.Length > 31)
				{
					throw new InvalidOperationException("The user password cannot have more than 31 characters.");
				}

				this.userPassword = value;
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="FirstName"]/*'/>		
		public string FirstName
		{
			get { return this.firstName; }
			set { this.firstName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="LastName"]/*'/>
		public string LastName
		{
			get { return this.lastName; }
			set { this.lastName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="MiddleName"]/*'/>
		public string MiddleName
		{
			get { return this.middleName; }
			set { this.middleName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="UserID"]/*'/>
		public int UserID
		{
			get { return this.userID; }
			set { this.userID = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="GroupID"]/*'/>
		public int GroupID
		{
			get { return this.groupID; }
			set { this.groupID = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="GroupName"]/*'/>
		public string GroupName
		{
			get { return this.groupName; }
			set { this.groupName = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbUserData"]/property[@name="RoleName"]/*'/>
		public string RoleName
		{
			get { return this.roleName; }
			set { this.roleName = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbUserData"]/constructor[@name="FbUserData"]/*'/>
		public FbUserData()
		{
			this.userName		= String.Empty;
			this.firstName		= String.Empty;
			this.lastName		= String.Empty;
			this.middleName		= String.Empty;
			this.userPassword	= String.Empty;
			this.roleName		= String.Empty;
		}

		#endregion
	}
}
