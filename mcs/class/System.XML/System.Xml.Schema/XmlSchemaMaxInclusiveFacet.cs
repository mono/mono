// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaMaxInclusiveFacet.
	/// </summary>
	public class XmlSchemaMaxInclusiveFacet : XmlSchemaFacet
	{
		private static string xmlname = "maxInclusive";

		public XmlSchemaMaxInclusiveFacet()
		{
		}

		//<maxInclusive
		//  fixed = boolean : false
		//  id = ID
		//  value = anySimpleType
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</maxInclusive>
		internal static XmlSchemaMaxInclusiveFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaMaxInclusiveFacet maxi = new XmlSchemaMaxInclusiveFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaMaxInclusiveFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			maxi.LineNumber = reader.LineNumber;
			maxi.LinePosition = reader.LinePosition;
			maxi.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					maxi.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					maxi.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					maxi.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						maxi.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						maxi.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return maxi;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaMaxInclusiveFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						maxi.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return maxi;
		}
	}
}
