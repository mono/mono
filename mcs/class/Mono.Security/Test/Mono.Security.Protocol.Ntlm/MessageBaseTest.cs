//
// Mono.Security.Protocol.Ntlm.MessageBase Unit Tests
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;

using Mono.Security.Protocol.Ntlm;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Protocol.Ntlm {

	[TestFixture]
	public class MessageBaseTest {
	
		// 
	
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Decode_Null () 
		{
			Type3Message msg = new Type3Message ((byte[])null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Decode_MinimalValidLength () 
		{
			Type3Message msg = new Type3Message (new byte [8]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Decode_BadHeader () 
		{
			byte[] header = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x01, 0x00, 0x00, 0x00, 0x00 };
			Type3Message msg = new Type3Message (header);
		}

	}
}
