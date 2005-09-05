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
	/// <include file='Doc/en_EN/FbLog.xml'	path='doc/class[@name="FbLog"]/overview/*'/>
	public sealed class FbLog : FbService
	{
		#region Constructors

		/// <include file='Doc/en_EN/FbLog.xml'	path='doc/class[@name="FbLog"]/constructor[@name="ctor"]/*'/>
		public FbLog() : base()
		{
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbLog.xml'	path='doc/class[@name="FbLog"]/method[@name="Execute"]/*'/>
		public void Execute()
		{
			try
			{
				// Configure Spb
				this.StartSpb = this.CreateParameterBuffer();

				this.StartSpb.Append(IscCodes.isc_action_svc_get_ib_log);

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
