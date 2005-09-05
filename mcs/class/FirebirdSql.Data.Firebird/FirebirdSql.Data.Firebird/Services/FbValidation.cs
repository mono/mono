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
	/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/overview/*'/>
	public sealed class FbValidation : FbService
	{
		#region Fields

		private FbValidationFlags options;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/property[@name="Options"]/*'/>
		public FbValidationFlags Options
		{
			get { return this.options; }
			set { this.options = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/constructor[@name="ctor"]/*'/>
		public FbValidation() : base()
		{
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/class[@name="FbValidation"]/method[@name="Execute"]/*'/>
		public void Execute()
		{
			try
			{
				this.StartSpb = this.CreateParameterBuffer();

				// Configure Spb
				this.StartSpb.Append(IscCodes.isc_action_svc_repair);
				this.StartSpb.Append(IscCodes.isc_spb_dbname, this.Database);
				this.StartSpb.Append(IscCodes.isc_spb_options, (int)this.options);

				// Start execution
				this.StartTask();

				// Process service output
				this.ProcessServiceOutput();
			}
			catch
			{
				throw;
			}
			finally
			{
				this.Close();
			}
		}

		#endregion
	}
}
