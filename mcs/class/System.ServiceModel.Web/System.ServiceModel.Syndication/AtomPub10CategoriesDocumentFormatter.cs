//
// AtomPub10CategoriesDocumentFormatter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	public class AtomPub10CategoriesDocumentFormatter : CategoriesDocumentFormatter, IXmlSerializable
	{
		public AtomPub10CategoriesDocumentFormatter ()
		{
		}

		public AtomPub10CategoriesDocumentFormatter (CategoriesDocument documentToWrite)
			: base (documentToWrite)
		{
		}

		public AtomPub10CategoriesDocumentFormatter (Type inlineDocumentType, Type referencedDocumentType)
		{
			if (inlineDocumentType == null)
				throw new ArgumentNullException ("inlineDocumentType");
			if (referencedDocumentType == null)
				throw new ArgumentNullException ("referencedDocumentType");

			inline_type = inlineDocumentType;
			ref_type = referencedDocumentType;
		}

		Type inline_type, ref_type;

		public override string Version {
			get { return "http://www.w3.org/2007/app"; }
		}

		[MonoTODO]
		public override bool CanRead (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		protected override InlineCategoriesDocument CreateInlineCategoriesDocument ()
		{
			return (InlineCategoriesDocument) Activator.CreateInstance (inline_type, new object [0]);
		}

		protected override ReferencedCategoriesDocument CreateReferencedCategoriesDocument ()
		{
			return (ReferencedCategoriesDocument) Activator.CreateInstance (ref_type, new object [0]);
		}

		[MonoTODO]
		public override void ReadFrom (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadFrom (reader);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			WriteTo (writer);
		}
	}
}
