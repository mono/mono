//
// ServiceDocument.cs
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
	public class ServiceDocument
	{
		[MonoTODO]
		public static TServiceDocument Load<TServiceDocument> (XmlReader reader)
			where TServiceDocument : ServiceDocument, new()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceDocument Load (XmlReader reader)
		{
			throw new NotImplementedException ();
		}


		public ServiceDocument ()
		{
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
			Workspaces = new Collection<Workspace> ();
		}

		public ServiceDocument (IEnumerable<Workspace> workspaces)
			: this ()
		{
			if (workspaces == null)
				throw new ArgumentNullException ("workspaces");

			foreach (var w in workspaces)
				Workspaces.Add (w);
		}

		ServiceDocumentFormatter formatter;

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

		public string Language { get; set; }

		public Collection<Workspace> Workspaces { get; private set; }


		protected internal virtual Workspace CreateWorkspace ()
		{
			return new Workspace ();
		}

		public ServiceDocumentFormatter GetFormatter ()
		{
			if (formatter == null)
				formatter = new AtomPub10ServiceDocumentFormatter (this);
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
