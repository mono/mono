//
// RelaxngDatatype.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
//

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

namespace Commons.Xml.Relaxng
{
	public abstract class RelaxngDatatype
	{
		public abstract string Name { get; }
		public abstract string NamespaceURI { get; }

		internal virtual bool IsContextDependent {
			// safe default value
			get { return true; }
		}

		public abstract object Parse (string text, XmlReader reader);

		public virtual bool Compare (object o1, object o2)
		{
			return o1 != null ? o1.Equals (o2) : o2 == null;
		}

		public virtual bool CompareString (string s1, string s2, XmlReader reader)
		{
			return Compare (Parse (s1, reader), Parse (s2, reader));
		}

		public virtual bool IsValid (string text, XmlReader reader) 
		{
			try {
				Parse (text, reader);
			} catch (Exception) {
				return false;
			}
			return true;
		}
	}
}
