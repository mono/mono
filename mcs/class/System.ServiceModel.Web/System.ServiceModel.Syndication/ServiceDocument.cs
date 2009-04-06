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
	internal class Utility
	{
		public static void WriteAttributeExtensions (Dictionary<XmlQualifiedName,string> attributes, XmlWriter writer, string version)
		{
			foreach (var p in attributes)
				writer.WriteAttributeString (p.Key.Name, p.Key.Namespace, p.Value);
		}

		public static void WriteElementExtensions (SyndicationElementExtensionCollection elements, XmlWriter writer, string version)
		{
			foreach (var x in elements)
				x.WriteTo (writer);
		}
	}

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
			Workspaces = new Collection<Workspace> ();
		}

		public ServiceDocument (IEnumerable<Workspace> workspaces)
			: this ()
		{
			if (workspaces != null)
				foreach (var w in workspaces)
					Workspaces.Add (w);
		}

		ServiceDocumentFormatter formatter;
		SyndicationExtensions extensions = new SyndicationExtensions ();

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public Uri BaseUri { get; set; }

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

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

		public void Save (XmlWriter writer)
		{
			GetFormatter ().WriteTo (writer);
		}

		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			if (version != "http://www.w3.org/2007/app")
				return false;

			switch (ns) {
			case "http://www.w3.org/XML/1998/namespace":
				switch (name) {
				case "base":
					BaseUri = new Uri (value, UriKind.RelativeOrAbsolute);
					return true;
				case "lang":
					Language = value;
					return true;
				}
				return false;
			}
			return false;
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
