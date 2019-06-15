//
// X509Store.cs: Handles a X.509 certificates/CRLs store
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Pablo Ruiz <pruiz@netway.org>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// (C) 2010 Pablo Ruiz.
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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Mono.Security.X509.Extensions;

using SSCX = System.Security.Cryptography.X509Certificates;

using OpenFlags = System.Security.Cryptography.X509Certificates.OpenFlags;
using StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation;

namespace Mono.Security.X509 {

#if INSIDE_CORLIB || INSIDE_SYSTEM
	internal
#else
	public 
#endif
	class X509Store {

		[DllImport ("crypt32")]
		internal static extern bool CertCloseStore (IntPtr handle, int flags);

		internal class SafeCertStoreHandle : SafeHandle {
			protected SafeCertStoreHandle() : base(IntPtr.Zero, true)
			{
			}

			public override bool IsInvalid
			{
				get { return handle == IntPtr.Zero; }
			}

			protected override bool ReleaseHandle()
			{
				return CertCloseStore (handle, 0);
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		internal struct CERT_CONTEXT
		{
			public uint dwCertEncodingType;
			public IntPtr pbCertEncoded;
			public uint cbCertEncoded;
			public IntPtr pCertInfo;
			public IntPtr hCertStore;
		}

		[DllImport ("crypt32")]
		internal static extern SafeCertStoreHandle CertOpenStore([MarshalAs(UnmanagedType.LPStr)] string lpszStoreProvider, uint dwMsgAndCertEncodingType, IntPtr hCryptProv, uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pvProv);

		[DllImport ("crypt32")]
		internal static extern IntPtr CertEnumCertificatesInStore (SafeCertStoreHandle hCertStore, IntPtr pPrevCertContext);

		private string _name;
		private StoreLocation _location;
		private SafeCertStoreHandle _handle; 

		internal X509Store (string name, StoreLocation location, OpenFlags flags)
		{
			_name = name;
			_location = location;

			// Open handle
			uint dwFlags = 0;
            if ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived)
                dwFlags |= 0x200;
            if ((flags & OpenFlags.MaxAllowed) == OpenFlags.MaxAllowed)
                dwFlags |= 0x1000;
            if ((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly)
                dwFlags |= 0x4000;
            if ((flags & OpenFlags.ReadWrite) == 0)
                dwFlags |= 0x8000;
            if ((_location == StoreLocation.CurrentUser))
                dwFlags |= 0x10000;
            if ((_location == StoreLocation.LocalMachine))
                dwFlags |= 0x20000; 

			_handle = CertOpenStore("System", 0x00010001, IntPtr.Zero, dwFlags, _name);
            if (_handle.IsInvalid)
                throw new CryptographicException ("CertOpenStore failed");
		}

		// properties

		public X509CertificateCollection Certificates {
			get { 
				X509CertificateCollection result = new X509CertificateCollection ();
				IntPtr cert_context = IntPtr.Zero;

				while ((cert_context = CertEnumCertificatesInStore (_handle, cert_context)) != IntPtr.Zero)
				{
					CERT_CONTEXT context = Marshal.PtrToStructure<CERT_CONTEXT> (cert_context);
					byte[] raw_data = new byte[context.cbCertEncoded];
					Marshal.Copy (context.pbCertEncoded, raw_data, 0, (int)context.cbCertEncoded);
					X509Certificate cert = new X509Certificate(raw_data);
					result.Add (cert);
				}

				return result;
			}
		}

		public ArrayList Crls {
			get {
				throw new NotImplementedException ("Mono.Security.X509.X509Store.get_Crls");
			}
		}

		public string Name {
			get {
				return _name;
			}
		}

		// methods

		public void Clear () 
		{
			throw new NotImplementedException ("Mono.Security.X509.X509Store.Clear()");
		}

		public void Close ()
		{
			_handle.Dispose ();
		}

		public void Import (X509Certificate certificate) 
		{
			throw new NotImplementedException ("Mono.Security.X509.X509Store.Import(X509Certificate)");
		}

		public void Import (X509Crl crl) 
		{
			throw new NotImplementedException ("Mono.Security.X509.X509Store.Import(X509Crl)");
		}

		public void Remove (X509Certificate certificate) 
		{
			throw new NotImplementedException ("Mono.Security.X509.X509Store.Remove(X509Certificate)");
		}

		public void Remove (X509Crl crl) 
		{
			throw new NotImplementedException ("Mono.Security.X509.X509Store.Remove(X509Crl)");
		}
	}
}
