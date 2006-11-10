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
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaImport.
	/// </summary>
	public class XmlSchemaImport : XmlSchemaExternal
	{
		private XmlSchemaAnnotation annotation;
		private string nameSpace;
		const string xmlname = "import";

		public XmlSchemaImport()
		{
		}
		
		[System.Xml.Serialization.XmlAttribute("namespace", DataType="anyURI")]
		public string Namespace 
		{
			get{ return  nameSpace; } 
			set{ nameSpace = value; }
		}

		[XmlElement("annotation", Type=typeof (XmlSchemaAnnotation))]
		public XmlSchemaAnnotation Annotation 
		{
			get{ return  annotation; } 
			set{ annotation = value; }
		}
		//<import 
		//  id = ID 
		//  namespace = anyURI 
		//  schemaLocation = anyURI 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</import>
		internal static XmlSchemaImport Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaImport import = new XmlSchemaImport();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != "import")
			{
				error(h,"Should not happen :1: XmlSchemaImport.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			import.LineNumber = reader.LineNumber;
			import.LinePosition = reader.LinePosition;
			import.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					import.Id = reader.Value;
				}
				else if(reader.Name == "namespace")
				{
					import.nameSpace = reader.Value;
				}
				else if(reader.Name == "schemaLocation")
				{
					import.SchemaLocation = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for import",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,import);
				}
			}

			reader.MoveToElement();	
			if(reader.IsEmptyElement)
				return import;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaImport.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						import.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return import;
		}
	}
}
