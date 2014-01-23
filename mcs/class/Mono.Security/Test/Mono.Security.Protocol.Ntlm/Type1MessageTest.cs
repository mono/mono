//
// Mono.Security.Protocol.Ntlm.Type1MessageTest
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

using Mono.Security.Protocol.Ntlm;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Protocol.Ntlm {

	[TestFixture]
	public class Type1MessageTest : Assertion {

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void Encode1 () 
		{
			Type1Message msg = new Type1Message ();
			AssertEquals ("Type", 1, msg.Type);
			msg.Domain = "Ursa-Minor";
			msg.Host = "LightCity";
			AssertEquals ("GetBytes", "4E-54-4C-4D-53-53-50-00-01-00-00-00-07-B2-00-00-0A-00-0A-00-29-00-00-00-09-00-09-00-20-00-00-00-4C-49-47-48-54-43-49-54-59-55-52-53-41-2D-4D-49-4E-4F-52", BitConverter.ToString (msg.GetBytes ()));
		}

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void Decode1 () 
		{
			byte[] data = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0xb2, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x29, 0x00, 0x00, 0x00, 0x09, 0x00, 0x09, 0x00, 0x20, 0x00, 0x00, 0x00, 0x4c, 0x49, 0x47, 0x48, 0x54, 0x43, 0x49, 0x54, 0x59, 0x55, 0x52, 0x53, 0x41, 0x2d, 0x4d, 0x49, 0x4e, 0x4f, 0x52 };
			Type1Message msg = new Type1Message (data);
			AssertEquals ("Domain", "URSA-MINOR", msg.Domain);
			AssertEquals ("Flags", (NtlmFlags)0xb203, msg.Flags);
			AssertEquals ("Host", "LIGHTCITY", msg.Host);
			AssertEquals ("Type", 1, msg.Type);
		}

		[Test]
		// Example from http://davenport.sourceforge.net/ntlm.html#type1MessageExample
		public void Decode2 () 
		{
			byte[] data = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0x32, 0x00, 0x00, 0x06, 0x00, 0x06, 0x00, 0x2b, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x0b, 0x00, 0x20, 0x00, 0x00, 0x00, 0x57, 0x4f, 0x52, 0x4b, 0x53, 0x54, 0x41, 0x54, 0x49, 0x4f, 0x4e, 0x44, 0x4f, 0x4d, 0x41, 0x49, 0x4e };
			Type1Message msg = new Type1Message (data);
			AssertEquals ("Domain", "DOMAIN", msg.Domain);
			AssertEquals ("Flags", (NtlmFlags)0x3207, msg.Flags);
			AssertEquals ("Host", "WORKSTATION", msg.Host);
			AssertEquals ("Type", 1, msg.Type);
		}
	}
}
