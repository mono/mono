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
	}
}
