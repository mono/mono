// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAttributeGroupRef.
	/// </summary>
	public class XmlSchemaAttributeGroupRef : XmlSchemaAnnotated
	{
		private XmlQualifiedName refName;

		public XmlSchemaAttributeGroupRef()
		{}
		
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; }
			set{ refName = value; }
		}
	}
}
