//
// Mono.Security.Protocol.Ntlm.ChallengeResponseTest
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
	public class ChallengeResponseTest {

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void BeeblebroxSrvNonce () 
		{
			byte[] SrvNonce = Encoding.ASCII.GetBytes ("SrvNonce");
			using (ChallengeResponse ntlm = new ChallengeResponse ("Beeblebrox", SrvNonce)) {
				Assert.AreEqual ("E0-E0-0D-E3-10-4A-1B-F2-05-3F-07-C7-DD-A8-2D-3C-48-9A-E9-89-E1-B0-00-D3", BitConverter.ToString (ntlm.NT), "NT");
				Assert.AreEqual ("AD-87-CA-6D-EF-E3-46-85-B9-C4-3C-47-7A-8C-42-D6-00-66-7D-68-92-E7-E8-97", BitConverter.ToString (ntlm.LM), "LM");
			}
		}

		[Test]
		// Example from http://packetstormsecurity.nl/Crackers/NT/l0phtcrack/l0phtcrack2.5-readme.html
		public void L0phtCrack () 
		{
			byte[] SrvNonce = new byte [8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
			using (ChallengeResponse ntlm = new ChallengeResponse ("WELCOME", SrvNonce)) {
				Assert.AreEqual ("7A-CE-90-85-AB-CC-37-59-38-0B-1C-68-62-E3-98-C3-C0-EF-9C-FC-22-E8-A2-C2", BitConverter.ToString (ntlm.NT), "NT");
				Assert.AreEqual ("CA-12-00-72-3C-41-D5-77-AB-18-C7-64-C6-DE-F3-4F-A6-1B-FA-06-71-EA-5F-C8", BitConverter.ToString (ntlm.LM), "LM");
			}
		}

		[Test]
		public void NullPassword () 
		{
			byte[] SrvNonce = new byte [8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
			using (ChallengeResponse ntlm = new ChallengeResponse (null, SrvNonce)) {
				Assert.AreEqual ("4A-FD-81-EC-01-87-E8-8D-97-77-8D-F7-93-C6-DA-D4-F0-3A-36-63-66-9D-20-1C", BitConverter.ToString (ntlm.NT), "NT");
				// note the last 8 bytes... they are the same as the previous unit test ;-)
				Assert.AreEqual ("0A-39-2B-11-CF-05-2B-02-6D-65-CF-F5-68-BD-E4-15-A6-1B-FA-06-71-EA-5F-C8", BitConverter.ToString (ntlm.LM), "LM");
			}
		}

		[Test]
		public void EmptyPassword () 
		{
			byte[] SrvNonce = new byte [8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
			using (ChallengeResponse ntlm = new ChallengeResponse (String.Empty, SrvNonce)) {
				// same as the previous one as this is the same (null/empty) password expressed diffently
				Assert.AreEqual ("4A-FD-81-EC-01-87-E8-8D-97-77-8D-F7-93-C6-DA-D4-F0-3A-36-63-66-9D-20-1C", BitConverter.ToString (ntlm.NT), "NT");
				Assert.AreEqual ("0A-39-2B-11-CF-05-2B-02-6D-65-CF-F5-68-BD-E4-15-A6-1B-FA-06-71-EA-5F-C8", BitConverter.ToString (ntlm.LM), "LM");
			}
		}
		
		[Test] 
		public void NoPropertiesOutput () 
		{
			ChallengeResponse ntlm = new ChallengeResponse ("Mono", new byte [8]);
			// no out!
			Assert.IsNull (ntlm.Password, "Password");
			Assert.IsNull (ntlm.Challenge, "Challenge");
		}
		
		[Test] 
		[ExpectedException (typeof (ArgumentNullException))]
		public void Challenge_Null () 
		{
			ChallengeResponse ntlm = new ChallengeResponse ();
			ntlm.Challenge = null;
		}
		
		[Test] 
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Password_Disposed () 
		{
			ChallengeResponse ntlm = new ChallengeResponse ("Mono", new byte [8]);
			ntlm.Dispose ();
			ntlm.Password = "Mini";
		}

		[Test] 
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Challenge_Disposed () 
		{
			ChallengeResponse ntlm = new ChallengeResponse ("Mono", new byte [8]);
			ntlm.Dispose ();
			ntlm.Challenge = new byte [8];
		}
		
		[Test] 
		[ExpectedException (typeof (ObjectDisposedException))]
		public void NT_Disposed () 
		{
			ChallengeResponse ntlm = new ChallengeResponse ("Mono", new byte [8]);
			ntlm.Dispose ();
			Assert.IsNotNull (ntlm.NT, "NT");
		}

		[Test] 
		[ExpectedException (typeof (ObjectDisposedException))]
		public void LM_Disposed () 
		{
			ChallengeResponse ntlm = new ChallengeResponse ("Mono", new byte [8]);
			ntlm.Dispose ();
			Assert.IsNotNull (ntlm.LM, "LM");
		}
	}
}
