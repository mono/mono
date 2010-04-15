//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;
using System.Xml;

namespace System.Windows.Markup
{
	[ContentProperty ("Text")]
	public sealed class XData
	{
		string text;
		XmlReader reader;

		public string Text {
			get { return text; }
			set {
				if (value == null) {
					text = null;
					reader = null;
				}
				else
					text = value;
			}
		}

		public object XmlReader {
			get {
				if (reader == null)
					reader = System.Xml.XmlReader.Create (new StringReader (text));
				return reader;
			}
			set {
				// silly? yes, it's also a hack in .NET - who cares?
				reader = value as XmlReader;
				text = null;
			}
		}
	}
}
