//
// System.Web.SessionState.SessionId
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright (C) 2003,2006 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Security.Cryptography;
using System.Web.Util;

namespace System.Web.SessionState {

	internal class SessionId {

		internal const int IdLength = 24;
		const int half_len = IdLength / 2;
		static RandomNumberGenerator rng = RandomNumberGenerator.Create ();
		
		internal static string Create ()
		{
			byte[] key = new byte [half_len];

			lock (rng) {
				rng.GetBytes (key);
			}
			return MachineKeySectionUtils.GetHexString (key);
		}

#if !NET_2_0
		internal static string Lookup (HttpRequest request, bool cookieless)
		{
			if (cookieless)
				return (string) request.Headers [SessionStateModule.HeaderName];

			HttpCookie cookie = request.Cookies [SessionStateModule.CookieName];
			if (cookie == null)
				return null;

			return cookie.Value;
		}
#endif
	}

}

