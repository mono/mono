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
	/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/overview/*'/>
#if	(!NETCF)
	[Serializable]
#endif
	public enum FbCharset : int
	{
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Default"]/*'/>
		Default			= -1,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="None"]/*'/>
		None			= 0,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Octets"]/*'/>
		Octets			= 1,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Ascii"]/*'/>
		Ascii			= 2,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="UnicodeFss"]/*'/>
		UnicodeFss		= 3,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="ShiftJis0208"]/*'/>
		ShiftJis0208	= 5,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="EucJapanese0208"]/*'/>
		EucJapanese0208 = 6,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Iso2022Japanese"]/*'/>
		Iso2022Japanese = 7,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Dos437"]/*'/>
		Dos437			= 10,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Dos850"]/*'/>
		Dos850			= 11,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Dos865"]/*'/>
		Dos865			= 12,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Dos860"]/*'/>
		Dos860			= 13,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Dos863"]/*'/>
		Dos863			= 14,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Iso8859_1"]/*'/>
		Iso8859_1		= 21,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Iso8859_2"]/*'/>
		Iso8859_2		= 22,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Ksc5601"]/*'/>
		Ksc5601			= 44,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Dos861"]/*'/>
		Dos861			= 47,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1250"]/*'/>
		Windows1250		= 51,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1251"]/*'/>
		Windows1251		= 52,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1252"]/*'/>
		Windows1252		= 53,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1253"]/*'/>
		Windows1253		= 54,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1254"]/*'/>
		Windows1254		= 55,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Big5"]/*'/>
		Big5			= 56,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Gb2312"]/*'/>
		Gb2312			= 57,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1255"]/*'/>
		Windows1255		= 58,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1256"]/*'/>
		Windows1256		= 59,
		/// <include file='Doc/en_EN/FbCharset.xml'	path='doc/enum[@name="FbCharset"]/field[@name="Windows1257"]/*'/>
		Windows1257		= 60
	}
}
