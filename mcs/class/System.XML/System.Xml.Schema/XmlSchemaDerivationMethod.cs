// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaDerivationMethod.
	/// </summary>
	[Flags]
	public enum XmlSchemaDerivationMethod
	{
		[XmlEnum("empty")]
		Empty		= 0x00000000, 
		[XmlEnum("substitution")]
		Substitution= 0x00000001, 
		[XmlEnum("extension")]
		Extension	= 0x00000002, 
		[XmlEnum("restriction")]
		Restriction	= 0x00000004, 
		[XmlEnum("list")]
		List		= 0x00000008, 
		[XmlEnum("union")]
		Union		= 0x00000010, 
		[XmlEnum("#all")]
		All			= 0x000000FF,
		[XmlIgnore]
		None		= 0x00000100, 
	}
}
