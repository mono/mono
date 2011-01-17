//
// System.Security.Cryptography.X509Certificate2UI class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005, 2006 Novell Inc. (http://www.novell.com)
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

#if SECURITY_DEP

// Notes:
//
// We could P/Invoke both the display and selection under Windows. However 
// this would show the wrong certificate chain and the install would be 
// worthless (wrong certificate store).
//
// The alternative is to display our own UI - but without making the System.
// Security.dll assembly depends on SWF or GTK# (e.g. reflection). We should
// also use a factory to select the best UI. E.g. SWF on Windows, Gtk# 
// elsewhere (except if Gtk# isn't available then we fallback on SWF)
//

using System.Security.Permissions;

using Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

#if NET_4_0
	public static class X509Certificate2UI {
#else
	public sealed class X509Certificate2UI {

		// sadly this isn't a static class
		private X509Certificate2UI ()
		{
		}
#endif

		[MonoTODO]
		public static void DisplayCertificate (X509Certificate2 certificate)
		{
			// note: the LinkDemand won't interfere (by design) as this caller is trusted (correct behaviour)
			DisplayCertificate (certificate, IntPtr.Zero);
		}

		[MonoTODO]
		[UIPermission (SecurityAction.Demand, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void DisplayCertificate (X509Certificate2 certificate, IntPtr hwndParent) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			/*byte[] raw = */ certificate.GetRawCertData ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static X509Certificate2Collection SelectFromCollection (X509Certificate2Collection certificates, 
			string title, string message, X509SelectionFlag selectionFlag)
		{
			// note: the LinkDemand won't interfere (by design) as this caller is trusted (correct behaviour)
			return SelectFromCollection (certificates, title, message, selectionFlag, IntPtr.Zero);
		}

		[MonoTODO]
		[UIPermission (SecurityAction.Demand, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static X509Certificate2Collection SelectFromCollection (X509Certificate2Collection certificates, 
			string title, string message, X509SelectionFlag selectionFlag, IntPtr hwndParent)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");
			if ((selectionFlag < X509SelectionFlag.SingleSelection) || (selectionFlag > X509SelectionFlag.MultiSelection))
				throw new ArgumentException ("selectionFlag");

			throw new NotImplementedException ();
		}
	}
}

#endif
