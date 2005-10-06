//
// System.Web.SessionState.SessionId
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com), All rights reserved
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
using System.Text;
using System.Security.Cryptography;

namespace System.Web.SessionState {

	internal class SessionId {

		private static char [] allowed = { '0', '1', '2', '3', '4', '5',
						   '6', '7', '8', '9', 'A', 'B',
						   'C', 'D', 'E', 'F' };

		internal static readonly int IdLength = 30;
		private static readonly int half_len = 15;
		
		internal static string Create (RandomNumberGenerator rng)
		{
			if (rng == null)
				throw new ArgumentNullException ("rng");
			
			byte[] key = new byte [half_len];

			lock (rng) {
				rng.GetBytes (key);
			}
			return Encode (key);
		}

		internal static string Encode (byte[] key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (key.Length != half_len)
				throw new ArgumentException ("key must be 15 bytes long.");

			// Just a standard hex conversion
			char[] res = new char [IdLength];
			for (int i=0; i < half_len; i++) {
				int b = key [i];
				res [i * 2] = allowed [b >> 4];
				res [(i * 2) + 1] = allowed [b & 0xF];
			}
			return new String (res);
		}

		internal static string Lookup (HttpRequest request, bool cookieless)
		{
			if (cookieless)
				return (string) request.Headers [SessionStateModule.HeaderName];
			else if (request.Cookies [SessionStateModule.CookieName] != null)
				return request.Cookies [SessionStateModule.CookieName].Value;
			return null;
		}
	}

}

