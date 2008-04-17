//
// RSATest.cs - NUnit Test Cases for RSA
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	public class NonAbstractRSAForUnitTests : RSA {
		protected RSAParameters rsaParams;

		// not tested here - but we must implemented all abstract properties
		public override string KeyExchangeAlgorithm {
			get { return null; }
		}

		// not tested here - but we must implemented all abstract properties
		public override string SignatureAlgorithm {
			get { return null; }
		}

		// not tested here - but we must implemented all abstract methods
		public override byte [] DecryptValue (byte [] rgb)
		{
			return null;
		}

		// not tested here - but we must implemented all abstract methods
		public override byte [] EncryptValue (byte [] rgb)
		{
			return null;
		}

		// basic implementation for tests
		public override RSAParameters ExportParameters (bool includePrivateParameters)
		{
			if (includePrivateParameters)
				return rsaParams;
			else {
				RSAParameters rsaPublicParams = rsaParams;
				rsaPublicParams.D = null;
				rsaPublicParams.DP = null;
				rsaPublicParams.DQ = null;
				rsaPublicParams.P = null;
				rsaPublicParams.Q = null;
				rsaPublicParams.InverseQ = null;
				return rsaPublicParams;
			}
		}

		// basic implementation for tests
		public override void ImportParameters (RSAParameters parameters)
		{
			rsaParams = new RSAParameters ();
			if (parameters.D != null)
				rsaParams.D = (byte []) parameters.D.Clone ();
			if (parameters.DP != null)
				rsaParams.DP = (byte []) parameters.DP.Clone ();
			if (parameters.DQ != null)
				rsaParams.DQ = (byte []) parameters.DQ.Clone ();
			if (parameters.P != null)
				rsaParams.P = (byte []) parameters.P.Clone ();
			if (parameters.Q != null)
				rsaParams.Q = (byte []) parameters.Q.Clone ();
			if (parameters.InverseQ != null)
				rsaParams.InverseQ = (byte []) parameters.InverseQ.Clone ();
			// public key
			rsaParams.Exponent = (byte []) parameters.Exponent.Clone ();
			rsaParams.Modulus = (byte []) parameters.Modulus.Clone ();
		}

		// not tested here - but we must implemented all abstract methods
		protected override void Dispose (bool disposing)
		{
		}
	}

	[TestFixture]
	public class RSATest {
		protected RSA rsa;

		static byte [] rsaModulus =  { 0xbb, 0xf8, 0x2f, 0x09, 0x06, 0x82, 0xce, 0x9c, 0x23, 0x38, 0xac, 0x2b, 0x9d, 0xa8, 0x71, 0xf7, 
					    0x36, 0x8d, 0x07, 0xee, 0xd4, 0x10, 0x43, 0xa4, 0x40, 0xd6, 0xb6, 0xf0, 0x74, 0x54, 0xf5, 0x1f,
					    0xb8, 0xdf, 0xba, 0xaf, 0x03, 0x5c, 0x02, 0xab, 0x61, 0xea, 0x48, 0xce, 0xeb, 0x6f, 0xcd, 0x48,
					    0x76, 0xed, 0x52, 0x0d, 0x60, 0xe1, 0xec, 0x46, 0x19, 0x71, 0x9d, 0x8a, 0x5b, 0x8b, 0x80, 0x7f,
					    0xaf, 0xb8, 0xe0, 0xa3, 0xdf, 0xc7, 0x37, 0x72, 0x3e, 0xe6, 0xb4, 0xb7, 0xd9, 0x3a, 0x25, 0x84,
					    0xee, 0x6a, 0x64, 0x9d, 0x06, 0x09, 0x53, 0x74, 0x88, 0x34, 0xb2, 0x45, 0x45, 0x98, 0x39, 0x4e,
					    0xe0, 0xaa, 0xb1, 0x2d, 0x7b, 0x61, 0xa5, 0x1f, 0x52, 0x7a, 0x9a, 0x41, 0xf6, 0xc1, 0x68, 0x7f,
					    0xe2, 0x53, 0x72, 0x98, 0xca, 0x2a, 0x8f, 0x59, 0x46, 0xf8, 0xe5, 0xfd, 0x09, 0x1d, 0xbd, 0xcb };
		static byte [] rsaExponent = { 0x11 };
		static byte [] rsaP =  { 0xee, 0xcf, 0xae, 0x81, 0xb1, 0xb9, 0xb3, 0xc9, 0x08, 0x81, 0x0b, 0x10, 0xa1, 0xb5, 0x60, 0x01, 
				      0x99, 0xeb, 0x9f, 0x44, 0xae, 0xf4, 0xfd, 0xa4, 0x93, 0xb8, 0x1a, 0x9e, 0x3d, 0x84, 0xf6, 0x32,
				      0x12, 0x4e, 0xf0, 0x23, 0x6e, 0x5d, 0x1e, 0x3b, 0x7e, 0x28, 0xfa, 0xe7, 0xaa, 0x04, 0x0a, 0x2d,
				      0x5b, 0x25, 0x21, 0x76, 0x45, 0x9d, 0x1f, 0x39, 0x75, 0x41, 0xba, 0x2a, 0x58, 0xfb, 0x65, 0x99 };
		static byte [] rsaQ =  { 0xc9, 0x7f, 0xb1, 0xf0, 0x27, 0xf4, 0x53, 0xf6, 0x34, 0x12, 0x33, 0xea, 0xaa, 0xd1, 0xd9, 0x35,
				      0x3f, 0x6c, 0x42, 0xd0, 0x88, 0x66, 0xb1, 0xd0, 0x5a, 0x0f, 0x20, 0x35, 0x02, 0x8b, 0x9d, 0x86, 
				      0x98, 0x40, 0xb4, 0x16, 0x66, 0xb4, 0x2e, 0x92, 0xea, 0x0d, 0xa3, 0xb4, 0x32, 0x04, 0xb5, 0xcf,
				      0xce, 0x33, 0x52, 0x52, 0x4d, 0x04, 0x16, 0xa5, 0xa4, 0x41, 0xe7, 0x00, 0xaf, 0x46, 0x15, 0x03 };
		static byte [] rsaDP = { 0x54, 0x49, 0x4c, 0xa6, 0x3e, 0xba, 0x03, 0x37, 0xe4, 0xe2, 0x40, 0x23, 0xfc, 0xd6, 0x9a, 0x5a, 
				      0xeb, 0x07, 0xdd, 0xdc, 0x01, 0x83, 0xa4, 0xd0, 0xac, 0x9b, 0x54, 0xb0, 0x51, 0xf2, 0xb1, 0x3e, 
				      0xd9, 0x49, 0x09, 0x75, 0xea, 0xb7, 0x74, 0x14, 0xff, 0x59, 0xc1, 0xf7, 0x69, 0x2e, 0x9a, 0x2e, 
				      0x20, 0x2b, 0x38, 0xfc, 0x91, 0x0a, 0x47, 0x41, 0x74, 0xad, 0xc9, 0x3c, 0x1f, 0x67, 0xc9, 0x81 };
		static byte [] rsaDQ = { 0x47, 0x1e, 0x02, 0x90, 0xff, 0x0a, 0xf0, 0x75, 0x03, 0x51, 0xb7, 0xf8, 0x78, 0x86, 0x4c, 0xa9, 
				      0x61, 0xad, 0xbd, 0x3a, 0x8a, 0x7e, 0x99, 0x1c, 0x5c, 0x05, 0x56, 0xa9, 0x4c, 0x31, 0x46, 0xa7, 
				      0xf9, 0x80, 0x3f, 0x8f, 0x6f, 0x8a, 0xe3, 0x42, 0xe9, 0x31, 0xfd, 0x8a, 0xe4, 0x7a, 0x22, 0x0d, 
				      0x1b, 0x99, 0xa4, 0x95, 0x84, 0x98, 0x07, 0xfe, 0x39, 0xf9, 0x24, 0x5a, 0x98, 0x36, 0xda, 0x3d };
		static byte [] rsaInverseQ = { 0xb0, 0x6c, 0x4f, 0xda, 0xbb, 0x63, 0x01, 0x19, 0x8d, 0x26, 0x5b, 0xdb, 0xae, 0x94, 0x23, 0xb3, 
					    0x80, 0xf2, 0x71, 0xf7, 0x34, 0x53, 0x88, 0x50, 0x93, 0x07, 0x7f, 0xcd, 0x39, 0xe2, 0x11, 0x9f, 
					    0xc9, 0x86, 0x32, 0x15, 0x4f, 0x58, 0x83, 0xb1, 0x67, 0xa9, 0x67, 0xbf, 0x40, 0x2b, 0x4e, 0x9e, 
					    0x2e, 0x0f, 0x96, 0x56, 0xe6, 0x98, 0xea, 0x36, 0x66, 0xed, 0xfb, 0x25, 0x79, 0x80, 0x39, 0xf7 };
		static byte [] rsaD = { 0xa5, 0xda, 0xfc, 0x53, 0x41, 0xfa, 0xf2, 0x89, 0xc4, 0xb9, 0x88, 0xdb, 0x30, 0xc1, 0xcd, 0xf8,
				     0x3f, 0x31, 0x25, 0x1e, 0x06, 0x68, 0xb4, 0x27, 0x84, 0x81, 0x38, 0x01, 0x57, 0x96, 0x41, 0xb2, 
				     0x94, 0x10, 0xb3, 0xc7, 0x99, 0x8d, 0x6b, 0xc4, 0x65, 0x74, 0x5e, 0x5c, 0x39, 0x26, 0x69, 0xd6, 
				     0x87, 0x0d, 0xa2, 0xc0, 0x82, 0xa9, 0x39, 0xe3, 0x7f, 0xdc, 0xb8, 0x2e, 0xc9, 0x3e, 0xda, 0xc9, 
				     0x7f, 0xf3, 0xad, 0x59, 0x50, 0xac, 0xcf, 0xbc, 0x11, 0x1c, 0x76, 0xf1, 0xa9, 0x52, 0x94, 0x44, 
				     0xe5, 0x6a, 0xaf, 0x68, 0xc5, 0x6c, 0x09, 0x2c, 0xd3, 0x8d, 0xc3, 0xbe, 0xf5, 0xd2, 0x0a, 0x93,
				     0x99, 0x26, 0xed, 0x4f, 0x74, 0xa1, 0x3e, 0xdd, 0xfb, 0xe1, 0xa1, 0xce, 0xcc, 0x48, 0x94, 0xaf, 
				     0x94, 0x28, 0xc2, 0xb7, 0xb8, 0x88, 0x3f, 0xe4, 0x46, 0x3a, 0x4b, 0xc8, 0x5b, 0x1c, 0xb3, 0xc1 };

		static string xmlPrivate = "<RSAKeyValue><Modulus>u/gvCQaCzpwjOKwrnahx9zaNB+7UEEOkQNa28HRU9R+437qvA1wCq2HqSM7rb81Idu1SDWDh7EYZcZ2KW4uAf6+44KPfxzdyPua0t9k6JYTuamSdBglTdIg0skVFmDlO4KqxLXthpR9SeppB9sFof+JTcpjKKo9ZRvjl/Qkdvcs=</Modulus><Exponent>EQ==</Exponent><P>7s+ugbG5s8kIgQsQobVgAZnrn0Su9P2kk7ganj2E9jISTvAjbl0eO34o+ueqBAotWyUhdkWdHzl1QboqWPtlmQ==</P><Q>yX+x8Cf0U/Y0EjPqqtHZNT9sQtCIZrHQWg8gNQKLnYaYQLQWZrQukuoNo7QyBLXPzjNSUk0EFqWkQecAr0YVAw==</Q><DP>VElMpj66Azfk4kAj/NaaWusH3dwBg6TQrJtUsFHysT7ZSQl16rd0FP9ZwfdpLpouICs4/JEKR0F0rck8H2fJgQ==</DP><DQ>Rx4CkP8K8HUDUbf4eIZMqWGtvTqKfpkcXAVWqUwxRqf5gD+Pb4rjQukx/YrkeiING5mklYSYB/45+SRamDbaPQ==</DQ><InverseQ>sGxP2rtjARmNJlvbrpQjs4Dycfc0U4hQkwd/zTniEZ/JhjIVT1iDsWepZ79AK06eLg+WVuaY6jZm7fsleYA59w==</InverseQ><D>pdr8U0H68onEuYjbMMHN+D8xJR4GaLQnhIE4AVeWQbKUELPHmY1rxGV0Xlw5JmnWhw2iwIKpOeN/3LguyT7ayX/zrVlQrM+8ERx28alSlETlaq9oxWwJLNONw7710gqTmSbtT3ShPt374aHOzEiUr5Qowre4iD/kRjpLyFscs8E=</D></RSAKeyValue>";

		static string xmlPublic = "<RSAKeyValue><Modulus>u/gvCQaCzpwjOKwrnahx9zaNB+7UEEOkQNa28HRU9R+437qvA1wCq2HqSM7rb81Idu1SDWDh7EYZcZ2KW4uAf6+44KPfxzdyPua0t9k6JYTuamSdBglTdIg0skVFmDlO4KqxLXthpR9SeppB9sFof+JTcpjKKo9ZRvjl/Qkdvcs=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>";

		[SetUp]
		public void SetUp ()
		{
			rsa = new NonAbstractRSAForUnitTests ();
		}

		public void AssertEquals (string msg, byte [] array1, byte [] array2)
		{
			Assert.AreEqual (array1, array2, msg);
		}

		// may also help for RSA descendants
		public void AssertEquals (string message, RSAParameters expectedKey, RSAParameters actualKey, bool checkPrivateKey)
		{
			if (checkPrivateKey) {
				AssertEquals (message + " D", expectedKey.D, actualKey.D);
				AssertEquals (message + " DP", expectedKey.DP, actualKey.DP);
				AssertEquals (message + " DQ", expectedKey.DQ, actualKey.DQ);
				AssertEquals (message + " P", expectedKey.P, actualKey.P);
				AssertEquals (message + " Q", expectedKey.Q, actualKey.Q);
				AssertEquals (message + " InverseQ", expectedKey.InverseQ, actualKey.InverseQ);
			}
			AssertEquals (message + " Modulus", expectedKey.Modulus, actualKey.Modulus);
			AssertEquals (message + " Exponent", expectedKey.Exponent, actualKey.Exponent);
		}

		public RSAParameters GetKey (bool includePrivateKey)
		{
			RSAParameters p = new RSAParameters ();
			if (includePrivateKey) {
				p.D = rsaD;
				p.DP = rsaDP;
				p.DQ = rsaDQ;
				p.P = rsaP;
				p.Q = rsaQ;
				p.InverseQ = rsaInverseQ;
			} else {
				p.D = null;
				p.DP = null;
				p.DQ = null;
				p.P = null;
				p.Q = null;
				p.InverseQ = null;
			}
			p.Modulus = rsaModulus;
			p.Exponent = rsaExponent;
			return p;
		}

		// importing RSA keypair and exporting a RSA keypair 
		[Test]
		public void RSAImportPrivateExportPrivate ()
		{
			RSAParameters input = GetKey (true);
			rsa.ImportParameters (input);
			string xmlRSA = rsa.ToXmlString (true);
			rsa.FromXmlString (xmlRSA);
			Assert.AreEqual (xmlPrivate, xmlRSA, "RSA Import Private Export Private (xml)");
			RSAParameters output = rsa.ExportParameters (true);
			AssertEquals ("RSA Import Private Export Private (binary)", input, output, true);
		}

		// importing RSA keypair and exporting a RSA public key 
		[Test]
		public void RSAImportPrivateExportPublic ()
		{
			RSAParameters input = GetKey (true);
			rsa.ImportParameters (input);
			string xmlRSA = rsa.ToXmlString (false);
			rsa.FromXmlString (xmlRSA);
			Assert.AreEqual (xmlPublic, xmlRSA, "RSA Import Private Export Public (xml)");
			RSAParameters output = rsa.ExportParameters (false);
			AssertEquals ("RSA Import Private Export Public (binary)", input, output, false);
		}

		// importing RSA public key and exporting a RSA keypair (including private key!)
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RSAImportPublicExportPrivate ()
		{
			RSAParameters input = GetKey (false);
			rsa.ImportParameters (input);
			string xmlRSA = rsa.ToXmlString (true);
			//rsa.FromXmlString (xmlRSA);
			//RSAParameters output = rsa.ExportParameters (true);
			//AssertEquals ("RSA Import Public Export Private", input, output, true);
		}

		// importing RSA public key and exporting a RSA public key 
		[Test]
		public void RSAImportPublicExportPublic ()
		{
			RSAParameters input = GetKey (false);
			rsa.ImportParameters (input);
			string xmlRSA = rsa.ToXmlString (false);
			rsa.FromXmlString (xmlRSA);
			Assert.AreEqual (xmlPublic, xmlRSA, "RSA Import Public Export Public (xml)");
			RSAParameters output = rsa.ExportParameters (false);
			AssertEquals ("RSA Import Public Export Public (binary)", input, output, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlString_Null ()
		{
			rsa.FromXmlString (null);
		}

		[Test]
		public void FromXmlString_BadCase ()
		{
			string xml = "<rsakEYvALUE><Modulus>u/gvCQaCzpwjOKwrnahx9zaNB+7UEEOkQNa28HRU9R+437qvA1wCq2HqSM7rb81Idu1SDWDh7EYZcZ2KW4uAf6+44KPfxzdyPua0t9k6JYTuamSdBglTdIg0skVFmDlO4KqxLXthpR9SeppB9sFof+JTcpjKKo9ZRvjl/Qkdvcs=</Modulus><Exponent>EQ==</Exponent></rsakEYvALUE>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "BadCase");
		}

		[Test]
		public void FromXmlString_BadTop ()
		{
			string xml = "<MonoKeyValue><Modulus>u/gvCQaCzpwjOKwrnahx9zaNB+7UEEOkQNa28HRU9R+437qvA1wCq2HqSM7rb81Idu1SDWDh7EYZcZ2KW4uAf6+44KPfxzdyPua0t9k6JYTuamSdBglTdIg0skVFmDlO4KqxLXthpR9SeppB9sFof+JTcpjKKo9ZRvjl/Qkdvcs=</Modulus><Exponent>EQ==</Exponent></MonoKeyValue>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "BadTop");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void FromXmlString_BadItemCase ()
		{
			string xml = "<RSAKeyValue><mODULUS>u/gvCQaCzpwjOKwrnahx9zaNB+7UEEOkQNa28HRU9R+437qvA1wCq2HqSM7rb81Idu1SDWDh7EYZcZ2KW4uAf6+44KPfxzdyPua0t9k6JYTuamSdBglTdIg0skVFmDlO4KqxLXthpR9SeppB9sFof+JTcpjKKo9ZRvjl/Qkdvcs=</mODULUS><eXPONENT>EQ==</eXPONENT></RSAKeyValue>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "BadItemCase");
		}

		[Test]
		public void FromXmlString_InvalidTop ()
		{
			string xml = "<a><Modulus>u/gvCQaCzpwjOKwrnahx9zaNB+7UEEOkQNa28HRU9R+437qvA1wCq2HqSM7rb81Idu1SDWDh7EYZcZ2KW4uAf6+44KPfxzdyPua0t9k6JYTuamSdBglTdIg0skVFmDlO4KqxLXthpR9SeppB9sFof+JTcpjKKo9ZRvjl/Qkdvcs=</Modulus><Exponent>EQ==</Exponent></a>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "InvalidTop");
		}

		[Test]
		public void FromXmlString_Embedded ()
		{
			// from bug #355464
			string xml = "<SigningKey version=\"1.0\">" + xmlPublic + "</SigningKey>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "Embedded");
		}

		[Test]
		public void FromXmlString_EmbeddedTwoLevelWithExtraElement ()
		{
			string xml = "<b><u>" + xmlPublic + "</u></b><i>why not ?</i>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "Deep");
		}

		[Test]
		public void FromXmlString_TwoKeys ()
		{
			RSA second = RSA.Create ();
			string xml = "<two>" + xmlPublic + second.ToXmlString (false) + "</two>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "TwoKeys");
		}

		[Test]
		public void FromXmlString_InvalidXml ()
		{
			string xml = "<open>" + xmlPublic + "</close>";
			rsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, rsa.ToXmlString (false), "Embedded");
		}
	}
}
