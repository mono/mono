// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaMinInclusiveFacet.
	/// </summary>
	public class XmlSchemaMinInclusiveFacet : XmlSchemaFacet
	{
		private static string xmlname = "minInclusive";

		public XmlSchemaMinInclusiveFacet()
		{
		}
		//<minInclusive
		//  fixed = boolean : false
		//  id = ID
		//  value = anySimpleType
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</minInclusive>
		internal static XmlSchemaMinInclusiveFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaMinInclusiveFacet mini = new XmlSchemaMinInclusiveFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaMinInclusiveFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			mini.LineNumber = reader.LineNumber;
			mini.LinePosition = reader.LinePosition;
			mini.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					mini.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					mini.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					mini.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						mini.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						mini.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return mini;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaMinInclusiveFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						mini.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return mini;
		}
	}
}