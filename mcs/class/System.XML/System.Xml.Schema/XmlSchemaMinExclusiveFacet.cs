// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaMinExclusiveFacet.
	/// </summary>
	public class XmlSchemaMinExclusiveFacet : XmlSchemaFacet
	{
		private static string xmlname = "minExclusive";

		public XmlSchemaMinExclusiveFacet()
		{
		}

		//<minExclusive
		//  fixed = boolean : false
		//  id = ID
		//  value = anySimpleType
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</minExclusive>
		internal static XmlSchemaMinExclusiveFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaMinExclusiveFacet minex = new XmlSchemaMinExclusiveFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaMinExclusiveFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			minex.LineNumber = reader.LineNumber;
			minex.LinePosition = reader.LinePosition;
			minex.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					minex.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					minex.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					minex.Value = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					if(reader.Prefix == "xmlns")
						minex.Namespaces.Add(reader.LocalName, reader.Value);
					else if(reader.Name == "xmlns")
						minex.Namespaces.Add("",reader.Value);
					//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return minex;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaMinExclusiveFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						minex.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return minex;
		}	
	}
}
