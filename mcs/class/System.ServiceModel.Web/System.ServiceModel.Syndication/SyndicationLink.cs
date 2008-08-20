//
// SyndicationLink.cs
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
	public class SyndicationLink : ISyndicationElement
	{
		#region Static members

		public static SyndicationLink CreateAlternateLink (Uri uri)
		{
			return CreateAlternateLink (uri, null);
		}

		public static SyndicationLink CreateAlternateLink (Uri uri, string mediaType)
		{
			return new SyndicationLink (uri, "alternate", null, mediaType, 0);
		}

		public static SyndicationLink CreateMediaEnclosureLink (Uri uri, string mediaType, long length)

		{
			return new SyndicationLink (uri, "enclosure", null, mediaType, length);
		}

		public static SyndicationLink CreateSelfLink (Uri uri)
		{
			return CreateSelfLink (uri, null);
		}

		public static SyndicationLink CreateSelfLink (Uri uri, string mediaType)
		{
			return new SyndicationLink (uri, "self", null, mediaType, 0);
		}

		#endregion

		#region Instance members

		Uri base_uri, href;
		long length;
		string rel, title, type;
		SyndicationExtensions extensions = new SyndicationExtensions ();

		public SyndicationLink ()
		{
		}

		public SyndicationLink (Uri uri)
		{
			Uri = uri;
		}

		public SyndicationLink (Uri uri, string relationshipType, string title, string mediaType, long length)
		{
			Uri = uri;
			RelationshipType = relationshipType;
			Title = title;
			MediaType = mediaType;
			Length = length;
		}

		protected SyndicationLink (SyndicationLink source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			base_uri = source.base_uri;
			href = source.href;
			length = source.length;
			rel = source.rel;
			title = source.title;
			type = source.type;
			extensions = source.extensions.Clone ();
		}

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

		public Uri BaseUri {
			get { return base_uri; }
			set {
				if (value != null && !value.IsAbsoluteUri)
					throw new ArgumentException ("Base URI must not be relative");
				base_uri = value;
			}
		}

		public long Length {
			get { return length; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				length = value;
			}
		}

		public string MediaType {
			get { return type; }
			set { type = value; }
		}

		public string RelationshipType {
			get { return rel; }
			set { rel = value; }
		}

		public string Title {
			get { return title; }
			set { title = value; }
		}

		public Uri Uri {
			get { return href; }
			set { href = value; }
		}

		public virtual SyndicationLink Clone ()
		{
			return new SyndicationLink (this);
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

		public Uri GetAbsoluteUri ()
		{
			if (href == null)
				return null;
			// our BaseUri is always absolute (unlike .NET).
			return  base_uri != null ? new Uri (base_uri, href.ToString ()) : href.IsAbsoluteUri ? href : null;
		}

		#endregion
	}
}
