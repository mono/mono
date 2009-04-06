//
// Workspace.cs
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
	public class Workspace
	{
		public Workspace ()
		{
			Collections = new Collection<ResourceCollectionInfo> ();
		}

		public Workspace (TextSyndicationContent title, IEnumerable<ResourceCollectionInfo> collections)
			: this ()
		{
			Title = title;
			if (collections != null)
				foreach (var i in collections)
					Collections.Add (i);
		}

		public Workspace (string title, IEnumerable<ResourceCollectionInfo> collections)
			: this (new TextSyndicationContent (title), collections)
		{
		}

		SyndicationExtensions extensions = new SyndicationExtensions ();

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public Uri BaseUri { get; set; }

		public Collection<ResourceCollectionInfo> Collections { get; private set; }

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

		public TextSyndicationContent Title { get; set; }

		protected internal virtual ResourceCollectionInfo CreateResourceCollection ()
		{
			return new ResourceCollectionInfo ();
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
