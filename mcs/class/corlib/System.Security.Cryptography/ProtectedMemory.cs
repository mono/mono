//
// ProtectedMemory.cs: Protect (encrypt) memory without (user involved) key management
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;

namespace System.Security.Cryptography {

	// References:
	// a.	Windows Data Protection
	//	http://msdn.microsoft.com/library/en-us/dnsecure/html/windataprotection-dpapi.asp?frame=true

	public sealed class ProtectedMemory {

		private ProtectedMemory ()
		{
		}

		[MonoTODO]
		public static void Protect (byte[] userData, MemoryProtectionScope scope) 
		{
			if (userData == null)
				throw new ArgumentNullException ("userData");
			if (userData.Length % 16 != 0)
				throw new CryptographicException ("not a multiple of 16 bytes");

			// on Windows this is supported only under XP and later OS
			throw new PlatformNotSupportedException ();
		}

		[MonoTODO]
		public static void Unprotect (byte[] encryptedData, MemoryProtectionScope scope) 
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");
			if (encryptedData.Length % 16 != 0)
				throw new CryptographicException ("not a multiple of 16 bytes");

			// on Windows this is supported only under XP and later OS
			throw new PlatformNotSupportedException ();
		}
	} 
}

#endif
