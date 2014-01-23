//
// StrongNameMembershipConditionTest.cs -
//	NUnit Test Cases for StrongNameMembershipCondition
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
#if MOBILE
	[Ignore]
#endif
	public class StrongNameMembershipConditionTest {

		static Evidence allEmpty;
		static Evidence hostEcmaCorlibVersion;
		static Evidence hostEcmaVersion;
		static Evidence hostMsSystemSecurityVersion;
		static Evidence hostMsVersion;
		static Evidence hostOther;
		static Evidence assemblyEcmaCorlibVersion;
		static Evidence assemblyEcmaVersion;
		static Evidence assemblyMsSystemSecurityVersion;
		static Evidence assemblyMsVersion;
		static Evidence assemblyOther;
		static object wrongEvidence;

		static string name;
		static Version version;
		static StrongNamePublicKeyBlob blob;
		static StrongNamePublicKeyBlob ms;
		private static byte[] _msFinalKey = new byte[160] { 
			0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00,
			0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
			0x07, 0xD1, 0xFA, 0x57, 0xC4, 0xAE, 0xD9, 0xF0, 0xA3, 0x2E, 0x84, 0xAA, 0x0F, 0xAE, 0xFD, 0x0D, 
			0xE9, 0xE8, 0xFD, 0x6A, 0xEC, 0x8F, 0x87, 0xFB, 0x03, 0x76, 0x6C, 0x83, 0x4C, 0x99, 0x92, 0x1E, 
			0xB2, 0x3B, 0xE7, 0x9A, 0xD9, 0xD5, 0xDC, 0xC1, 0xDD, 0x9A, 0xD2, 0x36, 0x13, 0x21, 0x02, 0x90, 
			0x0B, 0x72, 0x3C, 0xF9, 0x80, 0x95, 0x7F, 0xC4, 0xE1, 0x77, 0x10, 0x8F, 0xC6, 0x07, 0x77, 0x4F, 
			0x29, 0xE8, 0x32, 0x0E, 0x92, 0xEA, 0x05, 0xEC, 0xE4, 0xE8, 0x21, 0xC0, 0xA5, 0xEF, 0xE8, 0xF1, 
			0x64, 0x5C, 0x4C, 0x0C, 0x93, 0xC1, 0xAB, 0x99, 0x28, 0x5D, 0x62, 0x2C, 0xAA, 0x65, 0x2C, 0x1D, 
			0xFA, 0xD6, 0x3D, 0x74, 0x5D, 0x6F, 0x2D, 0xE5, 0xF1, 0x7E, 0x5E, 0xAF, 0x0F, 0xC4, 0x96, 0x3D, 
			0x26, 0x1C, 0x8A, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6D, 0xC0, 0x93, 0x34, 0x4D, 0x5A, 0xD2, 0x93 };

		private Evidence CreateHostEvidence (object o)
		{
			Evidence e = new Evidence ();
			e.AddHost (o);
			return e;
		}

		private Evidence CreateAssemblyEvidence (object o)
		{
			Evidence e = new Evidence ();
			e.AddAssembly (o);
			return e;
		}

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			wrongEvidence = new Site ("test");
			allEmpty = new Evidence ();

			AssemblyName an = typeof (int).Assembly.GetName ();
			name = an.Name;
			version = an.Version;
			blob = new StrongNamePublicKeyBlob (an.GetPublicKey ());

			ms = new StrongNamePublicKeyBlob (_msFinalKey);

			hostEcmaCorlibVersion = CreateHostEvidence (new StrongName (blob, name, version));
			hostEcmaVersion = CreateHostEvidence (new StrongName (blob, " ", version));
			hostMsSystemSecurityVersion = CreateHostEvidence (new StrongName (ms, "System.Security", version));
			hostMsVersion = CreateHostEvidence (new StrongName (ms, " ", version));
			hostOther = CreateHostEvidence (wrongEvidence);

			assemblyEcmaCorlibVersion = CreateAssemblyEvidence (new StrongName (blob, name, version));
			assemblyEcmaVersion = CreateAssemblyEvidence (new StrongName (blob, " ", version));
			assemblyMsSystemSecurityVersion = CreateAssemblyEvidence (new StrongName (ms, "System.Security", version));
			assemblyMsVersion = CreateAssemblyEvidence (new StrongName (ms, " ", version));
			assemblyOther = CreateAssemblyEvidence (wrongEvidence);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StrongNameMembershipCondition_NullBlob ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (null, name, version);
		}

		private void Common (StrongNameMembershipCondition snmc)
		{
			Assert.IsFalse (snmc.Check (allEmpty), "Check(allEmpty)");
			Assert.IsFalse (snmc.Check (hostOther), "Check(hostOther)");
			Assert.IsFalse (snmc.Check (assemblyEcmaCorlibVersion), "Check(assemblyEcmaCorlibVersion)");
			Assert.IsFalse (snmc.Check (assemblyEcmaVersion), "Check(assemblyEcmaVersion)");
			Assert.IsFalse (snmc.Check (assemblyMsSystemSecurityVersion), "Check(assemblyMsSystemSecurityVersion)");
			Assert.IsFalse (snmc.Check (assemblyMsVersion), "Check(assemblyMsVersion)");
			Assert.IsFalse (snmc.Check (assemblyOther), "Check(assemblyOther)");

			StrongNameMembershipCondition copy = (StrongNameMembershipCondition)snmc.Copy ();
			Assert.IsTrue (copy.Equals (snmc), "copy.Equals (snmc)");
			Assert.IsTrue (snmc.Equals (copy), "snmc.Equals (copy)");
			copy.Name = null;
			copy.Version = null;
			bool original = ((snmc.Name == null) && (snmc.Version == null));
			Assert.AreEqual (original, copy.Equals (snmc), "bad.Equals (snmc)");
			Assert.AreEqual (original, snmc.Equals (copy), "snmc.Equals (bad)");

			SecurityElement se = snmc.ToXml ();
			copy.FromXml (se);
			Assert.AreEqual (snmc.PublicKey, copy.PublicKey, "PublicKey");
			Assert.AreEqual (snmc.Name, copy.Name, "Name");
			Assert.AreEqual (snmc.Version, copy.Version, "Version");
			Assert.AreEqual (snmc.GetHashCode (), copy.GetHashCode (), "GetHashCode ()");
			Assert.AreEqual (snmc.ToString (), copy.ToString (), "ToString ()");
			Assert.IsTrue (copy.Equals (snmc), "xml.Equals (snmc)");
			Assert.IsTrue (snmc.Equals (copy), "snmc.Equals (xml)");
		}

		[Test]
		public void StrongNameMembershipCondition_NullName ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, null, version);
			Assert.AreEqual (blob, snmc.PublicKey, "PublicKey");
			Assert.IsNull (snmc.Name, "Name");
			Assert.AreEqual (version, snmc.Version, "Version");
			Assert.AreEqual (blob.GetHashCode (), snmc.GetHashCode (), "GetHashCode ()");
			Assert.IsTrue (snmc.ToString ().StartsWith ("StrongName - 00000000000000000400000000000000 version = "), "ToString ()");

			Assert.IsTrue (snmc.Check (hostEcmaCorlibVersion), "Check(hostEcmaCorlibVersion)");
			Assert.IsTrue (snmc.Check (hostEcmaVersion), "Check(hostEcmaVersion)");
			Assert.IsFalse (snmc.Check (hostMsSystemSecurityVersion), "Check(hostMsSystemSecurityVersion)");
			Assert.IsFalse (snmc.Check (hostMsVersion), "Check(hostMsVersion)");
			Common (snmc);
		}

		[Test]
		public void StrongNameMembershipCondition_NullVersion ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, null);
			Assert.AreEqual (blob, snmc.PublicKey, "PublicKey");
			Assert.AreEqual (name, snmc.Name, "Name");
			Assert.IsNull (snmc.Version, "Version");
			Assert.AreEqual (blob.GetHashCode (), snmc.GetHashCode (), "GetHashCode ()");
			Assert.AreEqual ("StrongName - 00000000000000000400000000000000 name = mscorlib", snmc.ToString (), "ToString ()");

			Assert.IsTrue (snmc.Check (hostEcmaCorlibVersion), "Check(hostEcmaCorlibVersion)");
			Assert.IsFalse (snmc.Check (hostEcmaVersion), "Check(hostEcmaVersion)");
			Assert.IsFalse (snmc.Check (hostMsSystemSecurityVersion), "Check(hostMsSystemSecurityVersion)");
			Assert.IsFalse (snmc.Check (hostMsVersion), "Check(hostMsVersion)");
			Common (snmc);
		}

		[Test]
		public void StrongNameMembershipCondition_NullNameVersion ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, null, null);
			Assert.AreEqual (blob, snmc.PublicKey, "PublicKey");
			Assert.IsNull (snmc.Name, "Name");
			Assert.IsNull (snmc.Version, "Version");
			Assert.AreEqual (blob.GetHashCode (), snmc.GetHashCode (), "GetHashCode ()");
			Assert.AreEqual ("StrongName - 00000000000000000400000000000000", snmc.ToString (), "ToString ()");

			Assert.IsTrue (snmc.Check (hostEcmaCorlibVersion), "Check(hostEcmaCorlibVersion)");
			Assert.IsTrue (snmc.Check (hostEcmaVersion), "Check(hostEcmaVersion)");
			Assert.IsFalse (snmc.Check (hostMsSystemSecurityVersion), "Check(hostMsSystemSecurityVersion)");
			Assert.IsFalse (snmc.Check (hostMsVersion), "Check(hostMsVersion)");
			Common (snmc);
		}

		[Test]
		public void StrongNameMembershipCondition_Mscorlib ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			Assert.AreEqual (blob, snmc.PublicKey, "PublicKey");
			Assert.AreEqual ("mscorlib", snmc.Name, "Name");
			Assert.AreEqual (version, snmc.Version, "Version");
			Assert.AreEqual (blob.GetHashCode (), snmc.GetHashCode (), "GetHashCode ()");
			Assert.IsTrue (snmc.ToString ().StartsWith ("StrongName - 00000000000000000400000000000000 name = mscorlib version = "), "ToString ()");

			Assert.IsTrue (snmc.Check (hostEcmaCorlibVersion), "Check(hostEcmaCorlibVersion)");
			Assert.IsFalse (snmc.Check (hostEcmaVersion), "Check(hostEcmaVersion)");
			Assert.IsFalse (snmc.Check (hostMsSystemSecurityVersion), "Check(hostMsSystemSecurityVersion)");
			Assert.IsFalse (snmc.Check (hostMsVersion), "Check(hostMsVersion)");
			Common (snmc);
		}

		[Test]
		public void StrongNameMembershipCondition_MsKey ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (ms, null, null);
			Assert.AreEqual (ms, snmc.PublicKey, "PublicKey");
			Assert.IsNull (snmc.Name, "Name");
			Assert.IsNull (snmc.Version, "Version");
			Assert.AreEqual (ms.GetHashCode (), snmc.GetHashCode (), "GetHashCode ()");

			Assert.IsFalse (snmc.Check (hostEcmaCorlibVersion), "Check(hostEcmaCorlibVersion)");
			Assert.IsFalse (snmc.Check (hostEcmaVersion), "Check(hostEcmaVersion)");
			Assert.IsTrue (snmc.Check (hostMsSystemSecurityVersion), "Check(hostMsSystemSecurityVersion)");
			Assert.IsTrue (snmc.Check (hostMsVersion), "Check(hostMsVersion)");
			Common (snmc);
		}

		[Test]
		public void Name_Null ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			snmc.Name = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PublicKey_Null ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			snmc.PublicKey = null;
		}

		[Test]
		public void Version_Null ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			snmc.Version = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			snmc.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();
			se.Tag = "IMonoship";
			snmc.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidClass ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();
			se.Attributes ["class"] = "Hello world";
			snmc.FromXml (se);
		}

		[Test]
		public void FromXml_NoClass ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			snmc.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			w.AddAttribute ("PublicKeyBlob", se.Attribute ("PublicKeyBlob"));
			snmc.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			snmc.FromXml (w);
		}

		[Test]
		public void FromXml_PolicyLevel ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				StrongNameMembershipCondition spl = new StrongNameMembershipCondition (blob, name, version);
				spl.FromXml (se, pl);
				Assert.IsTrue (spl.Equals (snmc), "FromXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}

		[Test]
		public void ToXml_Null ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			// no ArgumentNullException here
			SecurityElement se = snmc.ToXml (null);
			Assert.IsNotNull (se, "ToXml(null)");
		}

		[Test]
		public void ToXml_PolicyLevel ()
		{
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (blob, name, version);
			SecurityElement se = snmc.ToXml ();
			string s = snmc.ToXml ().ToString ();
			// is it accepted for all policy levels ?
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel pl = e.Current as PolicyLevel;
				StrongNameMembershipCondition spl = new StrongNameMembershipCondition (blob, name, version);
				spl.FromXml (se, pl);
				Assert.AreEqual (s, spl.ToXml (pl).ToString (), "ToXml(PolicyLevel='" + pl.Label + "')");
			}
			// yes!
		}
	}
}
