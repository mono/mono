// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaInclude.
	/// </summary>
	public class XmlSchemaInclude : XmlSchemaExternal
	{
		private XmlSchemaAnnotation annotation;
		private static string xmlname = "include";

		public XmlSchemaInclude()
		{
		}
		[XmlElement("annotation",Namespace=XmlSchema.Namespace)]
		public XmlSchemaAnnotation Annotation 
		{
			get{ return  annotation; } 
			set{ annotation = value; }
		}

		//<include 
		//  id = ID 
		//  schemaLocation = anyURI 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</include>
		internal static XmlSchemaInclude Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaInclude include = new XmlSchemaInclude();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaInclude.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			include.LineNumber = reader.LineNumber;
			include.LinePosition = reader.LinePosition;
			include.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					include.Id = reader.Value;
				}
				else if(reader.Name == "schemaLocation")
				{
					include.SchemaLocation = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for include",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,include);
				}
			}

			reader.MoveToElement();	
			if(reader.IsEmptyElement)
				return include;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaInclude.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						include.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}

			return include;
		}
	}
}