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
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Specialized;

using FirebirdSql.Data;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/overview/*'/>
	public abstract class FbService
	{
		#region Events

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/event[@name="ServiceOutput"]/*'/>
		public event ServiceOutputEventHandler ServiceOutput;

		#endregion

		#region Fields

		private IServiceManager			svc;
		private FbServiceState			state;
		private ServiceParameterBuffer	querySpb;
		private FbConnectionString		csManager;
		private string					connectionString;
		private string					serviceName;
		private int						queryBufferSize;

		#endregion

		#region Protected Fields

		internal ServiceParameterBuffer StartSpb;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/property[@name="State"]/*'/>
		public FbServiceState State
		{
			get { return state; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/property[@name="ConnectionString"]/*'/>
		public string ConnectionString
		{
			get { return this.connectionString; }
			set
			{
				if (this.svc != null && this.state == FbServiceState.Open)
				{
					throw new InvalidOperationException("ConnectionString cannot be modified on active service instances.");
				}

				this.csManager.Load(value);

				if (value == null)
				{
					this.connectionString = String.Empty;
				}
				else
				{
					this.connectionString = value;
				}
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/property[@name="QueryBufferSize"]/*'/>
		public int QueryBufferSize
		{
			get { return this.queryBufferSize; }
			set { this.queryBufferSize = value; }
		}

		#endregion

		#region Protected Properties

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/property[@name="Database"]/*'/>
		protected string Database
		{
			get { return this.csManager.Database; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/constructor[@name="ctor"]/*'/>
		protected FbService()
		{
			this.csManager			= new FbConnectionString(true);
			this.state				= FbServiceState.Closed;
			this.connectionString	= String.Empty;
			this.serviceName		= "service_mgr";
			this.queryBufferSize	= 1024;
		}

		#endregion

		#region Protected Methods

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="Open"]/*'/>
		protected void Open()
		{
			if (this.state != FbServiceState.Closed)
			{
				throw new InvalidOperationException("Service already Open.");
			}

			if (this.csManager.UserID == null || this.csManager.UserID.Length == 0)
			{
				throw new InvalidOperationException("No user name was specified.");
			}

			if (this.csManager.Password == null || this.csManager.Password.Length == 0)
			{
				throw new InvalidOperationException("No user password was specified.");
			}

			try
			{
				if (this.svc == null)
				{
					// New instance	for	Service	handler
					this.svc = ClientFactory.CreateServiceManager(this.csManager.ServerType);
				}

				// Set service name
				string service = String.Empty;

				switch (this.csManager.ServerType)
				{
					case 0:
						service = this.csManager.DataSource + ":" + serviceName;
						break;

					default:
						service = serviceName;
						break;
				}

				// Initialize Services API
				this.svc.Attach(this.BuildSpb(), this.csManager.DataSource,
								this.csManager.Port, service);

				this.state = FbServiceState.Open;
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="Close"]/*'/>
		protected void Close()
		{
			if (this.state != FbServiceState.Open)
			{
				return;
			}

			try
			{
				this.svc.Detach();
				this.svc = null;

				this.state = FbServiceState.Closed;
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="startTask"]/*'/>
		protected void StartTask()
		{
			if (this.state == FbServiceState.Closed)
			{
				// Attach to Service Manager
				this.Open();
			}

			try
			{
				// Start service operation
				this.svc.Start(this.StartSpb);
			}
			catch (IscException ex)
			{
				throw new FbException(ex.Message, ex);
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="queryService"]/*'/>
		protected byte[] QueryService(byte[] items)
		{
			if (this.state == FbServiceState.Closed)
			{
				// Attach to Service Manager
				this.Open();
			}

			if (this.querySpb == null)
			{
				this.querySpb = new ServiceParameterBuffer();
			}

			// Response	buffer
			byte[] buffer = new byte[queryBufferSize];

			this.svc.Query(this.querySpb, items.Length, items, buffer.Length, buffer);

			return buffer;
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="parseQueryInfo"]/*'/>
		protected ArrayList ParseQueryInfo(byte[] buffer)
		{
			int pos = 0;
			int length = 0;
			int type = 0;

			ArrayList items = new ArrayList();

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				length = IscHelper.VaxInteger(buffer, pos, 2);
				pos += 2;

				if (length != 0)
				{
					switch (type)
					{
						case IscCodes.isc_info_svc_version:
						case IscCodes.isc_info_svc_get_license_mask:
						case IscCodes.isc_info_svc_capabilities:
						case IscCodes.isc_info_svc_get_licensed_users:
							items.Add(IscHelper.VaxInteger(buffer, pos, 4));
							pos += length;
							break;

						case IscCodes.isc_info_svc_server_version:
						case IscCodes.isc_info_svc_implementation:
						case IscCodes.isc_info_svc_get_env:
						case IscCodes.isc_info_svc_get_env_lock:
						case IscCodes.isc_info_svc_get_env_msg:
						case IscCodes.isc_info_svc_user_dbpath:
						case IscCodes.isc_info_svc_line:
						case IscCodes.isc_info_svc_to_eof:
							items.Add(Encoding.Default.GetString(buffer, pos, length));
							pos += length;
							break;

						case IscCodes.isc_info_svc_svr_db_info:
							items.Add(ParseDatabasesInfo(buffer, ref pos));
							break;

						case IscCodes.isc_info_svc_get_users:
							items.Add(ParseUserData(buffer, ref	pos));
							break;

						case IscCodes.isc_info_svc_get_config:
							items.Add(ParseServerConfig(buffer, ref	pos));
							break;
					}
				}
			}

			return items;
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="GetNextLine"]/*'/>
		protected string GetNextLine()
		{
			this.querySpb = new ServiceParameterBuffer();

			byte[] items	= new byte[] { IscCodes.isc_info_svc_line };
			byte[] buffer	= this.QueryService(items);

			ArrayList info = this.ParseQueryInfo(buffer);
			if (info.Count != 0)
			{
				return info[0] as string;
			}
			else
			{
				return null;
			}
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="FbService"]/method[@name="ProcessServiceOutput"]/*'/>
		protected void ProcessServiceOutput()
		{
			string line = null;

			while ((line = this.GetNextLine()) != null)
			{
				if (this.ServiceOutput != null)
				{
					this.ServiceOutput(this, new ServiceOutputEventArgs(line));
				}
			}
		}

		#endregion

		#region Internal Methods

		internal ServiceParameterBuffer CreateParameterBuffer()
		{
			if (this.svc == null)
			{
				// New instance	for	Service	handler
				this.svc = ClientFactory.CreateServiceManager(this.csManager.ServerType);
			}

			return this.svc.CreateParameterBuffer();
		}

		internal ServiceParameterBuffer BuildSpb()
		{
			ServiceParameterBuffer spb = this.CreateParameterBuffer();

			// SPB configuration				
			spb.Append(IscCodes.isc_spb_version);
			spb.Append(IscCodes.isc_spb_current_version);
			spb.Append((byte)IscCodes.isc_spb_user_name, this.csManager.UserID);
			spb.Append((byte)IscCodes.isc_spb_password, this.csManager.Password);
			spb.Append((byte)IscCodes.isc_spb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });

			if (this.csManager.Role != null && this.csManager.Role.Length > 0)
			{
				spb.Append((byte)IscCodes.isc_spb_sql_role_name, this.csManager.Role);
			}

			return spb;
		}

		#endregion

		#region Private	Static Methods

		private static FbServerConfig ParseServerConfig(byte[] buffer, ref int pos)
		{
			FbServerConfig config = new FbServerConfig();

			pos = 1;
			while (buffer[pos] != IscCodes.isc_info_flag_end)
			{
				pos++;

				int key = buffer[pos - 1];
				int keyValue = IscHelper.VaxInteger(buffer, pos, 4);

				pos += 4;

				switch (key)
				{
					case IscCodes.ISCCFG_LOCKMEM_KEY:
						config.LockMemSize = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKSEM_KEY:
						config.LockSemCount = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKSIG_KEY:
						config.LockSignal = keyValue;
						break;

					case IscCodes.ISCCFG_EVNTMEM_KEY:
						config.EventMemorySize = keyValue;
						break;

					case IscCodes.ISCCFG_PRIORITY_KEY:
						config.PrioritySwitchDelay = keyValue;
						break;

					case IscCodes.ISCCFG_MEMMIN_KEY:
						config.MinMemory = keyValue;
						break;

					case IscCodes.ISCCFG_MEMMAX_KEY:
						config.MaxMemory = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKORDER_KEY:
						config.LockGrantOrder = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKMEM_KEY:
						config.AnyLockMemory = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKSEM_KEY:
						config.AnyLockSemaphore = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKSIG_KEY:
						config.AnyLockSignal = keyValue;
						break;

					case IscCodes.ISCCFG_ANYEVNTMEM_KEY:
						config.AnyEventMemory = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKHASH_KEY:
						config.LockHashSlots = keyValue;
						break;

					case IscCodes.ISCCFG_DEADLOCK_KEY:
						config.DeadlockTimeout = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKSPIN_KEY:
						config.LockRequireSpins = keyValue;
						break;

					case IscCodes.ISCCFG_CONN_TIMEOUT_KEY:
						config.ConnectionTimeout = keyValue;
						break;

					case IscCodes.ISCCFG_DUMMY_INTRVL_KEY:
						config.DummyPacketInterval = keyValue;
						break;

					case IscCodes.ISCCFG_IPCMAP_KEY:
						config.IpcMapSize = keyValue;
						break;

					case IscCodes.ISCCFG_DBCACHE_KEY:
						config.DefaultDbCachePages = keyValue;
						break;
				}
			}

			pos++;

			return config;
		}

		private static FbDatabasesInfo ParseDatabasesInfo(byte[] buffer, ref int pos)
		{
			FbDatabasesInfo dbInfo = new FbDatabasesInfo();
			int type = 0;
			int length = 0;

			pos = 1;
			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				switch (type)
				{
					case IscCodes.isc_spb_num_att:
						dbInfo.ConnectionCount = IscHelper.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;

					case IscCodes.isc_spb_num_db:
						pos += 4;
						break;

					case IscCodes.isc_spb_dbname:
						length = IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						dbInfo.Databases.Add(Encoding.Default.GetString(buffer, pos, length));
						pos += length;
						break;
				}
			}
			pos--;

			return dbInfo;
		}

		private static FbUserData[] ParseUserData(byte[] buffer, ref int pos)
		{
			ArrayList users = new ArrayList();
			FbUserData currentUser = null;
			int type = 0;
			int length = 0;

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				switch (type)
				{
					case IscCodes.isc_spb_sec_username:
						{
							currentUser = new FbUserData();

							users.Add(currentUser);

							length = IscHelper.VaxInteger(buffer, pos, 2);
							pos += 2;
							currentUser.UserName = Encoding.Default.GetString(buffer, pos, length);
							pos += length;
						}
						break;

					case IscCodes.isc_spb_sec_firstname:
						length = IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						currentUser.FirstName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_middlename:
						length = IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						currentUser.MiddleName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_lastname:
						length = IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						currentUser.LastName = Encoding.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_userid:
						currentUser.UserID = IscHelper.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;

					case IscCodes.isc_spb_sec_groupid:
						currentUser.GroupID = IscHelper.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;
				}
			}
			pos--;

			return (FbUserData[])users.ToArray(typeof(FbUserData));
		}

		#endregion
	}
}