//
// Unit tests for AesManaged
//
// Author:
//      Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
	
	[TestFixture]
	public class AesManagedTest {
		
		[Test]
#if !MOBILE
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CFB_NotAllowed ()
		{
			// that's differnt from RjindaelManaged
			// and also different from AesCryptoServiceProvider
			// and both are different as well :-(
			using (var aes = new AesManaged ()) {
				aes.Mode = CipherMode.CFB;
			}
		}
		
		[Test]
#if !MOBILE
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void CTS_NotAllowed ()
		{
			// this check is normally (e.g. RijndaelManaged) done later
			using (var aes = new AesManaged ()) {
				aes.Mode = CipherMode.CTS;
			}
		}
		
		[Test]
#if !MOBILE
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void OFB_NotAllowed ()
		{
			// this check is normally (e.g. RijndaelManaged) done later
			using (var aes = new AesManaged ()) {
				aes.Mode = CipherMode.OFB;
			}
		}
		
		[Test]
		public void CBC_Allowed ()
		{
			using (var aes = new AesManaged ()) {
				aes.Mode = CipherMode.CBC;
			}
		}
		
		[Test]
		public void ECB_Allowed ()
		{
			using (var aes = new AesManaged ()) {
				aes.Mode = CipherMode.ECB;
			}
		}
		
		[Test]
		public void FeedbackSize ()
		{
			using (var aes = new AesManaged ()) {
				// no used (no CFB support) but matching Aes (and RjindaelManaged),
				// but not AesCryptoServiceProvider, default value 
				Assert.AreEqual (128, aes.FeedbackSize, "FeedbackSize");
			}
		}
	}
}