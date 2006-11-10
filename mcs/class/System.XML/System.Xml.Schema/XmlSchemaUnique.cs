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
	/// Summary description for XmlSchemaUnique.
	/// </summary>
	public class XmlSchemaUnique : XmlSchemaIdentityConstraint
	{
		const string xmlname = "unique";

		public XmlSchemaUnique()
		{
		}
		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			return base.Compile(h,schema);
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			return errorCount;
		}

		/*
		internal new void error(ValidationEventHandler handle, string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle, this, message);
		}
		*/
		//<unique 
		//  id = ID 
		//  name = NCName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (selector, field+))
		//</unique>
		internal static XmlSchemaUnique Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaUnique unique = new XmlSchemaUnique();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaUnique.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			unique.LineNumber = reader.LineNumber;
			unique.LinePosition = reader.LinePosition;
			unique.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					unique.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					unique.Name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for unique",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,unique);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return unique;

			//  Content: annotation?, selector, field+
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaUnion.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						unique.Annotation = annotation;
					continue;
				}
				if(level <= 2 && reader.LocalName == "selector")
				{
					level = 3;
					XmlSchemaXPath selector = XmlSchemaXPath.Read(reader,h,"selector");
					if(selector != null)
						unique.Selector = selector;
					continue;
				}
				if(level <= 3 && reader.LocalName == "field")
				{
					level = 3;
					if(unique.Selector == null)
						error(h,"selector must be defined before field declarations",null);
					XmlSchemaXPath field = XmlSchemaXPath.Read(reader, h, "field");
					if(field != null)
						unique.Fields.Add(field);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return unique;
		}
	}
}
