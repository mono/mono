// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaTotalDigitsFacet.
	/// </summary>
	public class XmlSchemaTotalDigitsFacet : XmlSchemaNumericFacet
	{
		private static string xmlname = "totalDigits";

		public XmlSchemaTotalDigitsFacet()
		{
		}
			
		internal override Facet ThisFacet { 
			get { return Facet.totalDigits;}
		}
	
		//<totalDigits
		//  fixed = boolean : false
		//  id = ID
		//  value = positiveInteger
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</totalDigits>
		internal static XmlSchemaTotalDigitsFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaTotalDigitsFacet td = new XmlSchemaTotalDigitsFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaTotalDigitsFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			td.LineNumber = reader.LineNumber;
			td.LinePosition = reader.LinePosition;
			td.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					td.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					td.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					td.Value = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,td);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return td;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaTotalDigitsFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						td.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return td;
		}	

	}
}
