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
		
		internal static string Create (RandomNumberGenerator rng)
		{
			if (rng == null)
				throw new ArgumentNullException ("rng");
			
			byte[] key = new byte[15];
			
			rng.GetBytes (key);
			return Encode (key);
		}

		internal static string Encode (byte[] key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (key.Length != 15)
				throw new ArgumentException ("key must be 15 bytes long.");

			// Just a standard hex conversion
			char[] res = new char [30];
			for (int i=0; i < 15; i++) {
				int b = key [i];
				res [i * 2] = allowed [b >> 4];
				res [(i * 2) + 1] = allowed [b & 0xF];
			}
			return new String (res);
		}
	}

}

