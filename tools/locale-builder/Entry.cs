//
// Entry.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Text;
using System.Collections.Generic;

namespace Mono.Tools.LocaleBuilder
{
	public class Entry
	{
		public static readonly Mapping General = new Mapping ();
		public static readonly Mapping Patterns  = new Mapping ();
		public static readonly Mapping DateTimeStrings  = new Mapping ();

		public class Mapping
		{
			// maps strings to indexes
			Dictionary<string, int> hash = new Dictionary<string, int> ();
			List<string> string_order = new List<string> ();
			// idx 0 is reserved to indicate null
			int curpos = 1;

			// serialize the strings in Hashtable.
			public string GetStrings ()
			{
				Console.WriteLine ("Total string data size: {0}", curpos);
				if (curpos > UInt16.MaxValue)
					throw new Exception ("need to increase idx size in culture-info.h");
				StringBuilder ret = new StringBuilder ();
				// the null entry
				ret.Append ("\t\"\\0\"\n");
				foreach (string s in string_order) {
					ret.Append ("\t\"");
					ret.Append (s);
					ret.Append ("\\0\"\n");
				}
				return ret.ToString ();
			}

			public int AddString (string s, int size)
			{
				if (!hash.ContainsKey (s)) {
					int ret;
					string_order.Add (s);
					ret = curpos;
					hash.Add (s, curpos);
					curpos += size + 1; // null terminator
					return ret;
				}

				return hash[s];
			}
		}

		protected static StringBuilder AppendNames (StringBuilder builder, IList<string> names)
		{
			builder.Append ('{');
			for (int i = 0; i < names.Count; i++) {
				if (i > 0)
					builder.Append (", ");

				builder.Append (Encode (DateTimeStrings, names[i]));
			}
			builder.Append ("}");

			return builder;
		}


		public static string EncodeStringIdx (string str)
		{
			return Encode (General, str);
		}

		protected static string EncodePatternStringIdx (string str)
		{
			return Encode (Patterns, str);
		}

		static string Encode (Mapping mapping, string str)
		{
			if (str == null)
				return "0";

			StringBuilder ret = new StringBuilder ();
			byte[] ba = new UTF8Encoding ().GetBytes (str);
			bool in_hex = false;
			foreach (byte b in ba) {
				if (b > 127 || (in_hex && is_hex (b))) {
					ret.AppendFormat ("\\x{0:x}", b);
					in_hex = true;
				} else {
					if (b == '\\')
						ret.Append ('\\');
					ret.Append ((char) b);
					in_hex = false;
				}
			}
			int res = mapping.AddString (ret.ToString (), ba.Length);
			return res.ToString ();
		}

		static bool is_hex (int e)
		{
			return (e >= '0' && e <= '9') || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
		}
	}
}
