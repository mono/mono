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
	/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/overview/*'/>
	[Flags]
	public enum FbValidationFlags
	{
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="ValidateDatabase"]/*'/>
		ValidateDatabase	= 0x01,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="SweepDatabase"]/*'/>
		SweepDatabase		= 0x02,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="MendDatabase"]/*'/>
		MendDatabase		= 0x04,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="CheckDatabase"]/*'/>
		CheckDatabase		= 0x10,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="IgnoreChecksum"]/*'/>
		IgnoreChecksum		= 0x20,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="KillShadows"]/*'/>
		KillShadows			= 0x40,
		/// <include file='Doc/en_EN/FbValidation.xml' path='doc/struct[@name="FbValidationFlags"]/field[@name="Full"]/*'/>
		Full				= 0x80
	}
}
