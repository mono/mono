//
// Unit tests for AesCryptoServiceProvider
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

#if !MOBILE

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
	
	[TestFixture]
	public class AesCryptoServiceProviderTest {
		
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CTS_NotAllowed ()
		{
			// this check is normally (e.g. RijndaelManaged) done later
			using (var aes = new AesCryptoServiceProvider ()) {
				aes.Mode = CipherMode.CTS;
			}
		}
		
		[Test]
		public void CBC_Allowed ()
		{
			using (var aes = new AesCryptoServiceProvider ()) {
				aes.Mode = CipherMode.CBC;
			}
		}
		
		[Test]
		public void ECB_Allowed ()
		{
			using (var aes = new AesCryptoServiceProvider ()) {
				aes.Mode = CipherMode.ECB;
			}
		}
		
		[Test]
		public void OFB_Allowed ()
		{
			// this not supported by AesManaged
			using (var aes = new AesCryptoServiceProvider ()) {
				aes.Mode = CipherMode.OFB;
				// FIXME: check is really implemented (or if the check is only done later, like RjindaelManaged)
			}
		}
		
		[Test]
		public void CFB_Allowed ()
		{
			using (var aes = new AesCryptoServiceProvider ()) {
				// AesManaged does not support CFB at all
				aes.Mode = CipherMode.CFB;
				Assert.AreEqual (8, aes.FeedbackSize, "FeedbackSize (default)");
				int block_size = aes.BlockSize / 8;
				for (int i = 8; i <= 64; i += 8) {
					aes.FeedbackSize = i;
					using (ICryptoTransform t = aes.CreateEncryptor ()) {
						// RjindaelManaged transform block size are different!
						Assert.AreEqual (block_size, t.InputBlockSize, "InputBlockSize CFB{0}", i);
						Assert.AreEqual (block_size, t.OutputBlockSize, "OutputBlockSize CFB{0}", i);
					}
				}
			}
		}
		
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CFB_TooSmall ()
		{
			using (var aes = new AesCryptoServiceProvider ()) {
				aes.Mode = CipherMode.CFB;
				aes.FeedbackSize = 0;
			}
		}
		
		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void CFB_TooBig ()
		{
			using (var aes = new AesCryptoServiceProvider ()) {
				aes.Mode = CipherMode.CFB;
				aes.FeedbackSize = 72;
				Assert.AreEqual (72, aes.FeedbackSize, "FeedbackSize");
				// we can't set it but can't use it
				aes.CreateEncryptor (aes.Key, aes.IV);
			}
		}
	}
}

#endif