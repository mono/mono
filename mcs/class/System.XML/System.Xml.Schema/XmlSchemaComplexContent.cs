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
		{
		}
		[XmlElement]
		public override XmlSchemaContent Content 
		{
			get{ return  content; } 
			set{ content = value; }
		}
		[XmlAttribute]
		public bool IsMixed 
		{
			get{ return  isMixed; } 
			set{ isMixed = value; }
		}
	}
}
