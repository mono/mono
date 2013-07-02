//
// Unit tests for System.Security.Cryptography.CngAlgorithmGroup
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
	public class CngAlgorithmGroupTest {

		[Test]
		public void StaticProperties ()
		{
			Assert.IsNotNull (CngAlgorithmGroup.DiffieHellman, "DiffieHellman");
			Assert.IsNotNull (CngAlgorithmGroup.Dsa, "Dsa");
			Assert.IsNotNull (CngAlgorithmGroup.ECDiffieHellman, "ECDiffieHellman");
			Assert.IsNotNull (CngAlgorithmGroup.ECDsa, "ECDsa");
			Assert.IsNotNull (CngAlgorithmGroup.Rsa, "Rsa");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull ()
		{
			new CngAlgorithmGroup (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEmpty ()
		{
			new CngAlgorithmGroup (String.Empty);
		}

		static CngAlgorithmGroup mono = new CngAlgorithmGroup ("mono");

		private void Check (CngAlgorithmGroup group)
		{
			Assert.AreEqual (group.AlgorithmGroup, group.ToString (), "Algorithm/ToString");
			Assert.AreEqual (group.GetHashCode (), group.AlgorithmGroup.GetHashCode (), "GetHashCode");
			Assert.IsTrue (group.Equals (group), "Equals(self)");
			Assert.IsTrue (group.Equals ((object) group), "Equals((object)self)");

			CngAlgorithmGroup copy = new CngAlgorithmGroup (group.AlgorithmGroup);
			Assert.AreEqual (group.GetHashCode (), copy.GetHashCode (), "Copy");
			Assert.IsTrue (group.Equals (copy), "Equals(copy)");
			Assert.IsTrue (group.Equals ((object) copy), "Equals((object)copy)");
			Assert.IsTrue (group == copy, "algo==copy");
			Assert.IsFalse (group != copy, "algo!=copy");

			Assert.IsFalse (group.Equals (mono), "Equals(mono)");
			Assert.IsFalse (group.Equals ((object) mono), "Equals((object)mono)");
			Assert.IsFalse (group == mono, "algo==mono");
			Assert.IsTrue (group != mono, "algo!=mono");
		}

		[Test]
		public void ConstructorCustom ()
		{
			CngAlgorithmGroup group = new CngAlgorithmGroup ("custom");
			Check (group);
			Assert.IsFalse (group.Equals ((CngAlgorithmGroup) null), "Equals((CngAlgorithmGroup)null)");
			Assert.IsFalse (group.Equals ((object) null), "Equals((object)null)");
		}

		[Test]
		public void DiffieHellman ()
		{
			CngAlgorithmGroup group = CngAlgorithmGroup.DiffieHellman;
			Assert.AreEqual ("DH", group.AlgorithmGroup, "AlgorithmGroup");
			Assert.IsTrue (group.Equals (CngAlgorithmGroup.DiffieHellman), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (group, CngAlgorithmGroup.DiffieHellman), "ReferenceEquals");
			Check (group);
		}

		[Test]
		public void Dsa ()
		{
			CngAlgorithmGroup group = CngAlgorithmGroup.Dsa;
			Assert.AreEqual ("DSA", group.AlgorithmGroup, "AlgorithmGroup");
			Assert.IsTrue (group.Equals (CngAlgorithmGroup.Dsa), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (group, CngAlgorithmGroup.Dsa), "ReferenceEquals");
			Check (group);
		}

		[Test]
		public void ECDiffieHellman ()
		{
			CngAlgorithmGroup group = CngAlgorithmGroup.ECDiffieHellman;
			Assert.AreEqual ("ECDH", group.AlgorithmGroup, "AlgorithmGroup");
			Assert.IsTrue (group.Equals (CngAlgorithmGroup.ECDiffieHellman), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (group, CngAlgorithmGroup.ECDiffieHellman), "ReferenceEquals");
			Check (group);
		}

		[Test]
		public void ECDsa ()
		{
			CngAlgorithmGroup group = CngAlgorithmGroup.ECDsa;
			Assert.AreEqual ("ECDSA", group.AlgorithmGroup, "AlgorithmGroup");
			Assert.IsTrue (group.Equals (CngAlgorithmGroup.ECDsa), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (group, CngAlgorithmGroup.ECDsa), "ReferenceEquals");
			Check (group);
		}

		[Test]
		public void Rsa ()
		{
			CngAlgorithmGroup group = CngAlgorithmGroup.Rsa;
			Assert.AreEqual ("RSA", group.AlgorithmGroup, "AlgorithmGroup");
			Assert.IsTrue (group.Equals (CngAlgorithmGroup.Rsa), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (group, CngAlgorithmGroup.Rsa), "ReferenceEquals");
			Check (group);
		}
	}
}

#endif
