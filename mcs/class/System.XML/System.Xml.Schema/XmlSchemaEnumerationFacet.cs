// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaEnumerationFacet.
	/// </summary>
	public class XmlSchemaEnumerationFacet : XmlSchemaFacet
	{
		const string xmlname = "enumeration";
		public XmlSchemaEnumerationFacet()
		{
		}

		internal override Facet ThisFacet { 
			get { return Facet.enumeration;}
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
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,enumeration);
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
