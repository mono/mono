// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnnotated.
	/// </summary>
	// Include all childrens of this class
//	[XmlInclude(typeof(XmlSchemaAnyAttribute)),
//	XmlInclude(typeof(XmlSchemaAttribute)),
//	XmlInclude(typeof(XmlSchemaAttributeGroup)),
//	XmlInclude(typeof(XmlSchemaAttributeGroupRef)),
//	XmlInclude(typeof(XmlSchemaContent)),
//	XmlInclude(typeof(XmlSchemaContentModel)),
//	XmlInclude(typeof(XmlSchemaFacet)),
//	XmlInclude(typeof(XmlSchemaGroup)),
//	XmlInclude(typeof(XmlSchemaIdentityConstraint)),
//	XmlInclude(typeof(XmlSchemaNotation)),
//	XmlInclude(typeof(XmlSchemaParticle)),
//	XmlInclude(typeof(XmlSchemaSimpleTypeContent)),
//	XmlInclude(typeof(XmlSchemaType)),
//	XmlInclude(typeof(XmlSchemaXPath))]
	public class XmlSchemaAnnotated : XmlSchemaObject
	{
		private XmlSchemaAnnotation annotation;
		private string id;
		private XmlAttribute[] unhandledAttributes;

		public XmlSchemaAnnotated()
		{}
		
		[XmlElement("annotation",Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaAnnotation Annotation 
		{ 
			get{ return  annotation; } 
			set{ annotation = value; } 
		}
		
		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{ 
			get{ return  id; } 
			set{ id = value; } 
		}
		
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{ 
			get{ return unhandledAttributes; } 
			set{ unhandledAttributes = value; } 
		}
	}
}
