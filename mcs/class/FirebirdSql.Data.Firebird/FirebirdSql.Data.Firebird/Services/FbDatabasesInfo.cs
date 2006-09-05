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
using System.Collections.Specialized;

namespace FirebirdSql.Data.Firebird.Services
{
	/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbDatabasesInfo"]/overview/*'/>
	public struct FbDatabasesInfo
	{
		#region Fields

		private int connectionCount;
		private StringCollection databases;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbDatabasesInfo"]/field[@name="ConnectionCount"]/*'/>
		public int ConnectionCount
		{
			get { return this.connectionCount; }
			set { this.connectionCount = value; }
		}

		/// <include file='Doc/en_EN/FbService.xml'	path='doc/struct[@name="FbDatabasesInfo"]/field[@name="Databases"]/*'/>
		public StringCollection Databases
		{
			get
			{
				if (this.databases == null)
				{
					this.databases = new StringCollection();
				}
				return this.databases;
			}
		}

		#endregion
	}
}
