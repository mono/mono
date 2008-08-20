//
// SyndicationExtensions.cs
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
	// ISyndicationElement helper.
	internal class SyndicationExtensions
	{
		Dictionary<XmlQualifiedName, string> attributes;
		SyndicationElementExtensionCollection elements;

		public Dictionary<XmlQualifiedName, string> Attributes {
			get {
				if (attributes == null)
					attributes = new Dictionary<XmlQualifiedName, string> ();
				return attributes;
			}
		}

		public SyndicationElementExtensionCollection Elements {
			get {
				if (elements == null)
					elements = new SyndicationElementExtensionCollection ();
				return elements;
			}
		}

		public SyndicationExtensions Clone ()
		{
			SyndicationExtensions ret = new SyndicationExtensions ();
			ret.attributes = attributes == null ? null : new Dictionary<XmlQualifiedName, string> (attributes);
			ret.elements = elements == null ? null : new SyndicationElementExtensionCollection (elements);
			return ret;
		}

		internal void WriteAttributeExtensions (XmlWriter writer, string version)
		{
			if (attributes == null)
				return;

			foreach (KeyValuePair<XmlQualifiedName,string> pair in attributes)
				writer.WriteAttributeString (pair.Key.Name, pair.Key.Namespace, pair.Value);
		}

		internal void WriteElementExtensions (XmlWriter writer, string version)
		{
			if (elements == null)
				return;

			foreach (SyndicationElementExtension el in elements)
				if (el != null)
					el.WriteTo (writer);
		}
	}
}

