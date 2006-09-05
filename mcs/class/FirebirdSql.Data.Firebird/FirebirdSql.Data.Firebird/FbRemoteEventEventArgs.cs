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

namespace FirebirdSql.Data.Firebird
{
	#region Delegates

	/// <include file='Doc/en_EN/FbRemoteEventEventArgs.xml' path='doc/delegate[@name="FbRemoteEventEventHandler"]/overview/*'/>
	public delegate	void FbRemoteEventEventHandler(object sender, FbRemoteEventEventArgs e);

	#endregion

	/// <include file='Doc/en_EN/FbRemoteEventEventArgs.xml' path='doc/class[@name="FbRemoteEventEventArgs"]/overview/*'/>
	public sealed class	FbRemoteEventEventArgs : System.ComponentModel.CancelEventArgs
	{
		#region Fields

		private	string	name;
		private	int		counts;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbRemoteEventEventArgs.xml' path='doc/class[@name="FbRemoteEventEventArgs"]/property[@name="Name"]/*'/>
		public string Name
		{
			get	{ return this.name;	}
		}

		/// <include file='Doc/en_EN/FbRemoteEventEventArgs.xml' path='doc/class[@name="FbRemoteEventEventArgs"]/property[@name="Counts"]/*'/>
		public int Counts
		{
			get	{ return this.counts; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbRemoteEventEventArgs.xml' path='doc/class[@name="FbRemoteEventEventArgs"]/constructor[@name="ctor(System.String,System.Int32)"]/*'/>
		public FbRemoteEventEventArgs(string name, int counts) : this(name,	counts,	false)
		{
		}

		/// <include file='Doc/en_EN/FbRemoteEventEventArgs.xml' path='doc/class[@name="FbRemoteEventEventArgs"]/constructor[@name="ctor(System.String,System.Int32,System.Boolean)"]/*'/>
		public FbRemoteEventEventArgs(string name, int counts, bool	cancel)	: base(cancel)
		{
			this.name = name;
			this.counts = counts;
		}

		#endregion
	}
}
