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
		[MonoTODO]
		public static InlineCategoriesDocument Create (Collection<SyndicationCategory> categories)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ReferencedCategoriesDocument Create (Uri linkToCategoriesDocument)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static InlineCategoriesDocument Create (Collection<SyndicationCategory> categories, bool isFixed, string scheme)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static CategoriesDocument Load (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		public CategoriesDocument ()
		{
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
		}

		CategoriesDocumentFormatter formatter;

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

		public string Language { get; set; }

		public CategoriesDocumentFormatter GetFormatter ()
		{
			if (formatter == null)
				formatter = new AtomPub10CategoriesDocumentFormatter (this);
			return formatter;
		}

		[MonoTODO]
		public void Save (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void WriteAttributeExtensions (XmlWriter writer, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void WriteElementExtensions (XmlWriter writer, string version)
		{
			throw new NotImplementedException ();
		}
	}
}
