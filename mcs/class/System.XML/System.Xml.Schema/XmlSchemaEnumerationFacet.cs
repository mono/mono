// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaEnumerationFacet.
	/// </summary>
	public class XmlSchemaEnumerationFacet : XmlSchemaFacet
	{
		private static string xmlname = "enumeration";
		public XmlSchemaEnumerationFacet()
		{
		}

		//<enumeration
		//  id = ID
		//  value = anySimpleType
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</enumeration>
		internal static XmlSchemaEnumerationFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaEnumerationFacet enumeration = new XmlSchemaEnumerationFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaEnumerationFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			enumeration.LineNumber = reader.LineNumber;
			enumeration.LinePosition = reader.LinePosition;
			enumeration.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					enumeration.Id = reader.Value;
				}
				else if(reader.Name == "value")
				{
					enumeration.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						enumeration.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						enumeration.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return enumeration;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaEnumerationFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						enumeration.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return enumeration;
		}
	}
}
