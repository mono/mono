// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaContentProcessing.
	/// </summary>
	public enum XmlSchemaContentProcessing 
	{
		[XmlIgnore]
		None	= 0,
		[XmlEnum("skip")]
		Skip	= 1,
		[XmlEnum("lax")]
		Lax		= 2, 
		[XmlEnum("strict")]
		Strict	= 3, 
	}
}
