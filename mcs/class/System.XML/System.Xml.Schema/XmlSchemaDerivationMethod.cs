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
		[XmlEnum]
		Empty		= 0x00000000, 
		[XmlEnum]
		Substitution= 0x00000001, 
		[XmlEnum]
		Extension	= 0x00000002, 
		[XmlEnum]
		Restriction	= 0x00000004, 
		[XmlEnum]
		List		= 0x00000008, 
		[XmlEnum]
		Union		= 0x00000010, 
		[XmlEnum]
		All			= 0x000000FF,
		[XmlIgnore]
		None		= 0x00000100, 
	}
}
