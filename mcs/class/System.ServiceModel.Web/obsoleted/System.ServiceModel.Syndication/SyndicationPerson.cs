//
// SyndicationPerson.cs
//
// Author:
//      Stephen A Jazdzewski (Steve@Jazd.com)
//
// Copyright (C) 2007 Stephen A Jazdzewski
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
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel.Syndication;

namespace System.ServiceModel.Syndication
{
	[MonoTODO]
	public class SyndicationPerson {
		[MonoTODO]
		public SyndicationPerson ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SyndicationPerson (string email)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SyndicationPerson (string email, string name, string uri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Collection <TExtension> ReadElementExtensions <TExtension> (string extensionName, string extensionNamespace)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Collection <TExtension> ReadElementExtensions <TExtension> (string extensionName, string extensionNamespace,
										   XmlSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Collection <TExtension> ReadElementExtensions <TExtension> (string extensionName, string extensionNamespace,
										   XmlObjectSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseElement (XmlReader reader, SyndicationSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseAttribute (string name, string ns, string value,
								   SyndicationSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader GetReaderAtElementExtensions ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void WriteAttributeExtensions (XmlWriter writer, SyndicationSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void WriteElementExtensions (XmlWriter writer, SyndicationSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		public Dictionary <XmlQualifiedName, string> AttributeExtensions {
			get {throw new NotImplementedException ();}
		}

		public SyndicationElementExtensionCollection ElementExtensions {
			get {throw new NotImplementedException ();}
		}

		public string Email {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		public string Name {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		public string Uri {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}
	}
}
