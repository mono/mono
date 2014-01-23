//
// CFB Unit Tests 
//
// Author:
//	Sebastien Pouliot  <sebastien@xamarin.com>
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
using System.Collections.Generic;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
	
	public abstract class CfbTests {

		protected abstract SymmetricAlgorithm GetInstance ();
		
		protected void ProcessBlockSizes (SymmetricAlgorithm algo)
		{
			algo.Padding = PaddingMode.None;
			foreach (KeySizes bs in algo.LegalBlockSizes) {
				for (int blockSize = bs.MinSize; blockSize <= bs.MaxSize; blockSize += bs.SkipSize) {
					algo.BlockSize = blockSize;
					ProcessKeySizes (algo);
					// SkipSize can be 0 (e.g. DES) if only one block size is available
					if (blockSize == bs.MaxSize)
						break;
				}
			}
		}
		
		protected void ProcessKeySizes (SymmetricAlgorithm algo)
		{
			foreach (KeySizes ks in algo.LegalKeySizes) {
				for (int keySize = ks.MinSize; keySize <= ks.MaxSize; keySize += ks.SkipSize) {
					algo.KeySize = keySize;
					algo.Key = GetKey (algo);
					algo.IV = new byte [algo.BlockSize / 8];
					ProcessPadding (algo);
					// SkipSize can be 0 (e.g. DES) if only one key size is available
					if (keySize == ks.MaxSize)
						break;
				}
			}
		}

		protected abstract PaddingMode[] PaddingModes { get; }

		protected void ProcessPadding (SymmetricAlgorithm algo)
		{
			foreach (var padding in PaddingModes) {
				algo.Padding = padding;
				CFB (algo);
			}
		}
		
		protected virtual byte [] GetKey (SymmetricAlgorithm algo)
		{
			return new byte [algo.KeySize / 8];
		}
		
		protected abstract void CFB (SymmetricAlgorithm algo);
		
		protected int GetId (SymmetricAlgorithm algo)
		{
			return (algo.BlockSize << 24) + (algo.KeySize << 16) + ((int) algo.Padding << 8) + algo.FeedbackSize;
		}
		
		protected virtual string GetExpectedResult (SymmetricAlgorithm algo, byte [] encryptedData)
		{
			int id = GetId (algo);
			string expected = BitConverter.ToString (encryptedData);
			Console.WriteLine ("// block size: {0}, key size: {1}, padding: {2}, feedback: {3}", algo.BlockSize, algo.KeySize, algo.Padding, algo.FeedbackSize);
			Console.WriteLine ("{{ {0}, \"{1}\" }},", id, expected);
			return expected;
		}
		
		protected void CFB (SymmetricAlgorithm algo, int feedbackSize)
		{
			byte [] data = new byte [feedbackSize >> 3];
			for (int i = 0; i < data.Length; i++)
				data [i] = (byte) (0xff - i);
			byte [] encdata = Encryptor (algo, data);
			string expected = GetExpectedResult (algo, encdata);
			string actual = null;
			if (algo.Padding == PaddingMode.ISO10126) {
				// ISO10126 uses random data so we can't compare the last bytes with a test vector
				actual = BitConverter.ToString (encdata, 0, data.Length);
				expected = expected.Substring (0, actual.Length);
			} else {
				actual = BitConverter.ToString (encdata);
			}
			Assert.AreEqual (expected, actual, "encrypted value");
			byte [] decdata = Decryptor (algo, encdata);
			if (algo.Padding == PaddingMode.Zeros) {
				// this requires manually unpadding the decrypted data - but unlike ISO10126
				// we know the rest of the data will be 0 (not random) so we check that
				byte [] resize = new byte [data.Length];
				Array.Copy (decdata, 0, resize, 0, resize.Length);
				// all zeros afterward!
				for (int i = resize.Length; i < decdata.Length; i++)
					Assert.AreEqual (0, decdata [i], "padding zero {0}", i);
				decdata = resize;
			}
			Assert.AreEqual (data, decdata, "Roundtrip {0} {1}", algo.Mode, algo.FeedbackSize);
		}
		
		protected virtual int GetTransformBlockSize (SymmetricAlgorithm algo)
		{
			return algo.BlockSize / 8;
		}
		
		byte [] Encryptor (SymmetricAlgorithm algo, byte [] data)
		{
			using (ICryptoTransform t = algo.CreateEncryptor (algo.Key, algo.IV)) {
				int size = GetTransformBlockSize (algo);
				Assert.That (t.InputBlockSize == size, "Encryptor InputBlockSize {0} {1}", algo.Mode, algo.FeedbackSize);
				Assert.That (t.OutputBlockSize == size, "Encryptor OutputBlockSize {0} {1}", algo.Mode, algo.FeedbackSize);
				return t.TransformFinalBlock (data, 0, data.Length);
			}
		}
		
		byte [] Decryptor (SymmetricAlgorithm algo, byte [] encdata)
		{
			using (ICryptoTransform t = algo.CreateDecryptor (algo.Key, algo.IV)) {
				int size = GetTransformBlockSize (algo);
				Assert.That (t.InputBlockSize == size, "Decryptor InputBlockSize {0} {1}", algo.Mode, algo.FeedbackSize);
				Assert.That (t.OutputBlockSize == size, "Decryptor OutputBlockSize {0} {1}", algo.Mode, algo.FeedbackSize);
				return t.TransformFinalBlock (encdata, 0, encdata.Length);
			}
		}
	}
	
	// most algorithms are "limited" and only support CFB8
	public abstract class LimitedCfbTests : CfbTests {

		// all *CryptoServiceProvider implementation refuse Padding.None
		static PaddingMode[] csp_padding_modes = new [] { PaddingMode.PKCS7, PaddingMode.Zeros, PaddingMode.ANSIX923, PaddingMode.ISO10126 };

		protected override PaddingMode [] PaddingModes {
			get { return csp_padding_modes; }
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Cfb_None ()
		{
			using (var algo = GetInstance ()) {
				algo.Padding = PaddingMode.None;
				CFB (algo, 8);
			}
		}

		protected override void CFB (SymmetricAlgorithm algo)
		{
			algo.Mode = CipherMode.CFB;
			// System.Security.Cryptography.CryptographicException : Feedback size for the cipher feedback mode (CFB) must be 8 bits.
			algo.FeedbackSize = 8;
			CFB (algo, algo.FeedbackSize);
		}
	}
	
	// DES and 3DES won't accept a key with all zero (since it's a weak key for them)
	public abstract class WeakKeyCfbTests : LimitedCfbTests {
		
		protected override byte [] GetKey (SymmetricAlgorithm algo)
		{
			var key = base.GetKey (algo);
			for (byte i = 0; i < key.Length; i++)
				key [i] = i;
			return key;
		}
	}
}