// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaContentModel.
	/// </summary>
	public abstract class XmlSchemaContentModel : XmlSchemaAnnotated
	{
		protected XmlSchemaContentModel()
		{
		}
		[XmlIgnore]
		public abstract XmlSchemaContent Content {get; set;}
	}
}
