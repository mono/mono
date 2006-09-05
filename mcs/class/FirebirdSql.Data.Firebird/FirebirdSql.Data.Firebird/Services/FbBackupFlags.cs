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
	/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/overview/*'/>
	[Flags]
	public enum FbBackupFlags
	{
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="IgnoreChecksums"]/*'/>
		IgnoreChecksums		= 0x01,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="IgnoreLimbo"]/*'/>
		IgnoreLimbo			= 0x02,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="MetaDataOnly"]/*'/>
		MetaDataOnly		= 0x04,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="NoGarbageCollect"]/*'/>
		NoGarbageCollect	= 0x08,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="OldDescriptions"]/*'/>
		OldDescriptions		= 0x10,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="NonTransportable"]/*'/>
		NonTransportable	= 0x20,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="Convert"]/*'/>
		Convert				= 0x40,
		/// <include file='Doc/en_EN/FbBackup.xml' path='doc/enum[@name="FbBackupFlags"]/field[@name="Expand"]/*'/>
		Expand				= 0x80
	}
}
