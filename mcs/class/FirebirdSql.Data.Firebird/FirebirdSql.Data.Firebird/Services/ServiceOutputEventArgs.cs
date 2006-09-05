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
	#region Delegates

	/// <include file='Doc/en_EN/FbService.xml'	path='doc/delegate[@name="ServiceOutputEventHandler(System.Object,ServiceOutputEventArgs)"]/*'/>
	public delegate void ServiceOutputEventHandler(object sender, ServiceOutputEventArgs e);

	#endregion

	/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="ServiceOutputEventArgs"]/overview/*'/>
	public sealed class ServiceOutputEventArgs : EventArgs
	{
		#region Fields

		private string message;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="ServiceOutputEventArgs"]/property[@name="Message"]/*'/>
		public string Message
		{
			get { return this.message; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/class[@name="ServiceOutputEventArgs"]/constructor[@name="ctor(System.String)"]/*'/>
		public ServiceOutputEventArgs(string message)
		{
			this.message = message;
		}

		#endregion
	}
}
