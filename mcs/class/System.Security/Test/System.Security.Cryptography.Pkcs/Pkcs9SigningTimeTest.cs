//
// Pkcs9SigningTimeTest.cs - NUnit tests for Pkcs9SigningTime
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs9SigningTimeTest : Assertion {

		static string signingTimeOid = "1.2.840.113549.1.9.5";
		static string signingTimeName = "Signing Time";

		[Test]
		public void ConstructorEmpty () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime ();
			AssertEquals ("Oid.FriendlyName", signingTimeName, st.Oid.FriendlyName);
			AssertEquals ("Oid.Value", signingTimeOid, st.Oid.Value);
			AssertEquals ("Values", 1, st.Values.Count);
		}

		[Test]
		public void ConstructorDateTime () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.UtcNow);
			AssertEquals ("Oid.FriendlyName", signingTimeName, st.Oid.FriendlyName);
			AssertEquals ("Oid.Value", signingTimeOid, st.Oid.Value);
			AssertEquals ("Values", 1, st.Values.Count);
		}

		[Test]
		public void ConstructorMin () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.MinValue);
			AssertEquals ("Oid.FriendlyName", signingTimeName, st.Oid.FriendlyName);
			AssertEquals ("Oid.Value", signingTimeOid, st.Oid.Value);
			AssertEquals ("Values", 1, st.Values.Count);
			DateTime signingTime = (DateTime) st.Values [0];
			AssertEquals ("Values[0]", DateTime.MinValue.Ticks, signingTime.Ticks);
		}

		[Test]
		public void ConstructorMax () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.MaxValue);
			AssertEquals ("Oid.FriendlyName", signingTimeName, st.Oid.FriendlyName);
			AssertEquals ("Oid.Value", signingTimeOid, st.Oid.Value);
			AssertEquals ("Values", 1, st.Values.Count);
			DateTime signingTime = (DateTime) st.Values [0];
			AssertEquals ("Values[0]", DateTime.MaxValue.Ticks, signingTime.Ticks);
		}
	}
}

#endif
