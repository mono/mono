//
// Aes(CryptoServiceProvider) CFB Unit Tests 
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
	public class AesCfbTests : CfbTests {

		protected override SymmetricAlgorithm GetInstance ()
		{
			return Aes.Create ();
		}

		[Test]
		[Category ("AndroidNotWorking")] // Exception is thrown: CryptographicException : Bad PKCS7 padding. Invalid length 236.
		[Category ("MobileNotWorking")] // On testing_aot_full, above exception is thrown as well
		public void Roundtrip ()
		{
			// that will return a AesCryptoServiceProvider
			var aes = GetInstance ();
#if MONOTOUCH
			Assert.AreEqual ("System.Security.Cryptography.AesManaged", aes.ToString (), "Default");
			Assert.AreEqual (128, aes.FeedbackSize, "FeedbackSize");
#else
			Assert.AreEqual ("System.Security.Cryptography.AesCryptoServiceProvider", aes.ToString (), "Default");
			Assert.AreEqual (8, aes.FeedbackSize, "FeedbackSize");
#endif
			ProcessBlockSizes (aes);
		}

		// AesCryptoServiceProvider is not *Limited* since it supports CFB8-64
		// but like all *CryptoServiceProvider implementations it refuse Padding.None
		static PaddingMode[] csp_padding_modes = new [] { PaddingMode.PKCS7, PaddingMode.Zeros, PaddingMode.ANSIX923, PaddingMode.ISO10126 };
		
		protected override PaddingMode [] PaddingModes {
			get { return csp_padding_modes; }
		}

		protected override void CFB (SymmetricAlgorithm algo)
		{
			algo.Mode = CipherMode.CFB;
			// limited from 8-64 bits (RijndaelManaged goes to blocksize - but is incompatible)
			for (int i = 8; i <= 64; i += 8) {
				algo.FeedbackSize = i;
				CFB (algo, i);
			}
		}
		
		protected override string GetExpectedResult (SymmetricAlgorithm algo, byte [] encryptedData)
		{
#if false
			return base.GetExpectedResult (algo, encryptedData);
#else
			return test_vectors [GetId (algo)];
#endif
		}
		
		static Dictionary<int, string> test_vectors = new Dictionary<int, string> () {
			// padding None : The input data is not a complete block.
			// block size: 128, key size: 128, padding: PKCS7, feedback: 8
			{ -2139094520, "99-69-66-99-00-71-BD-07-C1-51-7A-60-DD-3C-03-A6" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 16
			{ -2139094512, "99-98-A2-5F-57-12-44-8B-38-01-A6-01-AD-0B-B8-59" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 24
			{ -2139094504, "99-98-51-14-92-57-8A-B9-F5-B7-3D-CC-C9-C0-1D-0E" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 32
			{ -2139094496, "99-98-51-E5-E2-1D-47-05-A5-A6-5C-A4-FF-EC-30-E5" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 40
			{ -2139094488, "99-98-51-E5-15-6E-C4-AE-D1-53-58-D1-D7-68-EA-FD" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 48
			{ -2139094480, "99-98-51-E5-15-9F-DC-EE-C9-47-73-AB-74-B8-32-08" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 56
			{ -2139094472, "99-98-51-E5-15-9F-2F-57-BD-43-B4-63-71-18-39-B6" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 64
			{ -2139094464, "99-98-51-E5-15-9F-2F-A6-09-DC-C8-55-6F-EA-FC-21" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 8
			{ -2139094264, "99-66-48-F8-D6-AB-E6-A6-A6-D8-B3-32-66-2F-44-6E" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 16
			{ -2139094256, "99-98-AC-6E-83-98-A2-FF-4B-20-E9-79-31-9A-66-81" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 24
			{ -2139094248, "99-98-51-19-42-6B-6F-34-29-48-1C-F4-2D-66-2E-48" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 32
			{ -2139094240, "99-98-51-E5-EE-9B-3F-56-C7-47-3A-E5-5A-2D-6E-2E" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 40
			{ -2139094232, "99-98-51-E5-15-65-32-85-14-5A-C6-74-13-0A-F5-DC" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 48
			{ -2139094224, "99-98-51-E5-15-9F-D6-2F-2D-31-57-7C-AC-8C-53-B9" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 56
			{ -2139094216, "99-98-51-E5-15-9F-2F-5E-F4-F8-DF-50-AE-70-DB-E9" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 64
			{ -2139094208, "99-98-51-E5-15-9F-2F-A6-01-E3-E0-EB-87-35-30-74" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 8
			{ -2139094008, "99-66-48-F8-D6-AB-E6-A6-A6-D8-B3-32-66-2F-44-61" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 16
			{ -2139094000, "99-98-AC-6E-83-98-A2-FF-4B-20-E9-79-31-9A-66-8F" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 24
			{ -2139093992, "99-98-51-19-42-6B-6F-34-29-48-1C-F4-2D-66-2E-45" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 32
			{ -2139093984, "99-98-51-E5-EE-9B-3F-56-C7-47-3A-E5-5A-2D-6E-22" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 40
			{ -2139093976, "99-98-51-E5-15-65-32-85-14-5A-C6-74-13-0A-F5-D7" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 48
			{ -2139093968, "99-98-51-E5-15-9F-D6-2F-2D-31-57-7C-AC-8C-53-B3" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 56
			{ -2139093960, "99-98-51-E5-15-9F-2F-5E-F4-F8-DF-50-AE-70-DB-E0" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 64
			{ -2139093952, "99-98-51-E5-15-9F-2F-A6-01-E3-E0-EB-87-35-30-7C" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 8
			{ -2139093752, "99-1B-4F-28-42-3F-FE-50-C4-1A-E3-27-7A-BF-95-EB" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 16
			{ -2139093744, "99-98-E4-AE-6B-9D-EC-6A-4E-52-E9-60-30-26-E0-01" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 24
			{ -2139093736, "99-98-51-BF-A0-E9-53-CD-4E-50-35-A3-73-48-F1-E1" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 32
			{ -2139093728, "99-98-51-E5-73-AA-BD-FC-D8-28-E0-5D-CB-B5-3C-70" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 40
			{ -2139093720, "99-98-51-E5-15-DF-BF-29-0B-30-44-52-B6-FD-5E-66" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 48
			{ -2139093712, "99-98-51-E5-15-9F-54-26-F7-10-58-54-5A-EB-6D-07" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 56
			{ -2139093704, "99-98-51-E5-15-9F-2F-6D-F7-54-EC-5E-63-DE-42-4F" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 64
			{ -2139093696, "99-98-51-E5-15-9F-2F-A6-7B-00-DA-C3-BC-C3-79-96" },
			// padding None : The input data is not a complete block.
			// block size: 128, key size: 192, padding: PKCS7, feedback: 8
			{ -2134900216, "55-1E-15-41-27-60-35-C7-73-7F-23-4F-75-0E-AF-FB" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 16
			{ -2134900208, "55-EF-7E-C3-9E-85-57-7B-10-47-8C-CB-89-2E-47-76" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 24
			{ -2134900200, "55-EF-8D-2B-1A-10-5E-6A-D8-D0-93-61-3D-47-E4-E4" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 32
			{ -2134900192, "55-EF-8D-DA-C8-DB-63-C4-44-4F-F4-B9-D3-D6-49-FE" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 40
			{ -2134900184, "55-EF-8D-DA-3F-66-3B-8F-9E-56-CE-FB-9D-1C-D2-57" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 48
			{ -2134900176, "55-EF-8D-DA-3F-97-F7-50-D1-C0-8C-1C-8A-23-72-80" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 56
			{ -2134900168, "55-EF-8D-DA-3F-97-04-72-4C-8B-56-79-92-0E-DD-64" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 64
			{ -2134900160, "55-EF-8D-DA-3F-97-04-83-3B-83-33-8C-CD-B0-D3-F9" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 8
			{ -2134899960, "55-11-47-22-49-48-50-3E-D1-F9-E7-86-20-CC-0A-97" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 16
			{ -2134899952, "55-EF-70-F3-BB-AA-FD-C8-A7-86-43-2C-4E-95-99-43" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 24
			{ -2134899944, "55-EF-8D-26-F9-A2-3A-A2-E8-AD-93-34-53-56-B5-54" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 32
			{ -2134899936, "55-EF-8D-DA-C4-D3-27-45-07-BF-C8-0E-EB-06-10-86" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 40
			{ -2134899928, "55-EF-8D-DA-3F-6D-6A-BB-66-D5-AF-2E-C8-DF-BC-19" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 48
			{ -2134899920, "55-EF-8D-DA-3F-97-FD-FA-4D-76-73-78-F6-6D-8F-0B" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 56
			{ -2134899912, "55-EF-8D-DA-3F-97-04-7B-C7-27-61-14-44-3E-2A-88" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 64
			{ -2134899904, "55-EF-8D-DA-3F-97-04-83-33-87-1A-F8-6A-27-BA-60" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 8
			{ -2134899704, "55-11-47-22-49-48-50-3E-D1-F9-E7-86-20-CC-0A-98" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 16
			{ -2134899696, "55-EF-70-F3-BB-AA-FD-C8-A7-86-43-2C-4E-95-99-4D" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 24
			{ -2134899688, "55-EF-8D-26-F9-A2-3A-A2-E8-AD-93-34-53-56-B5-59" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 32
			{ -2134899680, "55-EF-8D-DA-C4-D3-27-45-07-BF-C8-0E-EB-06-10-8A" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 40
			{ -2134899672, "55-EF-8D-DA-3F-6D-6A-BB-66-D5-AF-2E-C8-DF-BC-12" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 48
			{ -2134899664, "55-EF-8D-DA-3F-97-FD-FA-4D-76-73-78-F6-6D-8F-01" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 56
			{ -2134899656, "55-EF-8D-DA-3F-97-04-7B-C7-27-61-14-44-3E-2A-81" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 64
			{ -2134899648, "55-EF-8D-DA-3F-97-04-83-33-87-1A-F8-6A-27-BA-68" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 8
			{ -2134899448, "55-E6-52-56-F3-5C-82-2E-04-E8-9C-72-F5-56-61-C2" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 16
			{ -2134899440, "55-EF-58-DB-49-72-12-E1-2D-B2-B7-33-B3-92-76-91" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 24
			{ -2134899432, "55-EF-8D-55-FF-7B-89-F7-B9-22-76-47-D8-BA-52-D7" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 32
			{ -2134899424, "55-EF-8D-DA-B1-B7-68-3A-54-47-71-4D-43-48-C2-50" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 40
			{ -2134899416, "55-EF-8D-DA-3F-84-6C-2C-98-E7-AE-B6-C2-97-1C-7E" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 48
			{ -2134899408, "55-EF-8D-DA-3F-97-C3-8F-63-2D-6B-B3-86-D2-61-85" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 56
			{ -2134899400, "55-EF-8D-DA-3F-97-04-2D-7B-E6-5A-5B-10-5F-B5-9E" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 64
			{ -2134899392, "55-EF-8D-DA-3F-97-04-83-95-74-A3-86-78-66-13-3A" },
			// padding None : The input data is not a complete block.
			// block size: 128, key size: 256, padding: PKCS7, feedback: 8
			{ -2130705912, "23-D9-77-80-5B-FA-F1-6D-6D-39-98-60-DF-75-DF-49" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 16
			{ -2130705904, "23-28-07-BE-7A-18-9F-BC-B1-4D-F5-65-4B-5B-AD-D5" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 24
			{ -2130705896, "23-28-F4-C9-30-DA-57-28-5F-8F-9E-BF-05-DF-D9-26" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 32
			{ -2130705888, "23-28-F4-38-83-DF-89-E9-C9-5C-87-D5-FA-19-56-54" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 40
			{ -2130705880, "23-28-F4-38-74-E0-5C-D0-D1-05-5D-42-AA-FC-2F-EF" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 48
			{ -2130705872, "23-28-F4-38-74-11-19-31-30-84-5D-FB-BE-69-BB-98" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 56
			{ -2130705864, "23-28-F4-38-74-11-EA-29-1F-A5-02-D5-AA-78-4C-E8" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 64
			{ -2130705856, "23-28-F4-38-74-11-EA-D8-68-29-E3-14-6B-BF-C4-2D" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 8
			{ -2130705656, "23-D6-E9-75-83-FA-22-B3-96-27-CF-6D-BE-23-A4-D0" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 16
			{ -2130705648, "23-28-09-B9-8E-0B-01-57-EE-D8-4F-44-69-F0-8A-28" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 24
			{ -2130705640, "23-28-F4-C4-B7-B5-79-63-F0-CD-35-C6-39-3B-4D-02" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 32
			{ -2130705632, "23-28-F4-38-8F-78-58-EE-93-06-FA-CA-21-64-70-96" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 40
			{ -2130705624, "23-28-F4-38-74-EB-52-74-A3-80-87-48-3D-18-76-19" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 48
			{ -2130705616, "23-28-F4-38-74-11-13-09-09-B5-B9-95-A9-FF-02-EE" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 56
			{ -2130705608, "23-28-F4-38-74-11-EA-20-2A-87-0E-39-29-3A-84-A6" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 64
			{ -2130705600, "23-28-F4-38-74-11-EA-D8-60-04-E9-5D-7A-C1-A8-85" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 8
			{ -2130705400, "23-D6-E9-75-83-FA-22-B3-96-27-CF-6D-BE-23-A4-DF" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 16
			{ -2130705392, "23-28-09-B9-8E-0B-01-57-EE-D8-4F-44-69-F0-8A-26" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 24
			{ -2130705384, "23-28-F4-C4-B7-B5-79-63-F0-CD-35-C6-39-3B-4D-0F" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 32
			{ -2130705376, "23-28-F4-38-8F-78-58-EE-93-06-FA-CA-21-64-70-9A" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 40
			{ -2130705368, "23-28-F4-38-74-EB-52-74-A3-80-87-48-3D-18-76-12" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 48
			{ -2130705360, "23-28-F4-38-74-11-13-09-09-B5-B9-95-A9-FF-02-E4" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 56
			{ -2130705352, "23-28-F4-38-74-11-EA-20-2A-87-0E-39-29-3A-84-AF" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 64
			{ -2130705344, "23-28-F4-38-74-11-EA-D8-60-04-E9-5D-7A-C1-A8-8D" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 8
			{ -2130705144, "23-33-4F-8B-09-74-D9-8F-1F-78-F5-BD-31-C3-02-19" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 16
			{ -2130705136, "23-28-33-EE-86-CE-4B-89-A0-DE-8F-10-4E-4D-27-86" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 24
			{ -2130705128, "23-28-F4-44-74-47-57-7E-18-29-5B-3B-CB-64-3E-F9" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 32
			{ -2130705120, "23-28-F4-38-09-D6-8E-A7-CE-40-BA-83-6D-5D-E0-7D" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 40
			{ -2130705112, "23-28-F4-38-74-AD-A8-85-C7-78-BB-15-9E-39-32-14" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 48
			{ -2130705104, "23-28-F4-38-74-11-95-73-21-90-F4-B8-E0-DB-5D-6B" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 56
			{ -2130705096, "23-28-F4-38-74-11-EA-F8-5D-72-DE-4D-9E-75-5F-75" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 64
			{ -2130705088, "23-28-F4-38-74-11-EA-D8-8A-E6-AB-F8-FD-8C-8B-19" },
		};
	}
}
