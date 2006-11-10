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
	/// Summary description for XmlSchemaWhiteSpaceFacet.
	/// </summary>
	public class XmlSchemaWhiteSpaceFacet : XmlSchemaFacet
	{
		const string xmlname = "whiteSpace";

		public XmlSchemaWhiteSpaceFacet()
		{
		}
			
		internal override Facet ThisFacet { 
			get { return Facet.whiteSpace;}
		}
	
		//	<whiteSpace
		//	fixed = boolean : false
		//	id = ID
		//	value = (collapse | preserve | replace)
		//	{any attributes with non-schema namespace . . .}>
		//	Content: (annotation?)
		//	</whiteSpace>
		internal static XmlSchemaWhiteSpaceFacet Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaWhiteSpaceFacet ws = new XmlSchemaWhiteSpaceFacet();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaWhiteSpaceFacet.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			ws.LineNumber = reader.LineNumber;
			ws.LinePosition = reader.LinePosition;
			ws.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					ws.Id = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					Exception innerex;
					ws.IsFixed = XmlSchemaUtil.ReadBoolAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for fixed attribute",innerex);
				}
				else if(reader.Name == "value")
				{
					ws.Value = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for "+xmlname,null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,ws);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return ws;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaWhiteSpaceFacet.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						ws.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return ws;
		}


	}
}
