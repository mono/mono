//
// System.Web.SessionState.SessionId
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com), All rights reserved
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
			
			rng.GetBytes (key);
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

