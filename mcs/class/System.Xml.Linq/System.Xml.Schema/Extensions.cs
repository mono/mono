//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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

#if !NET_2_1

using System;
using System.Xml;
using System.Xml.Linq;

namespace System.Xml.Schema
{
	public static class Extensions
	{
		public static IXmlSchemaInfo GetSchemaInfo (this XAttribute attribute)
		{
			return attribute.Annotation<IXmlSchemaInfo> ();
		}

		public static IXmlSchemaInfo GetSchemaInfo (this XElement element)
		{
			return element.Annotation<IXmlSchemaInfo> ();
		}

		[MonoTODO]
		public static void Validate (this XAttribute attribute, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Validate (this XAttribute attribute, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler handler, bool addSchemaInfo)
		{
			throw new NotImplementedException ();
		}

		public static void Validate (this XDocument document, XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			Validate (document, schemas, handler, false);
		}

		public static void Validate (this XDocument document, XmlSchemaSet schemas, ValidationEventHandler handler, bool addSchemaInfo)
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			var xrs = new XmlReaderSettings () { ValidationType = ValidationType.Schema };
			xrs.Schemas = schemas;
			xrs.ValidationEventHandler += handler;
			var source = new XNodeReader (document);
			var xr = XmlReader.Create (source, xrs);
			while (xr.Read ()) {
				if (addSchemaInfo) {
					if (xr.NodeType == XmlNodeType.Element) {
						source.CurrentNode.AddAnnotation (xr.SchemaInfo);
						while (xr.MoveToNextAttribute ())
							if (xr.NamespaceURI != XUtil.XmlnsNamespace)
								source.GetCurrentAttribute ().AddAnnotation (xr.SchemaInfo);
						xr.MoveToElement ();
					}
				}
			}
		}

		[MonoTODO]
		public static void Validate (this XElement element, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Validate (this XElement element, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler handler, bool addSchemaInfo)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
