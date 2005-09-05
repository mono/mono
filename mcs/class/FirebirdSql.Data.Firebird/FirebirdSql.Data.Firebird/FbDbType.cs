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
	/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/overview/*'/>
#if	(!NETCF)
	[Serializable]
#endif
	public enum FbDbType
	{
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Array"]/*'/>
		Array,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="BigInt"]/*'/>
		BigInt,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Binary"]/*'/>
		Binary,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Char"]/*'/>
		Char,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Date"]/*'/>
		Date,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Decimal"]/*'/>
		Decimal,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Double"]/*'/>
		Double,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Float"]/*'/>
		Float,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Guid"]/*'/>
		Guid,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Integer"]/*'/>
		Integer,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Numeric"]/*'/>
		Numeric,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="SmallInt"]/*'/>
		SmallInt,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Text"]/*'/>
		Text,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="Time"]/*'/>
		Time,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="TimeStamp"]/*'/>
		TimeStamp,
		/// <include file='Doc/en_EN/FbDbType.xml' path='doc/enum[@name="FbDbType"]/field[@name="VarChar"]/*'/>
		VarChar
	}
}
