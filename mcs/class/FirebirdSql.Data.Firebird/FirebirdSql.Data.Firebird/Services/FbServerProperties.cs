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
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/overview/*'/>
	public sealed class FbServerProperties : FbService
	{
		#region Properties

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="Version"]/*'/>
		public int Version
		{
			get { return this.GetInt32(IscCodes.isc_info_svc_version); }
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="ServerVersion"]/*'/>
		public string ServerVersion
		{
			get { return this.GetString(IscCodes.isc_info_svc_server_version); }
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="Implementation"]/*'/>
		public string Implementation
		{
			get { return this.GetString(IscCodes.isc_info_svc_implementation); }
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="RootDirectory"]/*'/>
		public string RootDirectory
		{
			get { return this.GetString(IscCodes.isc_info_svc_get_env); }
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="LockManager"]/*'/>
		public string LockManager
		{
			get { return this.GetString(IscCodes.isc_info_svc_get_env_lock); }
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="MessageFile"]/*'/>
		public string MessageFile
		{
			get { return this.GetString(IscCodes.isc_info_svc_get_env_msg); }
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="DatabasesInfo"]/*'/>
		public FbDatabasesInfo DatabasesInfo
		{
			get
			{
				ArrayList info = this.GetInfo(IscCodes.isc_info_svc_svr_db_info);

				return info.Count != 0 ? (FbDatabasesInfo)info[0] : new FbDatabasesInfo();
			}
		}

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/property[@name="ServerConfig"]/*'/>
		public FbServerConfig ServerConfig
		{
			get
			{
				ArrayList info = this.GetInfo(IscCodes.isc_info_svc_get_config);

				return info.Count != 0 ? (FbServerConfig)info[0] : new FbServerConfig();
			}
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbServerProperties.xml' path='doc/class[@name="FbServerProperties"]/constructor[@name="FbServerProperties"]/*'/>
		public FbServerProperties() : base()
		{
		}

		#endregion

		#region Private	Methods

		private string GetString(int item)
		{
			ArrayList info = this.GetInfo(item);

			return info.Count != 0 ? (string)info[0] : null;
		}

		private int GetInt32(int item)
		{
			ArrayList info = this.GetInfo(item);

			return info.Count != 0 ? (int)info[0] : 0;
		}

		private ArrayList GetInfo(int item)
		{
			return this.GetInfo(new byte[] { (byte)item });
		}

		private ArrayList GetInfo(byte[] items)
		{
			byte[] buffer = this.QueryService(items);

			return this.ParseQueryInfo(buffer);
		}

		#endregion
	}
}
