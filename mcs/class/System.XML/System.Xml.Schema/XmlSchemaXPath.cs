// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaXPath.
	/// </summary>
	public class XmlSchemaXPath : XmlSchemaAnnotated
	{
		private string xpath;

		public XmlSchemaXPath()
		{
		}
		[DefaultValue("")]
		[XmlAttribute]
		public string XPath 
		{
			get{ return  xpath; } 
			set{ xpath = value; }
		}
	}
}