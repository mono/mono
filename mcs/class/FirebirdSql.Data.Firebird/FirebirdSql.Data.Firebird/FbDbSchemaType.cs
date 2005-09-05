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
	/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/overview/*'/>
#if	(!NETCF)
	[Serializable]
#endif
	[Obsolete("Use the FbConnection.GetSchema() methods.")]
	public enum FbDbSchemaType
	{
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="CharacterSets"]/*'/>
		CharacterSets,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="CheckConstraints"]/*'/>
		CheckConstraints,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="CheckConstraintsByTable"]/*'/>
		CheckConstraintsByTable,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Collations"]/*'/>
		Collations,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Columns"]/*'/>
		Columns,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="ColumnPrivileges"]/*'/>
		ColumnPrivileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="DataTypes"]/*'/>
		DataTypes,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Domains"]/*'/>
		Domains,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="ForeignKeys"]/*'/>
		ForeignKeys,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Functions"]/*'/>
		Functions,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Generators"]/*'/>
		Generators,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Indexes"]/*'/>
		Indexes,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="MetaDataCollections"]/*'/>
		MetaDataCollections,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="PrimaryKeys"]/*'/>
		PrimaryKeys,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="ProcedureParameters"]/*'/>
		ProcedureParameters,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="ProcedurePrivileges"]/*'/>
		ProcedurePrivileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Procedures"]/*'/>
		Procedures,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Restrictions"]/*'/>
		Restrictions,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Roles"]/*'/>
		Roles,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="TableConstraints"]/*'/>
		TableConstraints,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="TablePrivileges"]/*'/>
		TablePrivileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Tables"]/*'/>
		Tables,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Triggers"]/*'/>
		Triggers,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="UniqueKeys"]/*'/>
		UniqueKeys,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="ViewColumnUsage"]/*'/>
		ViewColumnUsage,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="ViewPrivileges"]/*'/>
		ViewPrivileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Views"]/*'/>
		Views
	}
}
