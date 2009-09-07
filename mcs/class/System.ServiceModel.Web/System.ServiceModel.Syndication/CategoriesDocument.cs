//
// CategoriesDocument.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public abstract class CategoriesDocument
	{
		public static InlineCategoriesDocument Create (Collection<SyndicationCategory> categories)
		{
			return new InlineCategoriesDocument (categories);
		}

		public static ReferencedCategoriesDocument Create (Uri linkToCategoriesDocument)
		{
			return new ReferencedCategoriesDocument (linkToCategoriesDocument);
		}

		public static InlineCategoriesDocument Create (Collection<SyndicationCategory> categories, bool isFixed, string scheme)
		{
			return new InlineCategoriesDocument (categories, isFixed, scheme);
		}

		public static CategoriesDocument Load (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			var f = new AtomPub10CategoriesDocumentFormatter ();
			reader.MoveToContent ();

			CategoriesDocument doc;
			if (reader.GetAttribute ("href") == null)
				doc = new InlineCategoriesDocument ();
			else
				doc = new ReferencedCategoriesDocument ();
			doc.GetFormatter ().ReadFrom (reader);

			return doc;
		}

		internal CategoriesDocument ()
		{
		}

		CategoriesDocumentFormatter formatter;
		SyndicationExtensions extensions = new SyndicationExtensions ();

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public Uri BaseUri { get; set; }

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

		public string Language { get; set; }

		public CategoriesDocumentFormatter GetFormatter ()
		{
			if (formatter == null)
				formatter = new AtomPub10CategoriesDocumentFormatter (this);
			return formatter;
		}

		public void Save (XmlWriter writer)
		{
			GetFormatter ().WriteTo (writer);
		}

		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			var inline = this as InlineCategoriesDocument;
			if (name == "lang" && ns == Namespaces.Xml)
				Language = value;
			else if (name == "base" && ns == Namespaces.Xml)
				BaseUri = new Uri (value, UriKind.RelativeOrAbsolute);
			else if (name == "href" && ns == String.Empty && this is ReferencedCategoriesDocument)
				((ReferencedCategoriesDocument) this).Link = new Uri (value, UriKind.RelativeOrAbsolute);
			else if (name == "fixed" && ns == String.Empty && inline != null && value == "true")
				inline.IsFixed = true;
			else if (name == "scheme" && ns == String.Empty && inline != null)
				inline.Scheme = value;
			else
				return false;
			return true;
		}

		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			var f = GetFormatter ();
			if (!f.CanRead (reader))
				return false;
			f.ReadFrom (reader);
			return true;
		}

		protected internal virtual void WriteAttributeExtensions (XmlWriter writer, string version)
		{
			extensions.WriteAttributeExtensions (writer, version);
		}

		protected internal virtual void WriteElementExtensions (XmlWriter writer, string version)
		{
			extensions.WriteElementExtensions (writer, version);
		}
	}
}
