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
		public static IXmlSchemaInfo GetSchemaInfo (this XAttribute source)
		{
			return source.Annotation<IXmlSchemaInfo> ();
		}

		public static IXmlSchemaInfo GetSchemaInfo (this XElement source)
		{
			return source.Annotation<IXmlSchemaInfo> ();
		}

		public static void Validate (this XAttribute source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			Validate (source, partialValidationType, schemas, validationEventHandler, false);
		}

		public static void Validate (this XAttribute source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			var nsmgr = new XmlNamespaceManager (new NameTable ());
			var v = new XmlSchemaValidator (nsmgr.NameTable, schemas, nsmgr, XmlSchemaValidationFlags.None);
			if (validationEventHandler != null)
				v.ValidationEventHandler += validationEventHandler;
			if (partialValidationType != null)
				v.Initialize (partialValidationType);
			else
				v.Initialize ();
			var xi = addSchemaInfo ? new XmlSchemaInfo () : null;
			v.ValidateAttribute (source.Name.LocalName, source.Name.NamespaceName, source.Value, xi);
		}

		public static void Validate (this XDocument source, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			Validate (source, schemas, validationEventHandler, false);
		}

		public static void Validate (this XDocument source, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (schemas == null)
				throw new ArgumentNullException ("schemas");
			var xrs = new XmlReaderSettings () { ValidationType = ValidationType.Schema };
			xrs.Schemas = schemas;
			xrs.ValidationEventHandler += validationEventHandler;
			var xsource = new XNodeReader (source);
			var xr = XmlReader.Create (xsource, xrs);
			while (xr.Read ()) {
				if (addSchemaInfo) {
					if (xr.NodeType == XmlNodeType.Element) {
						xsource.CurrentNode.AddAnnotation (xr.SchemaInfo);
						while (xr.MoveToNextAttribute ())
							if (xr.NamespaceURI != XUtil.XmlnsNamespace)
								xsource.GetCurrentAttribute ().AddAnnotation (xr.SchemaInfo);
						xr.MoveToElement ();
					}
				}
			}
		}

		[MonoTODO]
		public static void Validate (this XElement source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Validate (this XElement source, XmlSchemaObject partialValidationType, XmlSchemaSet schemas, ValidationEventHandler validationEventHandler, bool addSchemaInfo)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
