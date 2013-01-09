//
// RC2 CFB Unit Tests 
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
	public class Rc2CbcTests : LimitedCfbTests {
		
		protected override SymmetricAlgorithm GetInstance ()
		{
			return RC2.Create ();
		}
		
		[Test]
		public void Roundtrip ()
		{
			ProcessBlockSizes (GetInstance ());
		}

		static Dictionary<int, string> test_vectors = new Dictionary<int, string> () {
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 40, padding: PKCS7, feedback: 8
			{ 1076363784, "3F-B2-FC-4A-44-A6-47-31" },
			// block size: 64, key size: 40, padding: Zeros, feedback: 8
			{ 1076364040, "3F-B5-03-E3-28-89-FD-01" },
			// block size: 64, key size: 40, padding: ANSIX923, feedback: 8
			{ 1076364296, "3F-B5-03-E3-28-89-FD-06" },
			// block size: 64, key size: 40, padding: ISO10126, feedback: 8
			{ 1076364552, "3F-1D-16-2D-49-17-90-3D" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 48, padding: PKCS7, feedback: 8
			{ 1076888072, "2D-78-28-8C-D0-94-A1-3A" },
			// block size: 64, key size: 48, padding: Zeros, feedback: 8
			{ 1076888328, "2D-7F-12-B8-25-CD-45-9B" },
			// block size: 64, key size: 48, padding: ANSIX923, feedback: 8
			{ 1076888584, "2D-7F-12-B8-25-CD-45-9C" },
			// block size: 64, key size: 48, padding: ISO10126, feedback: 8
			{ 1076888840, "2D-21-AA-BF-FC-09-24-11" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 56, padding: PKCS7, feedback: 8
			{ 1077412360, "CB-48-99-CB-FF-73-EB-24" },
			// block size: 64, key size: 56, padding: Zeros, feedback: 8
			{ 1077412616, "CB-4F-5B-19-90-24-2F-E4" },
			// block size: 64, key size: 56, padding: ANSIX923, feedback: 8
			{ 1077412872, "CB-4F-5B-19-90-24-2F-E3" },
			// block size: 64, key size: 56, padding: ISO10126, feedback: 8
			{ 1077413128, "CB-02-DF-DB-D7-31-01-25" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 64, padding: PKCS7, feedback: 8
			{ 1077936648, "14-42-51-73-8F-E0-F6-6D" },
			// block size: 64, key size: 64, padding: Zeros, feedback: 8
			{ 1077936904, "14-45-77-33-55-01-58-25" },
			// block size: 64, key size: 64, padding: ANSIX923, feedback: 8
			{ 1077937160, "14-45-77-33-55-01-58-22" },
			// block size: 64, key size: 64, padding: ISO10126, feedback: 8
			{ 1077937416, "14-FB-AE-82-D0-19-6F-1D" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 72, padding: PKCS7, feedback: 8
			{ 1078460936, "7E-BC-54-EF-A3-24-49-16" },
			// block size: 64, key size: 72, padding: Zeros, feedback: 8
			{ 1078461192, "7E-BB-E3-35-54-06-5B-E4" },
			// block size: 64, key size: 72, padding: ANSIX923, feedback: 8
			{ 1078461448, "7E-BB-E3-35-54-06-5B-E3" },
			// block size: 64, key size: 72, padding: ISO10126, feedback: 8
			{ 1078461704, "7E-1D-43-1C-9A-92-07-BD" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 80, padding: PKCS7, feedback: 8
			{ 1078985224, "D5-15-A2-A6-64-90-AA-E0" },
			// block size: 64, key size: 80, padding: Zeros, feedback: 8
			{ 1078985480, "D5-12-CA-68-08-80-BF-9A" },
			// block size: 64, key size: 80, padding: ANSIX923, feedback: 8
			{ 1078985736, "D5-12-CA-68-08-80-BF-9D" },
			// block size: 64, key size: 80, padding: ISO10126, feedback: 8
			{ 1078985992, "D5-3F-2F-2D-4E-5F-74-D4" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 88, padding: PKCS7, feedback: 8
			{ 1079509512, "65-D3-D6-A7-50-E1-08-40" },
			// block size: 64, key size: 88, padding: Zeros, feedback: 8
			{ 1079509768, "65-D4-55-EF-48-D3-F9-D1" },
			// block size: 64, key size: 88, padding: ANSIX923, feedback: 8
			{ 1079510024, "65-D4-55-EF-48-D3-F9-D6" },
			// block size: 64, key size: 88, padding: ISO10126, feedback: 8
			{ 1079510280, "65-E0-13-D5-55-8A-47-F8" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 96, padding: PKCS7, feedback: 8
			{ 1080033800, "9F-45-33-EB-04-82-11-32" },
			// block size: 64, key size: 96, padding: Zeros, feedback: 8
			{ 1080034056, "9F-42-40-E4-97-D4-86-EA" },
			// block size: 64, key size: 96, padding: ANSIX923, feedback: 8
			{ 1080034312, "9F-42-40-E4-97-D4-86-ED" },
			// block size: 64, key size: 96, padding: ISO10126, feedback: 8
			{ 1080034568, "9F-1A-3A-46-7C-73-7D-58" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 104, padding: PKCS7, feedback: 8
			{ 1080558088, "57-FC-F7-61-8A-3C-BC-9F" },
			// block size: 64, key size: 104, padding: Zeros, feedback: 8
			{ 1080558344, "57-FB-71-C9-C6-5C-08-D1" },
			// block size: 64, key size: 104, padding: ANSIX923, feedback: 8
			{ 1080558600, "57-FB-71-C9-C6-5C-08-D6" },
			// block size: 64, key size: 104, padding: ISO10126, feedback: 8
			{ 1080558856, "57-10-94-F8-51-B5-98-5D" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 112, padding: PKCS7, feedback: 8
			{ 1081082376, "AE-21-D0-F5-89-E3-80-1E" },
			// block size: 64, key size: 112, padding: Zeros, feedback: 8
			{ 1081082632, "AE-26-C6-2E-05-FA-AF-68" },
			// block size: 64, key size: 112, padding: ANSIX923, feedback: 8
			{ 1081082888, "AE-26-C6-2E-05-FA-AF-6F" },
			// block size: 64, key size: 112, padding: ISO10126, feedback: 8
			{ 1081083144, "AE-4D-6F-DE-06-5F-40-71" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 120, padding: PKCS7, feedback: 8
			{ 1081606664, "22-63-22-B7-1F-5A-75-DB" },
			// block size: 64, key size: 120, padding: Zeros, feedback: 8
			{ 1081606920, "22-64-0F-2B-17-29-20-E7" },
			// block size: 64, key size: 120, padding: ANSIX923, feedback: 8
			{ 1081607176, "22-64-0F-2B-17-29-20-E0" },
			// block size: 64, key size: 120, padding: ISO10126, feedback: 8
			{ 1081607432, "22-C9-FD-42-71-DF-E1-E8" },
			// padding None : Length of the data to encrypt is invalid.
			// block size: 64, key size: 128, padding: PKCS7, feedback: 8
			{ 1082130952, "7E-00-0A-7E-E1-52-24-2F" },
			// block size: 64, key size: 128, padding: Zeros, feedback: 8
			{ 1082131208, "7E-07-99-69-E5-4E-3D-7D" },
			// block size: 64, key size: 128, padding: ANSIX923, feedback: 8
			{ 1082131464, "7E-07-99-69-E5-4E-3D-7A" },
			// block size: 64, key size: 128, padding: ISO10126, feedback: 8
			{ 1082131720, "7E-B2-6D-A9-60-A9-CC-05" },
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