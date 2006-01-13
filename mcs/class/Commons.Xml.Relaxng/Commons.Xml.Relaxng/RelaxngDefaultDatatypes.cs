//
// RelaxngDefaultDatatypes.cs
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
	public class RelaxngString : RelaxngDatatype
	{
		static RelaxngString instance;
		static RelaxngString ()
		{
			instance = new RelaxngString ();
		}

		internal static RelaxngString Instance {
			get { return instance; }
		}

		public override string Name { get { return "string"; } }
		public override string NamespaceURI { get { return String.Empty; } }

		internal override bool IsContextDependent {
			get { return false; }
		}

		public override bool IsValid (string text, XmlReader reader)
		{
			return true;
		}

		public override object Parse (string text, XmlReader reader)
		{
			return text;
		}

		public override bool Compare (object o1, object o2)
		{
			return (string) o1 == (string) o2;
		}
	}

	public class RelaxngToken : RelaxngDatatype
	{
		static RelaxngToken instance;
		static RelaxngToken ()
		{
			instance = new RelaxngToken ();
		}

		internal static RelaxngToken Instance {
			get { return instance; }
		}

		public override string Name { get { return "token"; } }
		public override string NamespaceURI { get { return String.Empty; } }

		internal override bool IsContextDependent {
			get { return false; }
		}

		public override bool IsValid (string text, XmlReader reader)
		{
			return true;
		}

		public override object Parse (string text, XmlReader reader)
		{
			return Util.NormalizeWhitespace (text);
		}

		int SkipWhitespaces (string s, int i)
		{
			while (i < s.Length) {
				switch (s [i]) {
				case '\n': case '\r': case ' ': case '\t':
					i++;
					continue;
				}
				break;
			}
			return i;
		}

		public override bool Compare (object o1, object o2)
		{
			string s1 = o1 as string;
			string s2 = o2 as string;

			int i1 = 0;
			int i2 = 0;

			while (i1 < s1.Length && i2 < s2.Length) {
				i1 = SkipWhitespaces (s1, i1);
				i2 = SkipWhitespaces (s2, i2);
				while (i1 < s1.Length && i2 < s2.Length) {
					if (s1 [i1] != s2 [i2])
						return false;
					i1++;
					i2++;
					if (i1 == s1.Length || i2 == s2.Length)
						break;
					if (XmlChar.IsWhitespace (s1 [i1])) {
						if (!XmlChar.IsWhitespace (s2 [i2]))
							return false;
						else
							break;
					}
					else if (XmlChar.IsWhitespace (s2 [i2]))
						return false;
				}
			}
			i1 = SkipWhitespaces (s1, i1);
			i2 = SkipWhitespaces (s2, i2);
			return i1 == s1.Length && i2 == s2.Length;
		}

		public override bool CompareString (string s1, string s2, XmlReader reader)
		{
			return Compare (s1, s2);
		}
	}
}
