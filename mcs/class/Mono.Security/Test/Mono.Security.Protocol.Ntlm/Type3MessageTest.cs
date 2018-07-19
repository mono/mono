//
// Mono.Security.Protocol.Ntlm.Type3MessageTest
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Text;

using Mono.Security.Protocol.Ntlm;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Protocol.Ntlm {

	[TestFixture]
	public class Type3MessageTest {

		static byte[] nonce = { 0x53, 0x72, 0x76, 0x4e, 0x6f, 0x6e, 0x63, 0x65 };

		static byte[] data1 = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x72, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x8a, 0x00, 0x00, 0x00, 0x14, 0x00, 0x14, 0x00, 0x40, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x0c, 0x00, 0x54, 0x00, 0x00, 0x00, 0x12, 0x00, 0x12, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa2, 0x00, 0x00, 0x00, 0x01, 0x82, 0x00, 0x00, 0x55, 0x00, 0x52, 0x00, 0x53, 0x00, 0x41, 0x00, 0x2d, 0x00, 0x4d, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x4f, 0x00, 0x52, 0x00, 0x5a, 0x00, 0x61, 0x00, 0x70, 0x00, 0x68, 0x00, 0x6f, 0x00, 0x64, 0x00, 0x4c, 0x00, 0x49, 0x00, 0x47, 0x00, 0x48, 0x00, 0x54, 0x00, 0x43, 0x00, 0x49, 0x00, 0x54, 0x00, 0x59, 0x00, 0xad, 0x87, 0xca, 0x6d, 0xef, 0xe3, 0x46, 0x85, 0xb9, 0xc4, 0x3c, 0x47, 0x7a, 0x8c, 0x42, 0xd6, 0x00, 0x66, 0x7d, 0x68, 0x92, 0xe7, 0xe8, 0x97, 0xe0, 0xe0, 0x0d, 0xe3, 0x10, 0x4a, 0x1b, 0xf2, 0x05, 0x3f, 0x07, 0xc7, 0xdd, 0xa8, 0x2d, 0x3c, 0x48, 0x9a, 0xe9, 0x89, 0xe1, 0xb0, 0x00, 0xd3 };
		static byte[] data2 = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x6a, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x82, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x0c, 0x00, 0x40, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x4c, 0x00, 0x00, 0x00, 0x16, 0x00, 0x16, 0x00, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9a, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x44, 0x00, 0x4f, 0x00, 0x4d, 0x00, 0x41, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x75, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x57, 0x00, 0x4f, 0x00, 0x52, 0x00, 0x4b, 0x00, 0x53, 0x00, 0x54, 0x00, 0x41, 0x00, 0x54, 0x00, 0x49, 0x00, 0x4f, 0x00, 0x4e, 0x00, 0xc3, 0x37, 0xcd, 0x5c, 0xbd, 0x44, 0xfc, 0x97, 0x82, 0xa6, 0x67, 0xaf, 0x6d, 0x42, 0x7c, 0x6d, 0xe6, 0x7c, 0x20, 0xc2, 0xd3, 0xe7, 0x7c, 0x56, 0x25, 0xa9, 0x8c, 0x1c, 0x31, 0xe8, 0x18, 0x47, 0x46, 0x6b, 0x29, 0xb2, 0xdf, 0x46, 0x80, 0xf3, 0x99, 0x58, 0xfb, 0x8c, 0x21, 0x3a, 0x9c, 0xc6 };
		
		static Type3MessageTest ()
		{
			// Explicitly select legacy-mode.
			Type3Message.DefaultAuthLevel = NtlmAuthLevel.LM_and_NTLM;
		}

		[Test]
		// Example for a password smaller than 8 characters - which implies a weak DES key
		public void SmallPassword () 
		{
			Type3Message msg = new Type3Message ();
			msg.Challenge = new byte [8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
			msg.Domain = "DOMAIN";
			msg.Host = "HOST";
			msg.Password = "WELCOME";
			msg.Username = "username";
			Assert.AreEqual (3, msg.Type, "Type");
			Assert.AreEqual ("4E-54-4C-4D-53-53-50-00-03-00-00-00-18-00-18-00-64-00-00-00-18-00-18-00-7C-00-00-00-0C-00-0C-00-40-00-00-00-10-00-10-00-4C-00-00-00-08-00-08-00-5C-00-00-00-00-00-00-00-94-00-00-00-01-B2-00-00-44-00-4F-00-4D-00-41-00-49-00-4E-00-75-00-73-00-65-00-72-00-6E-00-61-00-6D-00-65-00-48-00-4F-00-53-00-54-00-CA-12-00-72-3C-41-D5-77-AB-18-C7-64-C6-DE-F3-4F-A6-1B-FA-06-71-EA-5F-C8-7A-CE-90-85-AB-CC-37-59-38-0B-1C-68-62-E3-98-C3-C0-EF-9C-FC-22-E8-A2-C2", BitConverter.ToString (msg.GetBytes ()), "GetBytes");
		}

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void Encode1 () 
		{
			Type3Message msg = new Type3Message ();
			msg.Challenge = nonce;
			// Type3Message now encodes domain and host case-sensitive.
			msg.Domain = "URSA-MINOR";
			msg.Host = "LIGHTCITY";
			msg.Password = "Beeblebrox";
			msg.Username = "Zaphod";
			Assert.AreEqual (3, msg.Type, "Type");
			Assert.AreEqual ("4E-54-4C-4D-53-53-50-00-03-00-00-00-18-00-18-00-72-00-00-00-18-00-18-00-8A-00-00-00-14-00-14-00-40-00-00-00-0C-00-0C-00-54-00-00-00-12-00-12-00-60-00-00-00-00-00-00-00-A2-00-00-00-01-B2-00-00-55-00-52-00-53-00-41-00-2D-00-4D-00-49-00-4E-00-4F-00-52-00-5A-00-61-00-70-00-68-00-6F-00-64-00-4C-00-49-00-47-00-48-00-54-00-43-00-49-00-54-00-59-00-AD-87-CA-6D-EF-E3-46-85-B9-C4-3C-47-7A-8C-42-D6-00-66-7D-68-92-E7-E8-97-E0-E0-0D-E3-10-4A-1B-F2-05-3F-07-C7-DD-A8-2D-3C-48-9A-E9-89-E1-B0-00-D3", BitConverter.ToString (msg.GetBytes ()), "GetBytes");
		}

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void Decode1 () 
		{
			Type3Message msg = new Type3Message (data1);
			Assert.AreEqual ("URSA-MINOR", msg.Domain, "Domain");
			Assert.AreEqual ("LIGHTCITY", msg.Host, "Host");
			Assert.AreEqual ("Zaphod", msg.Username, "Username");
			Assert.AreEqual ((NtlmFlags)0x8201, msg.Flags, "Flags");
			Assert.AreEqual (3, msg.Type, "Type");
			Assert.IsNull (msg.Password, "Password");
			Assert.AreEqual ("AD-87-CA-6D-EF-E3-46-85-B9-C4-3C-47-7A-8C-42-D6-00-66-7D-68-92-E7-E8-97", BitConverter.ToString (msg.LM), "LM");
			Assert.AreEqual ("E0-E0-0D-E3-10-4A-1B-F2-05-3F-07-C7-DD-A8-2D-3C-48-9A-E9-89-E1-B0-00-D3", BitConverter.ToString (msg.NT), "NT");
		}

		[Test]
		// Example from http://davenport.sourceforge.net/ntlm.html#type3MessageExample
		public void Decode2 () 
		{
			Type3Message msg = new Type3Message (data2);
			Assert.AreEqual ("DOMAIN", msg.Domain, "Domain");
			Assert.AreEqual ("WORKSTATION", msg.Host, "Host");
			Assert.AreEqual ("user", msg.Username, "Username");
			Assert.AreEqual ((NtlmFlags)0x201, msg.Flags, "Flags");
			Assert.AreEqual (3, msg.Type, "Type");
			Assert.IsNull (msg.Password, "Password");
			Assert.AreEqual ("C3-37-CD-5C-BD-44-FC-97-82-A6-67-AF-6D-42-7C-6D-E6-7C-20-C2-D3-E7-7C-56", BitConverter.ToString (msg.LM), "LM");
			Assert.AreEqual ("25-A9-8C-1C-31-E8-18-47-46-6B-29-B2-DF-46-80-F3-99-58-FB-8C-21-3A-9C-C6", BitConverter.ToString (msg.NT), "NT");
		}

		[Test]
		public void Challenge () 
		{
			Type3Message msg = new Type3Message ();
			Assert.IsNull (msg.Challenge, "Challenge");
			
			byte[] c = new byte [8];
			msg.Challenge = c;
			Assert.AreEqual (8, msg.Challenge.Length, "Challenge.Length");
			
			c [0] = 1;
			Assert.AreEqual (0, msg.Challenge [0], "Challenge not directly accessible");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Challenge_Null () 
		{
			Type3Message msg = new Type3Message ();
			msg.Challenge = null;
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Challenge_InvalidLength () 
		{
			Type3Message msg = new Type3Message ();
			msg.Challenge = new byte [9];
		}
	}
}
