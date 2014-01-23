//
// DES CFB Unit Tests 
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
	
	[TestFixture]
	public class DesCfbTests : WeakKeyCfbTests {
		
		protected override SymmetricAlgorithm GetInstance ()
		{
			return DES.Create ();
		}

		[Test]
		public void Roundtrip ()
		{
			ProcessBlockSizes (GetInstance ());
		}
		
		static Dictionary<int, string> test_vectors = new Dictionary<int, string> () {
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 64, padding: PKCS7, feedback: 8
			{ 1077936648, "5A-44-C0-F3-21-56-A4-8E" },
			// block size: 64, key size: 64, padding: Zeros, feedback: 8
			{ 1077936904, "5A-43-7C-5D-A9-15-AB-5A" },
			// block size: 64, key size: 64, padding: ANSIX923, feedback: 8
			{ 1077937160, "5A-43-7C-5D-A9-15-AB-5D" },
			// block size: 64, key size: 64, padding: ISO10126, feedback: 8
			{ 1077937416, "5A-E6-7D-EF-3B-F8-E9-1C" },
		};
		
		protected override string GetExpectedResult (SymmetricAlgorithm algo, byte [] encryptedData)
		{
#if false
			return base.GetExpectedResult (algo, encryptedData);
#else
			return test_vectors [GetId (algo)];
#endif
		}
	}
}