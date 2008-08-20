//
// XmlDictionary.cs
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xml
{
	public class XmlDictionary : IXmlDictionary
	{
		internal class EmptyDictionary : XmlDictionary
		{
			public static readonly EmptyDictionary Instance =
				new EmptyDictionary ();

			public EmptyDictionary ()
				: base (1)
			{
			}
		}

		static XmlDictionary empty = new XmlDictionary (true);

		public static IXmlDictionary Empty {
			get { return empty; }
		}

		readonly bool is_readonly;
		Dictionary<string, XmlDictionaryString> dict;
		List<XmlDictionaryString> list;

		public XmlDictionary ()
		{
			dict = new Dictionary<string, XmlDictionaryString> ();
			list = new List<XmlDictionaryString> ();
		}

		public XmlDictionary (int capacity)
		{
			dict = new Dictionary<string, XmlDictionaryString> (capacity);
			list = new List<XmlDictionaryString> (capacity);
		}

		// for static empty.
		private XmlDictionary (bool isReadOnly)
			: this (1)
		{
			is_readonly = isReadOnly;
		}

		public virtual XmlDictionaryString Add (string value)
		{
			if (is_readonly)
				throw new InvalidOperationException ();
			XmlDictionaryString ret;
			if (dict.TryGetValue (value, out ret))
				return ret;
			ret = new XmlDictionaryString (this, value, dict.Count);
			dict.Add (value, ret);
			list.Add (ret);
			return ret;
		}

		public virtual bool TryLookup (
			int key, out XmlDictionaryString result)
		{
			if (key < 0 || dict.Count <= key) {
				result = null;
				return false;
			}
			result = list [key];
			return true;
		}

		public virtual bool TryLookup (string value,
			out XmlDictionaryString result)
		{
			if (value == null)
				throw new ArgumentNullException ();
			return dict.TryGetValue (value, out result);
		}

		public virtual bool TryLookup (XmlDictionaryString value,
			out XmlDictionaryString result)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (value.Dictionary != this) {
				result = null;
				return false;
			}
			for (int i = 0; i < list.Count; i++) {
				if (object.ReferenceEquals (list [i], value)) {
					result = value;
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}
#endif
