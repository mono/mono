//
// GenericIdentityTest.cs - NUnit Test Cases for GenericIdentity
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;

namespace MonoTests.System.Security.Principal {

	[TestFixture]
	public class GenericIdentityTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullName () 
		{
			GenericIdentity gi = new GenericIdentity (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullAuthenticationType () 
		{
			GenericIdentity gi = new GenericIdentity ("user", null);
		}

		[Test]
		public void Name () 
		{
			GenericIdentity gi = new GenericIdentity ("user");
			Assert.AreEqual ("user", gi.Name, "Name");
			Assert.AreEqual (String.Empty, gi.AuthenticationType, "AuthenticationType");
			Assert.IsTrue (gi.IsAuthenticated, "IsAuthenticated");
		}

		[Test]
		public void NameAuthenticationType () 
		{
			GenericIdentity gi = new GenericIdentity ("user", "blood oath");
			Assert.AreEqual ("user", gi.Name, "Name");
			Assert.AreEqual ("blood oath", gi.AuthenticationType, "AuthenticationType");
			Assert.IsTrue (gi.IsAuthenticated, "IsAuthenticated");
		}

		[Test]
		public void EmptyName () 
		{
			GenericIdentity gi = new GenericIdentity ("");
			Assert.AreEqual (String.Empty, gi.Name, "Name");
			Assert.AreEqual (String.Empty, gi.AuthenticationType, "AuthenticationType");
			Assert.IsFalse (gi.IsAuthenticated, "IsAuthenticated");
		}

		[Test]
		public void SerializationRoundtrip ()
		{
			GenericIdentity gi = new GenericIdentity ("mono", "dna");
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, gi);

			//Console.WriteLine (BitConverter.ToString (ms.ToArray ()));

			ms.Position = 0;
			GenericIdentity clone = (GenericIdentity) bf.Deserialize (ms);
			Assert.AreEqual (gi.Name, clone.Name, "Name");
			Assert.AreEqual (gi.AuthenticationType, clone.AuthenticationType, "AuthenticationType");
			Assert.AreEqual (gi.IsAuthenticated, clone.IsAuthenticated, "IsAuthenticated");
		}

		static byte[] identity = { 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00, 0x29, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x53, 0x65, 0x63, 0x75, 0x72, 0x69, 0x74, 0x79, 0x2E, 0x50, 0x72, 0x69, 0x6E, 0x63, 0x69, 0x70, 0x61, 0x6C, 0x2E, 0x47, 0x65, 0x6E, 0x65, 0x72, 0x69, 0x63, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x02, 0x00, 0x00, 0x00, 0x06, 0x6D, 0x5F, 0x6E, 0x61, 0x6D, 0x65, 0x06, 0x6D, 0x5F, 0x74, 0x79, 0x70, 0x65, 0x01, 0x01, 0x06, 0x02, 0x00, 0x00, 0x00, 0x04, 0x6D, 0x6F, 0x6E, 0x6F, 0x06, 0x03, 0x00, 0x00, 0x00, 0x03, 0x64, 0x6E, 0x61, 0x0B };

		[Test]
		public void Deserialize ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream (identity);
			GenericIdentity gi = (GenericIdentity) bf.Deserialize (ms);
			Assert.AreEqual ("mono", gi.Name, "Name");
			Assert.AreEqual ("dna", gi.AuthenticationType, "AuthenticationType");
			Assert.IsTrue (gi.IsAuthenticated, "IsAuthenticated");
		}
	}
}
