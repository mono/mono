//
// RijndaelManaged CFB Unit Tests 
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
	public class RijndaelCfbTests : CfbTests {

		protected override SymmetricAlgorithm GetInstance ()
		{
			return Rijndael.Create ();
		}

		[Test]
		public void Roundtrip ()
		{
			var aes = GetInstance ();
			Assert.AreEqual (128, aes.FeedbackSize, "FeedbackSize");
			ProcessBlockSizes (aes);
		}

		// all *CryptoServiceProvider implementation refuse Padding.None
		// but RijndaelManaged treats it like Padding.Zeros
		//static PaddingMode[] all_padding_modes = new [] { PaddingMode.None, PaddingMode.PKCS7, PaddingMode.Zeros, PaddingMode.ANSIX923, PaddingMode.ISO10126 };
		// FIXME: RijndaelManaged CFB is incompatible with other CFB modes, currently mono only supports None and Zeros
		static PaddingMode[] all_padding_modes = new [] { PaddingMode.None, PaddingMode.Zeros, PaddingMode.ISO10126 };

		protected override PaddingMode [] PaddingModes {
			get { return all_padding_modes; }
		}

		// unlike the other ciphers CFB is supported from 8 to BlockSize
		// maybe because it was the only managed implementation in th eoriginal framework ?!?
		protected override void CFB (SymmetricAlgorithm algo)
		{
			algo.Mode = CipherMode.CFB;
			// FIXME: mono currently only support CFB8 for RijndaelManaged
			for (int i = 8; i <= 8 /*algo.BlockSize*/; i += 8) {
				algo.FeedbackSize = i;
				CFB (algo, i);
			}
		}
		
		// the ICryptoTransform returned by RijndaelManaged do not bbehave like the rest of the framework
		protected override int GetTransformBlockSize (SymmetricAlgorithm algo)
		{
			return algo.FeedbackSize / 8;
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
			// block size: 128, key size: 128, padding: None, feedback: 8
			{ -2139094776, "99" },
			// block size: 128, key size: 128, padding: None, feedback: 16
			{ -2139094768, "99-17" },
			// block size: 128, key size: 128, padding: None, feedback: 24
			{ -2139094760, "99-17-B6" },
			// block size: 128, key size: 128, padding: None, feedback: 32
			{ -2139094752, "99-17-B6-28" },
			// block size: 128, key size: 128, padding: None, feedback: 40
			{ -2139094744, "99-17-B6-28-14" },
			// block size: 128, key size: 128, padding: None, feedback: 48
			{ -2139094736, "99-17-B6-28-14-70" },
			// block size: 128, key size: 128, padding: None, feedback: 56
			{ -2139094728, "99-17-B6-28-14-70-D5" },
			// block size: 128, key size: 128, padding: None, feedback: 64
			{ -2139094720, "99-17-B6-28-14-70-D5-C3" },
			// block size: 128, key size: 128, padding: None, feedback: 72
			{ -2139094712, "99-17-B6-28-14-70-D5-C3-7F" },
			// block size: 128, key size: 128, padding: None, feedback: 80
			{ -2139094704, "99-17-B6-28-14-70-D5-C3-7F-BA" },
			// block size: 128, key size: 128, padding: None, feedback: 88
			{ -2139094696, "99-17-B6-28-14-70-D5-C3-7F-BA-0F" },
			// block size: 128, key size: 128, padding: None, feedback: 96
			{ -2139094688, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD" },
			// block size: 128, key size: 128, padding: None, feedback: 104
			{ -2139094680, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39" },
			// block size: 128, key size: 128, padding: None, feedback: 112
			{ -2139094672, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6" },
			// block size: 128, key size: 128, padding: None, feedback: 120
			{ -2139094664, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA" },
			// block size: 128, key size: 128, padding: None, feedback: 128
			{ -2139094656, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-DE" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 8
			{ -2139094520, "99-67" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 16
			{ -2139094512, "99-17-02-C9" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 24
			{ -2139094504, "99-17-B6-55-B7-D6" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 32
			{ -2139094496, "99-17-B6-28-7E-F3-45-8A" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 40
			{ -2139094488, "99-17-B6-28-14-16-51-E5-D6-B4" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 48
			{ -2139094480, "99-17-B6-28-14-70-04-B5-5F-15-91-70" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 56
			{ -2139094472, "99-17-B6-28-14-70-D5-B8-03-99-DC-CE-58-69" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 64
			{ -2139094464, "99-17-B6-28-14-70-D5-C3-60-DA-04-D9-68-82-F6-F2" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 72
			{ -2139094456, "99-17-B6-28-14-70-D5-C3-7F-A9-38-68-AE-F2-E1-A3-85-6A" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 80
			{ -2139094448, "99-17-B6-28-14-70-D5-C3-7F-BA-0E-CB-7F-93-EF-9F-C4-02-0B-AE" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 88
			{ -2139094440, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-DA-86-18-46-7F-39-96-E0-40-0D-78" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 96
			{ -2139094432, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-B0-CB-DB-F1-29-4B-8D-8D-2C-D0-BB-48" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 104
			{ -2139094424, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-40-09-85-69-CC-F5-10-EF-AF-A3-D8-9A-65" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 112
			{ -2139094416, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-10-28-77-22-43-0D-4B-7C-B0-9F-4B-49-A0-82" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 120
			{ -2139094408, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-42-07-D8-D5-10-22-5A-7A-FF-16-F2-C3-4E-DE-54" },
			// block size: 128, key size: 128, padding: PKCS7, feedback: 128
			{ -2139094400, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-DE-B2-F4-C7-44-49-E6-0B-30-3E-62-07-00-DF-61-80-8E" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 8
			{ -2139094264, "99" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 16
			{ -2139094256, "99-17" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 24
			{ -2139094248, "99-17-B6" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 32
			{ -2139094240, "99-17-B6-28" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 40
			{ -2139094232, "99-17-B6-28-14" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 48
			{ -2139094224, "99-17-B6-28-14-70" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 56
			{ -2139094216, "99-17-B6-28-14-70-D5" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 64
			{ -2139094208, "99-17-B6-28-14-70-D5-C3" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 72
			{ -2139094200, "99-17-B6-28-14-70-D5-C3-7F" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 80
			{ -2139094192, "99-17-B6-28-14-70-D5-C3-7F-BA" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 88
			{ -2139094184, "99-17-B6-28-14-70-D5-C3-7F-BA-0F" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 96
			{ -2139094176, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 104
			{ -2139094168, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 112
			{ -2139094160, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 120
			{ -2139094152, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA" },
			// block size: 128, key size: 128, padding: Zeros, feedback: 128
			{ -2139094144, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-DE" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 8
			{ -2139094008, "99-67" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 16
			{ -2139094000, "99-17-00-C9" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 24
			{ -2139093992, "99-17-B6-56-B4-D6" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 32
			{ -2139093984, "99-17-B6-28-7A-F7-41-8A" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 40
			{ -2139093976, "99-17-B6-28-14-13-54-E0-D3-B4" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 48
			{ -2139093968, "99-17-B6-28-14-70-02-B3-59-13-97-70" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 56
			{ -2139093960, "99-17-B6-28-14-70-D5-BF-04-9E-DB-C9-5F-69" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 64
			{ -2139093952, "99-17-B6-28-14-70-D5-C3-68-D2-0C-D1-60-8A-FE-F2" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 72
			{ -2139093944, "99-17-B6-28-14-70-D5-C3-7F-A0-31-61-A7-FB-E8-AA-8C-6A" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 80
			{ -2139093936, "99-17-B6-28-14-70-D5-C3-7F-BA-04-C1-75-99-E5-95-CE-08-01-AE" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 88
			{ -2139093928, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-D1-8D-13-4D-74-32-9D-EB-4B-06-78" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 96
			{ -2139093920, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-BC-C7-D7-FD-25-47-81-81-20-DC-B7-48" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 104
			{ -2139093912, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-4D-04-88-64-C1-F8-1D-E2-A2-AE-D5-97-65" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 112
			{ -2139093904, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-1E-26-79-2C-4D-03-45-72-BE-91-45-47-AE-82" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 120
			{ -2139093896, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-4D-08-D7-DA-1F-2D-55-75-F0-19-FD-CC-41-D1-54" },
			// block size: 128, key size: 128, padding: ANSIX923, feedback: 128
			{ -2139093888, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-DE-A2-E4-D7-54-59-F6-1B-20-2E-72-17-10-CF-71-90-8E" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 8
			{ -2139093752, "99-67" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 16
			{ -2139093744, "99-17-4A-C9" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 24
			{ -2139093736, "99-17-B6-46-A0-D6" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 32
			{ -2139093728, "99-17-B6-28-1F-8C-7C-8A" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 40
			{ -2139093720, "99-17-B6-28-14-88-0E-B8-56-B4" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 48
			{ -2139093712, "99-17-B6-28-14-70-7C-46-6E-84-B3-70" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 56
			{ -2139093704, "99-17-B6-28-14-70-D5-85-55-D0-11-34-C2-69" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 64
			{ -2139093696, "99-17-B6-28-14-70-D5-C3-85-4E-39-EB-26-DE-24-F2" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 72
			{ -2139093688, "99-17-B6-28-14-70-D5-C3-7F-E5-6E-F0-D8-71-56-31-71-6A" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 80
			{ -2139093680, "99-17-B6-28-14-70-D5-C3-7F-BA-48-DB-3C-85-E2-0C-E1-C4-FA-AE" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 88
			{ -2139093672, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-BF-46-E3-78-D6-A7-D9-A8-FF-69-78" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 96
			{ -2139093664, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-8C-09-27-1D-25-09-82-E6-75-9D-D1-48" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 104
			{ -2139093656, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-01-21-5D-1C-E5-4C-B9-96-46-D5-4B-FD-65" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 112
			{ -2139093648, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-BB-11-A0-98-9D-98-73-54-79-D7-C5-7A-E3-82" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 120
			{ -2139093640, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-9A-BB-7F-69-E8-B6-05-59-D8-FB-AC-EC-DC-1D-54" },
			// block size: 128, key size: 128, padding: ISO10126, feedback: 128
			{ -2139093632, "99-17-B6-28-14-70-D5-C3-7F-BA-0F-AD-39-C6-DA-DE-DF-46-3B-3D-32-25-85-BD-18-F8-1A-03-77-EA-80-8E" },
			// block size: 128, key size: 192, padding: None, feedback: 8
			{ -2134900472, "55" },
			// block size: 128, key size: 192, padding: None, feedback: 16
			{ -2134900464, "55-1E" },
			// block size: 128, key size: 192, padding: None, feedback: 24
			{ -2134900456, "55-1E-94" },
			// block size: 128, key size: 192, padding: None, feedback: 32
			{ -2134900448, "55-1E-94-6E" },
			// block size: 128, key size: 192, padding: None, feedback: 40
			{ -2134900440, "55-1E-94-6E-57" },
			// block size: 128, key size: 192, padding: None, feedback: 48
			{ -2134900432, "55-1E-94-6E-57-45" },
			// block size: 128, key size: 192, padding: None, feedback: 56
			{ -2134900424, "55-1E-94-6E-57-45-AB" },
			// block size: 128, key size: 192, padding: None, feedback: 64
			{ -2134900416, "55-1E-94-6E-57-45-AB-5B" },
			// block size: 128, key size: 192, padding: None, feedback: 72
			{ -2134900408, "55-1E-94-6E-57-45-AB-5B-1F" },
			// block size: 128, key size: 192, padding: None, feedback: 80
			{ -2134900400, "55-1E-94-6E-57-45-AB-5B-1F-02" },
			// block size: 128, key size: 192, padding: None, feedback: 88
			{ -2134900392, "55-1E-94-6E-57-45-AB-5B-1F-02-5C" },
			// block size: 128, key size: 192, padding: None, feedback: 96
			{ -2134900384, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A" },
			// block size: 128, key size: 192, padding: None, feedback: 104
			{ -2134900376, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A" },
			// block size: 128, key size: 192, padding: None, feedback: 112
			{ -2134900368, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2" },
			// block size: 128, key size: 192, padding: None, feedback: 120
			{ -2134900360, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA" },
			// block size: 128, key size: 192, padding: None, feedback: 128
			{ -2134900352, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-27" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 8
			{ -2134900216, "55-10" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 16
			{ -2134900208, "55-1E-18-51" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 24
			{ -2134900200, "55-1E-94-BC-8A-49" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 32
			{ -2134900192, "55-1E-94-6E-66-F8-AE-CA" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 40
			{ -2134900184, "55-1E-94-6E-57-F5-1E-F6-31-6C" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 48
			{ -2134900176, "55-1E-94-6E-57-45-FF-2D-66-AA-7E-CB" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 56
			{ -2134900168, "55-1E-94-6E-57-45-AB-82-F5-BB-97-67-87-B7" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 64
			{ -2134900160, "55-1E-94-6E-57-45-AB-5B-91-41-F0-06-67-C3-DF-93" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 72
			{ -2134900152, "55-1E-94-6E-57-45-AB-5B-1F-BF-A4-52-BB-F4-36-2C-33-D5" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 80
			{ -2134900144, "55-1E-94-6E-57-45-AB-5B-1F-02-6C-4F-F8-F5-77-F2-55-1D-AB-87" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 88
			{ -2134900136, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-76-39-3E-B8-9B-00-1B-70-80-EE-A5" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 96
			{ -2134900128, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-7F-73-DF-B8-DA-17-95-11-39-61-89-F7" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 104
			{ -2134900120, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-24-C9-1A-2A-80-3A-D8-76-F5-90-06-40-67" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 112
			{ -2134900112, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-EA-D6-F2-8E-18-3C-16-4F-BE-38-82-35-0A-CA" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 120
			{ -2134900104, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-88-C0-1B-49-FA-0D-00-F7-0B-05-CF-C8-66-6E-EC" },
			// block size: 128, key size: 192, padding: PKCS7, feedback: 128
			{ -2134900096, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-27-97-AB-F5-1B-D5-57-10-32-FC-83-FE-2E-D9-D3-EE-B3" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 8
			{ -2134899960, "55" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 16
			{ -2134899952, "55-1E" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 24
			{ -2134899944, "55-1E-94" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 32
			{ -2134899936, "55-1E-94-6E" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 40
			{ -2134899928, "55-1E-94-6E-57" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 48
			{ -2134899920, "55-1E-94-6E-57-45" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 56
			{ -2134899912, "55-1E-94-6E-57-45-AB" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 64
			{ -2134899904, "55-1E-94-6E-57-45-AB-5B" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 72
			{ -2134899896, "55-1E-94-6E-57-45-AB-5B-1F" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 80
			{ -2134899888, "55-1E-94-6E-57-45-AB-5B-1F-02" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 88
			{ -2134899880, "55-1E-94-6E-57-45-AB-5B-1F-02-5C" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 96
			{ -2134899872, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 104
			{ -2134899864, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 112
			{ -2134899856, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 120
			{ -2134899848, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA" },
			// block size: 128, key size: 192, padding: Zeros, feedback: 128
			{ -2134899840, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-27" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 8
			{ -2134899704, "55-10" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 16
			{ -2134899696, "55-1E-1A-51" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 24
			{ -2134899688, "55-1E-94-BF-89-49" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 32
			{ -2134899680, "55-1E-94-6E-62-FC-AA-CA" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 40
			{ -2134899672, "55-1E-94-6E-57-F0-1B-F3-34-6C" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 48
			{ -2134899664, "55-1E-94-6E-57-45-F9-2B-60-AC-78-CB" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 56
			{ -2134899656, "55-1E-94-6E-57-45-AB-85-F2-BC-90-60-80-B7" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 64
			{ -2134899648, "55-1E-94-6E-57-45-AB-5B-99-49-F8-0E-6F-CB-D7-93" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 72
			{ -2134899640, "55-1E-94-6E-57-45-AB-5B-1F-B6-AD-5B-B2-FD-3F-25-3A-D5" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 80
			{ -2134899632, "55-1E-94-6E-57-45-AB-5B-1F-02-66-45-F2-FF-7D-F8-5F-17-A1-87" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 88
			{ -2134899624, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-7D-32-35-B3-90-0B-10-7B-8B-E5-A5" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 96
			{ -2134899616, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-73-7F-D3-B4-D6-1B-99-1D-35-6D-85-F7" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 104
			{ -2134899608, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-29-C4-17-27-8D-37-D5-7B-F8-9D-0B-4D-67" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 112
			{ -2134899600, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-E4-D8-FC-80-16-32-18-41-B0-36-8C-3B-04-CA" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 120
			{ -2134899592, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-87-CF-14-46-F5-02-0F-F8-04-0A-C0-C7-69-61-EC" },
			// block size: 128, key size: 192, padding: ANSIX923, feedback: 128
			{ -2134899584, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-27-87-BB-E5-0B-C5-47-00-22-EC-93-EE-3E-C9-C3-FE-B3" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 8
			{ -2134899448, "55-10" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 16
			{ -2134899440, "55-1E-B1-51" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 24
			{ -2134899432, "55-1E-94-E4-37-49" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 32
			{ -2134899424, "55-1E-94-6E-9B-EF-03-CA" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 40
			{ -2134899416, "55-1E-94-6E-57-74-19-94-44-6C" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 48
			{ -2134899408, "55-1E-94-6E-57-45-CA-8C-78-A0-02-CB" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 56
			{ -2134899400, "55-1E-94-6E-57-45-AB-66-75-65-B0-C6-B2-B7" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 64
			{ -2134899392, "55-1E-94-6E-57-45-AB-5B-49-03-E4-0F-F1-4F-75-93" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 72
			{ -2134899384, "55-1E-94-6E-57-45-AB-5B-1F-88-54-54-E5-03-33-32-5C-D5" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 80
			{ -2134899376, "55-1E-94-6E-57-45-AB-5B-1F-02-93-71-19-31-49-67-24-65-AD-87" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 88
			{ -2134899368, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-07-AA-77-C9-96-2E-22-2E-8D-DE-A5" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 96
			{ -2134899360, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-27-C4-C9-10-18-4C-1B-96-91-68-09-F7" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 104
			{ -2134899352, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-77-99-91-CE-8B-BA-08-9F-7A-AF-07-D6-67" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 112
			{ -2134899344, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-EE-3C-70-18-F1-7E-32-3D-93-45-6F-06-BC-CA" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 120
			{ -2134899336, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-57-45-7D-1B-F4-65-31-62-7B-72-12-69-81-C4-EC" },
			// block size: 128, key size: 192, padding: ISO10126, feedback: 128
			{ -2134899328, "55-1E-94-6E-57-45-AB-5B-1F-02-5C-9A-3A-C2-FA-27-17-DE-64-F7-B8-F7-BD-08-DB-17-80-4B-CA-73-56-B3" },
			// block size: 128, key size: 256, padding: None, feedback: 8
			{ -2130706168, "23" },
			// block size: 128, key size: 256, padding: None, feedback: 16
			{ -2130706160, "23-6B" },
			// block size: 128, key size: 256, padding: None, feedback: 24
			{ -2130706152, "23-6B-3D" },
			// block size: 128, key size: 256, padding: None, feedback: 32
			{ -2130706144, "23-6B-3D-84" },
			// block size: 128, key size: 256, padding: None, feedback: 40
			{ -2130706136, "23-6B-3D-84-59" },
			// block size: 128, key size: 256, padding: None, feedback: 48
			{ -2130706128, "23-6B-3D-84-59-BA" },
			// block size: 128, key size: 256, padding: None, feedback: 56
			{ -2130706120, "23-6B-3D-84-59-BA-70" },
			// block size: 128, key size: 256, padding: None, feedback: 64
			{ -2130706112, "23-6B-3D-84-59-BA-70-71" },
			// block size: 128, key size: 256, padding: None, feedback: 72
			{ -2130706104, "23-6B-3D-84-59-BA-70-71-5A" },
			// block size: 128, key size: 256, padding: None, feedback: 80
			{ -2130706096, "23-6B-3D-84-59-BA-70-71-5A-BE" },
			// block size: 128, key size: 256, padding: None, feedback: 88
			{ -2130706088, "23-6B-3D-84-59-BA-70-71-5A-BE-57" },
			// block size: 128, key size: 256, padding: None, feedback: 96
			{ -2130706080, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0" },
			// block size: 128, key size: 256, padding: None, feedback: 104
			{ -2130706072, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61" },
			// block size: 128, key size: 256, padding: None, feedback: 112
			{ -2130706064, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76" },
			// block size: 128, key size: 256, padding: None, feedback: 120
			{ -2130706056, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1" },
			// block size: 128, key size: 256, padding: None, feedback: 128
			{ -2130706048, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-77" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 8
			{ -2130705912, "23-D7" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 16
			{ -2130705904, "23-6B-63-19" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 24
			{ -2130705896, "23-6B-3D-2E-EB-ED" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 32
			{ -2130705888, "23-6B-3D-84-59-96-6E-D5" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 40
			{ -2130705880, "23-6B-3D-84-59-6A-16-24-B5-6B" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 48
			{ -2130705872, "23-6B-3D-84-59-BA-FC-77-85-57-8B-1F" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 56
			{ -2130705864, "23-6B-3D-84-59-BA-70-AB-5F-2B-90-86-DB-2E" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 64
			{ -2130705856, "23-6B-3D-84-59-BA-70-71-FC-79-4D-59-35-F7-00-BB" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 72
			{ -2130705848, "23-6B-3D-84-59-BA-70-71-5A-DA-58-CD-4D-00-36-70-4E-E4" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 80
			{ -2130705840, "23-6B-3D-84-59-BA-70-71-5A-BE-C7-39-35-7C-F5-24-4D-9C-DC-38" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 88
			{ -2130705832, "23-6B-3D-84-59-BA-70-71-5A-BE-57-69-73-64-5E-4D-53-35-75-F9-21-81" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 96
			{ -2130705824, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-6C-24-B4-8B-C0-D7-4E-B7-03-06-67-AB" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 104
			{ -2130705816, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-F7-16-F5-1B-43-CF-5B-0A-59-14-C9-CC-B0" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 112
			{ -2130705808, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-38-92-CF-4A-AB-2E-87-18-E0-CB-37-4A-E7-5E" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 120
			{ -2130705800, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-15-D1-72-05-96-75-AF-76-9D-72-1A-CA-F0-B2-85" },
			// block size: 128, key size: 256, padding: PKCS7, feedback: 128
			{ -2130705792, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-77-0D-3D-2D-FD-F3-DA-1D-F6-5A-79-C0-20-39-54-DB-4E" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 8
			{ -2130705656, "23" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 16
			{ -2130705648, "23-6B" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 24
			{ -2130705640, "23-6B-3D" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 32
			{ -2130705632, "23-6B-3D-84" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 40
			{ -2130705624, "23-6B-3D-84-59" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 48
			{ -2130705616, "23-6B-3D-84-59-BA" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 56
			{ -2130705608, "23-6B-3D-84-59-BA-70" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 64
			{ -2130705600, "23-6B-3D-84-59-BA-70-71" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 72
			{ -2130705592, "23-6B-3D-84-59-BA-70-71-5A" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 80
			{ -2130705584, "23-6B-3D-84-59-BA-70-71-5A-BE" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 88
			{ -2130705576, "23-6B-3D-84-59-BA-70-71-5A-BE-57" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 96
			{ -2130705568, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 104
			{ -2130705560, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 112
			{ -2130705552, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 120
			{ -2130705544, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1" },
			// block size: 128, key size: 256, padding: Zeros, feedback: 128
			{ -2130705536, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-77" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 8
			{ -2130705400, "23-D7" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 16
			{ -2130705392, "23-6B-61-19" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 24
			{ -2130705384, "23-6B-3D-2D-E8-ED" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 32
			{ -2130705376, "23-6B-3D-84-5D-92-6A-D5" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 40
			{ -2130705368, "23-6B-3D-84-59-6F-13-21-B0-6B" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 48
			{ -2130705360, "23-6B-3D-84-59-BA-FA-71-83-51-8D-1F" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 56
			{ -2130705352, "23-6B-3D-84-59-BA-70-AC-58-2C-97-81-DC-2E" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 64
			{ -2130705344, "23-6B-3D-84-59-BA-70-71-F4-71-45-51-3D-FF-08-BB" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 72
			{ -2130705336, "23-6B-3D-84-59-BA-70-71-5A-D3-51-C4-44-09-3F-79-47-E4" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 80
			{ -2130705328, "23-6B-3D-84-59-BA-70-71-5A-BE-CD-33-3F-76-FF-2E-47-96-D6-38" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 88
			{ -2130705320, "23-6B-3D-84-59-BA-70-71-5A-BE-57-62-78-6F-55-46-58-3E-7E-F2-2A-81" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 96
			{ -2130705312, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-60-28-B8-87-CC-DB-42-BB-0F-0A-6B-AB" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 104
			{ -2130705304, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-FA-1B-F8-16-4E-C2-56-07-54-19-C4-C1-B0" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 112
			{ -2130705296, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-36-9C-C1-44-A5-20-89-16-EE-C5-39-44-E9-5E" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 120
			{ -2130705288, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-1A-DE-7D-0A-99-7A-A0-79-92-7D-15-C5-FF-BD-85" },
			// block size: 128, key size: 256, padding: ANSIX923, feedback: 128
			{ -2130705280, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-77-1D-2D-3D-ED-E3-CA-0D-E6-4A-69-D0-30-29-44-CB-4E" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 8
			{ -2130705144, "23-D7" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 16
			{ -2130705136, "23-6B-8C-19" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 24
			{ -2130705128, "23-6B-3D-B6-84-ED" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 32
			{ -2130705120, "23-6B-3D-84-83-51-BF-D5" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 40
			{ -2130705112, "23-6B-3D-84-59-59-16-74-49-6B" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 48
			{ -2130705104, "23-6B-3D-84-59-BA-F3-EA-8C-22-0F-1F" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 56
			{ -2130705096, "23-6B-3D-84-59-BA-70-79-E1-C4-8D-3A-C7-2E" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 64
			{ -2130705088, "23-6B-3D-84-59-BA-70-71-36-C0-AC-52-76-3A-3E-BB" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 72
			{ -2130705080, "23-6B-3D-84-59-BA-70-71-5A-77-CC-F8-0B-BD-D4-EB-BD-E4" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 80
			{ -2130705072, "23-6B-3D-84-59-BA-70-71-5A-BE-F0-65-7E-26-E6-C6-30-DC-0B-38" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 88
			{ -2130705064, "23-6B-3D-84-59-BA-70-71-5A-BE-57-96-A1-88-12-60-E4-14-CF-D2-A0-81" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 96
			{ -2130705056, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-A6-12-7F-95-0A-63-E2-28-7A-3D-32-AB" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 104
			{ -2130705048, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-C5-14-54-72-59-92-7E-20-36-66-E3-58-B0" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 112
			{ -2130705040, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-DB-9C-03-8F-0F-1D-E1-5C-47-2E-BA-7C-FF-5E" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 120
			{ -2130705032, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-6B-3E-C8-43-9D-1C-79-46-D1-35-2B-F2-01-07-85" },
			// block size: 128, key size: 256, padding: ISO10126, feedback: 128
			{ -2130705024, "23-6B-3D-84-59-BA-70-71-5A-BE-57-E0-61-76-D1-77-53-AD-CD-7F-49-6E-E9-AF-C1-29-05-18-09-B3-D8-4E" },
			// block size: 192, key size: 128, padding: None, feedback: 8
			{ -1065352952, "56" },
			// block size: 192, key size: 128, padding: None, feedback: 16
			{ -1065352944, "56-D9" },
			// block size: 192, key size: 128, padding: None, feedback: 24
			{ -1065352936, "56-D9-CF" },
			// block size: 192, key size: 128, padding: None, feedback: 32
			{ -1065352928, "56-D9-CF-17" },
			// block size: 192, key size: 128, padding: None, feedback: 40
			{ -1065352920, "56-D9-CF-17-B3" },
			// block size: 192, key size: 128, padding: None, feedback: 48
			{ -1065352912, "56-D9-CF-17-B3-77" },
			// block size: 192, key size: 128, padding: None, feedback: 56
			{ -1065352904, "56-D9-CF-17-B3-77-72" },
			// block size: 192, key size: 128, padding: None, feedback: 64
			{ -1065352896, "56-D9-CF-17-B3-77-72-41" },
			// block size: 192, key size: 128, padding: None, feedback: 72
			{ -1065352888, "56-D9-CF-17-B3-77-72-41-79" },
			// block size: 192, key size: 128, padding: None, feedback: 80
			{ -1065352880, "56-D9-CF-17-B3-77-72-41-79-3B" },
			// block size: 192, key size: 128, padding: None, feedback: 88
			{ -1065352872, "56-D9-CF-17-B3-77-72-41-79-3B-78" },
			// block size: 192, key size: 128, padding: None, feedback: 96
			{ -1065352864, "56-D9-CF-17-B3-77-72-41-79-3B-78-61" },
			// block size: 192, key size: 128, padding: None, feedback: 104
			{ -1065352856, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F" },
			// block size: 192, key size: 128, padding: None, feedback: 112
			{ -1065352848, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E" },
			// block size: 192, key size: 128, padding: None, feedback: 120
			{ -1065352840, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3" },
			// block size: 192, key size: 128, padding: None, feedback: 128
			{ -1065352832, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10" },
			// block size: 192, key size: 128, padding: None, feedback: 136
			{ -1065352824, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD" },
			// block size: 192, key size: 128, padding: None, feedback: 144
			{ -1065352816, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C" },
			// block size: 192, key size: 128, padding: None, feedback: 152
			{ -1065352808, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD" },
			// block size: 192, key size: 128, padding: None, feedback: 160
			{ -1065352800, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41" },
			// block size: 192, key size: 128, padding: None, feedback: 168
			{ -1065352792, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD" },
			// block size: 192, key size: 128, padding: None, feedback: 176
			{ -1065352784, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71" },
			// block size: 192, key size: 128, padding: None, feedback: 184
			{ -1065352776, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1" },
			// block size: 192, key size: 128, padding: None, feedback: 192
			{ -1065352768, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-A1" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 8
			{ -1065352696, "56-58" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 16
			{ -1065352688, "56-D9-A3-21" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 24
			{ -1065352680, "56-D9-CF-29-90-6D" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 32
			{ -1065352672, "56-D9-CF-17-CB-39-5A-06" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 40
			{ -1065352664, "56-D9-CF-17-B3-E1-23-9A-CC-90" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 48
			{ -1065352656, "56-D9-CF-17-B3-77-59-18-0A-A2-5A-92" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 56
			{ -1065352648, "56-D9-CF-17-B3-77-72-E6-9C-5B-AB-0C-C9-F9" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 64
			{ -1065352640, "56-D9-CF-17-B3-77-72-41-FB-72-CF-FA-BE-99-9D-00" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 72
			{ -1065352632, "56-D9-CF-17-B3-77-72-41-79-65-29-24-E8-17-13-12-4D-DA" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 80
			{ -1065352624, "56-D9-CF-17-B3-77-72-41-79-3B-67-07-D4-A7-65-9A-22-8B-25-76" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 88
			{ -1065352616, "56-D9-CF-17-B3-77-72-41-79-3B-78-21-5C-69-FF-9C-6C-D3-F6-34-FE-CD" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 96
			{ -1065352608, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-29-5A-55-87-14-E0-DA-37-AB-AB-15-70" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 104
			{ -1065352600, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-5B-FA-64-69-37-A7-8E-2D-BA-58-6C-05-63" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 112
			{ -1065352592, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-46-6D-35-D3-ED-E2-41-71-73-32-C8-FA-5E-A9" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 120
			{ -1065352584, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-EE-53-9A-49-08-A6-74-85-51-D5-EC-A3-D2-8D-30" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 128
			{ -1065352576, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-0C-AA-91-24-16-36-B2-E5-8C-39-75-65-6C-98-ED-64" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 136
			{ -1065352568, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-99-50-C8-E9-2C-E4-B3-13-C7-0C-56-39-29-F1-BD-AB-5B" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 144
			{ -1065352560, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-AA-FE-C9-F0-80-D9-48-23-95-26-2C-94-EE-DF-5A-95-51-E1" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 152
			{ -1065352552, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-62-9F-20-64-7C-CE-5A-64-E3-9F-71-5D-D0-83-F8-71-2B-F8-B2" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 160
			{ -1065352544, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-C9-19-BB-19-13-22-09-2E-28-3A-53-B2-DC-09-60-EC-07-F6-1F-4B" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 168
			{ -1065352536, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-79-AC-18-57-04-B8-E9-9B-89-74-F1-C3-93-5A-FC-97-C5-5C-3B-55-E6" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 176
			{ -1065352528, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-05-08-47-C9-A3-AA-ED-1A-F0-8C-56-E2-3D-AD-9F-44-66-CD-0E-D6-EF-18" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 184
			{ -1065352520, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-3A-14-37-E3-29-09-6D-B1-63-2C-F6-63-10-E5-69-9D-F0-49-E9-48-64-81-D0" },
			// block size: 192, key size: 128, padding: PKCS7, feedback: 192
			{ -1065352512, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-A1-22-E6-DC-F3-96-FD-7D-14-1A-D8-54-E0-04-22-5F-06-F5-A9-8D-F8-AB-13-3E-FE" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 8
			{ -1065352440, "56" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 16
			{ -1065352432, "56-D9" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 24
			{ -1065352424, "56-D9-CF" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 32
			{ -1065352416, "56-D9-CF-17" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 40
			{ -1065352408, "56-D9-CF-17-B3" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 48
			{ -1065352400, "56-D9-CF-17-B3-77" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 56
			{ -1065352392, "56-D9-CF-17-B3-77-72" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 64
			{ -1065352384, "56-D9-CF-17-B3-77-72-41" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 72
			{ -1065352376, "56-D9-CF-17-B3-77-72-41-79" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 80
			{ -1065352368, "56-D9-CF-17-B3-77-72-41-79-3B" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 88
			{ -1065352360, "56-D9-CF-17-B3-77-72-41-79-3B-78" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 96
			{ -1065352352, "56-D9-CF-17-B3-77-72-41-79-3B-78-61" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 104
			{ -1065352344, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 112
			{ -1065352336, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 120
			{ -1065352328, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 128
			{ -1065352320, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 136
			{ -1065352312, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 144
			{ -1065352304, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 152
			{ -1065352296, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 160
			{ -1065352288, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 168
			{ -1065352280, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 176
			{ -1065352272, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 184
			{ -1065352264, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1" },
			// block size: 192, key size: 128, padding: Zeros, feedback: 192
			{ -1065352256, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-A1" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 8
			{ -1065352184, "56-58" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 16
			{ -1065352176, "56-D9-A1-21" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 24
			{ -1065352168, "56-D9-CF-2A-93-6D" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 32
			{ -1065352160, "56-D9-CF-17-CF-3D-5E-06" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 40
			{ -1065352152, "56-D9-CF-17-B3-E4-26-9F-C9-90" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 48
			{ -1065352144, "56-D9-CF-17-B3-77-5F-1E-0C-A4-5C-92" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 56
			{ -1065352136, "56-D9-CF-17-B3-77-72-E1-9B-5C-AC-0B-CE-F9" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 64
			{ -1065352128, "56-D9-CF-17-B3-77-72-41-F3-7A-C7-F2-B6-91-95-00" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 72
			{ -1065352120, "56-D9-CF-17-B3-77-72-41-79-6C-20-2D-E1-1E-1A-1B-44-DA" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 80
			{ -1065352112, "56-D9-CF-17-B3-77-72-41-79-3B-6D-0D-DE-AD-6F-90-28-81-2F-76" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 88
			{ -1065352104, "56-D9-CF-17-B3-77-72-41-79-3B-78-2A-57-62-F4-97-67-D8-FD-3F-F5-CD" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 96
			{ -1065352096, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-25-56-59-8B-18-EC-D6-3B-A7-A7-19-70" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 104
			{ -1065352088, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-56-F7-69-64-3A-AA-83-20-B7-55-61-08-63" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 112
			{ -1065352080, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-48-63-3B-DD-E3-EC-4F-7F-7D-3C-C6-F4-50-A9" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 120
			{ -1065352072, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-E1-5C-95-46-07-A9-7B-8A-5E-DA-E3-AC-DD-82-30" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 128
			{ -1065352064, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-1C-BA-81-34-06-26-A2-F5-9C-29-65-75-7C-88-FD-64" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 136
			{ -1065352056, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-88-41-D9-F8-3D-F5-A2-02-D6-1D-47-28-38-E0-AC-BA-5B" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 144
			{ -1065352048, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-B8-EC-DB-E2-92-CB-5A-31-87-34-3E-86-FC-CD-48-87-43-E1" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 152
			{ -1065352040, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-71-8C-33-77-6F-DD-49-77-F0-8C-62-4E-C3-90-EB-62-38-EB-B2" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 160
			{ -1065352032, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-0D-AF-0D-07-36-1D-3A-3C-2E-47-A6-C8-1D-74-F8-13-E2-0B-4B" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 168
			{ -1065352024, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-6C-B9-0D-42-11-AD-FC-8E-9C-61-E4-D6-86-4F-E9-82-D0-49-2E-40-E6" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 176
			{ -1065352016, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-13-1E-51-DF-B5-BC-FB-0C-E6-9A-40-F4-2B-BB-89-52-70-DB-18-C0-F9-18" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 184
			{ -1065352008, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-2D-03-20-F4-3E-1E-7A-A6-74-3B-E1-74-07-F2-7E-8A-E7-5E-FE-5F-73-96-D0" },
			// block size: 192, key size: 128, padding: ANSIX923, feedback: 192
			{ -1065352000, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-A1-3A-FE-C4-EB-8E-E5-65-0C-02-C0-4C-F8-1C-3A-47-1E-ED-B1-95-E0-B3-0B-26-FE" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 8
			{ -1065351928, "56-58" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 16
			{ -1065351920, "56-D9-F0-21" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 24
			{ -1065351912, "56-D9-CF-5D-9F-6D" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 32
			{ -1065351904, "56-D9-CF-17-36-34-23-06" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 40
			{ -1065351896, "56-D9-CF-17-B3-B9-76-1A-31-90" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 48
			{ -1065351888, "56-D9-CF-17-B3-77-F6-2C-C9-61-7D-92" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 56
			{ -1065351880, "56-D9-CF-17-B3-77-72-1B-05-CB-B4-72-87-F9" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 64
			{ -1065351872, "56-D9-CF-17-B3-77-72-41-4C-F1-F4-D8-B0-BB-29-00" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 72
			{ -1065351864, "56-D9-CF-17-B3-77-72-41-79-25-01-58-8B-4F-CA-EB-22-DA" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 80
			{ -1065351856, "56-D9-CF-17-B3-77-72-41-79-3B-42-86-90-34-20-C0-1D-F1-7A-76" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 88
			{ -1065351848, "56-D9-CF-17-B3-77-72-41-79-3B-78-EF-82-2B-AD-FA-17-E9-F0-80-89-CD" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 96
			{ -1065351840, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-06-7C-0F-10-37-7D-D6-63-EF-6D-28-70" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 104
			{ -1065351832, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-8C-E2-2C-52-3B-27-36-C6-62-D1-A6-91-63" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 112
			{ -1065351824, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-69-EB-D9-1C-9B-FD-62-9B-58-DF-6B-75-45-A9" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 120
			{ -1065351816, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-55-C7-03-07-7D-2D-55-12-84-F7-CD-6C-85-AB-30" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 128
			{ -1065351808, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-5A-E0-C9-18-4F-7C-0A-F5-C0-6D-9E-38-C4-13-11-64" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 136
			{ -1065351800, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1F-09-EF-7A-60-A7-AF-A5-FF-E1-2C-2F-2E-A4-F9-08-5B" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 144
			{ -1065351792, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-9C-90-84-28-E2-3E-FB-BB-76-CE-CE-66-83-BC-AB-36-01-E1" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 152
			{ -1065351784, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-9D-C4-24-FE-3A-F9-12-2B-B2-F3-E2-E3-73-9E-FC-48-0E-87-B2" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 160
			{ -1065351776, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-39-F4-B6-78-24-43-AE-8F-65-B6-1C-88-60-3A-83-8F-2C-42-10-4B" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 168
			{ -1065351768, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-F2-07-8C-09-CF-62-25-8D-93-9D-92-49-2D-6F-C7-21-3E-C8-26-ED-E6" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 176
			{ -1065351760, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-0F-03-DC-03-F5-D2-48-22-42-9D-98-3A-4A-83-28-32-79-21-50-39-97-18" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 184
			{ -1065351752, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-19-FA-75-2B-55-3C-E1-BD-D4-01-08-E9-0D-63-46-53-F3-E1-5F-72-14-4F-D0" },
			// block size: 192, key size: 128, padding: ISO10126, feedback: 192
			{ -1065351744, "56-D9-CF-17-B3-77-72-41-79-3B-78-61-2F-6E-F3-10-BD-1C-BD-41-DD-71-D1-A1-15-EA-B7-A2-E3-5C-7A-FA-CA-96-63-24-CA-40-10-07-F8-76-A3-8F-50-CB-36-FE" },
			// block size: 192, key size: 192, padding: None, feedback: 8
			{ -1061158648, "39" },
			// block size: 192, key size: 192, padding: None, feedback: 16
			{ -1061158640, "39-CA" },
			// block size: 192, key size: 192, padding: None, feedback: 24
			{ -1061158632, "39-CA-76" },
			// block size: 192, key size: 192, padding: None, feedback: 32
			{ -1061158624, "39-CA-76-1E" },
			// block size: 192, key size: 192, padding: None, feedback: 40
			{ -1061158616, "39-CA-76-1E-FB" },
			// block size: 192, key size: 192, padding: None, feedback: 48
			{ -1061158608, "39-CA-76-1E-FB-FD" },
			// block size: 192, key size: 192, padding: None, feedback: 56
			{ -1061158600, "39-CA-76-1E-FB-FD-43" },
			// block size: 192, key size: 192, padding: None, feedback: 64
			{ -1061158592, "39-CA-76-1E-FB-FD-43-3C" },
			// block size: 192, key size: 192, padding: None, feedback: 72
			{ -1061158584, "39-CA-76-1E-FB-FD-43-3C-5F" },
			// block size: 192, key size: 192, padding: None, feedback: 80
			{ -1061158576, "39-CA-76-1E-FB-FD-43-3C-5F-4B" },
			// block size: 192, key size: 192, padding: None, feedback: 88
			{ -1061158568, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97" },
			// block size: 192, key size: 192, padding: None, feedback: 96
			{ -1061158560, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D" },
			// block size: 192, key size: 192, padding: None, feedback: 104
			{ -1061158552, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF" },
			// block size: 192, key size: 192, padding: None, feedback: 112
			{ -1061158544, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73" },
			// block size: 192, key size: 192, padding: None, feedback: 120
			{ -1061158536, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6" },
			// block size: 192, key size: 192, padding: None, feedback: 128
			{ -1061158528, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52" },
			// block size: 192, key size: 192, padding: None, feedback: 136
			{ -1061158520, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC" },
			// block size: 192, key size: 192, padding: None, feedback: 144
			{ -1061158512, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0" },
			// block size: 192, key size: 192, padding: None, feedback: 152
			{ -1061158504, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B" },
			// block size: 192, key size: 192, padding: None, feedback: 160
			{ -1061158496, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2" },
			// block size: 192, key size: 192, padding: None, feedback: 168
			{ -1061158488, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71" },
			// block size: 192, key size: 192, padding: None, feedback: 176
			{ -1061158480, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75" },
			// block size: 192, key size: 192, padding: None, feedback: 184
			{ -1061158472, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73" },
			// block size: 192, key size: 192, padding: None, feedback: 192
			{ -1061158464, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-50" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 8
			{ -1061158392, "39-7D" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 16
			{ -1061158384, "39-CA-D8-BF" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 24
			{ -1061158376, "39-CA-76-98-DB-D0" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 32
			{ -1061158368, "39-CA-76-1E-1F-57-B5-D2" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 40
			{ -1061158360, "39-CA-76-1E-FB-BD-48-34-BA-32" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 48
			{ -1061158352, "39-CA-76-1E-FB-FD-B5-E4-27-0D-8C-16" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 56
			{ -1061158344, "39-CA-76-1E-FB-FD-43-0F-C1-E8-6C-8D-06-88" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 64
			{ -1061158336, "39-CA-76-1E-FB-FD-43-3C-B6-CA-E9-26-30-5D-67-3C" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 72
			{ -1061158328, "39-CA-76-1E-FB-FD-43-3C-5F-AA-04-EC-5B-71-C5-3E-AF-FE" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 80
			{ -1061158320, "39-CA-76-1E-FB-FD-43-3C-5F-4B-C9-96-D0-AB-05-4C-78-31-17-C0" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 88
			{ -1061158312, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-C3-7F-F9-ED-D0-5E-E0-CB-CA-9C-7A" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 96
			{ -1061158304, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-10-2A-92-73-28-29-A8-6D-9B-77-A3-A9" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 104
			{ -1061158296, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-C7-CC-92-6A-D9-34-53-3F-EF-E6-50-7B-A2" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 112
			{ -1061158288, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-A1-40-AF-66-96-D7-EF-4E-94-92-77-27-35-89" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 120
			{ -1061158280, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-C8-02-E1-4D-4C-4E-DA-A6-07-3B-C2-5B-2C-BA-62" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 128
			{ -1061158272, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-D2-0B-AA-AA-58-19-35-DE-7D-08-C0-CE-BF-E8-EC-51" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 136
			{ -1061158264, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-A7-7D-0B-BA-B1-13-4C-23-95-5E-FA-7D-82-4E-26-E4-75" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 144
			{ -1061158256, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-E4-7A-2B-65-B4-31-DC-EB-8F-5D-64-4D-C3-7C-B7-C7-F9-26" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 152
			{ -1061158248, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-77-4F-F8-52-CA-93-89-CD-FE-36-7B-2B-59-CE-F9-97-6A-A0-FE" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 160
			{ -1061158240, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-3F-25-00-AB-7E-77-79-02-4A-28-56-64-77-DF-EA-EE-F4-BF-EC-10" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 168
			{ -1061158232, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-00-44-77-16-31-A1-E3-23-4E-49-30-7B-9D-3F-FE-67-D2-EB-EF-96-77" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 176
			{ -1061158224, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-8A-B0-B9-6F-F4-51-14-D6-5D-61-6D-64-7B-AC-F3-AD-3B-79-32-AA-B9-CF" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 184
			{ -1061158216, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-FA-1C-10-C8-54-37-C0-BB-E4-1C-F5-6C-4B-52-89-60-8E-44-CF-D4-9B-36-89" },
			// block size: 192, key size: 192, padding: PKCS7, feedback: 192
			{ -1061158208, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-50-B2-FE-11-81-96-32-D7-98-44-B9-B0-51-22-4F-89-35-79-AE-55-11-0E-72-47-93" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 8
			{ -1061158136, "39" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 16
			{ -1061158128, "39-CA" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 24
			{ -1061158120, "39-CA-76" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 32
			{ -1061158112, "39-CA-76-1E" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 40
			{ -1061158104, "39-CA-76-1E-FB" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 48
			{ -1061158096, "39-CA-76-1E-FB-FD" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 56
			{ -1061158088, "39-CA-76-1E-FB-FD-43" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 64
			{ -1061158080, "39-CA-76-1E-FB-FD-43-3C" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 72
			{ -1061158072, "39-CA-76-1E-FB-FD-43-3C-5F" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 80
			{ -1061158064, "39-CA-76-1E-FB-FD-43-3C-5F-4B" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 88
			{ -1061158056, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 96
			{ -1061158048, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 104
			{ -1061158040, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 112
			{ -1061158032, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 120
			{ -1061158024, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 128
			{ -1061158016, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 136
			{ -1061158008, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 144
			{ -1061158000, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 152
			{ -1061157992, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 160
			{ -1061157984, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 168
			{ -1061157976, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 176
			{ -1061157968, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 184
			{ -1061157960, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73" },
			// block size: 192, key size: 192, padding: Zeros, feedback: 192
			{ -1061157952, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-50" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 8
			{ -1061157880, "39-7D" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 16
			{ -1061157872, "39-CA-DA-BF" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 24
			{ -1061157864, "39-CA-76-9B-D8-D0" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 32
			{ -1061157856, "39-CA-76-1E-1B-53-B1-D2" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 40
			{ -1061157848, "39-CA-76-1E-FB-B8-4D-31-BF-32" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 48
			{ -1061157840, "39-CA-76-1E-FB-FD-B3-E2-21-0B-8A-16" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 56
			{ -1061157832, "39-CA-76-1E-FB-FD-43-08-C6-EF-6B-8A-01-88" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 64
			{ -1061157824, "39-CA-76-1E-FB-FD-43-3C-BE-C2-E1-2E-38-55-6F-3C" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 72
			{ -1061157816, "39-CA-76-1E-FB-FD-43-3C-5F-A3-0D-E5-52-78-CC-37-A6-FE" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 80
			{ -1061157808, "39-CA-76-1E-FB-FD-43-3C-5F-4B-C3-9C-DA-A1-0F-46-72-3B-1D-C0" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 88
			{ -1061157800, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-C8-74-F2-E6-DB-55-EB-C0-C1-97-7A" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 96
			{ -1061157792, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-1C-26-9E-7F-24-25-A4-61-97-7B-AF-A9" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 104
			{ -1061157784, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-CA-C1-9F-67-D4-39-5E-32-E2-EB-5D-76-A2" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 112
			{ -1061157776, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-AF-4E-A1-68-98-D9-E1-40-9A-9C-79-29-3B-89" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 120
			{ -1061157768, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-C7-0D-EE-42-43-41-D5-A9-08-34-CD-54-23-B5-62" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 128
			{ -1061157760, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-C2-1B-BA-BA-48-09-25-CE-6D-18-D0-DE-AF-F8-FC-51" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 136
			{ -1061157752, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-B6-6C-1A-AB-A0-02-5D-32-84-4F-EB-6C-93-5F-37-F5-75" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 144
			{ -1061157744, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-F6-68-39-77-A6-23-CE-F9-9D-4F-76-5F-D1-6E-A5-D5-EB-26" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 152
			{ -1061157736, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-64-5C-EB-41-D9-80-9A-DE-ED-25-68-38-4A-DD-EA-84-79-B3-FE" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 160
			{ -1061157728, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-2B-31-14-BF-6A-63-6D-16-5E-3C-42-70-63-CB-FE-FA-E0-AB-F8-10" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 168
			{ -1061157720, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-15-51-62-03-24-B4-F6-36-5B-5C-25-6E-88-2A-EB-72-C7-FE-FA-83-77" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 176
			{ -1061157712, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-9C-A6-AF-79-E2-47-02-C0-4B-77-7B-72-6D-BA-E5-BB-2D-6F-24-BC-AF-CF" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 184
			{ -1061157704, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-ED-0B-07-DF-43-20-D7-AC-F3-0B-E2-7B-5C-45-9E-77-99-53-D8-C3-8C-21-89" },
			// block size: 192, key size: 192, padding: ANSIX923, feedback: 192
			{ -1061157696, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-50-AA-E6-09-99-8E-2A-CF-80-5C-A1-A8-49-3A-57-91-2D-61-B6-4D-09-16-6A-5F-93" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 8
			{ -1061157624, "39-7D" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 16
			{ -1061157616, "39-CA-1B-BF" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 24
			{ -1061157608, "39-CA-76-54-71-D0" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 32
			{ -1061157600, "39-CA-76-1E-88-E4-D5-D2" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 40
			{ -1061157592, "39-CA-76-1E-FB-68-16-74-7A-32" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 48
			{ -1061157584, "39-CA-76-1E-FB-FD-C7-82-F7-F9-26-16" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 56
			{ -1061157576, "39-CA-76-1E-FB-FD-43-51-AC-35-47-37-DE-88" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 64
			{ -1061157568, "39-CA-76-1E-FB-FD-43-3C-17-05-01-46-FD-9F-A2-3C" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 72
			{ -1061157560, "39-CA-76-1E-FB-FD-43-3C-5F-45-46-F8-3D-10-F7-D0-29-FE" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 80
			{ -1061157552, "39-CA-76-1E-FB-FD-43-3C-5F-4B-08-DE-A9-20-1F-A5-49-77-3E-C0" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 88
			{ -1061157544, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-56-7C-62-8D-0C-1D-59-64-9A-AB-7A" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 96
			{ -1061157536, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-2B-77-64-50-5F-CD-21-37-29-DC-66-A9" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 104
			{ -1061157528, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-37-E4-50-E2-2D-ED-F9-C9-35-ED-1A-C0-A2" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 112
			{ -1061157520, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-6D-FE-99-D1-E8-AC-D7-AD-CA-4F-F3-C9-A9-89" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 120
			{ -1061157512, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-88-05-AB-B0-D3-98-A9-37-FA-EB-C2-98-02-6D-62" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 128
			{ -1061157504, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-07-9F-A9-4C-62-78-B5-62-3F-DC-58-84-59-E5-6D-51" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 136
			{ -1061157496, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-45-EF-0D-23-71-8A-33-0E-15-70-04-55-1D-98-5F-08-75" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 144
			{ -1061157488, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-B9-FD-FE-EA-CB-29-B7-4E-6B-0D-EC-6B-59-0A-32-6E-10-26" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 152
			{ -1061157480, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-92-9C-B2-8E-5A-1E-DF-8E-37-86-5D-EF-25-62-33-79-A9-CE-FE" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 160
			{ -1061157472, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-39-EE-42-8E-F4-73-03-BE-54-69-AC-E4-3D-CA-AF-56-50-3B-15-10" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 168
			{ -1061157464, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-DA-61-74-22-45-DC-75-D7-3B-DC-7C-9C-FF-A6-1F-E0-B0-9E-82-46-77" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 176
			{ -1061157456, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-38-56-C4-D9-BB-15-47-40-F7-A1-5E-4E-3F-33-5E-A0-27-88-7F-42-5C-CF" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 184
			{ -1061157448, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-7E-40-BA-A6-A9-54-4C-D5-4C-04-01-B3-BC-20-8F-70-36-80-12-43-D5-A3-89" },
			// block size: 192, key size: 192, padding: ISO10126, feedback: 192
			{ -1061157440, "39-CA-76-1E-FB-FD-43-3C-5F-4B-97-7D-FF-73-B6-52-AC-C0-9B-E2-71-75-73-50-A9-33-DF-1D-0E-01-DD-8D-2D-16-32-A0-6B-A2-B8-04-A6-02-F7-1D-24-EF-CD-93" },
			// block size: 192, key size: 256, padding: None, feedback: 8
			{ -1056964344, "E8" },
			// block size: 192, key size: 256, padding: None, feedback: 16
			{ -1056964336, "E8-FE" },
			// block size: 192, key size: 256, padding: None, feedback: 24
			{ -1056964328, "E8-FE-B3" },
			// block size: 192, key size: 256, padding: None, feedback: 32
			{ -1056964320, "E8-FE-B3-7C" },
			// block size: 192, key size: 256, padding: None, feedback: 40
			{ -1056964312, "E8-FE-B3-7C-94" },
			// block size: 192, key size: 256, padding: None, feedback: 48
			{ -1056964304, "E8-FE-B3-7C-94-54" },
			// block size: 192, key size: 256, padding: None, feedback: 56
			{ -1056964296, "E8-FE-B3-7C-94-54-08" },
			// block size: 192, key size: 256, padding: None, feedback: 64
			{ -1056964288, "E8-FE-B3-7C-94-54-08-90" },
			// block size: 192, key size: 256, padding: None, feedback: 72
			{ -1056964280, "E8-FE-B3-7C-94-54-08-90-0B" },
			// block size: 192, key size: 256, padding: None, feedback: 80
			{ -1056964272, "E8-FE-B3-7C-94-54-08-90-0B-6A" },
			// block size: 192, key size: 256, padding: None, feedback: 88
			{ -1056964264, "E8-FE-B3-7C-94-54-08-90-0B-6A-20" },
			// block size: 192, key size: 256, padding: None, feedback: 96
			{ -1056964256, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B" },
			// block size: 192, key size: 256, padding: None, feedback: 104
			{ -1056964248, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B" },
			// block size: 192, key size: 256, padding: None, feedback: 112
			{ -1056964240, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02" },
			// block size: 192, key size: 256, padding: None, feedback: 120
			{ -1056964232, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81" },
			// block size: 192, key size: 256, padding: None, feedback: 128
			{ -1056964224, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68" },
			// block size: 192, key size: 256, padding: None, feedback: 136
			{ -1056964216, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF" },
			// block size: 192, key size: 256, padding: None, feedback: 144
			{ -1056964208, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B" },
			// block size: 192, key size: 256, padding: None, feedback: 152
			{ -1056964200, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A" },
			// block size: 192, key size: 256, padding: None, feedback: 160
			{ -1056964192, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0" },
			// block size: 192, key size: 256, padding: None, feedback: 168
			{ -1056964184, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A" },
			// block size: 192, key size: 256, padding: None, feedback: 176
			{ -1056964176, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8" },
			// block size: 192, key size: 256, padding: None, feedback: 184
			{ -1056964168, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50" },
			// block size: 192, key size: 256, padding: None, feedback: 192
			{ -1056964160, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-AD" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 8
			{ -1056964088, "E8-58" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 16
			{ -1056964080, "E8-FE-C6-31" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 24
			{ -1056964072, "E8-FE-B3-CC-A6-61" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 32
			{ -1056964064, "E8-FE-B3-7C-C1-E5-B6-27" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 40
			{ -1056964056, "E8-FE-B3-7C-94-99-20-0B-54-6C" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 48
			{ -1056964048, "E8-FE-B3-7C-94-54-62-37-F3-25-AF-61" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 56
			{ -1056964040, "E8-FE-B3-7C-94-54-08-B0-B1-34-63-F7-AB-26" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 64
			{ -1056964032, "E8-FE-B3-7C-94-54-08-90-11-8E-8C-0F-20-A4-66-39" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 72
			{ -1056964024, "E8-FE-B3-7C-94-54-08-90-0B-E9-8A-4B-E6-60-BF-18-14-79" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 80
			{ -1056964016, "E8-FE-B3-7C-94-54-08-90-0B-6A-70-0F-85-26-08-CE-E1-7D-90-68" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 88
			{ -1056964008, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-A2-47-E0-39-9A-87-7F-BD-D1-A5-0D" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 96
			{ -1056964000, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-05-FA-DC-C2-C0-A3-2A-D8-1A-D5-02-F4" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 104
			{ -1056963992, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-95-AC-25-17-40-B9-34-EA-C2-8E-89-DF-89" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 112
			{ -1056963984, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-37-6A-34-FF-A9-8B-C8-66-C1-FC-29-CE-FA-2D" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 120
			{ -1056963976, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-59-FF-63-D3-27-4F-8C-59-66-09-CF-D8-3C-5B-21" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 128
			{ -1056963968, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-9A-D7-E4-90-5D-BF-1A-A9-E8-E1-5E-6C-C0-07-1B-AC" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 136
			{ -1056963960, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-05-25-6A-07-71-2D-39-B7-D3-C5-A7-53-E8-D1-FF-46-EC" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 144
			{ -1056963952, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-E0-B6-F0-15-C0-3E-A2-DF-C7-EA-40-AA-0D-DB-B0-0E-40-1C" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 152
			{ -1056963944, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-6A-F4-54-DA-20-07-99-12-BA-92-1A-6A-BD-A3-A5-20-84-D7-3E" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 160
			{ -1056963936, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-5E-01-E7-B8-9B-EC-42-00-9E-3D-16-12-4E-A9-7E-0F-5B-00-71-DF" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 168
			{ -1056963928, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-52-DB-A9-21-97-72-DE-97-16-5D-8D-75-AA-62-4E-23-BD-B6-D5-52-AE" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 176
			{ -1056963920, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-CA-C2-DE-8D-66-AD-B8-91-FA-55-7D-BF-25-CF-BE-92-A8-CD-52-3D-93-7F" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 184
			{ -1056963912, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-F3-D7-43-76-09-53-6E-CF-A2-B6-33-A1-1A-6B-4A-F7-C1-16-DF-A9-13-97-DC" },
			// block size: 192, key size: 256, padding: PKCS7, feedback: 192
			{ -1056963904, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-AD-C8-72-5E-D6-76-3D-9D-50-88-A3-5A-8C-0E-FD-31-56-8C-11-BD-90-14-07-58-40" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 8
			{ -1056963832, "E8" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 16
			{ -1056963824, "E8-FE" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 24
			{ -1056963816, "E8-FE-B3" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 32
			{ -1056963808, "E8-FE-B3-7C" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 40
			{ -1056963800, "E8-FE-B3-7C-94" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 48
			{ -1056963792, "E8-FE-B3-7C-94-54" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 56
			{ -1056963784, "E8-FE-B3-7C-94-54-08" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 64
			{ -1056963776, "E8-FE-B3-7C-94-54-08-90" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 72
			{ -1056963768, "E8-FE-B3-7C-94-54-08-90-0B" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 80
			{ -1056963760, "E8-FE-B3-7C-94-54-08-90-0B-6A" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 88
			{ -1056963752, "E8-FE-B3-7C-94-54-08-90-0B-6A-20" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 96
			{ -1056963744, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 104
			{ -1056963736, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 112
			{ -1056963728, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 120
			{ -1056963720, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 128
			{ -1056963712, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 136
			{ -1056963704, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 144
			{ -1056963696, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 152
			{ -1056963688, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 160
			{ -1056963680, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 168
			{ -1056963672, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 176
			{ -1056963664, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 184
			{ -1056963656, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50" },
			// block size: 192, key size: 256, padding: Zeros, feedback: 192
			{ -1056963648, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-AD" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 8
			{ -1056963576, "E8-58" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 16
			{ -1056963568, "E8-FE-C4-31" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 24
			{ -1056963560, "E8-FE-B3-CF-A5-61" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 32
			{ -1056963552, "E8-FE-B3-7C-C5-E1-B2-27" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 40
			{ -1056963544, "E8-FE-B3-7C-94-9C-25-0E-51-6C" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 48
			{ -1056963536, "E8-FE-B3-7C-94-54-64-31-F5-23-A9-61" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 56
			{ -1056963528, "E8-FE-B3-7C-94-54-08-B7-B6-33-64-F0-AC-26" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 64
			{ -1056963520, "E8-FE-B3-7C-94-54-08-90-19-86-84-07-28-AC-6E-39" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 72
			{ -1056963512, "E8-FE-B3-7C-94-54-08-90-0B-E0-83-42-EF-69-B6-11-1D-79" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 80
			{ -1056963504, "E8-FE-B3-7C-94-54-08-90-0B-6A-7A-05-8F-2C-02-C4-EB-77-9A-68" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 88
			{ -1056963496, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-A9-4C-EB-32-91-8C-74-B6-DA-AE-0D" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 96
			{ -1056963488, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-09-F6-D0-CE-CC-AF-26-D4-16-D9-0E-F4" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 104
			{ -1056963480, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-98-A1-28-1A-4D-B4-39-E7-CF-83-84-D2-89" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 112
			{ -1056963472, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-39-64-3A-F1-A7-85-C6-68-CF-F2-27-C0-F4-2D" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 120
			{ -1056963464, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-56-F0-6C-DC-28-40-83-56-69-06-C0-D7-33-54-21" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 128
			{ -1056963456, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-8A-C7-F4-80-4D-AF-0A-B9-F8-F1-4E-7C-D0-17-0B-AC" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 136
			{ -1056963448, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-14-34-7B-16-60-3C-28-A6-C2-D4-B6-42-F9-C0-EE-57-EC" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 144
			{ -1056963440, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-F2-A4-E2-07-D2-2C-B0-CD-D5-F8-52-B8-1F-C9-A2-1C-52-1C" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 152
			{ -1056963432, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-79-E7-47-C9-33-14-8A-01-A9-81-09-79-AE-B0-B6-33-97-C4-3E" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 160
			{ -1056963424, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-4A-15-F3-AC-8F-F8-56-14-8A-29-02-06-5A-BD-6A-1B-4F-14-65-DF" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 168
			{ -1056963416, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-47-CE-BC-34-82-67-CB-82-03-48-98-60-BF-77-5B-36-A8-A3-C0-47-AE" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 176
			{ -1056963408, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-DC-D4-C8-9B-70-BB-AE-87-EC-43-6B-A9-33-D9-A8-84-BE-DB-44-2B-85-7F" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 184
			{ -1056963400, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-E4-C0-54-61-1E-44-79-D8-B5-A1-24-B6-0D-7C-5D-E0-D6-01-C8-BE-04-80-DC" },
			// block size: 192, key size: 256, padding: ANSIX923, feedback: 192
			{ -1056963392, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-AD-D0-6A-46-CE-6E-25-85-48-90-BB-42-94-16-E5-29-4E-94-09-A5-88-0C-1F-40-40" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 8
			{ -1056963320, "E8-58" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 16
			{ -1056963312, "E8-FE-D9-31" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 24
			{ -1056963304, "E8-FE-B3-E7-C4-61" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 32
			{ -1056963296, "E8-FE-B3-7C-C4-F3-EA-27" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 40
			{ -1056963288, "E8-FE-B3-7C-94-3E-48-75-D0-6C" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 48
			{ -1056963280, "E8-FE-B3-7C-94-54-95-31-ED-CF-DD-61" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 56
			{ -1056963272, "E8-FE-B3-7C-94-54-08-E8-B7-FD-B9-9C-8C-26" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 64
			{ -1056963264, "E8-FE-B3-7C-94-54-08-90-2A-3D-AF-9D-AF-BB-AD-39" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 72
			{ -1056963256, "E8-FE-B3-7C-94-54-08-90-0B-7F-70-2C-39-2F-B5-0D-58-79" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 80
			{ -1056963248, "E8-FE-B3-7C-94-54-08-90-0B-6A-E1-01-99-B2-14-52-3E-B1-0B-68" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 88
			{ -1056963240, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-73-B3-7C-77-A8-95-75-6F-61-E0-0D" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 96
			{ -1056963232, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-F2-D6-E8-F6-F1-3B-54-8A-C0-E0-33-F4" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 104
			{ -1056963224, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-36-D0-70-66-EB-94-9D-A4-92-DA-62-98-89" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 112
			{ -1056963216, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-CD-15-1B-F2-32-6E-4C-58-73-CF-6F-60-9E-2D" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 120
			{ -1056963208, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-4B-A0-63-4E-13-B7-4B-0E-3B-69-8D-97-F7-55-21" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 128
			{ -1056963200, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-6A-49-71-50-C1-B4-57-D9-17-62-95-12-B9-0F-E4-AC" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 136
			{ -1056963192, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-F2-7A-FE-BC-C2-DF-48-3B-E8-0D-F7-E7-C4-3C-A7-6A-EC" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 144
			{ -1056963184, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-C5-CA-6E-BE-28-BF-B3-E9-8A-88-2B-24-B5-34-13-33-0E-1C" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 152
			{ -1056963176, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-64-87-33-05-C1-67-D7-1E-BF-B8-CF-8B-C0-1C-67-25-A9-43-3E" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 160
			{ -1056963168, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-7C-43-EB-60-33-8C-47-C6-45-3C-A1-22-16-C1-DA-9F-F0-54-07-DF" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 168
			{ -1056963160, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-F7-23-E8-3F-1C-73-9E-DC-91-A7-34-EB-59-AA-80-EC-66-09-04-55-AE" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 176
			{ -1056963152, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-B9-18-5E-5D-7E-95-06-EB-B7-3C-27-E7-92-5A-A1-93-CF-24-03-08-B1-7F" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 184
			{ -1056963144, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-98-2E-B2-75-CD-2C-15-E2-D5-25-70-C3-F6-A9-26-ED-60-18-6B-86-D8-28-DC" },
			// block size: 192, key size: 256, padding: ISO10126, feedback: 192
			{ -1056963136, "E8-FE-B3-7C-94-54-08-90-0B-6A-20-9B-6B-02-81-68-CF-9B-2A-E0-6A-D8-50-AD-E0-ED-C8-EA-F9-08-C1-14-6F-C6-2B-34-7C-26-FF-56-DB-11-1E-57-0B-63-33-40" },
			// block size: 256, key size: 128, padding: None, feedback: 8
			{ 8388872, "59" },
			// block size: 256, key size: 128, padding: None, feedback: 16
			{ 8388880, "59-6D" },
			// block size: 256, key size: 128, padding: None, feedback: 24
			{ 8388888, "59-6D-4F" },
			// block size: 256, key size: 128, padding: None, feedback: 32
			{ 8388896, "59-6D-4F-74" },
			// block size: 256, key size: 128, padding: None, feedback: 40
			{ 8388904, "59-6D-4F-74-24" },
			// block size: 256, key size: 128, padding: None, feedback: 48
			{ 8388912, "59-6D-4F-74-24-87" },
			// block size: 256, key size: 128, padding: None, feedback: 56
			{ 8388920, "59-6D-4F-74-24-87-57" },
			// block size: 256, key size: 128, padding: None, feedback: 64
			{ 8388928, "59-6D-4F-74-24-87-57-A3" },
			// block size: 256, key size: 128, padding: None, feedback: 72
			{ 8388936, "59-6D-4F-74-24-87-57-A3-E0" },
			// block size: 256, key size: 128, padding: None, feedback: 80
			{ 8388944, "59-6D-4F-74-24-87-57-A3-E0-A1" },
			// block size: 256, key size: 128, padding: None, feedback: 88
			{ 8388952, "59-6D-4F-74-24-87-57-A3-E0-A1-91" },
			// block size: 256, key size: 128, padding: None, feedback: 96
			{ 8388960, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6" },
			// block size: 256, key size: 128, padding: None, feedback: 104
			{ 8388968, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85" },
			// block size: 256, key size: 128, padding: None, feedback: 112
			{ 8388976, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1" },
			// block size: 256, key size: 128, padding: None, feedback: 120
			{ 8388984, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63" },
			// block size: 256, key size: 128, padding: None, feedback: 128
			{ 8388992, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0" },
			// block size: 256, key size: 128, padding: None, feedback: 136
			{ 8389000, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34" },
			// block size: 256, key size: 128, padding: None, feedback: 144
			{ 8389008, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99" },
			// block size: 256, key size: 128, padding: None, feedback: 152
			{ 8389016, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29" },
			// block size: 256, key size: 128, padding: None, feedback: 160
			{ 8389024, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21" },
			// block size: 256, key size: 128, padding: None, feedback: 168
			{ 8389032, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91" },
			// block size: 256, key size: 128, padding: None, feedback: 176
			{ 8389040, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D" },
			// block size: 256, key size: 128, padding: None, feedback: 184
			{ 8389048, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7" },
			// block size: 256, key size: 128, padding: None, feedback: 192
			{ 8389056, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC" },
			// block size: 256, key size: 128, padding: None, feedback: 200
			{ 8389064, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31" },
			// block size: 256, key size: 128, padding: None, feedback: 208
			{ 8389072, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0" },
			// block size: 256, key size: 128, padding: None, feedback: 216
			{ 8389080, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB" },
			// block size: 256, key size: 128, padding: None, feedback: 224
			{ 8389088, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0" },
			// block size: 256, key size: 128, padding: None, feedback: 232
			{ 8389096, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C" },
			// block size: 256, key size: 128, padding: None, feedback: 240
			{ 8389104, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1" },
			// block size: 256, key size: 128, padding: None, feedback: 248
			{ 8389112, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9" },
			// block size: 256, key size: 128, padding: None, feedback: 256
			{ 8389120, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-71" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 8
			{ 8389128, "59-57" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 16
			{ 8389136, "59-6D-C9-62" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 24
			{ 8389144, "59-6D-4F-26-3A-63" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 32
			{ 8389152, "59-6D-4F-74-86-95-BA-BB" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 40
			{ 8389160, "59-6D-4F-74-24-D0-8B-E7-31-ED" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 48
			{ 8389168, "59-6D-4F-74-24-87-69-D1-5A-3D-2C-98" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 56
			{ 8389176, "59-6D-4F-74-24-87-57-F1-13-9B-F2-93-C8-25" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 64
			{ 8389184, "59-6D-4F-74-24-87-57-A3-A4-AF-13-9A-1E-A1-49-B5" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 72
			{ 8389192, "59-6D-4F-74-24-87-57-A3-E0-53-20-D6-1B-D2-B0-B6-47-32" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 80
			{ 8389200, "59-6D-4F-74-24-87-57-A3-E0-A1-95-0A-4E-84-2E-F1-2C-4B-C7-50" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 88
			{ 8389208, "59-6D-4F-74-24-87-57-A3-E0-A1-91-EC-86-5C-58-1E-F6-FF-3C-EE-A4-D1" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 96
			{ 8389216, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-97-E1-8C-22-7D-D4-F6-31-2D-C2-1C-A3" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 104
			{ 8389224, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-45-AC-4A-49-B7-DB-74-CF-97-CE-AE-E6-84" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 112
			{ 8389232, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-02-94-D2-E3-69-71-F5-07-64-CE-38-B1-84-54" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 120
			{ 8389240, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C7-11-3D-36-4D-8C-79-C1-44-34-AA-0D-E2-AB-2F" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 128
			{ 8389248, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-96-7B-ED-35-C5-8B-0A-76-F7-4A-28-BF-B9-FE-35-81" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 136
			{ 8389256, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-9E-40-AA-B6-D2-D5-32-F5-40-EE-81-05-B1-24-BC-A1-A9" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 144
			{ 8389264, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-A6-E7-29-7F-BB-D9-8D-E1-DB-4F-3F-E2-98-16-61-FE-3A-3E" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 152
			{ 8389272, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-F3-39-7E-1A-6E-4B-02-6A-89-1E-14-53-F9-C5-59-8A-81-AE-29" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 160
			{ 8389280, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-0A-92-AA-7E-A5-8D-A9-89-A0-1E-A0-D0-0B-F2-D8-C9-02-DF-E5-FE" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 168
			{ 8389288, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-DE-42-EF-32-0F-3E-9C-1C-A3-70-23-58-8C-90-56-42-47-F7-B5-0F-C1" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 176
			{ 8389296, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-76-C6-F0-FF-4D-06-8B-8A-64-16-84-DA-2C-9E-2D-F0-AD-E9-83-CE-71-76" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 184
			{ 8389304, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-EA-48-4B-62-60-47-81-3F-6D-43-85-DC-B9-E5-FE-70-0F-0A-AD-84-05-B0-05" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 192
			{ 8389312, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-5B-B4-08-45-DB-C3-3C-5E-9F-58-70-51-75-E5-3C-95-E1-49-7F-5E-01-3F-95-58" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 200
			{ 8389320, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-0D-DD-AA-5D-51-9E-3C-FA-14-F2-36-E2-FC-F6-8E-1A-91-CD-32-BF-33-4E-25-6A-C5" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 208
			{ 8389328, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-BE-4D-C5-57-22-B3-FC-F5-94-42-C7-C8-0B-9B-38-19-20-96-A9-2F-83-7D-9C-86-6F-23" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 216
			{ 8389336, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-61-69-56-8E-2A-02-8A-13-F5-2E-22-76-B4-EC-CF-04-28-F9-D5-7F-DF-01-93-C8-F5-87-27" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 224
			{ 8389344, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-6B-A2-24-3F-4D-69-DA-42-08-52-3C-4A-F0-E4-0E-92-6F-62-36-A6-80-67-DC-8E-47-94-B1-70" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 232
			{ 8389352, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-B6-4F-7C-C4-A6-A0-6A-1F-5B-95-5A-75-03-AC-9D-07-33-6B-35-E8-81-51-AC-82-D0-DB-27-1D-25" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 240
			{ 8389360, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-1B-D8-DE-50-0D-D2-4B-C9-97-7A-BC-E8-0F-EB-3C-39-96-D6-EF-21-B3-89-74-3B-10-84-25-40-28-BD" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 248
			{ 8389368, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-11-62-58-0C-FE-AF-52-F4-97-61-4E-28-02-FF-F1-D3-6A-01-0B-E3-64-67-0C-88-01-92-32-E5-66-44-FE" },
			// block size: 256, key size: 128, padding: PKCS7, feedback: 256
			{ 8389376, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-71-70-40-F2-31-76-EB-F4-70-37-F4-9C-9E-C9-4B-B7-FA-F2-F9-FF-ED-2D-0E-68-C1-4B-34-0E-B1-80-69-4F-CE" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 8
			{ 8389384, "59" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 16
			{ 8389392, "59-6D" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 24
			{ 8389400, "59-6D-4F" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 32
			{ 8389408, "59-6D-4F-74" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 40
			{ 8389416, "59-6D-4F-74-24" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 48
			{ 8389424, "59-6D-4F-74-24-87" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 56
			{ 8389432, "59-6D-4F-74-24-87-57" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 64
			{ 8389440, "59-6D-4F-74-24-87-57-A3" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 72
			{ 8389448, "59-6D-4F-74-24-87-57-A3-E0" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 80
			{ 8389456, "59-6D-4F-74-24-87-57-A3-E0-A1" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 88
			{ 8389464, "59-6D-4F-74-24-87-57-A3-E0-A1-91" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 96
			{ 8389472, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 104
			{ 8389480, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 112
			{ 8389488, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 120
			{ 8389496, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 128
			{ 8389504, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 136
			{ 8389512, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 144
			{ 8389520, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 152
			{ 8389528, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 160
			{ 8389536, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 168
			{ 8389544, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 176
			{ 8389552, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 184
			{ 8389560, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 192
			{ 8389568, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 200
			{ 8389576, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 208
			{ 8389584, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 216
			{ 8389592, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 224
			{ 8389600, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 232
			{ 8389608, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 240
			{ 8389616, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 248
			{ 8389624, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9" },
			// block size: 256, key size: 128, padding: Zeros, feedback: 256
			{ 8389632, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-71" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 8
			{ 8389640, "59-57" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 16
			{ 8389648, "59-6D-CB-62" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 24
			{ 8389656, "59-6D-4F-25-39-63" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 32
			{ 8389664, "59-6D-4F-74-82-91-BE-BB" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 40
			{ 8389672, "59-6D-4F-74-24-D5-8E-E2-34-ED" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 48
			{ 8389680, "59-6D-4F-74-24-87-6F-D7-5C-3B-2A-98" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 56
			{ 8389688, "59-6D-4F-74-24-87-57-F6-14-9C-F5-94-CF-25" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 64
			{ 8389696, "59-6D-4F-74-24-87-57-A3-AC-A7-1B-92-16-A9-41-B5" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 72
			{ 8389704, "59-6D-4F-74-24-87-57-A3-E0-5A-29-DF-12-DB-B9-BF-4E-32" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 80
			{ 8389712, "59-6D-4F-74-24-87-57-A3-E0-A1-9F-00-44-8E-24-FB-26-41-CD-50" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 88
			{ 8389720, "59-6D-4F-74-24-87-57-A3-E0-A1-91-E7-8D-57-53-15-FD-F4-37-E5-AF-D1" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 96
			{ 8389728, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-9B-ED-80-2E-71-D8-FA-3D-21-CE-10-A3" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 104
			{ 8389736, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-48-A1-47-44-BA-D6-79-C2-9A-C3-A3-EB-84" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 112
			{ 8389744, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-0C-9A-DC-ED-67-7F-FB-09-6A-C0-36-BF-8A-54" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 120
			{ 8389752, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C8-1E-32-39-42-83-76-CE-4B-3B-A5-02-ED-A4-2F" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 128
			{ 8389760, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-86-6B-FD-25-D5-9B-1A-66-E7-5A-38-AF-A9-EE-25-81" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 136
			{ 8389768, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-8F-51-BB-A7-C3-C4-23-E4-51-FF-90-14-A0-35-AD-B0-A9" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 144
			{ 8389776, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-B4-F5-3B-6D-A9-CB-9F-F3-C9-5D-2D-F0-8A-04-73-EC-28-3E" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 152
			{ 8389784, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-E0-2A-6D-09-7D-58-11-79-9A-0D-07-40-EA-D6-4A-99-92-BD-29" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 160
			{ 8389792, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-1E-86-BE-6A-B1-99-BD-9D-B4-0A-B4-C4-1F-E6-CC-DD-16-CB-F1-FE" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 168
			{ 8389800, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-CB-57-FA-27-1A-2B-89-09-B6-65-36-4D-99-85-43-57-52-E2-A0-1A-C1" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 176
			{ 8389808, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-60-D0-E6-E9-5B-10-9D-9C-72-00-92-CC-3A-88-3B-E6-BB-FF-95-D8-67-76" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 184
			{ 8389816, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-FD-5F-5C-75-77-50-96-28-7A-54-92-CB-AE-F2-E9-67-18-1D-BA-93-12-A7-05" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 192
			{ 8389824, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-43-AC-10-5D-C3-DB-24-46-87-40-68-49-6D-FD-24-8D-F9-51-67-46-19-27-8D-58" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 200
			{ 8389832, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-14-C4-B3-44-48-87-25-E3-0D-EB-2F-FB-E5-EF-97-03-88-D4-2B-A6-2A-57-3C-73-C5" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 208
			{ 8389840, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-A4-57-DF-4D-38-A9-E6-EF-8E-58-DD-D2-11-81-22-03-3A-8C-B3-35-99-67-86-9C-75-23" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 216
			{ 8389848, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-7A-72-4D-95-31-19-91-08-EE-35-39-6D-AF-F7-D4-1F-33-E2-CE-64-C4-1A-88-D3-EE-9C-27" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 224
			{ 8389856, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-77-BE-38-23-51-75-C6-5E-14-4E-20-56-EC-F8-12-8E-73-7E-2A-BA-9C-7B-C0-92-5B-88-AD-70" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 232
			{ 8389864, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-AB-52-61-D9-BB-BD-77-02-46-88-47-68-1E-B1-80-1A-2E-76-28-F5-9C-4C-B1-9F-CD-C6-3A-00-25" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 240
			{ 8389872, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-05-C6-C0-4E-13-CC-55-D7-89-64-A2-F6-11-F5-22-27-88-C8-F1-3F-AD-97-6A-25-0E-9A-3B-5E-36-BD" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 248
			{ 8389880, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-0E-7D-47-13-E1-B0-4D-EB-88-7E-51-37-1D-E0-EE-CC-75-1E-14-FC-7B-78-13-97-1E-8D-2D-FA-79-5B-FE" },
			// block size: 256, key size: 128, padding: ANSIX923, feedback: 256
			{ 8389888, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-71-50-60-D2-11-56-CB-D4-50-17-D4-BC-BE-E9-6B-97-DA-D2-D9-DF-CD-0D-2E-48-E1-6B-14-2E-91-A0-49-6F-CE" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 8
			{ 8389896, "59-57" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 16
			{ 8389904, "59-6D-FC-62" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 24
			{ 8389912, "59-6D-4F-1B-26-63" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 32
			{ 8389920, "59-6D-4F-74-CF-D9-49-BB" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 40
			{ 8389928, "59-6D-4F-74-24-1A-98-33-59-ED" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 48
			{ 8389936, "59-6D-4F-74-24-87-23-42-3C-72-A9-98" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 56
			{ 8389944, "59-6D-4F-74-24-87-57-C8-65-D1-ED-79-C0-25" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 64
			{ 8389952, "59-6D-4F-74-24-87-57-A3-FD-1E-6B-D0-BE-9D-C6-B5" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 72
			{ 8389960, "59-6D-4F-74-24-87-57-A3-E0-48-C7-A7-04-B0-E8-6B-66-32" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 80
			{ 8389968, "59-6D-4F-74-24-87-57-A3-E0-A1-BB-86-E3-B6-B9-57-EF-75-A1-50" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 88
			{ 8389976, "59-6D-4F-74-24-87-57-A3-E0-A1-91-9F-E0-C0-2B-7C-51-A8-D5-F5-4B-D1" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 96
			{ 8389984, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-A5-7C-65-6D-81-CF-1D-0C-DA-F4-16-A3" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 104
			{ 8389992, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-75-42-40-A5-8A-37-4C-16-43-74-3F-C1-84" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 112
			{ 8390000, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-16-DB-C8-A9-46-6F-CC-9B-05-57-C6-86-37-54" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 120
			{ 8390008, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-EB-76-2E-69-4C-FC-1E-55-18-53-36-DD-97-FE-2F" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 128
			{ 8390016, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-1A-80-F8-C1-FE-F0-3C-4C-CA-BE-1D-94-21-FC-45-81" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 136
			{ 8390024, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-43-74-ED-DC-12-27-D8-D1-3D-AF-9B-20-CC-46-D6-62-A9" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 144
			{ 8390032, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-32-2A-D9-17-EE-1C-B5-47-A6-90-9B-34-DE-F2-7C-A3-01-3E" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 152
			{ 8390040, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-EB-4B-EE-E6-31-E4-A5-B8-71-79-42-AA-51-23-07-05-2A-B2-29" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 160
			{ 8390048, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-EA-3F-51-82-01-52-77-4E-42-92-6A-E0-94-22-6C-F0-0E-6C-72-FE" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 168
			{ 8390056, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-D0-1F-0D-D9-07-6C-D0-B3-39-3B-C5-BA-AC-A3-A3-CA-C3-91-81-73-C1" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 176
			{ 8390064, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-87-04-50-0B-59-55-47-FD-FA-02-3B-A9-DE-24-AF-50-26-C8-A9-53-43-76" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 184
			{ 8390072, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-C2-DF-19-13-27-89-E6-8E-68-CF-F8-79-1B-81-36-46-EE-58-A0-95-C5-BF-05" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 192
			{ 8390080, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-DA-51-9C-62-9B-54-03-B1-86-BD-10-02-8A-31-3F-B1-54-EC-CC-7D-23-9E-E5-58" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 200
			{ 8390088, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-61-13-C7-DB-6E-3A-DC-C4-FB-8B-12-FA-9B-69-C7-89-99-56-96-77-44-85-1C-AD-C5" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 208
			{ 8390096, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-9A-D3-50-01-46-E6-CB-9F-D5-90-F3-62-1E-30-76-AA-CB-D2-49-8F-22-9F-FD-84-99-23" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 216
			{ 8390104, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-62-C2-C4-A2-A3-1B-6D-19-DF-66-F8-1A-E8-90-49-4B-B9-D0-1A-09-CF-30-01-B0-1B-A6-27" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 224
			{ 8390112, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-72-C2-56-BC-B5-98-82-7D-23-D9-98-DF-17-01-24-74-36-BA-35-BF-2F-38-E3-0B-97-4A-38-70" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 232
			{ 8390120, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-46-5C-E7-43-CD-08-C0-94-7E-91-81-70-9E-C8-92-46-C5-3D-75-0B-29-3F-A9-A7-CE-13-E1-0E-25" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 240
			{ 8390128, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-B1-32-4E-7D-D3-C2-D4-A4-52-EA-D4-F4-E6-5F-F6-2B-37-DF-89-B5-17-D6-87-83-6D-9E-1F-C5-62-BD" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 248
			{ 8390136, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-E1-0B-E6-65-8C-FE-55-FD-F4-BD-AC-8B-E1-49-FA-D6-9E-3D-EF-B2-F1-DA-2E-15-AD-44-0C-2A-81-8E-FE" },
			// block size: 256, key size: 128, padding: ISO10126, feedback: 256
			{ 8390144, "59-6D-4F-74-24-87-57-A3-E0-A1-91-F6-85-B1-63-C0-34-99-29-21-91-6D-F7-CC-31-F0-CB-B0-4C-A1-A9-71-E8-D0-1E-83-3D-6F-06-1A-33-3E-D0-58-44-49-9B-CE-78-6B-8E-DD-52-A6-39-97-0D-03-DE-07-F2-38-A5-CE" },
			// block size: 256, key size: 192, padding: None, feedback: 8
			{ 12583176, "06" },
			// block size: 256, key size: 192, padding: None, feedback: 16
			{ 12583184, "06-D9" },
			// block size: 256, key size: 192, padding: None, feedback: 24
			{ 12583192, "06-D9-CB" },
			// block size: 256, key size: 192, padding: None, feedback: 32
			{ 12583200, "06-D9-CB-C2" },
			// block size: 256, key size: 192, padding: None, feedback: 40
			{ 12583208, "06-D9-CB-C2-0E" },
			// block size: 256, key size: 192, padding: None, feedback: 48
			{ 12583216, "06-D9-CB-C2-0E-49" },
			// block size: 256, key size: 192, padding: None, feedback: 56
			{ 12583224, "06-D9-CB-C2-0E-49-4D" },
			// block size: 256, key size: 192, padding: None, feedback: 64
			{ 12583232, "06-D9-CB-C2-0E-49-4D-60" },
			// block size: 256, key size: 192, padding: None, feedback: 72
			{ 12583240, "06-D9-CB-C2-0E-49-4D-60-BD" },
			// block size: 256, key size: 192, padding: None, feedback: 80
			{ 12583248, "06-D9-CB-C2-0E-49-4D-60-BD-68" },
			// block size: 256, key size: 192, padding: None, feedback: 88
			{ 12583256, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C" },
			// block size: 256, key size: 192, padding: None, feedback: 96
			{ 12583264, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4" },
			// block size: 256, key size: 192, padding: None, feedback: 104
			{ 12583272, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B" },
			// block size: 256, key size: 192, padding: None, feedback: 112
			{ 12583280, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6" },
			// block size: 256, key size: 192, padding: None, feedback: 120
			{ 12583288, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4" },
			// block size: 256, key size: 192, padding: None, feedback: 128
			{ 12583296, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE" },
			// block size: 256, key size: 192, padding: None, feedback: 136
			{ 12583304, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E" },
			// block size: 256, key size: 192, padding: None, feedback: 144
			{ 12583312, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89" },
			// block size: 256, key size: 192, padding: None, feedback: 152
			{ 12583320, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D" },
			// block size: 256, key size: 192, padding: None, feedback: 160
			{ 12583328, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D" },
			// block size: 256, key size: 192, padding: None, feedback: 168
			{ 12583336, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9" },
			// block size: 256, key size: 192, padding: None, feedback: 176
			{ 12583344, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E" },
			// block size: 256, key size: 192, padding: None, feedback: 184
			{ 12583352, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7" },
			// block size: 256, key size: 192, padding: None, feedback: 192
			{ 12583360, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7" },
			// block size: 256, key size: 192, padding: None, feedback: 200
			{ 12583368, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77" },
			// block size: 256, key size: 192, padding: None, feedback: 208
			{ 12583376, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE" },
			// block size: 256, key size: 192, padding: None, feedback: 216
			{ 12583384, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2" },
			// block size: 256, key size: 192, padding: None, feedback: 224
			{ 12583392, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0" },
			// block size: 256, key size: 192, padding: None, feedback: 232
			{ 12583400, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0" },
			// block size: 256, key size: 192, padding: None, feedback: 240
			{ 12583408, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D" },
			// block size: 256, key size: 192, padding: None, feedback: 248
			{ 12583416, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E" },
			// block size: 256, key size: 192, padding: None, feedback: 256
			{ 12583424, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-CA" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 8
			{ 12583432, "06-67" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 16
			{ 12583440, "06-D9-AD-1C" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 24
			{ 12583448, "06-D9-CB-DD-A0-89" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 32
			{ 12583456, "06-D9-CB-C2-86-BD-51-2C" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 40
			{ 12583464, "06-D9-CB-C2-0E-D5-C9-22-4D-18" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 48
			{ 12583472, "06-D9-CB-C2-0E-49-20-A9-D4-75-78-E7" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 56
			{ 12583480, "06-D9-CB-C2-0E-49-4D-89-7D-D5-07-17-E0-3F" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 64
			{ 12583488, "06-D9-CB-C2-0E-49-4D-60-50-41-E0-EA-89-77-36-53" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 72
			{ 12583496, "06-D9-CB-C2-0E-49-4D-60-BD-31-24-D6-E4-1B-0E-F4-3E-94" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 80
			{ 12583504, "06-D9-CB-C2-0E-49-4D-60-BD-68-1F-21-40-A6-83-79-35-18-D9-36" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 88
			{ 12583512, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-4F-E5-57-B2-05-AC-AE-51-D8-74-CD" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 96
			{ 12583520, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-0E-5F-CC-47-42-52-BF-7D-C7-28-9F-0C" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 104
			{ 12583528, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-70-0E-F5-63-E0-A3-0C-16-64-86-87-B4-C4" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 112
			{ 12583536, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-6B-EB-08-BE-79-4B-14-92-55-75-B8-8A-B2-73" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 120
			{ 12583544, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-EE-5F-63-84-EE-C3-9F-AE-DD-27-46-86-E0-95-F3" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 128
			{ 12583552, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-AB-7D-81-62-8B-2D-37-9A-51-DD-AA-28-BE-3D-48-01" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 136
			{ 12583560, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-3C-C4-5F-C4-07-95-39-DE-60-04-4D-90-E1-D6-55-17-91" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 144
			{ 12583568, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-27-B3-9B-E4-B2-1D-25-BD-C6-F6-7E-97-D9-A6-40-A3-2E-98" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 152
			{ 12583576, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-51-84-71-17-D5-5C-11-E2-C6-A9-54-A4-EB-F0-B3-92-84-FD-A2" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 160
			{ 12583584, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-23-17-C8-C3-33-77-E5-0E-AB-66-DD-B2-17-EC-D3-D2-99-61-E6-48" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 168
			{ 12583592, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-9B-99-5A-B1-F2-50-95-C4-3E-44-84-F4-2C-04-85-C6-27-2D-27-D0-6D" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 176
			{ 12583600, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-88-1F-B7-2A-CD-62-16-65-4C-8D-DC-61-8E-BB-69-CC-C4-C7-E9-0E-74-C6" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 184
			{ 12583608, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-7B-FB-E5-C6-A8-AF-93-42-9A-A4-71-9B-94-44-D4-93-54-CE-C1-11-45-6B-E1" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 192
			{ 12583616, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-9D-D1-45-9A-18-FD-9F-AC-1F-94-D4-0C-FC-65-BD-94-BC-59-FF-72-96-72-DC-0B" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 200
			{ 12583624, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-F9-39-E2-EB-A1-24-9D-0A-5A-A9-12-07-9C-15-FE-E5-2F-ED-B5-F0-4D-99-CE-A2-40" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 208
			{ 12583632, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-DF-A0-AE-62-9B-2A-24-9D-FC-B8-17-32-D8-B3-7A-B0-A7-2C-CB-3A-BC-30-DA-BE-42-F3" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 216
			{ 12583640, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-B6-1B-FB-84-5F-82-97-F3-92-B6-FE-05-61-F5-30-2F-F0-21-F7-50-23-C1-87-B1-29-66-BF" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 224
			{ 12583648, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-66-B8-C0-5E-C2-1C-3E-25-D7-7A-BC-B3-82-97-06-61-62-77-B7-BE-5F-3B-6D-F6-2F-17-F2-C8" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 232
			{ 12583656, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-40-47-C8-2C-69-EF-4E-B9-AF-27-B7-9E-ED-87-CE-DD-4B-AF-A9-C4-C2-2A-54-2E-E5-1B-FB-65-55" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 240
			{ 12583664, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-83-FD-DB-91-F4-34-DF-5A-9E-67-C3-00-D8-A6-92-4D-BF-32-CD-28-58-A5-8F-1C-C6-01-2E-9C-0D-5A" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 248
			{ 12583672, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-B2-02-B8-B2-BC-48-A1-EF-57-EF-E4-D8-BA-CA-24-B6-03-ED-8F-36-49-04-C2-45-69-94-1C-16-7B-F5-E4" },
			// block size: 256, key size: 192, padding: PKCS7, feedback: 256
			{ 12583680, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-CA-43-23-C4-F1-75-9B-7A-AC-35-C5-68-C3-95-55-3E-DC-2E-A8-18-40-5E-69-12-A7-2D-05-81-28-21-41-99-E3" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 8
			{ 12583688, "06" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 16
			{ 12583696, "06-D9" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 24
			{ 12583704, "06-D9-CB" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 32
			{ 12583712, "06-D9-CB-C2" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 40
			{ 12583720, "06-D9-CB-C2-0E" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 48
			{ 12583728, "06-D9-CB-C2-0E-49" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 56
			{ 12583736, "06-D9-CB-C2-0E-49-4D" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 64
			{ 12583744, "06-D9-CB-C2-0E-49-4D-60" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 72
			{ 12583752, "06-D9-CB-C2-0E-49-4D-60-BD" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 80
			{ 12583760, "06-D9-CB-C2-0E-49-4D-60-BD-68" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 88
			{ 12583768, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 96
			{ 12583776, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 104
			{ 12583784, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 112
			{ 12583792, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 120
			{ 12583800, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 128
			{ 12583808, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 136
			{ 12583816, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 144
			{ 12583824, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 152
			{ 12583832, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 160
			{ 12583840, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 168
			{ 12583848, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 176
			{ 12583856, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 184
			{ 12583864, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 192
			{ 12583872, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 200
			{ 12583880, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 208
			{ 12583888, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 216
			{ 12583896, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 224
			{ 12583904, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 232
			{ 12583912, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 240
			{ 12583920, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 248
			{ 12583928, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E" },
			// block size: 256, key size: 192, padding: Zeros, feedback: 256
			{ 12583936, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-CA" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 8
			{ 12583944, "06-67" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 16
			{ 12583952, "06-D9-AF-1C" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 24
			{ 12583960, "06-D9-CB-DE-A3-89" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 32
			{ 12583968, "06-D9-CB-C2-82-B9-55-2C" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 40
			{ 12583976, "06-D9-CB-C2-0E-D0-CC-27-48-18" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 48
			{ 12583984, "06-D9-CB-C2-0E-49-26-AF-D2-73-7E-E7" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 56
			{ 12583992, "06-D9-CB-C2-0E-49-4D-8E-7A-D2-00-10-E7-3F" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 64
			{ 12584000, "06-D9-CB-C2-0E-49-4D-60-58-49-E8-E2-81-7F-3E-53" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 72
			{ 12584008, "06-D9-CB-C2-0E-49-4D-60-BD-38-2D-DF-ED-12-07-FD-37-94" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 80
			{ 12584016, "06-D9-CB-C2-0E-49-4D-60-BD-68-15-2B-4A-AC-89-73-3F-12-D3-36" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 88
			{ 12584024, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-44-EE-5C-B9-0E-A7-A5-5A-D3-7F-CD" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 96
			{ 12584032, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-02-53-C0-4B-4E-5E-B3-71-CB-24-93-0C" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 104
			{ 12584040, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-7D-03-F8-6E-ED-AE-01-1B-69-8B-8A-B9-C4" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 112
			{ 12584048, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-65-E5-06-B0-77-45-1A-9C-5B-7B-B6-84-BC-73" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 120
			{ 12584056, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-E1-50-6C-8B-E1-CC-90-A1-D2-28-49-89-EF-9A-F3" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 128
			{ 12584064, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-BB-6D-91-72-9B-3D-27-8A-41-CD-BA-38-AE-2D-58-01" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 136
			{ 12584072, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-2D-D5-4E-D5-16-84-28-CF-71-15-5C-81-F0-C7-44-06-91" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 144
			{ 12584080, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-35-A1-89-F6-A0-0F-37-AF-D4-E4-6C-85-CB-B4-52-B1-3C-98" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 152
			{ 12584088, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-42-97-62-04-C6-4F-02-F1-D5-BA-47-B7-F8-E3-A0-81-97-EE-A2" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 160
			{ 12584096, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-37-03-DC-D7-27-63-F1-1A-BF-72-C9-A6-03-F8-C7-C6-8D-75-F2-48" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 168
			{ 12584104, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-8C-4F-A4-E7-45-80-D1-2B-51-91-E1-39-11-90-D3-32-38-32-C5-6D" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 176
			{ 12584112, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-9E-09-A1-3C-DB-74-00-73-5A-9B-CA-77-98-AD-7F-DA-D2-D1-FF-18-62-C6" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 184
			{ 12584120, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-6C-EC-F2-D1-BF-B8-84-55-8D-B3-66-8C-83-53-C3-84-43-D9-D6-06-52-7C-E1" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 192
			{ 12584128, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-85-C9-5D-82-00-E5-87-B4-07-8C-CC-14-E4-7D-A5-8C-A4-41-E7-6A-8E-6A-C4-0B" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 200
			{ 12584136, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-E0-20-FB-F2-B8-3D-84-13-43-B0-0B-1E-85-0C-E7-FC-36-F4-AC-E9-54-80-D7-BB-40" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 208
			{ 12584144, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-C5-BA-B4-78-81-30-3E-87-E6-A2-0D-28-C2-A9-60-AA-BD-36-D1-20-A6-2A-C0-A4-58-F3" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 216
			{ 12584152, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-AD-00-E0-9F-44-99-8C-E8-89-AD-E5-1E-7A-EE-2B-34-EB-3A-EC-4B-38-DA-9C-AA-32-7D-BF" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 224
			{ 12584160, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-7A-A4-DC-42-DE-00-22-39-CB-66-A0-AF-9E-8B-1A-7D-7E-6B-AB-A2-43-27-71-EA-33-0B-EE-C8" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 232
			{ 12584168, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-5D-5A-D5-31-74-F2-53-A4-B2-3A-AA-83-F0-9A-D3-C0-56-B2-B4-D9-DF-37-49-33-F8-06-E6-78-55" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 240
			{ 12584176, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-9D-E3-C5-8F-EA-2A-C1-44-80-79-DD-1E-C6-B8-8C-53-A1-2C-D3-36-46-BB-91-02-D8-1F-30-82-13-5A" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 248
			{ 12584184, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-AD-1D-A7-AD-A3-57-BE-F0-48-F0-FB-C7-A5-D5-3B-A9-1C-F2-90-29-56-1B-DD-5A-76-8B-03-09-64-EA-E4" },
			// block size: 256, key size: 192, padding: ANSIX923, feedback: 256
			{ 12584192, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-CA-63-03-E4-D1-55-BB-5A-8C-15-E5-48-E3-B5-75-1E-FC-0E-88-38-60-7E-49-32-87-0D-25-A1-08-01-61-B9-E3" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 8
			{ 12584200, "06-67" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 16
			{ 12584208, "06-D9-86-1C" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 24
			{ 12584216, "06-D9-CB-B7-3A-89" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 32
			{ 12584224, "06-D9-CB-C2-6F-09-89-2C" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 40
			{ 12584232, "06-D9-CB-C2-0E-95-E5-16-DC-18" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 48
			{ 12584240, "06-D9-CB-C2-0E-49-2F-A7-9B-9B-D1-E7" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 56
			{ 12584248, "06-D9-CB-C2-0E-49-4D-55-E2-80-0B-94-3B-3F" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 64
			{ 12584256, "06-D9-CB-C2-0E-49-4D-60-B9-18-B7-7F-25-F8-CF-53" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 72
			{ 12584264, "06-D9-CB-C2-0E-49-4D-60-BD-E1-3C-55-5B-BB-EC-DA-D6-94" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 80
			{ 12584272, "06-D9-CB-C2-0E-49-4D-60-BD-68-74-3D-0A-E3-5F-BB-51-50-2B-36" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 88
			{ 12584280, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-57-A1-DB-2B-7D-01-18-55-AA-CE-CD" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 96
			{ 12584288, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-D6-29-20-22-50-83-14-AA-53-07-C0-0C" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 104
			{ 12584296, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-47-A9-1B-06-AD-DB-94-CD-0D-A0-98-F8-C4" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 112
			{ 12584304, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-06-54-67-3D-72-D5-E1-08-3D-EC-4B-53-A3-73" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 120
			{ 12584312, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-3A-A2-3E-53-51-34-43-D6-B2-F7-D8-20-EF-B9-F3" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 128
			{ 12584320, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-DD-70-6E-52-E2-02-52-89-BF-DD-CF-D9-31-BB-24-01" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 136
			{ 12584328, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-11-8F-1C-34-F3-4B-0F-86-00-C8-72-3D-90-16-D5-ED-91" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 144
			{ 12584336, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-CA-D0-28-A1-FA-4C-9E-A4-49-67-F6-88-86-AF-16-12-F7-98" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 152
			{ 12584344, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-A1-81-0A-16-DF-B8-88-86-C6-AB-A6-C7-01-94-72-44-EE-AA-A2" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 160
			{ 12584352, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-90-A8-1C-B5-BE-11-CC-AB-1A-41-29-89-ED-DD-15-8B-F2-C9-43-48" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 168
			{ 12584360, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-AA-0F-DC-66-1A-46-D2-D9-4F-91-9D-40-F0-2C-3D-18-DF-AE-6E-CB-6D" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 176
			{ 12584368, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-AC-86-A6-53-4E-49-FD-EA-76-3C-38-8A-7A-7B-11-0B-5A-9C-76-C0-8E-C6" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 184
			{ 12584376, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-26-7E-A6-48-DC-81-C3-04-41-D6-1B-7E-7D-11-2E-B1-89-D1-5E-02-AB-E6-E1" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 192
			{ 12584384, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-D1-47-20-15-9A-B2-47-DE-DF-84-38-80-F8-9F-91-0F-C2-60-87-7D-89-6C-F5-0B" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 200
			{ 12584392, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-B4-3C-9B-DE-72-46-F9-B9-74-23-62-D4-44-9B-DA-DB-75-D2-F2-9C-4F-3F-E5-40" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 208
			{ 12584400, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-AF-21-4F-E5-D7-B5-DE-09-53-B7-FF-CD-69-47-6C-D9-73-87-96-6B-D3-66-51-A1-DE-F3" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 216
			{ 12584408, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-D6-18-D3-94-54-CC-47-96-E8-4A-1E-39-40-64-D3-F2-09-12-26-6A-79-80-D3-E6-2A-35-BF" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 224
			{ 12584416, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-A5-0D-92-70-23-92-DB-77-AD-91-D9-45-D5-6D-33-2E-8C-16-4B-68-A5-DC-65-02-85-90-F9-C8" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 232
			{ 12584424, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-A1-A9-32-C7-F1-08-DD-8D-35-BE-62-A2-9B-57-52-E8-96-0D-73-30-B0-A7-CD-5F-66-74-6B-C1-55" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 240
			{ 12584432, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-B6-74-21-D9-25-4C-A1-66-1F-24-06-27-BB-EB-AC-4E-E7-60-B3-DB-93-2A-51-5D-82-6B-BA-3F-78-5A" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 248
			{ 12584440, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-D4-A9-A3-A1-34-AE-17-1E-A7-52-BB-2C-D7-B8-86-40-E7-18-84-74-8B-38-E9-70-F5-01-A3-23-BF-A6-E4" },
			// block size: 256, key size: 192, padding: ISO10126, feedback: 256
			{ 12584448, "06-D9-CB-C2-0E-49-4D-60-BD-68-4C-E4-6B-B6-E4-DE-2E-89-1D-6D-E9-8E-A7-D7-77-CE-E2-E0-D0-3D-7E-CA-F9-67-06-F3-91-68-9C-6E-77-D7-43-A9-9E-BF-F9-4A-CE-A8-05-91-96-34-10-9E-90-4A-4A-DE-3F-16-95-E3" },
			// block size: 256, key size: 256, padding: None, feedback: 8
			{ 16777480, "39" },
			// block size: 256, key size: 256, padding: None, feedback: 16
			{ 16777488, "39-DC" },
			// block size: 256, key size: 256, padding: None, feedback: 24
			{ 16777496, "39-DC-83" },
			// block size: 256, key size: 256, padding: None, feedback: 32
			{ 16777504, "39-DC-83-8B" },
			// block size: 256, key size: 256, padding: None, feedback: 40
			{ 16777512, "39-DC-83-8B-BB" },
			// block size: 256, key size: 256, padding: None, feedback: 48
			{ 16777520, "39-DC-83-8B-BB-4D" },
			// block size: 256, key size: 256, padding: None, feedback: 56
			{ 16777528, "39-DC-83-8B-BB-4D-1C" },
			// block size: 256, key size: 256, padding: None, feedback: 64
			{ 16777536, "39-DC-83-8B-BB-4D-1C-C3" },
			// block size: 256, key size: 256, padding: None, feedback: 72
			{ 16777544, "39-DC-83-8B-BB-4D-1C-C3-AB" },
			// block size: 256, key size: 256, padding: None, feedback: 80
			{ 16777552, "39-DC-83-8B-BB-4D-1C-C3-AB-41" },
			// block size: 256, key size: 256, padding: None, feedback: 88
			{ 16777560, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D" },
			// block size: 256, key size: 256, padding: None, feedback: 96
			{ 16777568, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91" },
			// block size: 256, key size: 256, padding: None, feedback: 104
			{ 16777576, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4" },
			// block size: 256, key size: 256, padding: None, feedback: 112
			{ 16777584, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C" },
			// block size: 256, key size: 256, padding: None, feedback: 120
			{ 16777592, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A" },
			// block size: 256, key size: 256, padding: None, feedback: 128
			{ 16777600, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7" },
			// block size: 256, key size: 256, padding: None, feedback: 136
			{ 16777608, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9" },
			// block size: 256, key size: 256, padding: None, feedback: 144
			{ 16777616, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18" },
			// block size: 256, key size: 256, padding: None, feedback: 152
			{ 16777624, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE" },
			// block size: 256, key size: 256, padding: None, feedback: 160
			{ 16777632, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A" },
			// block size: 256, key size: 256, padding: None, feedback: 168
			{ 16777640, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32" },
			// block size: 256, key size: 256, padding: None, feedback: 176
			{ 16777648, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40" },
			// block size: 256, key size: 256, padding: None, feedback: 184
			{ 16777656, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53" },
			// block size: 256, key size: 256, padding: None, feedback: 192
			{ 16777664, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31" },
			// block size: 256, key size: 256, padding: None, feedback: 200
			{ 16777672, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF" },
			// block size: 256, key size: 256, padding: None, feedback: 208
			{ 16777680, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75" },
			// block size: 256, key size: 256, padding: None, feedback: 216
			{ 16777688, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84" },
			// block size: 256, key size: 256, padding: None, feedback: 224
			{ 16777696, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7" },
			// block size: 256, key size: 256, padding: None, feedback: 232
			{ 16777704, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42" },
			// block size: 256, key size: 256, padding: None, feedback: 240
			{ 16777712, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E" },
			// block size: 256, key size: 256, padding: None, feedback: 248
			{ 16777720, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B" },
			// block size: 256, key size: 256, padding: None, feedback: 256
			{ 16777728, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-13" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 8
			{ 16777736, "39-E9" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 16
			{ 16777744, "39-DC-EF-5B" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 24
			{ 16777752, "39-DC-83-D8-1B-92" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 32
			{ 16777760, "39-DC-83-8B-66-B9-90-66" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 40
			{ 16777768, "39-DC-83-8B-BB-12-12-5C-29-DA" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 48
			{ 16777776, "39-DC-83-8B-BB-4D-8D-95-06-B0-1E-B5" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 56
			{ 16777784, "39-DC-83-8B-BB-4D-1C-32-B3-F3-F8-BD-1F-FD" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 64
			{ 16777792, "39-DC-83-8B-BB-4D-1C-C3-6D-5E-30-62-F1-47-A1-32" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 72
			{ 16777800, "39-DC-83-8B-BB-4D-1C-C3-AB-FF-89-C5-A2-14-B9-79-7D-05" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 80
			{ 16777808, "39-DC-83-8B-BB-4D-1C-C3-AB-41-62-28-8B-FD-AD-D4-90-AE-CE-B2" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 88
			{ 16777816, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-45-0E-FB-81-78-A6-93-19-B4-0F-F2" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 96
			{ 16777824, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-E4-EA-52-CB-EF-1B-60-CC-13-8B-FF-66" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 104
			{ 16777832, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-68-97-6F-E3-7F-17-3C-44-DE-64-C0-DA-AB" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 112
			{ 16777840, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-D5-B7-80-1C-FE-B0-E4-73-01-F5-9A-16-2E-C4" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 120
			{ 16777848, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-CB-E1-28-DD-4B-D7-CB-48-9B-26-3C-E4-B6-95-D7" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 128
			{ 16777856, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-18-07-EB-8B-D2-ED-E8-29-6B-C7-50-23-87-BE-E6-9D" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 136
			{ 16777864, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-24-90-D0-81-3A-7A-E5-D6-9A-58-BE-B5-E1-D2-E2-D9-74" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 144
			{ 16777872, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-75-86-C1-4B-48-D6-BD-9D-E5-D4-F7-22-57-0A-97-8B-A7-52" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 152
			{ 16777880, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-2B-7C-A1-58-6A-47-F3-6A-D0-AD-C7-CC-47-2B-05-9F-88-7C-9A" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 160
			{ 16777888, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-A7-2F-4D-1B-11-B8-47-0D-77-36-5C-76-0D-5F-1E-56-98-79-9D-20" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 168
			{ 16777896, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-44-B6-9E-59-4F-65-BC-BE-4F-8B-C1-1A-2C-B5-C1-C5-4A-5C-4C-C1-96" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 176
			{ 16777904, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-73-7B-FF-E6-5B-40-22-21-8C-1A-3D-56-EF-6F-48-A6-0E-86-AE-CF-C9-F3" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 184
			{ 16777912, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-4C-20-D4-76-18-A3-BD-8D-B0-77-53-34-C3-0F-D4-D0-64-F0-D7-39-EE-6C-8A" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 192
			{ 16777920, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-BB-47-AA-FE-39-A5-B2-92-4B-62-4C-8C-09-60-36-96-B0-D4-FC-09-3C-F4-D9-3E" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 200
			{ 16777928, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-17-0A-6C-A5-49-D7-5F-9E-64-C6-99-67-02-69-0A-6D-D3-EB-BF-7D-DC-FB-BD-11-B7" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 208
			{ 16777936, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-A1-B0-3C-DD-27-0C-3C-71-58-5A-C8-F7-EE-F9-DA-58-0A-72-04-D7-7B-9B-0B-7F-94-C2" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 216
			{ 16777944, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-46-D8-85-76-B5-C1-74-7B-A1-3A-DB-C8-D1-40-3E-C1-B3-28-B8-35-0E-47-B2-ED-F9-C9-E4" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 224
			{ 16777952, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-0C-48-AD-D0-95-17-A0-6C-BD-D7-16-EE-81-DB-2B-ED-5E-3F-2A-B5-0C-1C-ED-AF-CF-97-CD-84" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 232
			{ 16777960, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-3A-CC-5D-D8-98-AB-64-D5-9C-D8-5E-1B-C8-4E-0D-A4-9C-8C-47-03-82-91-9A-B0-F9-73-49-57-F2" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 240
			{ 16777968, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-28-DB-F5-1A-40-D3-1B-1E-53-52-1A-35-63-17-9D-CC-A1-BE-9D-60-22-E2-6B-48-E9-4D-2E-5F-AB-CE" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 248
			{ 16777976, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-E3-E6-00-41-D1-29-8F-59-86-BD-CD-C7-04-16-5B-44-67-71-A2-92-00-08-5D-98-FB-5B-15-6A-65-A4-C8" },
			// block size: 256, key size: 256, padding: PKCS7, feedback: 256
			{ 16777984, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-13-45-8F-5D-DD-68-EE-57-8C-E3-9A-56-72-CE-97-DD-66-44-D2-E8-9A-F0-2A-11-9D-73-97-7E-EC-98-53-72-6E" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 8
			{ 16777992, "39" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 16
			{ 16778000, "39-DC" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 24
			{ 16778008, "39-DC-83" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 32
			{ 16778016, "39-DC-83-8B" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 40
			{ 16778024, "39-DC-83-8B-BB" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 48
			{ 16778032, "39-DC-83-8B-BB-4D" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 56
			{ 16778040, "39-DC-83-8B-BB-4D-1C" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 64
			{ 16778048, "39-DC-83-8B-BB-4D-1C-C3" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 72
			{ 16778056, "39-DC-83-8B-BB-4D-1C-C3-AB" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 80
			{ 16778064, "39-DC-83-8B-BB-4D-1C-C3-AB-41" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 88
			{ 16778072, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 96
			{ 16778080, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 104
			{ 16778088, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 112
			{ 16778096, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 120
			{ 16778104, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 128
			{ 16778112, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 136
			{ 16778120, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 144
			{ 16778128, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 152
			{ 16778136, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 160
			{ 16778144, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 168
			{ 16778152, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 176
			{ 16778160, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 184
			{ 16778168, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 192
			{ 16778176, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 200
			{ 16778184, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 208
			{ 16778192, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 216
			{ 16778200, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 224
			{ 16778208, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 232
			{ 16778216, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 240
			{ 16778224, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 248
			{ 16778232, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B" },
			// block size: 256, key size: 256, padding: Zeros, feedback: 256
			{ 16778240, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-13" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 8
			{ 16778248, "39-E9" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 16
			{ 16778256, "39-DC-ED-5B" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 24
			{ 16778264, "39-DC-83-DB-18-92" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 32
			{ 16778272, "39-DC-83-8B-62-BD-94-66" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 40
			{ 16778280, "39-DC-83-8B-BB-17-17-59-2C-DA" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 48
			{ 16778288, "39-DC-83-8B-BB-4D-8B-93-00-B6-18-B5" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 56
			{ 16778296, "39-DC-83-8B-BB-4D-1C-35-B4-F4-FF-BA-18-FD" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 64
			{ 16778304, "39-DC-83-8B-BB-4D-1C-C3-65-56-38-6A-F9-4F-A9-32" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 72
			{ 16778312, "39-DC-83-8B-BB-4D-1C-C3-AB-F6-80-CC-AB-1D-B0-70-74-05" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 80
			{ 16778320, "39-DC-83-8B-BB-4D-1C-C3-AB-41-68-22-81-F7-A7-DE-9A-A4-C4-B2" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 88
			{ 16778328, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-4E-05-F0-8A-73-AD-98-12-BF-04-F2" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 96
			{ 16778336, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-E8-E6-5E-C7-E3-17-6C-C0-1F-87-F3-66" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 104
			{ 16778344, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-65-9A-62-EE-72-1A-31-49-D3-69-CD-D7-AB" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 112
			{ 16778352, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-DB-B9-8E-12-F0-BE-EA-7D-0F-FB-94-18-20-C4" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 120
			{ 16778360, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-C4-EE-27-D2-44-D8-C4-47-94-29-33-EB-B9-9A-D7" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 128
			{ 16778368, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-08-17-FB-9B-C2-FD-F8-39-7B-D7-40-33-97-AE-F6-9D" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 136
			{ 16778376, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-35-81-C1-90-2B-6B-F4-C7-8B-49-AF-A4-F0-C3-F3-C8-74" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 144
			{ 16778384, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-67-94-D3-59-5A-C4-AF-8F-F7-C6-E5-30-45-18-85-99-B5-52" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 152
			{ 16778392, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-38-6F-B2-4B-79-54-E0-79-C3-BE-D4-DF-54-38-16-8C-9B-6F-9A" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 160
			{ 16778400, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-B3-3B-59-0F-05-AC-53-19-63-22-48-62-19-4B-0A-42-8C-6D-89-20" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 168
			{ 16778408, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-51-A3-8B-4C-5A-70-A9-AB-5A-9E-D4-0F-39-A0-D4-D0-5F-49-59-D4-96" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 176
			{ 16778416, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-65-6D-E9-F0-4D-56-34-37-9A-0C-2B-40-F9-79-5E-B0-18-90-B8-D9-DF-F3" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 184
			{ 16778424, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-5B-37-C3-61-0F-B4-AA-9A-A7-60-44-23-D4-18-C3-C7-73-E7-C0-2E-F9-7B-8A" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 192
			{ 16778432, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-A3-5F-B2-E6-21-BD-AA-8A-53-7A-54-94-11-78-2E-8E-A8-CC-E4-11-24-EC-C1-3E" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 200
			{ 16778440, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-0E-13-75-BC-50-CE-46-87-7D-DF-80-7E-1B-70-13-74-CA-F2-A6-64-C5-E2-A4-08-B7" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 208
			{ 16778448, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-BB-AA-26-C7-3D-16-26-6B-42-40-D2-ED-F4-E3-C0-42-10-68-1E-CD-61-81-11-65-8E-C2" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 216
			{ 16778456, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-5D-C3-9E-6D-AE-DA-6F-60-BA-21-C0-D3-CA-5B-25-DA-A8-33-A3-2E-15-5C-A9-F6-E2-D2-E4" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 224
			{ 16778464, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-10-54-B1-CC-89-0B-BC-70-A1-CB-0A-F2-9D-C7-37-F1-42-23-36-A9-10-00-F1-B3-D3-8B-D1-84" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 232
			{ 16778472, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-27-D1-40-C5-85-B6-79-C8-81-C5-43-06-D5-53-10-B9-81-91-5A-1E-9F-8C-87-AD-E4-6E-54-4A-F2" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 240
			{ 16778480, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-36-C5-EB-04-5E-CD-05-00-4D-4C-04-2B-7D-09-83-D2-BF-A0-83-7E-3C-FC-75-56-F7-53-30-41-B5-CE" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 248
			{ 16778488, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-FC-F9-1F-5E-CE-36-90-46-99-A2-D2-D8-1B-09-44-5B-78-6E-BD-8D-1F-17-42-87-E4-44-0A-75-7A-BB-C8" },
			// block size: 256, key size: 256, padding: ANSIX923, feedback: 256
			{ 16778496, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-13-65-AF-7D-FD-48-CE-77-AC-C3-BA-76-52-EE-B7-FD-46-64-F2-C8-BA-D0-0A-31-BD-53-B7-5E-CC-B8-73-52-6E" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 8
			{ 16778504, "39-E9" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 16
			{ 16778512, "39-DC-0C-5B" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 24
			{ 16778520, "39-DC-83-40-BB-92" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 32
			{ 16778528, "39-DC-83-8B-BE-A1-7B-66" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 40
			{ 16778536, "39-DC-83-8B-BB-8C-FF-7F-4A-DA" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 48
			{ 16778544, "39-DC-83-8B-BB-4D-C5-FD-2F-CB-B2-B5" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 56
			{ 16778552, "39-DC-83-8B-BB-4D-1C-68-29-C4-65-46-62-FD" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 64
			{ 16778560, "39-DC-83-8B-BB-4D-1C-C3-70-0D-85-84-EB-C0-35-32" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 72
			{ 16778568, "39-DC-83-8B-BB-4D-1C-C3-AB-1A-36-C3-E5-1F-5B-3F-4F-05" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 80
			{ 16778576, "39-DC-83-8B-BB-4D-1C-C3-AB-41-80-F5-3B-C4-1E-8F-74-FF-12-B2" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 88
			{ 16778584, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-C2-EF-6B-C1-BA-BF-45-59-13-01-F2" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 96
			{ 16778592, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-31-1D-B8-DF-52-6D-C5-0C-FF-6F-DD-66" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 104
			{ 16778600, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-F9-7A-79-D1-05-DC-AF-9C-B3-A9-1D-B3-AB" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 112
			{ 16778608, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-BD-15-F7-EC-8C-DC-FC-C3-DC-68-05-92-5B-C4" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 120
			{ 16778616, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-45-4D-A6-32-6D-C7-09-F9-8E-D0-FF-B2-5B-99-D7" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 128
			{ 16778624, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-27-6C-51-C4-8E-C5-3D-FD-D1-51-B2-C5-DA-63-56-9D" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 136
			{ 16778632, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-12-FA-86-90-3A-6B-34-02-06-B5-73-CD-DF-2A-2C-9E-74" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 144
			{ 16778640, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-77-43-6B-8E-B1-EC-CD-35-D5-C7-62-3B-FC-A4-F7-90-E5-52" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 152
			{ 16778648, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-BF-D1-DC-26-88-36-BC-20-29-73-12-D2-1F-63-80-DA-88-71-9A" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 160
			{ 16778656, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-05-2A-66-ED-C1-64-2C-40-57-89-B1-7A-45-E4-64-45-0E-A3-BB-20" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 168
			{ 16778664, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-FA-4C-C7-96-C2-1B-2F-FF-39-13-7A-AC-48-59-66-C1-10-92-05-70-96" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 176
			{ 16778672, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-80-37-5F-E2-64-EE-82-56-53-04-83-E7-54-8F-A5-63-40-CE-D1-4C-56-F3" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 184
			{ 16778680, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-26-38-11-2E-CD-2B-05-40-6A-F4-45-15-0D-82-8C-D6-AE-BB-37-22-19-CC-8A" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 192
			{ 16778688, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-A9-FF-97-82-8C-93-FD-A6-3F-22-F0-AF-B5-AF-A1-3A-14-64-62-FD-E4-41-61-3E" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 200
			{ 16778696, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-53-08-7F-74-F9-D6-9F-4B-B8-62-51-15-9E-C0-FE-7B-80-37-4B-28-86-C7-6C-43-B7" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 208
			{ 16778704, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-ED-17-01-6B-58-57-4D-A9-EB-53-74-C4-A6-E3-39-E8-D3-18-42-4E-78-FA-8B-29-31-C2" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 216
			{ 16778712, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-10-94-40-86-0D-88-96-25-BE-76-6B-B2-9E-42-39-95-62-BE-0F-83-4E-CC-49-E6-B4-D5-E4" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 224
			{ 16778720, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-5B-3B-FA-86-80-44-C8-3C-06-79-1B-F8-EF-EA-78-69-79-70-45-02-4B-9C-4A-C4-FF-4B-98-84" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 232
			{ 16778728, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-9C-F5-4E-C6-22-B9-91-C8-9C-75-BE-26-03-2D-65-0B-F5-16-96-A8-AC-FD-2C-83-7E-B8-75-81-F2" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 240
			{ 16778736, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-22-0C-98-DC-7A-B5-1B-21-41-08-6D-E1-9E-43-05-5A-E2-6E-70-E5-9F-D0-F8-70-24-0E-55-8B-D1-CE" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 248
			{ 16778744, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-EF-A3-AC-C7-61-C9-8B-35-82-BB-7C-06-A9-7E-95-C8-EA-A1-D1-9F-19-22-C2-31-6D-D5-CA-77-02-8C-C8" },
			// block size: 256, key size: 256, padding: ISO10126, feedback: 256
			{ 16778752, "39-DC-83-8B-BB-4D-1C-C3-AB-41-8D-91-D4-7C-5A-F7-C9-18-CE-8A-32-40-53-31-EF-75-84-C7-42-1E-6B-13-F0-8B-16-2C-C5-72-EA-0B-85-4C-65-39-0B-46-8D-42-49-E2-DD-7F-57-3F-5B-18-84-71-9A-59-02-A4-02-6E" },
		};
	}
}