// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaKeyref.
	/// </summary>
	public class XmlSchemaKeyref : XmlSchemaIdentityConstraint
	{
		private XmlQualifiedName refer;

		public XmlSchemaKeyref()
		{
		}

		[System.Xml.Serialization.XmlAttribute("refer")]
		public XmlQualifiedName Refer
		{
			get{ return  refer; } 
			set{ refer = value; }
		}
	}
}
