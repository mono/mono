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
	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/overview/*'/>
	public sealed class FbBackup : FbService
	{
		#region Fields

		private bool			verbose;
		private int				factor;
		private ArrayList		backupFiles;
		private FbBackupFlags	options;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="BackupFiles"]/*'/>
		public ArrayList BackupFiles
		{
			get { return this.backupFiles; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Verbose"]/*'/>
		public bool Verbose
		{
			get { return this.verbose; }
			set { this.verbose = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Factor"]/*'/>
		public int Factor
		{
			get { return this.factor; }
			set { this.factor = value; }
		}

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/property[@name="Options"]/*'/>
		public FbBackupFlags Options
		{
			get { return this.options; }
			set { this.options = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/constructor[@name="ctor"]/*'/>
		public FbBackup() : base()
		{
			this.backupFiles = new ArrayList();
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/class[@name="FbBackup"]/method[@name="Execute"]/*'/>
		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = this.CreateParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_backup);
				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);

				foreach (FbBackupFile file in backupFiles)
				{
					this.StartSpb.Append(IscCodes.isc_spb_bkp_file, file.BackupFile);
					this.StartSpb.Append(IscCodes.isc_spb_bkp_length, file.BackupLength);
				}

				if (verbose)
				{
					this.StartSpb.Append(IscCodes.isc_spb_verbose);
				}

				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.options);

				// Start execution
				this.StartTask();

				if (this.verbose)
				{
					this.ProcessServiceOutput();
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				// Close
				this.Close();
			}
		}

		#endregion
	}
}
