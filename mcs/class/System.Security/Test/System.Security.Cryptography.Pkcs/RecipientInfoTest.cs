//
// RecipientInfoTest.cs - NUnit tests for RecipientInfo
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

	// RecipientInfo is an abstract class so this is a concrete implementation
	// to test the (non abstract) constructor and Type property 
	public class UTestRecipientInfo : RecipientInfo {

		public UTestRecipientInfo () : base () {}

		// properties

		public override byte[] EncryptedKey { 
			get { return null; }
		}

		public override AlgorithmIdentifier KeyEncryptionAlgorithm { 
			get { return null; }
		}

		public override SubjectIdentifier RecipientIdentifier {
			get { return null; }
		}

		public override int Version {
			get { return 0; }
		}
	}

	[TestFixture]
	public class RecipientInfoTest : Assertion {

		[Test]
		public void Type () 
		{
			UTestRecipientInfo ri = new UTestRecipientInfo ();
			AssertEquals ("Type", RecipientInfoType.Unknown, ri.Type);
		}
	}
}

#endif
