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
		[XmlEnum]
		Skip	= 1,
		[XmlEnum]
		Lax		= 2, 
		[XmlEnum]
		Strict	= 3, 
	}
}
