//
// XmlDictionaryString.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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

namespace System.Xml
{
	public class XmlDictionaryString
	{
		static XmlDictionaryString empty = new XmlDictionaryString (
			XmlDictionary.EmptyDictionary.Instance,
			String.Empty, 0);

		public static XmlDictionaryString Empty {
			get { return empty; }
		}

		readonly IXmlDictionary dict;
		readonly string value;
		readonly int key;

		public XmlDictionaryString (IXmlDictionary dictionary,
			string value, int key)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (key < 0 || key > (int.MaxValue/4))
				throw new ArgumentOutOfRangeException ("key");
			this.dict = dictionary;
			this.value = value;
			this.key = key;
		}

		public IXmlDictionary Dictionary {
			get { return dict; }
		}

		public int Key {
			get { return key; }
		}

		public string Value {
			get { return value; }
		}

		public override string ToString ()
		{
			return value;
		}
	}
}
