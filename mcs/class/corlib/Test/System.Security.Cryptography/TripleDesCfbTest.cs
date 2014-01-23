//
// TripleDES CFB Unit Tests 
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
	public class TripleDesCbcTests : WeakKeyCfbTests {
		
		protected override SymmetricAlgorithm GetInstance ()
		{
			return TripleDES.Create ();
		}
		
		[Test]
		public void Roundtrip ()
		{
			ProcessBlockSizes (GetInstance ());
		}

		static Dictionary<int, string> test_vectors = new Dictionary<int, string> () {
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 128, padding: PKCS7, feedback: 8
			{ 1082130952, "22-5F-A0-55-22-6A-CD-8E" },
			// block size: 64, key size: 128, padding: Zeros, feedback: 8
			{ 1082131208, "22-58-26-57-F6-3E-FF-C4" },
			// block size: 64, key size: 128, padding: ANSIX923, feedback: 8
			{ 1082131464, "22-58-26-57-F6-3E-FF-C3" },
			// block size: 64, key size: 128, padding: ISO10126, feedback: 8
			{ 1082131720, "22-86-F5-46-69-D1-49-C2" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 192, padding: PKCS7, feedback: 8
			{ 1086325256, "76-50-58-98-3F-4F-BE-F3" },
			// block size: 64, key size: 192, padding: Zeros, feedback: 8
			{ 1086325512, "76-57-62-F7-E3-0C-5A-3B" },
			// block size: 64, key size: 192, padding: ANSIX923, feedback: 8
			{ 1086325768, "76-57-62-F7-E3-0C-5A-3C" },
			// block size: 64, key size: 192, padding: ISO10126, feedback: 8
			{ 1086326024, "76-6E-F9-2B-AB-AD-30-E3" },
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