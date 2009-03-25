//
// XmlBinaryReaderSession.cs
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
using System.Collections.Generic;

namespace System.Xml
{
	public class XmlBinaryReaderSession : IXmlDictionary
	{
		XmlDictionary dic = new XmlDictionary ();
		Dictionary<int,XmlDictionaryString> store = new Dictionary<int,XmlDictionaryString> ();

		public XmlBinaryReaderSession ()
		{
		}

		public XmlDictionaryString Add (int id, string value)
		{
			var v = dic.Add (value);
			store [id] = v;
			return v;
		}

		public void Clear ()
		{
			store.Clear ();
		}

		public bool TryLookup (int key, out XmlDictionaryString result)
		{
			return store.TryGetValue (key, out result);
		}

		public bool TryLookup (string value, out XmlDictionaryString result)
		{
			foreach (var v in store.Values)
				if (v.Value == value) {
					result = v;
					return true;
				}
			result = null;
			return false;
		}

		public bool TryLookup (XmlDictionaryString value,
			out XmlDictionaryString result)
		{
			foreach (var v in store.Values)
				if (v == value) {
					result = v;
					return true;
				}
			result = null;
			return false;
		}
	}
}
#endif
