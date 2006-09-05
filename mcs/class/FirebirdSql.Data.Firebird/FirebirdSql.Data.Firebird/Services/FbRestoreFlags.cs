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
	/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/overview/*'/>
	[Flags]
	public enum FbRestoreFlags
	{
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="DeactivateIndexes"]/*'/>
		DeactivateIndexes	= 0x0100,
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="NoShadow"]/*'/>
		NoShadow			= 0x0200,
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="NoValidity"]/*'/>
		NoValidity			= 0x0400,
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="IndividualCommit"]/*'/>
		IndividualCommit	= 0x0800,
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="Replace"]/*'/>
		Replace				= 0x1000,
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="Create"]/*'/>
		Create				= 0x2000,
		/// <include file='Doc/en_EN/FbRestore.xml'	path='doc/enum[@name="FbRestoreFlags"]/field[@name="UseAllSpace"]/*'/>
		UseAllSpace			= 0x4000
	}
}
