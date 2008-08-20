//
// SyndicationPerson.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class SyndicationPerson : ISyndicationElement
	{
		string email, name, uri;
		SyndicationExtensions extensions = new SyndicationExtensions ();

		public SyndicationPerson ()
		{
		}

		public SyndicationPerson (string email)
		{
			Email = email;
		}

		public SyndicationPerson (string email, string name, string uri)
		{
			Email = email;
			Name = name;
			Uri = uri;
		}

		protected SyndicationPerson (SyndicationPerson source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			email = source.email;
			name = source.name;
			uri = source.uri;
			extensions = source.extensions.Clone ();
		}

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

		public string Email {
			get { return email; }
			set { email = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Uri {
			get { return uri; }
			set { uri = value; }
		}

		public virtual SyndicationPerson Clone ()
		{
			return new SyndicationPerson (this);
		}

		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			return false;
		}

		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			return false;
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
