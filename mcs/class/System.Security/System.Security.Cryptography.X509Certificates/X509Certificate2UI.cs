//
// System.Security.Cryptography.X509Certificate2UI class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
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

using System.Security.Permissions;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509Certificate2UI {

		// sadly this isn't a static class
		private X509Certificate2UI ()
		{
		}

		[MonoTODO]
		public static void DisplayCertificate (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			throw new NotImplementedException ();

			// TODO : we could P/Invoke this Windows but it would get us 
			// the wrong certificate chain (and the install would be worthless)
		}

		[MonoTODO]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void DisplayCertificate (X509Certificate2 certificate, IntPtr hwndParent) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static X509Certificate2Collection SelectFromCollection (X509Certificate2Collection certificates, 
			string title, string message, X509SelectionFlag selectionFlag)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");
			if ((selectionFlag < X509SelectionFlag.SingleSelection) || (selectionFlag > X509SelectionFlag.MultiSelection))
				throw new ArgumentNullException ("selectionFlag");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static X509Certificate2Collection SelectFromCollection (X509Certificate2Collection certificates, 
			string title, string message, X509SelectionFlag selectionFlag, IntPtr hwndParent)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");
			if ((selectionFlag < X509SelectionFlag.SingleSelection) || (selectionFlag > X509SelectionFlag.MultiSelection))
				throw new ArgumentNullException ("selectionFlag");

			throw new NotImplementedException ();
		}
	}
}

#endif
