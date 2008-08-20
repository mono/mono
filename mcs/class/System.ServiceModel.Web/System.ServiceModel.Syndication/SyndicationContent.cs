//
// SyndicationContent.cs
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
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	public abstract class SyndicationContent
	{
		#region Static members

		public static TextSyndicationContent CreateHtmlContent (string content)
		{
			return new TextSyndicationContent (content, TextSyndicationContentKind.Html);
		}

		public static TextSyndicationContent CreatePlaintextContent (string content)
		{
			return new TextSyndicationContent (content, TextSyndicationContentKind.Plaintext);
		}

		public static TextSyndicationContent CreateXhtmlContent (string content)
		{
			return new TextSyndicationContent (content, TextSyndicationContentKind.XHtml);
		}

		public static UrlSyndicationContent CreateUrlContent (Uri url, string mediaType)
		{
			return new UrlSyndicationContent (url, mediaType);
		}

		public static XmlSyndicationContent CreateXmlContent (object dataContractObject)
		{
			return new XmlSyndicationContent (null, dataContractObject, (XmlObjectSerializer) null);
		}

		public static XmlSyndicationContent CreateXmlContent (object dataContractObject, XmlObjectSerializer dataContractSerializer)
		{
			return new XmlSyndicationContent (null, dataContractObject, dataContractSerializer);
		}

		public static XmlSyndicationContent CreateXmlContent (object xmlSerializerObject, XmlSerializer serializer)
		{
			return new XmlSyndicationContent (null, xmlSerializerObject, serializer);
		}

		public static XmlSyndicationContent CreateXmlContent (XmlReader reader)
		{
			return new XmlSyndicationContent (reader);
		}

		#endregion

		#region Instance members

		SyndicationExtensions extensions = new SyndicationExtensions ();

		protected SyndicationContent ()
		{
		}

		protected SyndicationContent (SyndicationContent source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			extensions = source.extensions.Clone ();
		}

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public abstract string Type { get; }

		public abstract SyndicationContent Clone ();

		protected abstract void WriteContentsTo (XmlWriter writer);

		public void WriteTo (XmlWriter writer, string outerElementName, string outerElementNamespace)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (outerElementName == null)
				throw new ArgumentNullException ("outerElementName");
			if (outerElementNamespace == null)
				throw new ArgumentNullException ("outerElementNamespace");
			writer.WriteStartElement (outerElementName, outerElementNamespace);
			writer.WriteAttributeString ("type", Type);
			WriteContentsTo (writer);
			writer.WriteFullEndElement ();
		}

		#endregion
	}
}
