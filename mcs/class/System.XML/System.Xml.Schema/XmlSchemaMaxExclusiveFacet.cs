// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaMaxExclusiveFacet.
	/// </summary>
	public class XmlSchemaMaxExclusiveFacet : XmlSchemaFacet
	{
		private static string xmlname = "maxExclusive";

		public XmlSchemaMaxExclusiveFacet()
		{
		}

		//<maxExclusive
		//  fixed = boolean : false
		//  id = ID
		//  value = anySimpleType
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</maxExclusive>
		internal static XmlSchemaMaxExclusiveFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaMaxExclusiveFacet maxex = new XmlSchemaMaxExclusiveFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaMaxExclusiveFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			maxex.LineNumber = reader.LineNumber;
			maxex.LinePosition = reader.LinePosition;
			maxex.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					maxex.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					maxex.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					maxex.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						maxex.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						maxex.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return maxex;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaMaxExclusiveFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						maxex.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return maxex;
		}	
	}
}
