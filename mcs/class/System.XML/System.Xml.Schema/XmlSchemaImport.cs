// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaImport.
	/// </summary>
	public class XmlSchemaImport : XmlSchemaExternal
	{
		private XmlSchemaAnnotation annotation;
		private string nameSpace;
		private static string xmlname = "import";

		public XmlSchemaImport()
		{
		}
		
		[System.Xml.Serialization.XmlAttribute("namespace")]
		public string Namespace 
		{
			get{ return  nameSpace; } 
			set{ nameSpace = value; }
		}

		[XmlElement("annotation",Namespace=XmlSchema.Namespace)]
		public XmlSchemaAnnotation Annotation 
		{
			get{ return  annotation; } 
			set{ annotation = value; }
		}
		//<import 
		//  id = ID 
		//  namespace = anyURI 
		//  schemaLocation = anyURI 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</import>
		internal static XmlSchemaImport Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaImport import = new XmlSchemaImport();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != "import")
			{
				error(h,"Should not happen :1: XmlSchemaImport.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			import.LineNumber = reader.LineNumber;
			import.LinePosition = reader.LinePosition;
			import.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					import.Id = reader.Value;
				}
				else if(reader.Name == "namespace")
				{
					import.nameSpace = reader.Value;
				}
				else if(reader.Name == "schemaLocation")
				{
					import.SchemaLocation = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for import",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,import);
				}
			}

			reader.MoveToElement();	
			if(reader.IsEmptyElement)
				return import;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaImport.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						import.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return import;
		}
	}
}
