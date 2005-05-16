//
// CspProviderFlagsTest.cs - NUnit Test Cases for CspProviderFlags
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class CspProviderFlagsTest {

		[Test]
		public void Values () 
		{
#if NET_2_0
			Assert.AreEqual ((int)CspProviderFlags.NoFlags, 0, "NoFlags");
#endif
			Assert.AreEqual ((int)CspProviderFlags.UseMachineKeyStore, 1, "UseMachineKeyStore");
			Assert.AreEqual ((int)CspProviderFlags.UseDefaultKeyContainer, 2, "UseDefaultKeyContainer");
#if NET_2_0
			Assert.AreEqual ((int)CspProviderFlags.NoPrompt, 64, "NoPrompt");
			Assert.AreEqual ((int)CspProviderFlags.UseArchivableKey, 16, "UseArchivableKey");
			Assert.AreEqual ((int)CspProviderFlags.UseExistingKey, 8, "UseExistingKey");
			Assert.AreEqual ((int)CspProviderFlags.UseNonExportableKey, 4, "UseNonExportableKey");
			Assert.AreEqual ((int)CspProviderFlags.UseUserProtectedKey, 32, "UseUserProtectedKey");
#endif
		}

		[Test]
		public void Flags () 
		{
			CspProviderFlags cpf = CspProviderFlags.UseDefaultKeyContainer | CspProviderFlags.UseMachineKeyStore
#if NET_2_0
				| CspProviderFlags.NoFlags | CspProviderFlags.NoPrompt
				| CspProviderFlags.UseArchivableKey | CspProviderFlags.UseExistingKey 
				| CspProviderFlags.UseNonExportableKey | CspProviderFlags.UseUserProtectedKey;

			int expected = 127;
#else
				;
			int expected = 3;
#endif
			Assert.AreEqual (expected, (int)cpf, "All");
		}
	}
}
