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
using System.Data;
using System.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	#region Delegates

	/// <include file='Doc/en_EN/FbRowUpdatingEventArgs.xml' path='doc/delegate[@name="FbRowUpdatingEventHandler"]/*'/>
	public delegate void FbRowUpdatingEventHandler(object sender, FbRowUpdatingEventArgs e);

	#endregion

	/// <include file='Doc/en_EN/FbRowUpdatingEventArgs.xml' path='doc/class[@name="FbRowUpdatingEventArgs"]/overview/*'/>
	public sealed class FbRowUpdatingEventArgs : RowUpdatingEventArgs
	{
		#region Properties

		/// <include file='Doc/en_EN/FbRowUpdatingEventArgs.xml' path='doc/class[@name="FbRowUpdatingEventArgs"]/property[@name="Command"]/*'/>
		public new FbCommand Command
		{
			get { return (FbCommand)base.Command; }
			set { base.Command = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbRowUpdatingEventArgs.xml' path='doc/class[@name="FbRowUpdatingEventArgs"]/constructor[@name="ctor(DataRow,IDbCommand,StatementType,DataTableMapping)"]/*'/>
		public FbRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base(row, command, statementType, tableMapping)
		{
		}

		#endregion
	}
}
