// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaComplexContent.
	/// </summary>
	public class XmlSchemaComplexContent : XmlSchemaContentModel
	{
		private XmlSchemaContent content;
		private bool isMixed;

		public XmlSchemaComplexContent()
		{}

		[XmlElement("restriction",typeof(XmlSchemaComplexContentRestriction),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("extension",typeof(XmlSchemaComplexContentExtension),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaContent Content 
		{
			get{ return  content; } 
			set{ content = value; }
		}

		[System.Xml.Serialization.XmlAttribute("mixed")]
		public bool IsMixed 
		{
			get{ return  isMixed; } 
			set{ isMixed = value; }
		}
	}
}
