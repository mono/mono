// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaUse.
	/// </summary>
	public enum XmlSchemaUse 
	{
		[XmlIgnore]
		None = 0x00000000, 
		[XmlEnum]
		Optional = 0x00000001, 
		[XmlEnum]
		Prohibited = 0x00000002, 
		[XmlEnum]
		Required = 0x00000003, 
	}
}
