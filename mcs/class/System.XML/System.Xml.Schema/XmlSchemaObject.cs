// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaObject.
	/// </summary>
//	[XmlInclude(typeof(XmlSchema)),
//	XmlInclude(typeof(XmlSchemaAnnotated)),
//	XmlInclude(typeof(XmlSchemaAnnotation)),
//	XmlInclude(typeof(XmlSchemaAppInfo)),
//	XmlInclude(typeof(XmlSchemaDocumentation)),
//	XmlInclude(typeof(XmlSchemaExternal))]
	public abstract class XmlSchemaObject
	{
		private int lineNumber;
		private int linePosition;
		private string sourceUri;
		private XmlSerializerNamespaces namespaces;

		protected XmlSchemaObject()
		{
			namespaces = new XmlSerializerNamespaces();
		}
		[XmlIgnore]
		public int LineNumber 
		{ 
			get{ return lineNumber; } 
			set{ lineNumber = value; } 
		}
		[XmlIgnore]
		public int LinePosition 
		{ 
			get{ return linePosition; } 
			set{ linePosition = value; } 
		}
		[XmlIgnore]
		public string SourceUri 
		{ 
			get{ return sourceUri; } 
			set{ sourceUri = value; } 
		}

		// Undocumented Property
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces Namespaces 
		{ 
			get{ return namespaces; } 
			set{ namespaces = value; } 
		}
	}
}