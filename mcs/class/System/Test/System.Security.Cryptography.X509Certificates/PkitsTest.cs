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


using NUnit.Framework;

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Reflection;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

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
		private Hashtable cache;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			base_dir = String.Format ("{0}{1}Test{1}System.Security.Cryptography.X509Certificates{1}pkits{1}certs",
				Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), Path.DirectorySeparatorChar);
			if (!Directory.Exists (base_dir))
				Assert.Ignore ("PKITS tests data not found under '{0}'.", new object[] { base_dir });

			cache = new Hashtable ();
			// prepare the environment
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			if (cache != null)
				cache.Clear ();
			// clean-up, as best as possible, the stores
		}

		public X509Certificate2 GetCertificate (string filename)
		{
			X509Certificate2 result = (cache[filename] as X509Certificate2);
			if (result == null) {
				string full_path = Path.Combine (base_dir, filename);
				result = new X509Certificate2 (full_path);
				cache[filename] = result;
			}
			return result;
		}

		public X509Certificate2 TrustAnchorRoot {
			get { return GetCertificate ("TrustAnchorRootCertificate.crt"); }
		}

		public X509Certificate2 GoodCACert {
			get { return GetCertificate ("GoodCACert.crt"); }
		}

		// this method avoid having a dependance on the order of status
		public void CheckChainStatus (X509ChainStatusFlags expected, X509ChainStatus[] status, string msg)
		{
			if ((expected == X509ChainStatusFlags.NoError) && (status.Length == 0))
				return;

			X509ChainStatusFlags actual = X509ChainStatusFlags.NoError;
			foreach (X509ChainStatus s in status) {
				actual |= s.Status;
			}
			Assert.AreEqual (expected, actual, msg);
		}
	}
}

