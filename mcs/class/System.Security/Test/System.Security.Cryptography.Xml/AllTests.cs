//
// MonoTests.System.Security.Cryptography.Xml.AssertCrypto.cs
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	public class AssertCrypto {

		// because most crypto stuff works with byte[] buffers
		static public void AssertEquals (string msg, byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return;
			if (array1 == null)
				Assertion.Fail (msg + " -> First array is NULL");
			if (array2 == null)
				Assertion.Fail (msg + " -> Second array is NULL");

			bool a = (array1.Length == array2.Length);
			if (a) {
				for (int i = 0; i < array1.Length; i++) {
					if (array1 [i] != array2 [i]) {
						a = false;
						break;
					}
				}
			}
			msg += " -> Expected " + BitConverter.ToString (array1, 0);
			msg += " is different than " + BitConverter.ToString (array2, 0);
			Assertion.Assert (msg, a);
		}
	}
}
