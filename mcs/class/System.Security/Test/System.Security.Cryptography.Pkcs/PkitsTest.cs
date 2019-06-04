//
// PkitsTest.cs - NUnit tests for 
//	NIST Public Key Interoperability Test Suite (PKITS)
//	Certificate Path Validation, Version 1.0, September 2, 2004
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
#if !MOBILE

using NUnit.Framework;

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	/*
	 *	PKITS home page
	 *	http://csrs.nist.gov/pki/testing/x509paths.html
	 *
	 *	Documentation is available at
	 *	http://csrc.nist.gov/pki/testing/PKITS.pdf
	 *
	 *	Test data is available at
	 *	http://csrc.nist.gov/pki/testing/PKITS_data.zip
	 *
	 *	License information are available at
	 *	http://cio.nist.gov/esd/emaildir/lists/pkits/msg00048.html
	 */

	[Category ("PKITS")]
	public class PkitsTest {

		private string base_dir;
		private string certs_base_dir;
		private string smime_base_dir;
		private Hashtable cache;
		private Oid[] policies;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// reuse PKITS data installed in System (for X509Chain tests)
			base_dir = String.Format ("{0}{1}..{1}System{1}Test{1}System.Security.Cryptography.X509Certificates{1}pkits",
				Directory.GetCurrentDirectory (), Path.DirectorySeparatorChar);
			if (!Directory.Exists (base_dir))
				Assert.Ignore ("PKITS tests data not found under '{0}'.", new object[] { base_dir });
			certs_base_dir = Path.Combine (base_dir, "certs");
			smime_base_dir = Path.Combine (base_dir, "smime");

			cache = new Hashtable ();

			policies = new Oid[9];
			// any-policies
			policies[0] = new Oid ("2.5.29.32.0");
			// nist_test_policy_#
			for (int i=0; i < 9; i++)
				policies[i] = new Oid ("2.16.840.1.101.3.2.1.48." + i.ToString ());
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			cache.Clear ();
		}

		public X509Certificate2 GetCertificate (string filename)
		{
			X509Certificate2 result = (cache[filename] as X509Certificate2);
			if (result == null) {
				string full_path = Path.Combine (certs_base_dir, filename);
				result = new X509Certificate2 (full_path);
				cache[filename] = result;
			}
			return result;
		}

		public byte[] GetData (string filename)
		{
			string full_path = Path.Combine (smime_base_dir, filename);
			using (StreamReader sr = new StreamReader (full_path)) {
				string s = sr.ReadLine ();
				while (!sr.EndOfStream) {
					if (s.Length == 0)
						break;
					s = sr.ReadLine ();
				}
				s = sr.ReadToEnd ();
				return Convert.FromBase64String (s);
			}
		}

		public X509Certificate2 TrustAnchorRoot {
			get { return GetCertificate ("TrustAnchorRootCertificate.crt"); }
		}

		public X509Certificate2 GoodCACert {
			get { return GetCertificate ("GoodCACert.crt"); }
		}

		// Sadly both SignedCms.CheckHash and SignedCms.CheckSignature returns void and throw an exception.
		// This makes it difficult to use in tests because we want to be sure that the "expected exception"
		// is being thrown at the "right" place. The next 2 methods hacks around that limitation.
		public bool CheckHash (SignedCms cms)
		{
			try {
				cms.CheckSignature (false);
				return true;
			}
			catch {
			}
			return false;
		}

		public bool CheckSignature (SignedCms cms)
		{
			try {
				cms.CheckSignature (false);
				return true;
			}
			catch {
			}
			return false;
		}

		public Oid AnyPolicy {
			get { return policies [0]; }
		}

		public Oid NistPolicy (int n)
		{
			return policies[n];
		}
	}
}
#endif
