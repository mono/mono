//
// ProtectedData.cs: Protect (encrypt) data without (user involved) key management
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

	public sealed class ProtectedData {

		private ProtectedData ()
		{
		}

		// FIXME: interop could be important under windows - if one application protect some data using
		// mono and another one unprotects it using ms.net

		[MonoTODO ("interop with MS implementation ?")]
		public static byte[] Protect (byte[] userData, byte[] optionalEntropy, DataProtectionScope scope) 
		{
			if (userData == null)
				throw new ArgumentNullException ("userData");

			// on Windows this is supported only under 2000 and later OS
			throw new PlatformNotSupportedException ();
		}

		[MonoTODO ("interop with MS implementation ?")]
		public static byte[] Unprotect (byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope) 
		{
			if (encryptedData == null)
				throw new ArgumentNullException ("encryptedData");

			// on Windows this is supported only under 2000 and later OS
			throw new PlatformNotSupportedException ();
		}
 	} 
}

#endif
