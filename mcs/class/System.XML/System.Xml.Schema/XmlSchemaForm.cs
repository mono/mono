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
		[XmlEnum("qualified")]
		Qualified = 0x00000001, 
		[XmlEnum("unqualified")]
		Unqualified = 0x00000002, 
	}
}
