// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAny.
	/// </summary>
	public class XmlSchemaAny : XmlSchemaParticle
	{
		private string nameSpace;
		private XmlSchemaContentProcessing processing;

		public XmlSchemaAny()
		{
			nameSpace = string.Empty;
			processing = XmlSchemaContentProcessing.Strict;
		}
		[XmlAttribute]
		public string Namespace 
		{
			get{ return  nameSpace; } 
			set{ nameSpace = value; }
		}
		[DefaultValue(XmlSchemaContentProcessing.Strict)]
		[XmlAttribute]
		public XmlSchemaContentProcessing ProcessContents 
		{ 
			get{ return processing; } 
			set{ processing = value; }
		}
	}
}
