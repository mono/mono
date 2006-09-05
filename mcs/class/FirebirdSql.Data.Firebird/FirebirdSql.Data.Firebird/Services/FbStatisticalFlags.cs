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
	/// <include file='Doc/en_EN/FbStatistical.xml'	path='doc/enum[@name="FbStatisticalFlags"]/overview/*'/>
	[Flags]
	public enum FbStatisticalFlags
	{
		/// <include file='Doc/en_EN/FbStatistical.xml'	path='doc/enum[@name="FbStatisticalFlags"]/field[@name="DataPages"]/*'/>
		DataPages		= 0x01,
		/// <include file='Doc/en_EN/FbStatistical.xml'	path='doc/enum[@name="FbStatisticalFlags"]/field[@name="DatabaseLog"]/*'/>
		DatabaseLog		= 0x02,
		/// <include file='Doc/en_EN/FbStatistical.xml'	path='doc/enum[@name="FbStatisticalFlags"]/field[@name="HeaderPages"]/*'/>
		HeaderPages		= 0x04,
		/// <include file='Doc/en_EN/FbStatistical.xml'	path='doc/enum[@name="FbStatisticalFlags"]/field[@name="IndexPages"]/*'/>
		IndexPages		= 0x08,
		/// <include file='Doc/en_EN/FbStatistical.xml'	path='doc/enum[@name="FbStatisticalFlags"]/field[@name="SystemTablesRelations"]/*'/>
		SystemTablesRelations = 0x10,
	}
}
