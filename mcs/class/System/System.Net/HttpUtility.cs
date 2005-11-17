//
// System.Net.HttpUtility
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
using System.Globalization;
using System.IO;
using System.Text;
namespace System.Net {
	sealed class HttpUtility
	{
		private HttpUtility ()
		{
		}

		public static string UrlDecode (string s)
		{
			return UrlDecode (s, null);
		}

		static char [] GetChars (MemoryStream b, Encoding e)
		{
			return e.GetChars (b.GetBuffer (), 0, (int) b.Length);
		}

		public static string UrlDecode (string s, Encoding e)
		{
			if (null == s) 
				return null;

			if (s.IndexOf ('%') == -1 && s.IndexOf ('+') == -1)
				return s;

			if (e == null)
				e = Encoding.GetEncoding (28591);
	
			StringBuilder output = new StringBuilder ();
			long len = s.Length;
			NumberStyles hexa = NumberStyles.HexNumber;
			MemoryStream bytes = new MemoryStream ();
	
			for (int i = 0; i < len; i++) {
				if (s [i] == '%' && i + 2 < len) {
					if (s [i + 1] == 'u' && i + 5 < len) {
						if (bytes.Length > 0) {
							output.Append (GetChars (bytes, e));
							bytes.SetLength (0);
						}
						output.Append ((char) Int32.Parse (s.Substring (i + 2, 4), hexa));
						i += 5;
					} else {
						bytes.WriteByte ((byte) Int32.Parse (s.Substring (i + 1, 2), hexa));
						i += 2;
					}
					continue;
				}

				if (bytes.Length > 0) {
					output.Append (GetChars (bytes, e));
					bytes.SetLength (0);
				}

				if (s [i] == '+') {
					output.Append (' ');
				} else {
					output.Append (s [i]);
				}
	         	}
	
			if (bytes.Length > 0) {
				output.Append (GetChars (bytes, e));
			}

			bytes = null;
			return output.ToString ();
		}
	}
}
#endif

