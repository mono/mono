//
// TextSyndicationContent.cs
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
	public class TextSyndicationContent : SyndicationContent
	{
		string text;
		TextSyndicationContentKind kind;

		public TextSyndicationContent (string text)
			: this (text, TextSyndicationContentKind.Plaintext)
		{
		}

		public TextSyndicationContent (string text, TextSyndicationContentKind textKind)
		{
			this.text = text;
			kind = textKind;
		}

		protected TextSyndicationContent (TextSyndicationContent source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			text = source.text;
			kind = source.kind;
		}

		public override SyndicationContent Clone ()
		{
			return new TextSyndicationContent (this);
		}

		protected override void WriteContentsTo (XmlWriter writer)
		{
			writer.WriteString (text ?? String.Empty);
		}

		public string Text {
			get { return text; }
		}

		public override string Type {
			get {
				switch (kind) {
				case TextSyndicationContentKind.Html:
					return "html";
				case TextSyndicationContentKind.XHtml:
					return "xhtml";
				default:
					return "text";
				}
			}
		}
	}
}
