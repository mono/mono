// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaMinLengthFacet.
	/// </summary>
	public class XmlSchemaMinLengthFacet : XmlSchemaNumericFacet
	{
		private static string xmlname = "minLength";

		public XmlSchemaMinLengthFacet()
		{
		}

		//<minLength
		//  fixed = boolean : false
		//  id = ID
		//  value = nonNegativeInteger
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</minLength>
		internal static XmlSchemaMinLengthFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaMinLengthFacet length = new XmlSchemaMinLengthFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaMinLengthFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			length.LineNumber = reader.LineNumber;
			length.LinePosition = reader.LinePosition;
			length.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					length.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					length.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					length.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						length.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						length.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return length;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaMinLengthFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						length.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return length;
		}
	}
}
