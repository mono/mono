//
// HashMembershipConditionTest.cs -
//	NUnit Test Cases for HashMembershipCondition
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
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class HashMembershipConditionTest {

		static Hash hashEvidence;
		static MD5 md5;
		static SHA1 sha1;
		static byte[] digestMd5;
		static byte[] digestSha1;

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			hashEvidence = new Hash (a);

			md5 = MD5.Create ();
			digestMd5 = hashEvidence.GenerateHash (md5);

			sha1 = SHA1.Create ();
			digestSha1 = hashEvidence.GenerateHash (sha1);
		}

		[Test]
		public void Constructor_MD5 ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			Assert.IsNotNull (hash);
			Assert.AreEqual (md5, hash.HashAlgorithm, "HashAlgorithm");
			Assert.AreEqual (BitConverter.ToString (digestMd5), BitConverter.ToString (hash.HashValue), "HashValue");
		}

		[Test]
		public void Constructor_SHA1 ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (sha1, digestSha1);
			Assert.IsNotNull (hash);
			Assert.AreEqual (sha1, hash.HashAlgorithm, "HashAlgorithm");
			Assert.AreEqual (BitConverter.ToString (digestSha1), BitConverter.ToString (hash.HashValue), "HashValue");
		}

		[Test]
		public void HashValue ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			// we can't change the instance data by getting a reference inside it
			byte[] value = hash.HashValue;
			value [0] ^= 0xFF;
			Assert.IsFalse (value [0] == hash.HashValue [0], "reference");
			Assert.AreEqual (BitConverter.ToString (digestMd5), BitConverter.ToString (hash.HashValue), "HashValue");
			// and we can't change the instance data by keeping a reference to what we supply
			hash.HashValue = value;
			byte old_value = value [0];
			value [0] += 42;
			Assert.IsFalse (value [0] == hash.HashValue [0], "reference-2");
			Assert.AreEqual (old_value, hash.HashValue [0], "HashValue[0]");
		}

		[Test]
		public void Check ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			Evidence e = null;
			Assert.IsFalse (hash.Check (e), "Check (null)");
			e = new Evidence ();
			Assert.IsFalse (hash.Check (e), "Check (empty)");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.IsFalse (hash.Check (e), "Check (zone)");
			e.AddAssembly (hashEvidence);
			Assert.IsFalse (hash.Check (e), "Check (hash-assembly)");

			e = new Evidence ();
			e.AddHost (hashEvidence);
			Assert.IsTrue (hash.Check (e), "Check (MD5-host)");

			hash = new HashMembershipCondition (sha1, digestSha1);
			Assert.IsTrue (hash.Check (e), "Check (SHA1-host)");
		}

		[Test]
		public void Copy ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			HashMembershipCondition copy = (HashMembershipCondition)hash.Copy ();
			Assert.AreEqual (hash, copy, "Equals");
			Assert.IsFalse (Object.ReferenceEquals (hash, copy), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			Assert.IsFalse (hash.Equals (null), "Equals(null)");
			Assert.IsFalse (hash.Equals (new object ()), "Equals (object)");

			HashMembershipCondition h2 = new HashMembershipCondition (md5, digestMd5);
			Assert.IsTrue (hash.Equals (h2), "Equals(h2)");
			Assert.IsTrue (h2.Equals (hash), "Equals(hash)");

			// same assembly but different algorithm / value
			hash = new HashMembershipCondition (sha1, digestSha1);
			Assert.IsFalse (hash.Equals (h2), "Equals(h2)");
			Assert.IsFalse (h2.Equals (hash), "Equals(hash)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			hash.FromXml (null);
		}

		[Test]
		public void FromXml ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();
			hash.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();
			se.Tag = "IMonoship";
			hash.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();
			se.Tag = "IMEMBERSHIPCONDITION"; // instehash of IMembershipCondition
			hash.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();
			se.Attributes ["class"] = "Hello world";
			hash.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			hash.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			hash.FromXml (w);
			// doesn't seems to care about the version number!
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			hash.FromXml (w);
		}

		[Test]
		public void FromXml_Empty_HashAlgorithm ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "1");
			hash.FromXml (w);
			// this is accepted - but doesn't include a hash algorithm or value
			// both would throw ArgumentNullException from the constructor

			Assert.IsTrue ((hash.HashAlgorithm is SHA1Managed), "HashAlgorithm");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_Empty_HashValue()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "1");
			hash.FromXml (w);
			// this is accepted - but doesn't include a hash algorithm or value
			// both would throw ArgumentNullException from the constructor
			byte[] value = hash.HashValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_Empty_ToString ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "1");
			hash.FromXml (w);
			// this is accepted - but doesn't include a hash algorithm or value
			// both would throw ArgumentNullException from the constructor
			string s = hash.ToString ();
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			hash.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		public void FromXml_PolicyLevelNull ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();
			hash.FromXml (se, null);
		}

		[Test]
		public void GetHashCode_ ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			HashMembershipCondition copy = (HashMembershipCondition)hash.Copy ();
			Assert.AreEqual (hash.GetHashCode (), copy.GetHashCode ());
		}

		[Test]
		public void ToString_ ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			Assert.IsTrue (hash.ToString ().StartsWith ("Hash - System.Security.Cryptography.MD5"), "MD5");

			hash = new HashMembershipCondition (sha1, digestSha1);
			Assert.IsTrue (hash.ToString ().StartsWith ("Hash - System.Security.Cryptography.SHA1"), "SHA1");
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		public void ToXml ()
		{
			HashMembershipCondition hash = new HashMembershipCondition (md5, digestMd5);
			SecurityElement se = hash.ToXml ();
			Assert.AreEqual ("IMembershipCondition", se.Tag, "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("System.Security.Policy.HashMembershipCondition"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (se.ToString (), hash.ToXml (null).ToString (), "ToXml(null)");
			Assert.AreEqual (se.ToString (), hash.ToXml (PolicyLevel.CreateAppDomainLevel ()).ToString (), "ToXml(PolicyLevel)");
		}
	}
}
