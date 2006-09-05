//
// ApplicationIdTest.cs - NUnit Test Cases for ApplicationId
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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

#if NET_2_0

using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class ApplicationIdTest {

		static byte[] defaultPublicKeyToken = new byte [0];
		static string defaultName = "name";
		static Version defaultVersion = new Version (1, 0, 0, 0);
		static string defaultProc = "proc";
		static string defaultCulture = "culture";

		[Test]
		public void ApplicationId ()
		{
			ApplicationId id = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, defaultCulture);
			Assert.IsNotNull (id, "ApplicationId");
			// wait for NUnit 2.2 Assert.AreEqual (defaultPublicKeyToken, id.PublicKeyToken, "PublicKeyToken");
			Assert.AreEqual (defaultName, id.Name, "Name");
			Assert.AreEqual (defaultVersion, id.Version, "Version");
			Assert.AreEqual (defaultProc, id.ProcessorArchitecture, "ProcessorArchitecture");
			Assert.AreEqual (defaultCulture, id.Culture, "Culture");
			Assert.AreEqual ("name, culture=\"culture\", version=\"1.0.0.0\", publicKeyToken=\"\", processorArchitecture =\"proc\"", id.ToString (), "ToString");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplicationId_PublicKeyTokenNull ()
		{
			new ApplicationId (null, defaultName, defaultVersion, defaultProc, defaultCulture);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplicationId_NameNull ()
		{
			new ApplicationId (defaultPublicKeyToken, null, defaultVersion, defaultProc, defaultCulture);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplicationId_VersionNull ()
		{
			new ApplicationId (defaultPublicKeyToken, defaultName, null, defaultProc, defaultCulture);
		}

		[Test]
		public void ApplicationId_ProcessorArchitectureNull ()
		{
			ApplicationId id = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, null, defaultCulture);
			// wait for NUnit 2.2 Assert.AreEqual (defaultPublicKeyToken, id.PublicKeyToken, "PublicKeyToken");
			Assert.AreEqual (defaultName, id.Name, "Name");
			Assert.AreEqual (defaultVersion, id.Version, "Version");
			Assert.IsNull (id.ProcessorArchitecture, "ProcessorArchitecture");
			Assert.AreEqual (defaultCulture, id.Culture, "Culture");
			Assert.AreEqual ("name, culture=\"culture\", version=\"1.0.0.0\", publicKeyToken=\"\"", id.ToString (), "ToString");
		}

		[Test]
		public void ApplicationId_CultureNull ()
		{
			ApplicationId id = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, null);
			// wait for NUnit 2.2 Assert.AreEqual (defaultPublicKeyToken, id.PublicKeyToken, "PublicKeyToken");
			Assert.AreEqual (defaultName, id.Name, "Name");
			Assert.AreEqual (defaultVersion, id.Version, "Version");
			Assert.AreEqual (defaultProc, id.ProcessorArchitecture, "ProcessorArchitecture");
			Assert.IsNull (id.Culture, "Culture");
			Assert.AreEqual ("name, version=\"1.0.0.0\", publicKeyToken=\"\", processorArchitecture =\"proc\"", id.ToString (), "ToString");
		}

		[Test]
		public void PublicKeyToken ()
		{
			byte[] token = new byte [1];
			ApplicationId id = new ApplicationId (token, defaultName, defaultVersion, null, null);
			token = id.PublicKeyToken;
			Assert.AreEqual (0, token [0], "PublicKeyToken");
			token [0] = 1;
			Assert.AreEqual (1, token [0], "token");
			token = id.PublicKeyToken;
			Assert.AreEqual (0, token [0], "PublicKeyToken");
		}

		[Test]
		public void Copy () 
		{
			ApplicationId id1 = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, defaultCulture);
			ApplicationId id2 = id1.Copy ();
			Assert.IsTrue (id1.Equals (id2), "Equals-1");
			Assert.IsTrue (id2.Equals (id1), "Equals-2");
			Assert.IsFalse (Object.ReferenceEquals (id1, id2), "ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			ApplicationId id1 = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, defaultCulture);
			ApplicationId id2 = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, defaultCulture);
			Assert.IsTrue (id1.Equals (id2), "Equals-1");
			Assert.IsTrue (id2.Equals (id1), "Equals-2");
			Assert.AreEqual (id1.GetHashCode (), id2.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void Equals_Subset ()
		{
			ApplicationId id1 = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, defaultCulture);
			ApplicationId id2 = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, null, defaultCulture);
			Assert.IsFalse (id1.Equals (id2), "Equals-A1");
			Assert.IsFalse (id2.Equals (id1), "Equals-A2");
			// would have expected IsFalse
			Assert.IsTrue (id1.GetHashCode () == id2.GetHashCode (), "GetHashCode-A");

			ApplicationId id3 = new ApplicationId (defaultPublicKeyToken, defaultName, defaultVersion, defaultProc, null);
			Assert.IsFalse (id1.Equals (id3), "Equals-B1");
			Assert.IsFalse (id3.Equals (id1), "Equals-B2");
			// would have expected IsFalse
			Assert.IsTrue (id1.GetHashCode () == id3.GetHashCode (), "GetHashCode-B");
		}

		[Test]
		public void ToString_ ()
		{
			byte[] token = new byte [256];
			for (int i=0; i < token.Length; i++)
				token [i] = (byte)i;
			ApplicationId id = new ApplicationId (token, "Mono", new Version (1, 2), "Multiple", "neutral");
			Assert.AreEqual ("Mono, culture=\"neutral\", version=\"1.2\", publicKeyToken=\"000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F303132333435363738393A3B3C3D3E3F404142434445464748494A4B4C4D4E4F505152535455565758595A5B5C5D5E5F606162636465666768696A6B6C6D6E6F707172737475767778797A7B7C7D7E7F808182838485868788898A8B8C8D8E8F909192939495969798999A9B9C9D9E9FA0A1A2A3A4A5A6A7A8A9AAABACADAEAFB0B1B2B3B4B5B6B7B8B9BABBBCBDBEBFC0C1C2C3C4C5C6C7C8C9CACBCCCDCECFD0D1D2D3D4D5D6D7D8D9DADBDCDDDEDFE0E1E2E3E4E5E6E7E8E9EAEBECEDEEEFF0F1F2F3F4F5F6F7F8F9FAFBFCFDFEFF\", processorArchitecture =\"Multiple\"", id.ToString (), "ToString");
		}
	}
}

#endif