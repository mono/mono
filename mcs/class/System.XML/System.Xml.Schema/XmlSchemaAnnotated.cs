// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnnotated.
	/// </summary>

	public class XmlSchemaAnnotated : XmlSchemaObject
	{
		private XmlSchemaAnnotation annotation;
		private string id;
		private XmlAttribute[] unhandledAttributes;

		public XmlSchemaAnnotated()
		{}
		
		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{ 
			get{ return  id; } 
			set{ id = value; } 
		}
		
		[XmlElement("annotation",Namespace=XmlSchema.Namespace)]
		public XmlSchemaAnnotation Annotation 
		{ 
			get{ return  annotation; } 
			set{ annotation = value; } 
		}
		
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{ 
			get
			{
				if(unhandledAttributeList != null)
				{
					unhandledAttributes = (XmlAttribute[]) unhandledAttributeList.ToArray(typeof(XmlAttribute));
					unhandledAttributeList = null;
				}
				return unhandledAttributes;
			}
			set
			{ 
				unhandledAttributes = value; 
				unhandledAttributeList = null;
			}
		}
	}
}
