// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaForm.
	/// </summary>
	public enum XmlSchemaForm 
	{
		[XmlIgnore]
		None = 0x00000000, 
		[XmlEnum]
		Qualified = 0x00000001, 
		[XmlEnum]
		Unqualified = 0x00000002, 
	}
}
