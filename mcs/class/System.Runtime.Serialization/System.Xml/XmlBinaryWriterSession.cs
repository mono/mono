//
// XmlBinaryWriterSession.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005, 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;

namespace System.Xml
{
	public class XmlBinaryWriterSession
	{
		Dictionary<int,XmlDictionaryString> dic =
			new Dictionary<int,XmlDictionaryString> ();

		public XmlBinaryWriterSession ()
		{
		}

		public void Reset ()
		{
			dic.Clear ();
		}

		// Unlike XmlDictionary, it throws an InvalidOperationException
		// if the same string already exists.
		public virtual bool TryAdd (XmlDictionaryString value,
			out int key)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (TryLookup (value, out key))
				throw new InvalidOperationException ("Argument XmlDictionaryString was already added to the writer session");
			key = dic.Count;
			dic.Add (key, value);
			return true;
		}

		internal bool TryLookup (XmlDictionaryString value,
			out int key)
		{
			foreach (KeyValuePair<int,XmlDictionaryString> e in dic)
				if (e.Value.Value == value.Value) {
					key = e.Key;
					return true;
				}
			key = -1;
			return false;
		}
	}
}
#endif
