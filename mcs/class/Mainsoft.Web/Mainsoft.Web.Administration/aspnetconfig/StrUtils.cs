//
// System.Web.Util.StrUtils
//
// Author(s):
//  Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc, (http://www.novell.com)
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
using System.Globalization;
using System.Text;

namespace Mainsoft.Web.Util
{
	internal sealed class StrUtils {
		static CultureInfo invariant = CultureInfo.InvariantCulture;
		private StrUtils () { }
		
		public static bool StartsWith (string str1, string str2)
		{
			return StartsWith (str1, str2, false);
		}

		public static bool StartsWith (string str1, string str2, bool ignore_case)
		{
			int l2 = str2.Length;
			if (l2 == 0)
				return true;

			int l1 = str1.Length;
			if (l2 > l1)
				return false;

			return (0 == String.Compare (str1, 0, str2, 0, l2, ignore_case, invariant));
		}

		public static bool EndsWith (string str1, string str2)
		{
			return EndsWith (str1, str2, false);
		}

		public static bool EndsWith (string str1, string str2, bool ignore_case)
		{
			int l2 = str2.Length;
			if (l2 == 0)
				return true;

			int l1 = str1.Length;
			if (l2 > l1)
				return false;

			return (0 == String.Compare (str1, l1 - l2, str2, 0, l2, ignore_case, invariant));
		}

		public static string EscapeQuotesAndBackslashes (string attributeValue)
		{
			StringBuilder sb = null;
			for (int i = 0; i < attributeValue.Length; i++) {
				char ch = attributeValue [i];
				if (ch == '\'' || ch == '"' || ch == '\\') {
					if (sb == null) {
						sb = new StringBuilder ();
						sb.Append (attributeValue.Substring (0, i));
					}
					sb.Append ('\\');
					sb.Append (ch);
				}
				else {
					if (sb != null)
						sb.Append (ch);
				}
			}
			if (sb != null)
				return sb.ToString ();
			return attributeValue;
		}
	}
}

