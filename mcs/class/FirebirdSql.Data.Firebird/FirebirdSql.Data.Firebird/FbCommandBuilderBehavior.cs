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
	/// <include file='Doc/en_EN/FbCommandBuilderBehavior.xml' path='doc/enum[@name="FbCommandBuilderBehavior"]/overview/*'/>
#if	(!NETCF)
	[Serializable]
#endif
	public enum FbCommandBuilderBehavior
	{
		/// <include file='Doc/en_EN/FbCommandBuilderBehavior.xml' path='doc/enum[@name="FbCommandBuilderBehavior"]/field[@name="Default"]/*'/>
		Default,
		/// <include file='Doc/en_EN/FbCommandBuilderBehavior.xml' path='doc/enum[@name="FbCommandBuilderBehavior"]/field[@name="AllFields"]/*'/>
		AllFields,
		/// <include file='Doc/en_EN/FbCommandBuilderBehavior.xml' path='doc/enum[@name="FbCommandBuilderBehavior"]/field[@name="KeyFields"]/*'/>
		KeyFields,
		/// <include file='Doc/en_EN/FbCommandBuilderBehavior.xml' path='doc/enum[@name="FbCommandBuilderBehavior"]/field[@name="KeyAndTimestampFields"]/*'/>
		KeyAndTimestampFields
	}
}
