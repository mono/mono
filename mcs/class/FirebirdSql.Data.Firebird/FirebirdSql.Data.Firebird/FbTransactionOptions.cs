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

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/overview/*'/>
#if	(!NETCF)
	[Flags]
	[Serializable]
#endif
	public enum FbTransactionOptions : int
	{
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Consistency"]/*'/>
		Consistency		= 1,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Concurrency"]/*'/>
		Concurrency		= 2,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Shared"]/*'/>
		Shared			= 4,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Protected"]/*'/>
		Protected		= 8,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Exclusive"]/*'/>
		Exclusive		= 16,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Wait"]/*'/>
		Wait			= 32,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="NoWait"]/*'/>
		NoWait			= 64,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Read"]/*'/>
		Read			= 128,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Write"]/*'/>
		Write			= 256,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="LockRead"]/*'/>
		LockRead		= 512,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="LockWrite"]/*'/>
		LockWrite		= 1024,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="ReadCommitted"]/*'/>
		ReadCommitted	= 2048,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="Autocommit"]/*'/>
		Autocommit		= 4096,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="RecVersion"]/*'/>
		RecVersion		= 8192,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="NoRecVersion"]/*'/>
		NoRecVersion	= 16384,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="RestartRequests"]/*'/>
		RestartRequests = 32768,
		/// <include file='Doc/en_EN/FbTransactionOptions.xml' path='doc/enum[@name="FbTransactionOptions"]/field[@name="NoAutoUndo"]/*'/>
		NoAutoUndo		= 65536
	}
}
