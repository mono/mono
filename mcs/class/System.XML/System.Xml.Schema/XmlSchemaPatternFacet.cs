// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaPatternFacet.
	/// </summary>
	public class XmlSchemaPatternFacet : XmlSchemaFacet
	{
		private static string xmlname = "pattern";

		public XmlSchemaPatternFacet()
		{
		}
		//<pattern
		//  id = ID
		//  value = anySimpleType
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</pattern>
		internal static XmlSchemaPatternFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaPatternFacet pattern = new XmlSchemaPatternFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaPatternFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			pattern.LineNumber = reader.LineNumber;
			pattern.LinePosition = reader.LinePosition;
			pattern.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					pattern.Id = reader.Value;
				}
				else if(reader.Name == "value")
				{
					pattern.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						pattern.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						pattern.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return pattern;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaPatternFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						pattern.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return pattern;
		}
	}
}
