//
// Mono.Security.Protocol.Ntlm.ChallengeResponseTest
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
	public class ChallengeResponseTest : Assertion {

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void BeeblebroxSrvNonce () 
		{
			byte[] SrvNonce = Encoding.ASCII.GetBytes ("SrvNonce");
			using (ChallengeResponse ntlm = new ChallengeResponse ("Beeblebrox", SrvNonce)) {
				AssertEquals ("NT", "E0-E0-0D-E3-10-4A-1B-F2-05-3F-07-C7-DD-A8-2D-3C-48-9A-E9-89-E1-B0-00-D3", BitConverter.ToString (ntlm.NT));
				AssertEquals ("LM", "AD-87-CA-6D-EF-E3-46-85-B9-C4-3C-47-7A-8C-42-D6-00-66-7D-68-92-E7-E8-97", BitConverter.ToString (ntlm.LM));
			}
		}

		[Test]
		// Example from http://packetstormsecurity.nl/Crackers/NT/l0phtcrack/l0phtcrack2.5-readme.html
		public void L0phtCrack () 
		{
			byte[] SrvNonce = new byte [8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
			using (ChallengeResponse ntlm = new ChallengeResponse ("WELCOME", SrvNonce)) {
				AssertEquals ("NT", "7A-CE-90-85-AB-CC-37-59-38-0B-1C-68-62-E3-98-C3-C0-EF-9C-FC-22-E8-A2-C2", BitConverter.ToString (ntlm.NT));
				AssertEquals ("LM", "CA-12-00-72-3C-41-D5-77-AB-18-C7-64-C6-DE-F3-4F-A6-1B-FA-06-71-EA-5F-C8", BitConverter.ToString (ntlm.LM));
			}
		}
	}
}
