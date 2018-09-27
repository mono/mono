//
// I18N.Rare.Test.cs
//
// Author:
//	Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// Copyright (C) 2018 Microsoft
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
using System.IO;
using System.Text;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.I18N.Rare
{
	[TestFixture]
	public class TestRare
	{
		private global::I18N.Common.Manager Manager = global::I18N.Common.Manager.PrimaryManager;

		// ASCII compatible Rare codepages
		int[] cps = { 708, 852, 855, 857, 858, 862, 864, 866, 869 };

		void AssertDecode (string utf8file, string decfile, int codepage)
		{
			string decoded = null;
			byte [] encoded = null;
			using (StreamReader sr = new StreamReader (utf8file,
				Encoding.UTF8)) {
				decoded = sr.ReadToEnd ();
			}
			using (FileStream fs = File.OpenRead (decfile)) {
				encoded = new byte [fs.Length];
				fs.Read (encoded, 0, (int) fs.Length);
			}
			Encoding enc = Manager.GetEncoding (codepage);
			char [] actual;

			Assert.AreEqual (decoded.Length,
				enc.GetCharCount (encoded, 0, encoded.Length),
				"GetCharCount(byte[], 0, len)");
			actual = enc.GetChars (encoded, 0, encoded.Length);
			Assert.AreEqual (decoded.ToCharArray (), actual,
				"GetChars(byte[], 0, len)");
		}

		[Test]
		[TestCaseSource (nameof (cps))]
		public void Ascii_Test_All(int cp)
		{
			AssertDecode(TestResourceHelper.GetFullPathOfResource ("Test/texts/ascii-test.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/ascii-test.txt"), cp);
		}
	}
}
