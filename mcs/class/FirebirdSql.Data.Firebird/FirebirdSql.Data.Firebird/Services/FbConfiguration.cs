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
	/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/overview/*'/>
	public sealed class FbConfiguration : FbService
	{
		#region Constructors

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/constructor[@name="ctor"]/*'/>
		public FbConfiguration() : base()
		{
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetSqlDialect(System.Int32)"]/*'/>
		public void SetSqlDialect(int sqlDialect)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_set_sql_dialect, sqlDialect);

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetSweepInterval(System.Int32)"]/*'/>
		public void SetSweepInterval(int sweepInterval)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_sweep_interval, sweepInterval);

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetPageBuffers(System.Int32)"]/*'/>
		public void SetPageBuffers(int pageBuffers)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_prp_page_buffers, pageBuffers);

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="DatabaseShutdown(FbShutdownMode,System.Int32)"]/*'/>
		public void DatabaseShutdown(FbShutdownMode mode, int seconds)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			switch (mode)
			{
				case FbShutdownMode.Forced:
					this.StartSpb.Append(IscCodes.isc_spb_prp_shutdown_db, seconds);
					break;

				case FbShutdownMode.DenyTransaction:
					this.StartSpb.Append(
						IscCodes.isc_spb_prp_deny_new_transactions, seconds);
					break;

				case FbShutdownMode.DenyConnection:
					this.StartSpb.Append(
						IscCodes.isc_spb_prp_deny_new_attachments, seconds);
					break;
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="DatabaseOnline"]/*'/>
		public void DatabaseOnline()
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_db_online);

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="ActivateShadows"]/*'/>
		public void ActivateShadows()
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
			this.StartSpb.Append(IscCodes.isc_spb_options, IscCodes.isc_spb_prp_activate);

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetForcedWrites(System.Boolean)"]/*'/>
		public void SetForcedWrites(bool forcedWrites)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			// WriteMode
			if (forcedWrites)
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_sync);
			}
			else
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_prp_write_mode, (byte)IscCodes.isc_spb_prp_wm_async);
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetReserveSpace(System.Boolean)"]/*'/>
		public void SetReserveSpace(bool reserveSpace)
		{
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			// Reserve Space
			if (reserveSpace)
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res);
			}
			else
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_prp_reserve_space, (byte)IscCodes.isc_spb_prp_res_use_full);
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		/// <include file='Doc/en_EN/FbConfiguration.xml' path='doc/class[@name="FbConfiguration"]/method[@name="SetAccessMode(System.Boolean)"]/*'/>
		public void SetAccessMode(bool readOnly)
		{
			// Configure Spb
			this.StartSpb = this.CreateParameterBuffer();

			this.StartSpb.Append(IscCodes.isc_action_svc_properties);
			this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

			if (readOnly)
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_prp_access_mode, (byte)IscCodes.isc_spb_prp_am_readonly);
			}
			else
			{
				this.StartSpb.Append(
					IscCodes.isc_spb_prp_access_mode, (byte)IscCodes.isc_spb_prp_am_readwrite);
			}

			// Start execution
			this.StartTask();

			this.Close();
		}

		#endregion
	}
}
