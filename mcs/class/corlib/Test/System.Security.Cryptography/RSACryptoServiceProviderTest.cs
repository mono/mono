//
// RSACryptoServiceProviderTest.cs, NUnit Test Cases for RSACryptoServiceProvider
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004,2006,2011 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class RSACryptoServiceProviderTest {

	protected RSACryptoServiceProvider rsa;
	protected RSACryptoServiceProvider disposed;
	private string sha1OID;

	static int minKeySize = 384;

	private bool machineKeyStore;

	public RSACryptoServiceProviderTest () 
	{
		sha1OID = CryptoConfig.MapNameToOID ("SHA1");
		disposed = new RSACryptoServiceProvider (minKeySize);
		disposed.FromXmlString ("<RSAKeyValue><Modulus>vtXAf62+o50prNCTiVGTMzdhm4sMjK0QVDkKQLFGu2fJQCULt9NZBab14PiWfG1t</Modulus><Exponent>AQAB</Exponent><P>5y2AHOzIhTChIFzLsgZQAGfy3U8OPwFh</P><Q>01NUVJJv+hhIsnbFiSi24FLRrfr/qYuN</Q><DP>HKLAOdUCyazKaK3V9Yleo448wTkntJpB</DP><DQ>AH5MTxo8arAN02TVlzliG+n1lVtlp2at</DQ><InverseQ>ZpgJwTxSYpT81sQCuVUvX0AYrvSziNIw</InverseQ><D>CStiJYBmsZvincAj5qw5w3M8yGmE/9ls4yv7wenozzC4kZshpI2MuON0d2Z8f4aB</D></RSAKeyValue>");
		disposed.Clear ();
	}

	[SetUp]
	public void Setup ()
	{
		machineKeyStore = RSACryptoServiceProvider.UseMachineKeyStore;
	}

	[TearDown]
	public void TearDown ()
	{
		RSACryptoServiceProvider.UseMachineKeyStore = machineKeyStore;
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// may also help for RSA descendants
	public void AssertEquals (string message, RSAParameters expectedKey, RSAParameters actualKey, bool checkPrivateKey) 
	{
		if (checkPrivateKey) {
			Assert.AreEqual (expectedKey.P, actualKey.P, message + " P");
			Assert.AreEqual (expectedKey.Q, actualKey.Q, message + " Q");
			Assert.AreEqual (expectedKey.D, actualKey.D, message + " D");
			Assert.AreEqual (expectedKey.DP, actualKey.DP, message + " DP");
			Assert.AreEqual (expectedKey.DQ, actualKey.DQ, message + " DQ");
			Assert.AreEqual (expectedKey.InverseQ, actualKey.InverseQ, message + " InverseQ");
		}
		Assert.AreEqual (expectedKey.Modulus, actualKey.Modulus, message + " Modulus");
		Assert.AreEqual (expectedKey.Exponent, actualKey.Exponent, message + " Exponent");
	}

	[Test]
	public void ConstructorEmpty () 
	{
		// under Mono:: a new key pair isn't generated
		rsa = new RSACryptoServiceProvider ();
		// test default key size
		Assert.AreEqual (1024, rsa.KeySize, "ConstructorEmpty");
		Assert.IsFalse (rsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (rsa.PublicOnly, "PublicOnly");
	}

	[Test]
	public void ConstructorKeySize () 
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		// test default key size
		Assert.AreEqual (minKeySize, rsa.KeySize, "ConstructorKeySize");
		Assert.IsFalse (rsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (rsa.PublicOnly, "PublicOnly");
	}

	[Test]
	public void ConstructorCspParameters ()
	{
		CspParameters csp = new CspParameters (1, null, "Mono1024");
		// under MS a new keypair will only be generated the first time
		rsa = new RSACryptoServiceProvider (csp);
		// test default key size
		Assert.AreEqual (1024, rsa.KeySize, "ConstructorCspParameters");
		Assert.IsTrue (rsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (rsa.PublicOnly, "PublicOnly");
	}

	[Test]
	public void ConstructorKeySizeCspParameters ()
	{
		int keySize = 512;
		CspParameters csp = new CspParameters (1, null, "Mono512");
		rsa = new RSACryptoServiceProvider (keySize, csp);
		Assert.AreEqual (keySize, rsa.KeySize, "ConstructorCspParameters");
		Assert.IsTrue (rsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (rsa.PublicOnly, "PublicOnly");
	}

	[Test]
	[Ignore ("Much too long (with MS as Mono doesn't generates the keypair unless it need it)")]
	public void KeyGeneration () 
	{
		// Test every valid key size
		KeySizes LegalKeySize = rsa.LegalKeySizes [0];
		for (int i = LegalKeySize.MinSize; i <= LegalKeySize.MaxSize; i += LegalKeySize.SkipSize) {
			rsa = new RSACryptoServiceProvider (i);
			Assert.AreEqual (i, rsa.KeySize, "KeySize");
		}
	}

	[Test]
	public void LimitedKeyGeneration () 
	{
		// Test smallest valid key size
		rsa = new RSACryptoServiceProvider (384);	// MS generates keypair here
		byte[] hash = new byte [20];
		rsa.SignData (hash, SHA1.Create ());		// mono generates keypair here
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void TooSmallKeyPair () 
	{
		rsa = new RSACryptoServiceProvider (256);
		// in 2.0 MS delay the creation of the key pair until it is required
		// (same trick that Mono almost always used ;-) but they also delay
		// the parameter validation (what Mono didn't). So here we must "get"
		// the key (export) to trigger the exception
		rsa.ToXmlString (true);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void TooBigKeyPair () 
	{
		rsa = new RSACryptoServiceProvider (32768);
		// in 2.0 MS delay the creation of the key pair until it is required
		// (same trick that Mono almost always used ;-) but they also delay
		// the parameter validation (what Mono didn't). So here we must "get"
		// the key (export) to trigger the exception
		rsa.ToXmlString (true);
	}

	[Test]
	public void Properties () 
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		Assert.AreEqual (1, rsa.LegalKeySizes.Length, "LegalKeySize");
		Assert.AreEqual (minKeySize, rsa.LegalKeySizes [0].MinSize, "LegalKeySize.MinSize");
		Assert.AreEqual (16384, rsa.LegalKeySizes [0].MaxSize, "LegalKeySize.MaxSize");
		Assert.AreEqual (8, rsa.LegalKeySizes [0].SkipSize, "LegalKeySize.SkipSize");
		Assert.AreEqual ("RSA-PKCS1-KeyEx", rsa.KeyExchangeAlgorithm, "KeyExchangeAlgorithm");
		Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#rsa-sha1", rsa.SignatureAlgorithm, "SignatureAlgorithm");
		rsa.Clear ();
		Assert.AreEqual (1, rsa.LegalKeySizes.Length, "LegalKeySize(disposed)");
		Assert.AreEqual (minKeySize, rsa.LegalKeySizes [0].MinSize, "LegalKeySize.MinSize(disposed)");
		Assert.AreEqual (16384, rsa.LegalKeySizes [0].MaxSize, "LegalKeySize.MaxSize(disposed)");
		Assert.AreEqual (8, rsa.LegalKeySizes [0].SkipSize, "LegalKeySize.SkipSize(disposed)");
		Assert.AreEqual ("RSA-PKCS1-KeyEx", rsa.KeyExchangeAlgorithm, "KeyExchangeAlgorithm(disposed)");
		Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#rsa-sha1", rsa.SignatureAlgorithm, "SignatureAlgorithm(disposed)");
	}

	[Test]
	// LAMESPEC/BUG: Disposed object can still be used (but original keypair seems lost)
	[ExpectedException (typeof (ObjectDisposedException))]	// in MS.NET v.1.1
	public void DecryptDisposed () 
	{
		byte[] encdata = { 0x4C, 0xBF, 0xFD, 0xD9, 0xAD, 0xDB, 0x65, 0x15, 0xB3, 0xE8, 0xE6, 0xD3, 0x22, 0x99, 0x69, 0x56, 0xD3, 0x1F, 0x1D, 0x2A, 0x66, 0x07, 0x00, 0xBB, 0x77, 0x47, 0xB6, 0x6F, 0x8E, 0x3A, 0xBA, 0x37, 0xA3, 0x0F, 0x0A, 0xC8, 0x8D, 0x1F, 0x8D, 0xAB, 0xAC, 0xFD, 0x82, 0x6F, 0x7F, 0x88, 0x3B, 0xA1, 0x0F, 0x9B, 0x4B, 0x8A, 0x27, 0x3B, 0xEC, 0xFF, 0x69, 0x20, 0x57, 0x64, 0xE1, 0xD8, 0x9E, 0x96, 0x7A, 0x53, 0x6A, 0x80, 0x63, 0xB0, 0xEE, 0x84, 0xA7, 0x67, 0x38, 0xA5, 0x30, 0x06, 0xA8, 0xBB, 0x16, 0x77, 0x49, 0x67, 0x0F, 0x90, 0x67, 0xD5, 0xC5, 0x12, 0x92, 0x5A, 0xDA, 0xC3, 0xFD, 0xC4, 0x8A, 0x89, 0x77, 0x79, 0x11, 0xEC, 0x95, 0xF6, 0x6A, 0x3B, 0xAD, 0xA8, 0xDF, 0xA1, 0xB0, 0x51, 0x34, 0xE8, 0xC1, 0x05, 0xB9, 0x09, 0x23, 0x33, 0x2A, 0x3E, 0xE7, 0x6A, 0x77, 0x6F, 0xBD, 0x21 };
		byte[] data = disposed.Decrypt (encdata, false);
		int total = 0;
		for (int i=0; i < data.Length; i++)
			total += data [i];
		Assert.AreEqual (0, total, "DecryptDisposed");
	}

	[Test]
	//[ExpectedException (typeof (ObjectDisposedException))]
	// LAMESPEC/BUG: Disposed object can still be used (but original keypair seems lost)
	public void EncryptDisposed () 
	{
		byte[] data = new byte [20];
		try {
			byte[] encdata = disposed.Encrypt (data, false);
		}
		catch (ObjectDisposedException) {
			// on Mono we, indirectly, throw an ObjectDisposedException 
			// because we're calling other public methods to do the job
		}
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void SignDataDisposed () 
	{
		byte[] data = new byte [20];
		disposed.SignData (data, MD5.Create ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void SignDataByteArrayNull () 
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		byte[] data = null; // do not confuse compiler
		rsa.SignData (data, MD5.Create ());
	}

	[Test]
	//[ExpectedException (typeof (ArgumentNullException))]
	[ExpectedException (typeof (NullReferenceException))]
	public void SignDataStreamNull () 
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		Stream data = null; // do not confuse compiler
		rsa.SignData (data, MD5.Create ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void SignDataWithInvalidOid ()
	{
		byte[] data = new byte [5];
		rsa = new RSACryptoServiceProvider (minKeySize);

		rsa.SignData (data, "1.2.3");
	}

	[Test]
	public void SignDataWithOid ()
	{
		string oid  = CryptoConfig.MapNameToOID ("SHA256");
		byte[] data = new byte [5];
		rsa = new RSACryptoServiceProvider (minKeySize);

		rsa.SignData (data, oid);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void SignHashNullValue () 
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.SignHash (null, "1.3.14.3.2.26"); // SHA-1
	}

	[Test]
	public void SignHashNullOID ()
	{
		byte [] hash = new byte [20];
		rsa = new RSACryptoServiceProvider (minKeySize);
		byte[] signature = rsa.SignHash (hash, null);
		Assert.IsTrue (rsa.VerifyHash (hash, "1.3.14.3.2.26", signature), "Null OID == SHA1");
		Assert.IsTrue (rsa.VerifyHash (hash, null, signature), "Null OID");
	}

	void SignHash (string name, int size)
	{
		string oid = CryptoConfig.MapNameToOID (name);
		byte [] hash = new byte [size];
		rsa = new RSACryptoServiceProvider (1024);
		byte [] signature = rsa.SignHash (hash, oid);
		Assert.IsTrue (rsa.VerifyHash (hash, oid, signature), name);
		Assert.IsTrue (rsa.VerifyHash (hash, oid, signature), "OID");
	}

	[Test]
	public void SignHashMD5 ()
	{
		SignHash ("MD5", 16);
	}

	[Test]
	public void SignHashSHA1 ()
	{
		SignHash ("SHA1", 20);
	}

	[Test]
	public void SignHashSHA256 ()
	{
		SignHash ("SHA256", 32);
	}

	[Test]
	public void SignHashSHA384 ()
	{
		SignHash ("SHA384", 48);
	}

	[Test]
	public void SignHashSHA512 ()
	{
		SignHash ("SHA512", 64);
	}
	
	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void SignHashRIPEMD160 ()
	{
		string oid = CryptoConfig.MapNameToOID ("RIPEMD160");
		Assert.IsNotNull (oid);
		byte [] hash = new byte [20];
		rsa = new RSACryptoServiceProvider (minKeySize);
		// OID not supported
		rsa.SignHash (hash, oid);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void SignHashDisposed () 
	{
		byte[] hash = new byte [20];
		disposed.SignHash (hash, "1.3.14.3.2.26"); // SHA-1
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void SignHashInvalidLength () 
	{
		byte[] hash = new byte [19];
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.SignHash (hash, "1.3.14.3.2.26"); // SHA-1
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void VerifyDataDisposed () 
	{
		byte[] data = new byte [20];
		byte[] sign = new byte [(minKeySize << 3) - 1];
		disposed.VerifyData (data, SHA1.Create(), sign);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void VerifyDataNullData () 
	{
		byte[] sign = new byte [(minKeySize << 3)];
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.VerifyData (null, SHA1.Create(), sign);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void VerifyDataNullSignature () 
	{
		byte[] data = new byte [20];
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.VerifyData (data, SHA1.Create(), null);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void VerifyHashDisposed () 
	{
		byte[] hash = new byte [20];
		byte[] sign = new byte [(minKeySize << 3) - 1];
		disposed.VerifyHash (hash, "1.3.14.3.2.26", sign);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void VerifyHashInvalidHashLength () 
	{
		byte[] hash = new byte [19];
		byte[] sign = new byte [(minKeySize << 3)];
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.VerifyHash (hash, "1.3.14.3.2.26", sign);
		// note: invalid signature length doesn't throw an exception (but returns false)
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void VerifyHashNullHash () 
	{
		byte[] sign = new byte [(minKeySize << 3)];
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.VerifyHash (null, "1.3.14.3.2.26", sign);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void VerifyHashNullSignature () 
	{
		byte[] hash = new byte [20];
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.VerifyHash (hash, "1.3.14.3.2.26", null);
	}

#if !MOBILE
	[Test]
	[Category ("NotWorking")]
	public void ImportDisposed ()
	{
		RSACryptoServiceProvider import = new RSACryptoServiceProvider (minKeySize);
		import.Clear ();
		import.ImportParameters (AllTests.GetRsaKey (false));
		// no exception from Fx 2.0 +
	}
#else
	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void ImportDisposed () 
	{
		disposed.ImportParameters (AllTests.GetRsaKey (false));
	}
#endif

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void ExportDisposed () 
	{
		RSAParameters param = disposed.ExportParameters (false);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void RSAImportMissingExponent () 
	{
		RSAParameters input = AllTests.GetRsaKey (false);
		input.Exponent = null;
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.ImportParameters (input);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void RSAImportMissingModulus () 
	{
		RSAParameters input = AllTests.GetRsaKey (false);
		input.Modulus = null;
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.ImportParameters (input);
	}

	// all keypairs generated by CryptoAPI on Windows
	string CapiXml384 = "<RSAKeyValue><Modulus>vWi7cHIntTcrwIoD0zj/fxoJCDfUHtC5xkMe8pJri7G+T6nKs4zRcLDWRDA0cNhf</Modulus><Exponent>AQAB</Exponent><P>5DwRzr4EPAk1NOUSwI/z1yBJzG2EyrhR</P><Q>1HOEBwbXvsrPQvV7C0MWtHJ22UWgwgmv</Q><DP>qQrEtbePM1gujErOJMl59O/5OOw02mDB</DP><DQ>bUFGqXJsavLTaaTidSU4PO4MjqnPBVqD</DQ><InverseQ>Y+XUwMmlI2G63reScAyc9PHvTPX2fwCg</InverseQ><D>o1Ku5cwZf0IegPzBRZ5NeYy6oxJ430V8n4BLZ8G8/N6kRHnXe70OyzwAylPtxjGh</D></RSAKeyValue>";
	string CapiXml448 = "<RSAKeyValue><Modulus>slPik6EfNCiN2MwhFHngEOqqRlSXeBjmKoLyMVgLCPIQJQqe/BMcyA1gQg4mgionngIgXCMuSOU=</Modulus><Exponent>AQAB</Exponent><P>1mUfmQlTK+Q8sLCCWpsngp7gNc6RANI8mP4v0w==</P><Q>1O77OcIvsaCC6KmemMILqXqm8Bzb3Ud8G9bpZw==</Q><DP>h7L/2fRuAUT4KPm/uCumSWXYEhaJ7xQiqM+SYw==</DP><DQ>fcs1Vbj3ritSSxsx27L/ar9P8hhKd55snpHHTw==</DQ><InverseQ>U7txqL9bAt+BAjqqDfQOAVKTfx3wnAcK5dFx0w==</InverseQ><D>UTV8SEimoiUZu7HyGpYJ4QpMsqyRMgL8xj1Nt0JLKBObONObUBEPAPQKknuE98cM9j/2ufRlrx0=</D></RSAKeyValue>";
	string CapiXml512 = "<RSAKeyValue><Modulus>0ci216d7QTTKkh2Zknwbqlp/Eva1IfzzIA6JZKSI4lcYWMWKAadU7cJtJ5JKGe1aeMi3Y1CDWCpr8fUL28QEkQ==</Modulus><Exponent>AQAB</Exponent><P>/LakSOv8LQUmt3LXA9kMQPAWGk1l2QrwGFL3/k+T+Wc=</P><Q>1IMl0X1/dOqGbEL3agnvgBctWcQaqC2cTzTp7zJqv0c=</Q><DP>B2FmJuMNeFsgKFdoRCqAmxzn6Fi+UrppDKzPpVO5pJ0=</DP><DQ>gWMwPXJpjefU3EmRljBib9yssgDiMg9DIj6XSBmsQLU=</DQ><InverseQ>/IggsX0boXE6UJoWppUEqw6VT13WXxb8mAds5ebxFmE=</InverseQ><D>i+23mA0Mt6fA3smDoCPMSEroq/uHQk541Q8dMdZVv7JmKBJKk1PCppwISyv7fWKVmRTsnGuEnxZFl4N9IR/qyQ==</D></RSAKeyValue>";
	string CapiXml576 = "<RSAKeyValue><Modulus>si995MicOWrr2/VlH1rUAhyFMPVLj5PvYHU8P8I8R77kh1ePn66zuJ0jRsMCpenUzYGuVtqgn0VDjiaYjo7cE2PZYESLkKJl</Modulus><Exponent>AQAB</Exponent><P>3wDFrI6eUAh72YxjdZD0b93/a/NEV26pp1c8j5fN3UUzA14r</P><Q>zI0O3NKXCgn3cC3Pe2ZUYEPbQaQDyMdSAiF06L1miwVHu0mv</Q><DP>uQaC/LE4WV8wo0gAHcOvjEG9c2vcAE8pJFcVQG7LuBN6SAkR</DP><DQ>THffOoYvmL0pF3LIweT7XhGAAgYCtChvbAR95BQKJaaPrmBZ</DQ><InverseQ>KTOfvWHp363Cl6r2lXc/gINaRqNo+iWRLeoBa6/vk0Kt5sWn</InverseQ><D>QZ9L9h4Lqvm9s1xEya9htQVb6BPuqAoLdrK2ZaTbHnEnKNEN7oLMr1Ca5zh/E6xXKCmwClWyULeKiYq7vYLUXhaemtT1rN9h</D></RSAKeyValue>";
	string CapiXml640 = "<RSAKeyValue><Modulus>vqS9CNxiRdSc6x3nJOWnPi7gVA4Vsnhd8cTAaMBKuvI0FRhhDsTRsjr8PeH4Y7yMg8XSucr3I8jkYgapBW8s5hmSvdWZhPHRGEe1MiEIeMM=</Modulus><Exponent>AQAB</Exponent><P>7Ojjp/PqeacLRM/MP76wDnp4k+BpUSbitNwkPBAxL5KcBFVwhp+YVw==</P><Q>zgFxY3BFx/ISEciI3ONux4iXd/cIwxrmq9CS3tBIecHTq0JgjXNPdQ==</Q><DP>TstCdGjOsnlZaSCHuSfN3HLlSaGYxZHeUvLo5kUoZr8nPtW/4DaSbw==</DP><DQ>DCIhn425TnF/hvScuwXaPH5bDYHLTlKDS5NZUy5JVoKICQo7zZkBEQ==</DQ><InverseQ>Xhn+9dXsK07s51n6SyRs3wRJEPwuIaNDq5EQJUQNb2Gbdb4HgdpM2w==</InverseQ><D>JhKALeTVO1zaeZnfL18tpx11b1PgwWOIX2ALjN/aDLuR6ySTPX/Q4I6zBOzVa/KB6VMNNk4abIKUvOp6MBNYIMRMK8veaIJxYrn+JAOShwE=</D></RSAKeyValue>";
	string CapiXml704 = "<RSAKeyValue><Modulus>wbclY53TAIx2E6uPkYeYNVFPYnHgLDJGO1ZoSQcUm6ccySsJvVWAP9TWLIOTjJh2zaQn5gore87ONk50L/+FMZ5uaNrbxxPCxORBLzBofFaEbFxEfu9g6w==</Modulus><Exponent>AQAB</Exponent><P>80o27emwkBtzYHg/0yoYjwjcgqhtVSktbZ8IZ+tdBEbt7gtnpyDd3rpTjc8=</P><Q>y9XqvMruAcvSJjV4rHnCX5Na4XahLhbrvuSsATirpSV+YaD8LrTd5VHwfiU=</Q><DP>aYoSVhcAPyOJY5oGpgYm7TW84vlJpJ3eVSWeMeaKTWavpIpSBtBdL1fJE0U=</DP><DQ>DDbCPRPP1S24Zu+3TmZxXja/MFliaReYgrTDFcOmjVvEyebSlZ3i2fXh+j0=</DQ><InverseQ>O9RaZjZGqG0stVjpDDUaHGk9JNbFChHL9KQ3Kyb6qBLmDNvwobxPnHY1+k4=</InverseQ><D>Wfgbz2Zvp2OVO5GEvoyBbtHy0sAc46n94HVDPhehxKEax3vLrDnXtM2/IzCxXibWqBBmF8tw156Yo4Av2/EpGxvolidOpTXVxNX4aCb3dy05VQ/6WEem0Q==</D></RSAKeyValue>";
	string CapiXml768 = "<RSAKeyValue><Modulus>3VQEd4kKl0vQG/OzLDTVcgVqaBG9KAz6NzJOu5a6mlHOYSIG5ybYwOJJpXDzZuXewBChqQNpmxgGAae/SuTy+EDQNzAQ+mzeYrEaD6C8vqwMYVanRcbNYtDC9cxAH6Ef</Modulus><Exponent>AQAB</Exponent><P>/UKWrRmudV9RYnelAK2QqJ0DSE/oKHgVtHQ2AxyJueDnMelyzVrCv7ICnL1ymogp</P><Q>37j+EpSqJRP6JRyGMDXG2mj5UXnEqgqeYXaHHNHNYsFcZWAKitRt8uUQtLz9k6gH</Q><DP>fLkVYUwbeZwmhVqsvOe9LPyeSDdp+rwg3Ey66M9eGFdlJFR2gCFpdWRiGvTFgsr5</DP><DQ>1PuxbZD4NdpB3si7+vOHPvCGwgkRr+vyAcksMcSOKgD47E21W5uGnKFz+Qyev85L</DQ><InverseQ>dnez9inW0taRSo0QzoQ3CNGTfm9qzh9Mlx6GKjnyzLLBJ87cOfzW2qVksBlmkDJx</InverseQ><D>RK5ayIFFQRfsl4/zTMeEaOKXV34Rtcj5KIG6/ulSNKsoIOo/PCHI52oRMn6veYHhHrKclBeqtAlqOJG0xrEy/ZjorBJnCB/ZSoPPuS4h7budjF2zYiBHJkRwZcxhe+nx</D></RSAKeyValue>";
	string CapiXml832 = "<RSAKeyValue><Modulus>wv35yrTrD3gjLxJEJ6HMOJsUfshcCZKGLwlxZO42TlAHK90vJPT0Q7SekeHqSVi1DJ7AAB7XW7MqoSE4SKVVioQQZSFz/HkA1fklTOaubfwRwv0vy/A3js1xPhwTs/SbDpNlXMoiEAM=</Modulus><Exponent>AQAB</Exponent><P>87j8swLApJhpXG4kXLIN86xkdbh/ccqGlpeVE1EGC0yL9OJhbkKC87nvhmQZKMRqcreaAw==</P><Q>zNCRE+qsVtVPD5o8Kf50HFdTQlZY56IA3DLRLHd0qkpNkrCP3gPJNNhwt5kqewX1dlzSAQ==</Q><DP>VXli5kTo2tC44rmd9wRa8EJdWQvDZlzoppeyqHuZ6wyFaPSbxtd8pY1n+3HPgQShcGoDkQ==</DP><DQ>QK6JcqnFHXMmEb5ay8RRNPYbFDMixpwx+1iNGKbtEC6BCxd5h9rYOzkzd7gGY23BVE0CAQ==</DQ><InverseQ>FMqDOpFyGkiJ74uTwdqFABR9dBcN4x8rpYw5trf5DBOVC1Desh+mRDAZwKPVDLCBpqrg9A==</InverseQ><D>YuJPUbfr2J1xIkbeH0cS/MXQX/cVjZkrySC9y5RuH8q/yEPTy6cZVFh9bbemi2mb7vl2nfWO+VXLN+3OStN0SU3/2XQXfhZm4dqk5iU+R5n5g9kjPBNLH8DIWYjGVgCvBqH59DHhLAE=</D></RSAKeyValue>";
	string CapiXml896 = "<RSAKeyValue><Modulus>nNiWWLAg2htBUkoBD7DTDIALapuiIt45qHJdPVbRQE5PXTcJUJ2SAEHCtQYtIeQam0WL2D78PIfAf8hGFx/72Ghn9b+NcZ8b7UHTdna+5O6aBOHyaD80FOmyfEjAjI1jS95XnGlqoD0QaDpkk4xnMQ==</Modulus><Exponent>AQAB</Exponent><P>zgkddb3AAG9p8LeF+TbcJ2Q3xvvODITJRmUe55IDOb/BhRF1/JTuCAeuHiOtVWKuxcmsJp464Bc=</P><Q>wuG8338Tyk658L7cByQVoV5/AJx+5RKN6rO0CGrPSGjOkIPDyNzwgpmotelo+NiTEiEiNoa49/c=</Q><DP>Dgl+8VOhLiZpEFZgkU8UhraEOlFTg3TUhbBD/8Dp6VhQJfG/mRrIcNGdIj6KA6Q6hg0sZmEnX7c=</DP><DQ>MuxTsz78h9+8fKkSy5blRA5yN1GtYuRPSyX8BDsMwQoJ9/9GWKVK/4VxbV95e5T0EUexLfhUOw8=</DQ><InverseQ>NbWl5XrJ6hxvE8gpRcj2oc+hAUT96ej6eysD1JTQyV1eKzc9+Mpaoo22tGSrR52s9EZRqHWmKxM=</InverseQ><D>LGd9GRq0EkuJELz20/Rhq7ZMhSAOpQR5GmFWWFlN4IDLIz7DmlkhzoTPlORsvp2Pksn7r3sVeiUbL3S1rXfIpwEjeseHfBYR54d6EqmPABqPj9UoADXDoYA1VYz+Ni2uFYlMC7Wh4kzlT0zYRc1XKQ==</D></RSAKeyValue>";
	string CapiXml960 = "<RSAKeyValue><Modulus>3nb1VtiEuSii1rnDR+yrarcAHDlvbgLUTJGUvuVEMcvQ3vYNNabwBsM46X8ab78Ac2ZBk2jI2S4736KL2S8C2e8rXva7Jb/j+fyVt6N+NrI3dkNE2NoWZ8VM+rUEdKNvPANbf6n7hvDVfJLCvdCctdtEYACkujsb</Modulus><Exponent>AQAB</Exponent><P>/nWeSPagl8TOHzSKB+CRx4MbxOlIR9RNw3RAbBZq/1r4YZtTvvSutV6e1/2yIWYblHPwiALb/FyB4XUj</P><Q>38/Agt+WwnMNeyv4ngVk0r6ffValkVCMlbPdueEqEXCFqOguTvnneINg7+T1Q1YLPXDtjR6NFGKCLG2p</Q><DP>Kebkcc9rEpLt/mWAdVudpeUJJZvksy9avtzd3u6yH+qzDB+v4roYKvWx4o98TdOqpv+QlFUkNKJnIOFR</DP><DQ>SPH9XKpjCJ1XF34NWfOIGOfoM4G6FNKb/27QJXUtsOFrrtF9xl/NAYpQXd/R0FCK+UuFISmD8dDpfHGB</DQ><InverseQ>9YlJCPfSUw8ZEauBdt9hsAybMD20we2uoNdHdDRKvdTtK8m6spS4ohG2ovzfRkiKSxQVzB3jQoYcK9L6</InverseQ><D>jrmHQYZ78Ebv4g8gCD8A4uAxg+odYVkTV2R3J9nzXHdEtCbr5qYJjG0nUDapgVPrOB48qBQr95o/84RWPdIz3I+Jzpme1c4FpWs1/iXAbA/+xnzDGtXVTnJrMfkOsXt2MYBF7+f7zWGNlOGs+zkt/fP/jeOu76Nh</D></RSAKeyValue>";
	string CapiXml1024 = "<RSAKeyValue><Modulus>uNXUqvMdjnmd36IS0Gr/lTAp63mZqoayYcN/YHnGs82eVrEWS5vAYqPWx1488BhUBh7kHvVhP5kkKh+i6pfLKAEssy5jnw9h09lgCClBHyntqFT82AlmiaFK8uVkV8rCjDol2nBpvVgNonF+JhQyexWTEI94Phjwvj/6yZRqaLM=</Modulus><Exponent>AQAB</Exponent><P>3GIqx8V+yrPI8jsFd1NYTNxJjodtLNufqmYbn7q6fIaPEdC0QG0fcfYPREfKka8xpZ4K7mqgLjm0ivDWi9Y5Iw==</P><Q>1rTxeoBb9aQcoGLhUEeZQNhY+JhhrGsMtZH4fPxy/30JUtggQmZRR12XdjBZ0KcKmsOMbxCauYD8Y6ozFhKzMQ==</Q><DP>lBMZb3TRRl0aDTd+6rgDQlFY0v1Ha7Z9Rz6oHOCX4IeApZW3JvqrACU2CMi74Lr3/rF74smdqrF3D0vWu8pKRQ==</DP><DQ>SBWZ2UoNFcySe9qW0PAo6Nd6D4SBjnSmYLNwXO4Y4eQl5DWBpylY8n/eoSSckuvyKIGsvYEyoUNH+WIkIq4GkQ==</DQ><InverseQ>M3KkaBwusTLiKBlu048fx1WyRfLLEa7clsU7a4JN5wZ66h7wElKj77r9BCXz5UZFo4j3AiBBC/701S0mVQYXOA==</InverseQ><D>NVIqaa58xk87RfphZxKW7JjaXv3TYKg+6YkWQ+Sdd91HYkbv4Zvq4gnVuenrtm+uPZ3HvU6YYVpyXlyGCRsFFeXMwJ8Y7zrF6KEs+6hJ4jqBELWUoEPrzU4889v8aIoS+BCMYptpkmzvxjfEWdTPbqjd/tJ25ObbavggoST6ScE=</D></RSAKeyValue>";
	string CapiXml1536 = "<RSAKeyValue><Modulus>1bkuWuGsKLkvSvrYYhOPVD37bARRUw0U5/yRCjam50+gLGKmxAjzaRjhGQUaaeC2S5DioGckqiqr1zkfSLyQ7KQte5oynxkTdT0QCXHJeVWgOWAknX8sNrAVNt97LOrMXXYZ2JsYMmYiVNqJRTaURWXxdJ5XibXMCRL1pfHbwnL4iqUge1tuiQbFod//7pjl90TukIutHVEyf8BFn5LRQJI4xroQdMJg0/vvRSp8jgy0LoeJ8LkTG+RjUe3XsgmT</Modulus><Exponent>AQAB</Exponent><P>8Vz3+1ixDfMPPNENDw17Rmdt8IRM32fRBJ/TX8LYfVkxlCB6m25Pd4SxH7mcdF1sgD3qgC+uKekJZb69Q0XE/CoJxPD6mYq0o4XEsz0rwy4NU/u//YpxZp84v5cDKEoR</P><Q>4q8eodQoHbiuBH5f/WelCnm2Ym22AmFIEIWpwKGY8MlnvpK/pN+N4iXrkeNv7OVofOdE7g0lw5cTbDMZV2eMyEIYKUi7YOXA8TIlSdTuDqW/5mPg0inqNLEg2PZGahVj</Q><DP>lHw7Wve/RPOpBiMdw4rpsfBjZDogCLiXkB67LQhzovnCVHx+sSx12vNY/El2BOiMnYB5yY6LuODSlTN4v/AmNXOvOud9ZAQ/CPJ8hkA1sgecz3PrMxF+nkGJ6eP/X0Ph</DP><DQ>p2cHyh6xGXHfIPZq0OqPmTLVG89FkHBjFcB/4f/0wC0cbkJVQN7PGulCFFTPvTSVe1gXMW2IK+8PquH5nvCbqPAWg7ZwmlhRqk2L+ABFZY/GLdAooUvO5+a/CTqmOYVd</DQ><InverseQ>x3EJeS7EP1h5zuKKITblv0V9R3z6KEQAbKPp3zfckHX1fvuR+3VP4FLU63D0yxd2BOjhGTvyJvjrVSDCP+Ndk+h7ZT9MkzW/i6ifiHG/5L/xaEpxjJGq35OOOuHqCmKc</InverseQ><D>rtTUab3QImQSnuiCmABeMFCf2qXRjPnXj0qZr1wzvmbxpT1yJE0aKXATu27kQ5ZyKXC1IvgdEyLi/aWZxNuURjCrkD/8hw9xTmeMNd2iLaJw9l6CtV/x4C68u+2nCoBq/Gv/ht9RqYRS6ODUTk1aOL1mPNSHT/MeFNK+06lyQNSaf95fMxGPIcnnJ1vtdkHgk2vi5ALSQf5tGIgVY1EFr55M9ABTg1p98XwIQgjIxCZcL4BygfMk1YbESaQFvzth</D></RSAKeyValue>";
	string CapiXml2048 = "<RSAKeyValue><Modulus>9TFEYeTahPGOXtcRZrLQu1gHm875jv1jgxy0R9V3E/vDyEhNbsRK76rvEs3ljw6spQBQE1zd5wfUvFnhVETOuS0z9aXSAljxSIbyhkL/jT7/4vHEHe/GnBQZPdvJ9PtPr0ssLROy8xcsUEBDwxNRPNHSpmub3FHZzHsoMlJV0cOGmn0qqeMuMrP8zVDycXhXZNsn8v5RPC38qk6VDHdCqGz/90IRYonpFr5my9LaguJHVNCq8BRVa1oJkDtNUFhFXjVHnmxmhS9Mqm2obEpTp/jUk+bRqnygKB3up9OGYdLO1rAv2IoH/K2sxcvnedDzieRmPE/9Y9JBP/UvM+VULw==</Modulus><Exponent>AQAB</Exponent><P>//j7DyPW0E5t1/xvT9iojXQZfu9t/t0bdJMkRQVAYzJL5+UUILOA+70hMtPIvrU4gdeK0CPKfbbkY0UmD4QTb9nFq4aOsHruMZgWIeqHLlEo+VOYX3+hLv+p/aXX+Wj5PvQWLYZBH1sdOLE21nEVHFXZIVev3PcIYU+P0L8R3JE=</P><Q>9Tf9pWu4aWF16h2/nCmY8HAOPNMgcHoC6jtVuJJX+8pio6VLbA862e6ptB7fZdCDDXB2y/52tmG6uAcmOIqUDZB4eLHcOWxnYctcWrvjODMIABno7EuhR7zemelN89ME97zuY7nQSnVRroWD7l3oU5pN3yIe9Y5bnMKgr1nnhL8=</Q><DP>e4AhgZiFGFP+42rEOf4KtNUDSB81LvZ3PLORmEuEWVf3D5eTMoPpA4yo6+EKxhihfuQD8ZCTLjyDzPGb/3h1+E1V9gAh8DwfmIYMh6ikOFCoOEOBDPKDTi3EUsElhwyC1UDnQme4G+zWGHhIQQambNluvYuVKkN2I51Dgi/t6kE=</DP><DQ>ZIzd9Au0pXlyOVqTbDxeWxEHtYc5AQX21gcYgkN30mZNhh7MS3X/QserTJFwNzaF1mfsPn+MPALc5oL/+CVSyjEYRR1hWSaLSb1ylD4A0NWUDT6SlPn6GwlmGaRh833uxorxEXFq6G0s3iwfSgm1rzpRfhJmsXf7Ns9TxjNOTM8=</DQ><InverseQ>uGCs/TRjtVGBXsTV/9fPMMel0L+WbrdqIPu1GBSIiXk8tWWi+P1lACSTf0iCJ6SIKB26N57zAE9r7zaocVX0R/3NJ3+eDDztV9i6AOsK6X/AitxXetPQDIflbbjAEzeZUUJf2onSAITIw6TaAs620a8HpPw3W7Yd0Oe/lt2M+GA=</InverseQ><D>ao8oyuqs1U4ts6YAWAOql2DgnaRL7QrObrLQ3s802yh1o9tYW6VPc+1zzVZSR+P2wBbsth2MCtXqbJkbRoZI2U194WpZZM/GvOB5EkSXz4jrqHOt6dzFEhviBHI6yQ9XSDWBU23WPbN6fL4RNPx2N9pwlAb8S7n+z9FOXOBPj8NyMMtykGqgPyHEAQnuhfLuf/J6OGrHX8dzsZAo6o/bnk49Rv0C6nijE74TeAwJ36UuI9Lw+h6hwtSerEN6o1vy6mIjrjdDM07mKKUMFwkEhKJc6ckBb0xVp/3VRlJb1ocYQHbfjCAPjecR8OVfjaIy9WPMj8WXHegJpiBugWK9wQ==</D></RSAKeyValue>";
	// limit to 2048 bits (else key won't fit in a line for Visual Studio)

	// same keypairs but without CRT
	string CapiXml384woCRT = "<RSAKeyValue><Modulus>vWi7cHIntTcrwIoD0zj/fxoJCDfUHtC5xkMe8pJri7G+T6nKs4zRcLDWRDA0cNhf</Modulus><Exponent>AQAB</Exponent><D>o1Ku5cwZf0IegPzBRZ5NeYy6oxJ430V8n4BLZ8G8/N6kRHnXe70OyzwAylPtxjGh</D></RSAKeyValue>";
	string CapiXml448woCRT = "<RSAKeyValue><Modulus>slPik6EfNCiN2MwhFHngEOqqRlSXeBjmKoLyMVgLCPIQJQqe/BMcyA1gQg4mgionngIgXCMuSOU=</Modulus><Exponent>AQAB</Exponent><D>UTV8SEimoiUZu7HyGpYJ4QpMsqyRMgL8xj1Nt0JLKBObONObUBEPAPQKknuE98cM9j/2ufRlrx0=</D></RSAKeyValue>";
	string CapiXml512woCRT = "<RSAKeyValue><Modulus>0ci216d7QTTKkh2Zknwbqlp/Eva1IfzzIA6JZKSI4lcYWMWKAadU7cJtJ5JKGe1aeMi3Y1CDWCpr8fUL28QEkQ==</Modulus><Exponent>AQAB</Exponent><D>i+23mA0Mt6fA3smDoCPMSEroq/uHQk541Q8dMdZVv7JmKBJKk1PCppwISyv7fWKVmRTsnGuEnxZFl4N9IR/qyQ==</D></RSAKeyValue>";
	string CapiXml576woCRT = "<RSAKeyValue><Modulus>si995MicOWrr2/VlH1rUAhyFMPVLj5PvYHU8P8I8R77kh1ePn66zuJ0jRsMCpenUzYGuVtqgn0VDjiaYjo7cE2PZYESLkKJl</Modulus><Exponent>AQAB</Exponent><D>QZ9L9h4Lqvm9s1xEya9htQVb6BPuqAoLdrK2ZaTbHnEnKNEN7oLMr1Ca5zh/E6xXKCmwClWyULeKiYq7vYLUXhaemtT1rN9h</D></RSAKeyValue>";
	string CapiXml640woCRT = "<RSAKeyValue><Modulus>vqS9CNxiRdSc6x3nJOWnPi7gVA4Vsnhd8cTAaMBKuvI0FRhhDsTRsjr8PeH4Y7yMg8XSucr3I8jkYgapBW8s5hmSvdWZhPHRGEe1MiEIeMM=</Modulus><Exponent>AQAB</Exponent><D>JhKALeTVO1zaeZnfL18tpx11b1PgwWOIX2ALjN/aDLuR6ySTPX/Q4I6zBOzVa/KB6VMNNk4abIKUvOp6MBNYIMRMK8veaIJxYrn+JAOShwE=</D></RSAKeyValue>";
	string CapiXml704woCRT = "<RSAKeyValue><Modulus>wbclY53TAIx2E6uPkYeYNVFPYnHgLDJGO1ZoSQcUm6ccySsJvVWAP9TWLIOTjJh2zaQn5gore87ONk50L/+FMZ5uaNrbxxPCxORBLzBofFaEbFxEfu9g6w==</Modulus><Exponent>AQAB</Exponent><D>Wfgbz2Zvp2OVO5GEvoyBbtHy0sAc46n94HVDPhehxKEax3vLrDnXtM2/IzCxXibWqBBmF8tw156Yo4Av2/EpGxvolidOpTXVxNX4aCb3dy05VQ/6WEem0Q==</D></RSAKeyValue>";
	string CapiXml768woCRT = "<RSAKeyValue><Modulus>3VQEd4kKl0vQG/OzLDTVcgVqaBG9KAz6NzJOu5a6mlHOYSIG5ybYwOJJpXDzZuXewBChqQNpmxgGAae/SuTy+EDQNzAQ+mzeYrEaD6C8vqwMYVanRcbNYtDC9cxAH6Ef</Modulus><Exponent>AQAB</Exponent><D>RK5ayIFFQRfsl4/zTMeEaOKXV34Rtcj5KIG6/ulSNKsoIOo/PCHI52oRMn6veYHhHrKclBeqtAlqOJG0xrEy/ZjorBJnCB/ZSoPPuS4h7budjF2zYiBHJkRwZcxhe+nx</D></RSAKeyValue>";
	string CapiXml832woCRT = "<RSAKeyValue><Modulus>wv35yrTrD3gjLxJEJ6HMOJsUfshcCZKGLwlxZO42TlAHK90vJPT0Q7SekeHqSVi1DJ7AAB7XW7MqoSE4SKVVioQQZSFz/HkA1fklTOaubfwRwv0vy/A3js1xPhwTs/SbDpNlXMoiEAM=</Modulus><Exponent>AQAB</Exponent><D>YuJPUbfr2J1xIkbeH0cS/MXQX/cVjZkrySC9y5RuH8q/yEPTy6cZVFh9bbemi2mb7vl2nfWO+VXLN+3OStN0SU3/2XQXfhZm4dqk5iU+R5n5g9kjPBNLH8DIWYjGVgCvBqH59DHhLAE=</D></RSAKeyValue>";
	string CapiXml896woCRT = "<RSAKeyValue><Modulus>nNiWWLAg2htBUkoBD7DTDIALapuiIt45qHJdPVbRQE5PXTcJUJ2SAEHCtQYtIeQam0WL2D78PIfAf8hGFx/72Ghn9b+NcZ8b7UHTdna+5O6aBOHyaD80FOmyfEjAjI1jS95XnGlqoD0QaDpkk4xnMQ==</Modulus><Exponent>AQAB</Exponent><D>LGd9GRq0EkuJELz20/Rhq7ZMhSAOpQR5GmFWWFlN4IDLIz7DmlkhzoTPlORsvp2Pksn7r3sVeiUbL3S1rXfIpwEjeseHfBYR54d6EqmPABqPj9UoADXDoYA1VYz+Ni2uFYlMC7Wh4kzlT0zYRc1XKQ==</D></RSAKeyValue>";
	string CapiXml960woCRT = "<RSAKeyValue><Modulus>3nb1VtiEuSii1rnDR+yrarcAHDlvbgLUTJGUvuVEMcvQ3vYNNabwBsM46X8ab78Ac2ZBk2jI2S4736KL2S8C2e8rXva7Jb/j+fyVt6N+NrI3dkNE2NoWZ8VM+rUEdKNvPANbf6n7hvDVfJLCvdCctdtEYACkujsb</Modulus><Exponent>AQAB</Exponent><D>jrmHQYZ78Ebv4g8gCD8A4uAxg+odYVkTV2R3J9nzXHdEtCbr5qYJjG0nUDapgVPrOB48qBQr95o/84RWPdIz3I+Jzpme1c4FpWs1/iXAbA/+xnzDGtXVTnJrMfkOsXt2MYBF7+f7zWGNlOGs+zkt/fP/jeOu76Nh</D></RSAKeyValue>";
	string CapiXml1024woCRT = "<RSAKeyValue><Modulus>uNXUqvMdjnmd36IS0Gr/lTAp63mZqoayYcN/YHnGs82eVrEWS5vAYqPWx1488BhUBh7kHvVhP5kkKh+i6pfLKAEssy5jnw9h09lgCClBHyntqFT82AlmiaFK8uVkV8rCjDol2nBpvVgNonF+JhQyexWTEI94Phjwvj/6yZRqaLM=</Modulus><Exponent>AQAB</Exponent><D>NVIqaa58xk87RfphZxKW7JjaXv3TYKg+6YkWQ+Sdd91HYkbv4Zvq4gnVuenrtm+uPZ3HvU6YYVpyXlyGCRsFFeXMwJ8Y7zrF6KEs+6hJ4jqBELWUoEPrzU4889v8aIoS+BCMYptpkmzvxjfEWdTPbqjd/tJ25ObbavggoST6ScE=</D></RSAKeyValue>";
	string CapiXml1536woCRT = "<RSAKeyValue><Modulus>1bkuWuGsKLkvSvrYYhOPVD37bARRUw0U5/yRCjam50+gLGKmxAjzaRjhGQUaaeC2S5DioGckqiqr1zkfSLyQ7KQte5oynxkTdT0QCXHJeVWgOWAknX8sNrAVNt97LOrMXXYZ2JsYMmYiVNqJRTaURWXxdJ5XibXMCRL1pfHbwnL4iqUge1tuiQbFod//7pjl90TukIutHVEyf8BFn5LRQJI4xroQdMJg0/vvRSp8jgy0LoeJ8LkTG+RjUe3XsgmT</Modulus><Exponent>AQAB</Exponent><D>rtTUab3QImQSnuiCmABeMFCf2qXRjPnXj0qZr1wzvmbxpT1yJE0aKXATu27kQ5ZyKXC1IvgdEyLi/aWZxNuURjCrkD/8hw9xTmeMNd2iLaJw9l6CtV/x4C68u+2nCoBq/Gv/ht9RqYRS6ODUTk1aOL1mPNSHT/MeFNK+06lyQNSaf95fMxGPIcnnJ1vtdkHgk2vi5ALSQf5tGIgVY1EFr55M9ABTg1p98XwIQgjIxCZcL4BygfMk1YbESaQFvzth</D></RSAKeyValue>";
	string CapiXml2048woCRT = "<RSAKeyValue><Modulus>9TFEYeTahPGOXtcRZrLQu1gHm875jv1jgxy0R9V3E/vDyEhNbsRK76rvEs3ljw6spQBQE1zd5wfUvFnhVETOuS0z9aXSAljxSIbyhkL/jT7/4vHEHe/GnBQZPdvJ9PtPr0ssLROy8xcsUEBDwxNRPNHSpmub3FHZzHsoMlJV0cOGmn0qqeMuMrP8zVDycXhXZNsn8v5RPC38qk6VDHdCqGz/90IRYonpFr5my9LaguJHVNCq8BRVa1oJkDtNUFhFXjVHnmxmhS9Mqm2obEpTp/jUk+bRqnygKB3up9OGYdLO1rAv2IoH/K2sxcvnedDzieRmPE/9Y9JBP/UvM+VULw==</Modulus><Exponent>AQAB</Exponent><D>ao8oyuqs1U4ts6YAWAOql2DgnaRL7QrObrLQ3s802yh1o9tYW6VPc+1zzVZSR+P2wBbsth2MCtXqbJkbRoZI2U194WpZZM/GvOB5EkSXz4jrqHOt6dzFEhviBHI6yQ9XSDWBU23WPbN6fL4RNPx2N9pwlAb8S7n+z9FOXOBPj8NyMMtykGqgPyHEAQnuhfLuf/J6OGrHX8dzsZAo6o/bnk49Rv0C6nijE74TeAwJ36UuI9Lw+h6hwtSerEN6o1vy6mIjrjdDM07mKKUMFwkEhKJc6ckBb0xVp/3VRlJb1ocYQHbfjCAPjecR8OVfjaIy9WPMj8WXHegJpiBugWK9wQ==</D></RSAKeyValue>";

	// import/export XML keypairs
	// so we know that Mono can use keypairs generated by CryptoAPI
	[Test]
	public void CapiXmlImportExport () 
	{
		rsa = new RSACryptoServiceProvider ();

		rsa.FromXmlString (CapiXml384);
		Assert.AreEqual (CapiXml384, rsa.ToXmlString (true), "Capi-Xml384");

		rsa.FromXmlString (CapiXml448);
		Assert.AreEqual (CapiXml448, rsa.ToXmlString (true), "Capi-Xml448");

		rsa.FromXmlString (CapiXml512);
		Assert.AreEqual (CapiXml512, rsa.ToXmlString (true), "Capi-Xml512");

		rsa.FromXmlString (CapiXml576);
		Assert.AreEqual (CapiXml576, rsa.ToXmlString (true), "Capi-Xml576");

		rsa.FromXmlString (CapiXml640);
		Assert.AreEqual (CapiXml640, rsa.ToXmlString (true), "Capi-Xml640");

		rsa.FromXmlString (CapiXml704);
		Assert.AreEqual (CapiXml704, rsa.ToXmlString (true), "Capi-Xml704");

		rsa.FromXmlString (CapiXml768);
		Assert.AreEqual (CapiXml768, rsa.ToXmlString (true), "Capi-Xml768");

		rsa.FromXmlString (CapiXml832);
		Assert.AreEqual (CapiXml832, rsa.ToXmlString (true), "Capi-Xml832");

		rsa.FromXmlString (CapiXml896);
		Assert.AreEqual (CapiXml896, rsa.ToXmlString (true), "Capi-Xml896");

		rsa.FromXmlString (CapiXml960);
		Assert.AreEqual (CapiXml960, rsa.ToXmlString (true), "Capi-Xml960");

		rsa.FromXmlString (CapiXml1024);
		Assert.AreEqual (CapiXml1024, rsa.ToXmlString (true), "Capi-Xml1024");

		rsa.FromXmlString (CapiXml1536);
		Assert.AreEqual (CapiXml1536, rsa.ToXmlString (true), "Capi-Xml1536");

		rsa.FromXmlString (CapiXml2048);
		Assert.AreEqual (CapiXml2048, rsa.ToXmlString (true), "Capi-Xml2048");
	}

	[Test]
	public void CapiXmlImportWithoutCRT () 
	{
		rsa = new RSACryptoServiceProvider ();
		rsa.FromXmlString (CapiXml384woCRT);
		rsa.FromXmlString (CapiXml448woCRT);
		rsa.FromXmlString (CapiXml512woCRT);
		rsa.FromXmlString (CapiXml576woCRT);
		rsa.FromXmlString (CapiXml640woCRT);
		rsa.FromXmlString (CapiXml704woCRT);
		rsa.FromXmlString (CapiXml768woCRT);
		rsa.FromXmlString (CapiXml832woCRT);
		rsa.FromXmlString (CapiXml896woCRT);
		rsa.FromXmlString (CapiXml960woCRT);
		rsa.FromXmlString (CapiXml1024woCRT);
		rsa.FromXmlString (CapiXml1536woCRT);
		rsa.FromXmlString (CapiXml2048woCRT);
	}

	// LAMESPEC: SignHash and VerifyHash need an OID string (not the algorithm)
	// http://www.easycsharp.com/news/dotnet/article.php3?id=25510
	// LAMESPEC: SignHash and VerifyHash doesn't accept null for algo parameters
	private void SignAndVerify (string msg, RSACryptoServiceProvider rsa) 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		byte[] sign = rsa.SignHash (hash, sha1OID);
		// we don't need the private key to verify
		RSAParameters param = rsa.ExportParameters (false);
		RSACryptoServiceProvider key = (RSACryptoServiceProvider) RSA.Create ();
		key.ImportParameters (param);
		// the signature is never the same so the only way to know if 
		// it worked is to verify it ourselve (i.e. can't compare)
		bool ok = key.VerifyHash (hash, sha1OID, sign);
		Assert.IsTrue (ok, msg + "-SignAndVerify");
	}

	// Validate that we can sign with every keypair and verify the signature
	// With Mono this means that we can use CAPI keypair to sign and verify.
	// For Windows this doesn't mean much.
	[Test]
	public void CapiSignature () 
	{
		rsa = new RSACryptoServiceProvider ();

		rsa.FromXmlString (CapiXml384);
		SignAndVerify ("Capi-384", rsa);

		rsa.FromXmlString (CapiXml448);
		SignAndVerify ("Capi-448", rsa);

		rsa.FromXmlString (CapiXml512);
		SignAndVerify ("Capi-512", rsa);

		rsa.FromXmlString (CapiXml576);
		SignAndVerify ("Capi-576", rsa);

		rsa.FromXmlString (CapiXml640);
		SignAndVerify ("Capi-640", rsa);

		rsa.FromXmlString (CapiXml704);
		SignAndVerify ("Capi-704", rsa);

		rsa.FromXmlString (CapiXml768);
		SignAndVerify ("Capi-768", rsa);

		rsa.FromXmlString (CapiXml832);
		SignAndVerify ("Capi-832", rsa);

		rsa.FromXmlString (CapiXml896);
		SignAndVerify ("Capi-896", rsa);

		rsa.FromXmlString (CapiXml960);
		SignAndVerify ("Capi-960", rsa);

		rsa.FromXmlString (CapiXml1024);
		SignAndVerify ("Capi-1024", rsa);

		rsa.FromXmlString (CapiXml1536);
		SignAndVerify ("Capi-1536", rsa);

		rsa.FromXmlString (CapiXml2048);
		SignAndVerify ("Capi-2048", rsa);
	}

	// The .NET framework can import keypairs with private key BUT without
	// CRT but once imported they cannot be used or exported back
	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void SignatureWithoutCRT () 
	{
		try {
			rsa = new RSACryptoServiceProvider ();
			rsa.FromXmlString (CapiXml384woCRT);
		}
		catch {}
		// exception is HERE
		SignAndVerify ("Capi-384-WithoutCRT", rsa);
	}

	// Validate that we can verify a signature made with CAPI
	// With Mono this means that we can verify CAPI signatures.
	// For Windows this doesn't mean much.
	[Test]
	public void CapiVerify () 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		rsa = new RSACryptoServiceProvider ();

		rsa.FromXmlString (CapiXml384);
		byte[] sign384 = { 0x6D, 0x6F, 0xE1, 0x17, 0x28, 0x88, 0x0C, 0x72, 0x9D, 0x17, 0xBD, 0x06, 0x02, 0xBA, 0x6D, 0xF0, 0x16, 0x2A, 0xE6, 0x19, 0x40, 0x1D, 0x58, 0xD3, 0x44, 0x70, 0xDE, 0x57, 0x4C, 0xC8, 0xE5, 0x9C, 0x5D, 0xA1, 0x84, 0x7E, 0xD4, 0xC3, 0x61, 0xAA, 0x21, 0xC3, 0x83, 0x5B, 0xAF, 0x1C, 0xB7, 0x2B };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign384), "Capi-384-Verify");
		sign384[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign384), "Capi-384-VerBad");

		rsa.FromXmlString (CapiXml448);
		byte[] sign448 = { 0x59, 0xD9, 0x57, 0x80, 0x20, 0x18, 0xF6, 0x23, 0xDB, 0x3A, 0xE3, 0xB0, 0xFD, 0xEA, 0xD3, 0x2A, 0x52, 0x83, 0x41, 0x49, 0x48, 0xD8, 0xD5, 0x4F, 0xE7, 0xDB, 0x7A, 0x97, 0xF9, 0x07, 0x27, 0xD9, 0xAC, 0x60, 0xA2, 0x17, 0x76, 0x70, 0x09, 0x09, 0x23, 0x0E, 0xBF, 0x3D, 0x01, 0x93, 0x91, 0x97, 0xDB, 0xB3, 0x1F, 0x20, 0x78, 0xFA, 0x7E, 0x72 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign448), "Capi-448-Verify");
		sign448[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign448), "Capi-448-VerBad");

		rsa.FromXmlString (CapiXml512);
		byte[] sign512 = { 0x66, 0x80, 0x40, 0x76, 0x01, 0xDA, 0x66, 0xF2, 0x36, 0x5C, 0xCE, 0x4C, 0x47, 0xA1, 0x02, 0x1C, 0x4F, 0xC8, 0xCE, 0xB2, 0x01, 0xC2, 0xD1, 0x54, 0x55, 0x41, 0xAF, 0x12, 0x60, 0xD7, 0x49, 0x9A, 0x4B, 0x37, 0x20, 0x1A, 0x2A, 0xD8, 0x2B, 0x82, 0xCA, 0x84, 0xAF, 0xB3, 0xB1, 0x4D, 0xA9, 0x0B, 0x70, 0x17, 0x3B, 0xD8, 0x17, 0xAA, 0x51, 0xE7, 0x1F, 0xB7, 0xCF, 0xD8, 0xF9, 0x32, 0x60, 0x01 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign512), "Capi-512-Verify");
		sign512[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign512), "Capi-512-VerBad");

		rsa.FromXmlString (CapiXml576);
		byte[] sign576 = { 0x25, 0x39, 0x21, 0xDE, 0x34, 0x23, 0xEC, 0xFE, 0xEC, 0x28, 0xB2, 0xEA, 0x8D, 0x5A, 0x6C, 0x4B, 0x86, 0x2B, 0xEA, 0x6E, 0xB7, 0xBA, 0x1A, 0x38, 0x6A, 0x8C, 0xC9, 0xC5, 0xC1, 0x89, 0x84, 0x9F, 0x97, 0x5B, 0xA0, 0x58, 0xD6, 0x08, 0x09, 0x5B, 0x4C, 0x9A, 0x9F, 0xFF, 0x10, 0x8B, 0xD1, 0xEA, 0xED, 0x81, 0x62, 0x2D, 0x09, 0x87, 0x81, 0xED, 0x76, 0xCF, 0x47, 0xB8, 0xF8, 0x06, 0x84, 0x80, 0x8E, 0xBC, 0x8C, 0xDF, 0xF2, 0xEE, 0xAB, 0x47 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign576), "Capi-576-Verify");
		sign576[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign576), "Capi-576-VerBad");

		rsa.FromXmlString (CapiXml640);
		byte[] sign640 = { 0x7E, 0x58, 0x16, 0x4E, 0xD9, 0xFE, 0x4D, 0x73, 0x2E, 0x6D, 0x5B, 0x62, 0x05, 0x91, 0x43, 0xA4, 0xEF, 0x65, 0xA9, 0x68, 0xA6, 0x19, 0x65, 0x8F, 0x25, 0xBC, 0x2B, 0xE2, 0x93, 0xE2, 0xEF, 0x5E, 0x29, 0x11, 0x0C, 0xCE, 0x17, 0x01, 0x5D, 0xB7, 0x71, 0x79, 0x83, 0xC5, 0xAB, 0x51, 0xBF, 0x87, 0x80, 0x4A, 0x04, 0x7B, 0xB4, 0x46, 0xCA, 0x11, 0x71, 0xB1, 0x67, 0x28, 0x4C, 0x16, 0x82, 0x6B, 0xE3, 0x11, 0x72, 0x6D, 0xA9, 0x07, 0x4A, 0xE6, 0x33, 0x45, 0x30, 0x15, 0x83, 0x23, 0x5E, 0x05 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign640), "Capi-640-Verify");
		sign640[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign640), "Capi-640-VerBad");

		rsa.FromXmlString (CapiXml704);
		byte[] sign704 = { 0x91, 0x3C, 0x91, 0x7C, 0x2A, 0x26, 0xB1, 0x6F, 0x7C, 0xFD, 0x75, 0xCC, 0xB0, 0x63, 0x03, 0x76, 0xBF, 0x93, 0xAB, 0x3B, 0xD4, 0x71, 0x86, 0x86, 0xEF, 0x10, 0xD6, 0x51, 0x8D, 0x6B, 0xB4, 0xF2, 0xF9, 0xCC, 0x50, 0x3A, 0x63, 0x9A, 0xD2, 0xA2, 0xE2, 0x72, 0x06, 0x88, 0x4E, 0xCD, 0x38, 0x9B, 0x87, 0x72, 0xAD, 0x38, 0xF7, 0xCB, 0x6B, 0x69, 0x43, 0xA0, 0x4C, 0x9B, 0x10, 0x17, 0xAB, 0xE8, 0x50, 0x34, 0x6E, 0x8D, 0x56, 0xBC, 0x6C, 0x00, 0x87, 0x40, 0x05, 0x4A, 0x20, 0x16, 0x49, 0x4F, 0x93, 0x48, 0x5F, 0x7A, 0x10, 0x61, 0xC6, 0xC9 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign704), "Capi-704-Verify");
		sign704[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign704), "Capi-704-VerBad");

		rsa.FromXmlString (CapiXml768);
		byte[] sign768 = { 0x0D, 0x67, 0x37, 0x00, 0x3E, 0xD5, 0x02, 0x12, 0xF0, 0xF9, 0xC2, 0x63, 0x29, 0xD9, 0xEA, 0x7B, 0xDC, 0x5B, 0xF3, 0xF0, 0xB9, 0x3B, 0x04, 0x98, 0xD9, 0xE7, 0xA3, 0xB2, 0xD2, 0x01, 0x04, 0x7A, 0xF4, 0x11, 0x7F, 0x62, 0xC7, 0x18, 0x14, 0xA0, 0x54, 0x96, 0x51, 0x84, 0xC4, 0x6D, 0x6A, 0xCB, 0x67, 0x07, 0xD0, 0x33, 0x93, 0x99, 0xAD, 0x1F, 0x16, 0xA6, 0x7F, 0xE0, 0x60, 0x8A, 0xDC, 0x95, 0xB9, 0x0E, 0x68, 0xA9, 0x78, 0xE1, 0x0D, 0x47, 0x56, 0xF3, 0x01, 0x42, 0xF6, 0xD7, 0xE4, 0x4A, 0x98, 0x7E, 0xC4, 0x5E, 0x0E, 0x09, 0xC4, 0x92, 0x42, 0x69, 0x74, 0x7A, 0xC9, 0x0E, 0x9F, 0xF2 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign768), "Capi-768-Verify");
		sign768[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign768), "Capi-768-VerBad");

		rsa.FromXmlString (CapiXml832);
		byte[] sign832 = { 0x7B, 0xBC, 0xE6, 0xEC, 0x8B, 0x11, 0x7C, 0x28, 0x09, 0x3A, 0x06, 0x81, 0x52, 0x20, 0x7B, 0x50, 0x88, 0x74, 0x3C, 0x96, 0xBC, 0x40, 0xAF, 0x8E, 0xD3, 0x60, 0x49, 0xBF, 0x93, 0x6C, 0xF6, 0xE9, 0x34, 0xAB, 0x7E, 0xE7, 0x56, 0x2E, 0x33, 0x5A, 0x75, 0x4E, 0xFE, 0xFF, 0xC8, 0x15, 0x02, 0x8C, 0x30, 0x81, 0xD0, 0x46, 0x68, 0x42, 0x2E, 0xA8, 0x78, 0x5C, 0xDE, 0xEA, 0x98, 0x79, 0x81, 0xBF, 0xEA, 0xC4, 0x8B, 0x2F, 0x50, 0x7A, 0x50, 0x11, 0xAC, 0x01, 0xEB, 0x84, 0xA0, 0x9F, 0x7A, 0x67, 0xFA, 0x33, 0x3A, 0x65, 0x1C, 0xE9, 0xFA, 0xD9, 0x46, 0x06, 0xAF, 0x3E, 0x83, 0x4A, 0x67, 0x11, 0x0B, 0xD5, 0xD2, 0x56, 0x5E, 0x05, 0x69, 0xE8 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign832), "Capi-832-Verify");
		sign832[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign832), "Capi-832-VerBad");

		rsa.FromXmlString (CapiXml896);
		byte[] sign896 = { 0x60, 0xAA, 0x26, 0x4D, 0xBF, 0x0A, 0x38, 0x5E, 0x7B, 0x66, 0x1E, 0x1D, 0xED, 0x5F, 0xF2, 0x6A, 0xC4, 0xAB, 0x35, 0x43, 0x93, 0x67, 0x26, 0x90, 0xEB, 0x34, 0xC1, 0xCB, 0x22, 0x4C, 0x20, 0x00, 0xE5, 0xEB, 0xA8, 0x14, 0x06, 0x8A, 0xBD, 0x0A, 0x07, 0x02, 0x62, 0xC9, 0x82, 0x27, 0x84, 0x1D, 0x04, 0xC6, 0xFC, 0x20, 0xE4, 0x17, 0x07, 0x33, 0x03, 0xE7, 0x33, 0x8D, 0x4A, 0x69, 0x79, 0x51, 0xA5, 0x20, 0xED, 0xB8, 0x94, 0x02, 0x0B, 0xD2, 0xB9, 0x60, 0x71, 0x56, 0x47, 0x96, 0xE8, 0xA7, 0x1A, 0x4A, 0x71, 0xD8, 0xD1, 0x0C, 0x82, 0x86, 0xF9, 0x1C, 0x07, 0x46, 0xB6, 0x0F, 0xCB, 0x8C, 0x90, 0x95, 0x93, 0xBA, 0xF2, 0xC3, 0xB4, 0x68, 0xE8, 0xE4, 0x55, 0x6C, 0xEA, 0x1D, 0xA7, 0x53 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign896), "Capi-896-Verify");
		sign896[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign896), "Capi-896-VerBad");

		rsa.FromXmlString (CapiXml960);
		byte[] sign960 = { 0x57, 0x36, 0xBD, 0xF3, 0x5D, 0x99, 0x9A, 0x73, 0xF1, 0xE6, 0xC1, 0x4C, 0xDC, 0xBF, 0x5D, 0xC1, 0xC0, 0x7D, 0x32, 0x46, 0x2B, 0x0B, 0x51, 0xDB, 0x56, 0xA8, 0x25, 0xE7, 0xF9, 0x76, 0xB0, 0x0F, 0xC1, 0xBD, 0x49, 0x5E, 0x14, 0xF8, 0x1F, 0xD9, 0x94, 0x41, 0xA7, 0x4B, 0xF8, 0x9C, 0x4A, 0x9B, 0xA5, 0xF8, 0xDF, 0xE6, 0xF5, 0xF7, 0x6D, 0xFB, 0x59, 0x24, 0x9F, 0xAE, 0xBA, 0xDF, 0x51, 0x2F, 0xB2, 0x68, 0xF1, 0x35, 0xFB, 0xCC, 0xF7, 0x1D, 0x86, 0x2A, 0x2E, 0x53, 0x9F, 0xCA, 0xBC, 0x58, 0xBF, 0x6A, 0xBD, 0x3A, 0xAB, 0xCA, 0x64, 0x92, 0x42, 0x97, 0x87, 0x00, 0xEE, 0x4A, 0x3F, 0xE5, 0xED, 0x20, 0x49, 0xF4, 0x06, 0x53, 0xBA, 0xF8, 0x9D, 0x62, 0xC4, 0xC7, 0x99, 0x3E, 0x9C, 0xF9, 0xF1, 0xA6, 0xF7, 0x0D, 0x21, 0xBC, 0xF2, 0x8D };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign960), "Capi-960-Verify");
		sign960[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign960), "Capi-960-VerBad");

		rsa.FromXmlString (CapiXml1024);
		byte[] sign1024 = { 0x30, 0xB9, 0x7F, 0xBC, 0x72, 0x75, 0xB4, 0xEE, 0x8F, 0x8F, 0xF1, 0xB9, 0xC3, 0xCB, 0xC0, 0xB2, 0x94, 0x8C, 0x1D, 0xB0, 0x60, 0xE8, 0x5E, 0x8D, 0x3F, 0x36, 0xF2, 0x4E, 0x09, 0xCD, 0x48, 0x85, 0xD3, 0x93, 0xEC, 0xEB, 0x08, 0xA6, 0xD1, 0xEF, 0x9F, 0xF5, 0xAB, 0x2D, 0x30, 0x3E, 0x86, 0x64, 0x8F, 0x9B, 0xB4, 0x0F, 0xDB, 0x70, 0x4A, 0x59, 0x8D, 0x6F, 0x14, 0x03, 0xFE, 0x46, 0xFC, 0x81, 0xA8, 0xC9, 0x75, 0x40, 0xEE, 0xB1, 0xED, 0xC4, 0xD0, 0xE5, 0xE5, 0x8F, 0xCD, 0x10, 0x01, 0xB2, 0x9D, 0xFD, 0xE6, 0x1E, 0x43, 0x3A, 0x69, 0xDB, 0xF9, 0x6D, 0xC9, 0xDD, 0x57, 0x9C, 0x84, 0x94, 0x7D, 0x0C, 0xDC, 0xFA, 0xD9, 0xDA, 0xE6, 0x90, 0xBA, 0x8C, 0x15, 0x68, 0xEA, 0x09, 0xEE, 0x8D, 0x5D, 0x5D, 0x73, 0xDE, 0xD1, 0x76, 0xE2, 0xF1, 0x48, 0x23, 0x57, 0xA6, 0xFA, 0x37, 0x57, 0xE7 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign1024), "Capi-1024-Verify");
		sign1024[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign1024), "Capi-1024-VerBad");

		rsa.FromXmlString (CapiXml1536);
		byte[] sign1536 = { 0x38, 0x0B, 0x7A, 0xF8, 0x64, 0xEB, 0x21, 0xCA, 0xC6, 0x7F, 0x09, 0x40, 0xD0, 0x92, 0x8C, 0x96, 0xEE, 0x47, 0x05, 0xFE, 0x17, 0xE9, 0x93, 0x97, 0xEF, 0x9F, 0xC0, 0xF1, 0xF5, 0x41, 0x91, 0x83, 0xEA, 0x77, 0x1B, 0x6A, 0x77, 0x4A, 0x14, 0xE2, 0xD9, 0x88, 0x8D, 0x8E, 0x91, 0xAF, 0x26, 0x87, 0xB9, 0xBC, 0x14, 0x52, 0xBD, 0xAD, 0x7E, 0x90, 0x00, 0x33, 0x1B, 0xF0, 0x08, 0x41, 0x7E, 0x77, 0x55, 0x4E, 0xAA, 0xF3, 0xAC, 0x8F, 0x00, 0x76, 0x25, 0xC0, 0x72, 0x88, 0xC0, 0xA4, 0x03, 0x46, 0xCC, 0xBC, 0xFE, 0x5A, 0x44, 0x10, 0x0D, 0x14, 0x7F, 0x65, 0x2E, 0x47, 0x55, 0x85, 0xF6, 0xC9, 0xEB, 0x60, 0x84, 0x09, 0x91, 0x3D, 0xAC, 0x8E, 0x40, 0x44, 0xE5, 0x7C, 0x4F, 0xAE, 0xA4, 0x21, 0x7D, 0x16, 0x27, 0xFC, 0x0E, 0xC5, 0x6B, 0x0C, 0x63, 0xA9, 0x1E, 0x0E, 0xCF, 0x9E, 0x8E, 0xFF, 0xA9, 0xA5, 0xB3, 0x87, 0xDC, 0x80, 0x71, 0xA2, 0x4F, 0x21, 0x80, 0xC2, 0x88, 0xC0, 0xA4, 0x99, 0x46, 0x96, 0x25, 0xFB, 0x73, 0x16, 0xCA, 0x97, 0x25, 0x5A, 0x92, 0x19, 0x50, 0xCD, 0xBA, 0xA0, 0x6D, 0xCA, 0x9F, 0x23, 0x14, 0x71, 0x51, 0xB8, 0x94, 0x47, 0xAC, 0xEE, 0x12, 0x53, 0x24, 0x7E, 0xF4, 0xD0, 0xD9, 0x51, 0x89, 0xDA, 0x1C, 0x6F, 0xDE, 0x51, 0x9A, 0xD9, 0xDA, 0xA0, 0xE5, 0xBE };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign1536), "Capi-1536-Verify");
		sign1536[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign1536), "Capi-1536-VerBad");

		rsa.FromXmlString (CapiXml2048);
		byte[] sign2048 = { 0x62, 0xB8, 0xE3, 0xD0, 0x8B, 0x8E, 0x00, 0xFA, 0x59, 0x17, 0xC7, 0x0C, 0x81, 0x76, 0x3A, 0x35, 0xAA, 0xC7, 0xE4, 0xFA, 0x7C, 0x96, 0x9C, 0x88, 0x15, 0x88, 0xA5, 0xA6, 0x7C, 0x95, 0x55, 0xC5, 0xDD, 0xA5, 0x83, 0x25, 0x33, 0xFF, 0xEE, 0x9E, 0xDD, 0xF1, 0xF1, 0x85, 0xB3, 0xEE, 0x78, 0x11, 0xC7, 0x83, 0xA6, 0xD8, 0xB4, 0x34, 0xC4, 0x73, 0x2A, 0xE5, 0xC9, 0x2A, 0x5F, 0x5C, 0xF5, 0xAB, 0xB0, 0xB6, 0xAD, 0x52, 0xB3, 0xBA, 0x31, 0x0E, 0x5A, 0x09, 0xB1, 0x74, 0x90, 0xCC, 0xC7, 0x7C, 0x58, 0x2D, 0x49, 0x9F, 0xDC, 0x6F, 0x03, 0x58, 0xB0, 0x1D, 0xFA, 0x7F, 0xE5, 0x8E, 0x09, 0xAC, 0x17, 0xCF, 0xD8, 0x0F, 0xE0, 0x1F, 0x1B, 0xDD, 0x72, 0xA5, 0xCA, 0x5E, 0x51, 0x2F, 0x52, 0xD9, 0x66, 0xE9, 0xA5, 0x78, 0xCE, 0xE4, 0x73, 0x63, 0xCA, 0xE5, 0x58, 0x30, 0xE9, 0xB9, 0x57, 0x07, 0x56, 0x77, 0x67, 0xFC, 0xBC, 0xE2, 0xF6, 0xAF, 0xD6, 0x5D, 0x02, 0xBB, 0x14, 0x47, 0xD2, 0xDF, 0x12, 0xC8, 0x42, 0xF0, 0xA5, 0x42, 0x3B, 0x90, 0xB3, 0xC8, 0x9D, 0xD7, 0xB6, 0x89, 0x25, 0xDA, 0x7D, 0xC2, 0x53, 0x94, 0x1D, 0x20, 0x8E, 0x2C, 0x96, 0x32, 0x43, 0x4C, 0xE1, 0x20, 0x64, 0xEC, 0x15, 0xC5, 0x44, 0xEE, 0xCB, 0xEC, 0x89, 0xBF, 0x84, 0x1E, 0x93, 0x2B, 0x87, 0xEC, 0xE4, 0x97, 0x66, 0xF8, 0xD7, 0xD5, 0x4B, 0xFF, 0x9D, 0x4B, 0x5E, 0x73, 0xFA, 0x11, 0x50, 0xD5, 0xA0, 0xF3, 0x7C, 0x3D, 0xD8, 0x7B, 0x12, 0x04, 0xE8, 0xB7, 0x27, 0x5A, 0x11, 0xF0, 0x65, 0x33, 0x13, 0xC6, 0xDC, 0xC1, 0xFB, 0xD1, 0x7A, 0x93, 0xCF, 0x1B, 0xFB, 0x12, 0x72, 0xF6, 0xD9, 0x6F, 0x38, 0x0B, 0xFE, 0xBE, 0x4D, 0xE0, 0xF8, 0xEB, 0x21, 0xAE, 0x5D, 0xD2, 0x7C, 0x81, 0x74, 0x0A, 0x47, 0x1E };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign2048), "Capi-2048-Verify");
		sign2048[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign2048), "Capi-2048-VerBad");
	}

	// all keypairs generated by Mono on Windows
	string MonoXml384 = "<RSAKeyValue><Modulus>i9COhlY/aGJD+B0fjUtOR+bbDkKyrwDzXzvwbBE8TJmtFYNrjfLHOj/gjdKCEcqb</Modulus><Exponent>EQ==</Exponent><P>tJ2yzv36kToJOy2vURzL9X+W0YsKiHZr</P><Q>xitaV4u8NRFVtIuFvYWArakoLHFeUiiR</Q><DP>dN6CwiveXfhgU3fp6TDANXCs4fCOWEyf</DP><DQ>aOnGapVFhYGl2A2hKBmAW/AkU8OMSZ0B</DQ><InverseQ>bU13UMhy7nsqfFtm7AhyjkJT4GCd/OPT</InverseQ><D>GKxVYwApTqft4H2cJ/46/aE1tzjyWx5IzfYoG1Qmnzr+u1/rf+EGkyIzGWIEcyXR</D></RSAKeyValue>";
	string MonoXml448 = "<RSAKeyValue><Modulus>rGhbXQawiO5muvnbq7FuVZc3QuFp2CZNqHekmRA/fdUzmUP6jFH7usaVh1kZ+Fk0em0fJzjOYgU=</Modulus><Exponent>EQ==</Exponent><P>rp5tO6WKHonnIRHxS8e566iNe20quz30u3R2Yw==</P><Q>/MIc/2TV6WPi9ddM5NzLeaukuv7pmZf/VfFedw==</Q><DP>j83DXkwXZHGRKksC8xzzdscLGlnm9I1gIedwjQ==</DP><DQ>hdAteCZTITThkTW/S/xruOJmROFOfn2lS53mtw==</DQ><InverseQ>L0fk3nG45E5WbtKkFODlQjMQGIdSEwlsvt/TYA==</InverseQ><D>ZWqQGJqF9jHiE6IIv1lP9hy3GEhcYQd4+a/KOvAKrWm95W6eF+aDWFgzao9lR1/BLP86HhcucSk=</D></RSAKeyValue>";
	string MonoXml512 = "<RSAKeyValue><Modulus>jq1Bst+LWM5ScL29w+C/GVULDAebcT0Kl9ERGp/I2A2qJ4jHuzr7ltoQqtVGRquQwwDZQ/o1fH99bWraz5Fi7w==</Modulus><Exponent>EQ==</Exponent><P>1O1TVs0UsUSx7Vjg7H/nJnZyfteeLNmUSAPylutaXmk=</P><Q>q4nwfMxpskF8TIRVmcTl1yHcu9rlaMt50Tb/skhFu5c=</Q><DP>ica9dGaU6yxzIRtkXMs7NwFZJOXe0bn2iPN+2h/RD+k=</DP><DQ>MnPdUeHEu/Ukjvm+0t+O5OvXgous4pYy4y5LNG+cCf8=</DQ><InverseQ>fAvQlMRGT5B/FPjQBeArxevr9DN59L0JfhN5qO84pLs=</InverseQ><D>bRsUH1+my3CZZUXNWY2/T5tizPbCKWrp+5/f5y7k4XK2d7xCN+qSZxn+nT0patdhewBMFOI9/xp5zD4U/seSIQ==</D></RSAKeyValue>";
	string MonoXml576 = "<RSAKeyValue><Modulus>h18QgR8YiVXKVmcTRuPfHljm9Yz5vPt9UXg25GU2WztihEihEyZYMJaRzEb8/iS7eGg7jLXW8SrxhyLUnXfgWvtn/9uNGinj</Modulus><Exponent>EQ==</Exponent><P>x3NP/zn0CxxHMoce7+3UbMtWbtZCxxZZGMID5OCJKGeJUYZF</P><Q>rcC07OWzbx2h827VnJlAWlmxmRRGIaXFOp10tNG8C1bE4GYH</Q><DP>UiBsO+qvyFbwI91I+WHuDq4Uh/3fQusVoMhb9NTtLsE4ivsN</DP><DQ>mU+usujLgCk0i3/pqEr8i/TJ4Ww94XQmfwNm+eY8gnm8xf+r</DQ><InverseQ>KqPD9+C3p6VF6KLOhkyEOLtBlImFMY3kK79+1bmJLjEdyzUH</InverseQ><D>b3t3AOxujzeXknMA0Pfk69C+M6FGIyl2Qxe0vBcdtItCEpYpGKlxv+l1rPu1/VOy4F8Q3u+5dMuhc2OlLfX7F6WkL5+dN7pB</D></RSAKeyValue>";
	string MonoXml640 = "<RSAKeyValue><Modulus>4YELOUiQPs7Nw6AM2m14LxuKDk1dXGYhvKczD6mB2//U4sitp+MFReJ0ot840hQNwwCssw8hyyniFqIvSjKJknv+HFL4Lc3AyCP3pL5iFf0=</Modulus><Exponent>EQ==</Exponent><P>9Q+nEGCtAF5MIk3lFGn0YtMYSYGM9JI2Ai16dS10Dx6Wxeq1bnVHLw==</P><Q>65Ho89urLekWoDLeZb2ManjaLQS7se6A0eM+Sj54cFnHj8UDe0uqkw==</Q><DP>rPv9dPjyltkmrs2SpP9/VNE+UgEYNCr48np0jvLogySmqdLabBaMmQ==</DP><DQ>UyRwVhFLeZ2Pg9W356xPrRuYTB/JxlQtd0ElCyUbcvKCqzZ5swulnQ==</DQ><InverseQ>5OZh1s2cw3Ydxa+abpooyXC08yJwNS9omtFIl9ct2HZkK4EKvhqJvg==</InverseQ><D>rHG9SewyEelwLC8Y40SnMxUPOB0LKIpWCLwX/OsI84dmj04qcVNAQxB0O5hmt5GXswJd1VPOdRmf/YrMiLZhIHFbLgdrQP+gyWGCSu3koz0=</D></RSAKeyValue>";
	string MonoXml704 = "<RSAKeyValue><Modulus>mUDm0P3S9oPB06NJT5oDFS5+Xy/ACBvGdkvNjCiZo9bOLvVpOsiih9ipWZw9zQInjfP0uhGqDBkl9jw61yEHIqed151b9hkO2cmfb2o3GvPkbXkoMcoxBQ==</Modulus><Exponent>EQ==</Exponent><P>sMV0t0i+ucYTdpvFTL9lzhxTC902auu5vMDf5Q45WAiu57e9WcGNGbx3PqU=</P><Q>3fEKYrtMZmXoL/vf4xnrRDKpt106zSyrwaVnq5vwrQUZGqj/zPx+umBj+uE=</Q><DP>FMvvnRed97zzOyFigX/t3ANVEHRgwUjorMtlonok3S4ysdlhkhbFTlJoYbk=</DP><DQ>nKolct6QSEfvMO4HcyFaxrpZrpwpgcUt8hpnS/Wa1HwRuHdLJ0jR7Pi/C3E=</DQ><InverseQ>Sy1FwjQNv201hd5uEVIDrxuCXHAnCA5G8p2XDTGee+mlGnMqFvrNKdR57VM=</InverseQ><D>WiYta+CaNqfbbW8cELTywSpodDo0uXnACVnENFQeJCQA0FQfyDnJBJ2Qy0vuhbaAbz2qNXZcYp6QxbeBQQZGEB//nahkNns7KxKXDEFMMU1ghU9eoubr8Q==</D></RSAKeyValue>";
	string MonoXml768 = "<RSAKeyValue><Modulus>pnVBGKuCV1sdnZP7yHMKlMnyc+4MZicSRDu6ESWPKUx/Ab2fbwwOoHDE5pSW/wCGziaEA2Agfwt6YKAr/JyPmcBcHichUjhzjDvExBrdd3jmQ1WjjNzV829+qSEq0kUl</Modulus><Exponent>EQ==</Exponent><P>u3CSNJ8PqWVfZkLnDBKFGk9EhGnY2poXTBR8aWqLDoa/nG1OPB4Zx0CjwNyR5mSH</P><Q>41gBCJ7TC5URUbKIqzfxQKcoDbz45tJ/1/h0nhGGXehGHYFWlNeKDMcqLtPDB5/z</Q><DP>NyEb8Vv1jCzf0shiEpwJB7z2CNPWXmmOYausHwE39Taw06etmTYHlPTk3l79vDuv</DP><DQ>uzlqQ1Wevj6Gu8A0UMSoj5i3ktfcCWIO7hfnkTub1N1m6x84PlcmZOBA20T69zht</DQ><InverseQ>sJSC1ons7852EcNt7Cctp8ryAvWZxndAz2/W5vcLVi4sk5+6EQ8Kfyw66hYNPvTO</InverseQ><D>kt/uJNORH+b8A4KS3gsndDm3z7Ps0prx//h3ABIU6DRwEJhBYfuUb3KPnkbflbUqONprCS1FlC0mo+Hxald/r99a5PE3JWiVTN4GHuZ3zYEviEvRtNsOG7XYKydTUO3F</D></RSAKeyValue>";
	string MonoXml832 = "<RSAKeyValue><Modulus>3dVclQYr1ks5x8VXLcO0H0BRNlmg/XT01IFso0aQY5AwU/kFbDUwTSrmSZCuRBk2blp14BYvpwpviuP9WkIcjhTSuPwPD1XWSl33WHSWEc6uYEHxjn0gieHK3SiyVAgOrcVXtOaNJck=</Modulus><Exponent>EQ==</Exponent><P>5BX1qdEJBAhdRC3YXMur5Hs2AfznVNExYbkj7wrK3qLxpQOLQk+TSk8PzWFoOx4xnDG98w==</P><Q>+PuDZFrHTTsSLXrrbS7PhQTWd5BHEqVifUttqI+YSx3pRTxFmfulj5m8uccDE775VrUwUw==</Q><DP>KEAcSyTjeS6nDAgXH288c51FxB2SPCTqmMZRoqeNNlj9d3kYkzs4HCwR2PMSZMkX0EUDZw==</DP><DQ>K/AmPuLX4HPWCAaiBDVv6kwl2N05t/8RYWep8JHPlMkLDDfQDB1Zc7G35IyIEosc8S8Ihw==</DQ><InverseQ>p7fE4OUIq/uvESb5mExGPNxh7BZBLQ3GRxq67QvHhhhaw4KAiISMk/MJY2TxD+DZ2F25hQ==</InverseQ><D>aGRnzaiNGYzP5YoK6FwYhy1TZN7iWSf6vlsF8nuPPemAJ4Q+yYJxFUFdT8udTTkKjkiv8BrhBpTUlAjP9ge+BNftSibZ4zp8wd1pxewIPFYnhimAVIFasWv/1NnIP8QwH0bQQOshCxE=</D></RSAKeyValue>";
	string MonoXml896 = "<RSAKeyValue><Modulus>8SR9zUf+X33JO4Gen620JE7yqQhvP0RSnU+XGE5fSHTPmy/HtuP5/IsCbfDsKOa5aAa4uj6PpxsG/6GgrbgmkxSfJmj8OfYP7Eo7wjXheJmv87Iw8y0rJ3PhkKbF82GFU/CmYQO3A77QVK74v9NMRw==</Modulus><Exponent>EQ==</Exponent><P>+q7kK0s1I10ZVAXqqnTzZ8T7L+Q82UW658IUeH1aL8OteL9qbh/05vhqknbptbZ6Xjsf9UtoTT8=</P><Q>9kHMYmw0rlmWcxQOAnTkn1g6ZCyMDR05Yhvf7qQtbZQ92GKWrs0+MTl9N+RkYREqhskY5S5TVvk=</Q><DP>hLbxRBjBx22F4TBPDvKe+rOUCkunvlIXp984A42oN2eYEr+w0OO947Cw5CDWFOgiqluJY76Cgz8=</DP><DQ>n1fAe+upnf27s9/M1GnBG8+tT+CW208lIV1UqXlKkjK+mxKdvGavxX+NQkh9L7/QORi1wXhUCxk=</DQ><InverseQ>lsGlEhb8TteuqHjnPULwYF+naVMMThqPa9+sAMflXAHm6zQ7YB6JotrhnSnSVdxuri0bhZwbMt4=</InverseQ><D>Ko34FSrSiVJuzkQM7wCJM5V2HdRP3fz/hSwpuP7FhUHKSI/2ETdKO6APfNAprN1sA0x687Cv8E+4mTmZ7zr/6qhiXIxKO1CYJIssptb/IfAC9McjnXeRfvn7XtHwprzmFRCfTE06ZPWD4QXJG22WIQ==</D></RSAKeyValue>";
	string MonoXml960 = "<RSAKeyValue><Modulus>tsPl3wQ+EYH4tuzDokt/CknZUVNZMr9AEwZCGMzpXORmkHaBA5Dh5N24OtQwO1YQ4gXCosU9YpdQujIvki48txiL+r6/KtK1hIOm4h/LCZRZ0ztYZvufH2ysv6Sv22iEbv8/ABOmaAu7A0RueGI1dX8aZ+dIlqzz</Modulus><Exponent>EQ==</Exponent><P>1kRCrCRrHd9STbYjSUZ0bD/Sqcx44aXwEjn06z6starjsKQ/RAboA/VCalDWlA66UC8CPQxN2Sd0Sn09</P><Q>2lzq2YjgLtwcQzW7GdAXf5yAoXO7mghh8rgfRvpVtAN6wkTSR1XQXPdXYJRVDYyGyrA5bBOnheotuNXv</Q><DP>WDo5ki0dDEzlp4c7tMKoaM78ZAjmewgXjwjOQr90Ss3lSLwaDPPI8pIqaAMrLefyXUCIc32Jd4i3afdV</DP><DQ>QDlyP/sUwnz5QPGvgBAG6Uwl1SIKACCVR2NUfkmgvHl+dV+JJAouOXXshdFGMSlU0jPUp1ETJ2L+Y4o3</DQ><InverseQ>gssEimyKQEsy0cnyZlhx4h6Ua/owWnlz4bRac6tVYQRK05LG78sAbgtysi/aZ2rXqG6+ZBTrY0FHuR5+</InverseQ><D>rAOrLEA6atSt2VdOtt2kvmOfW5m9XPA8TiQB+TlUGzFReOgA9Ewf5nZTCjEeVfamel/GPtfBTbuXRdTf41eV1Ci1WHuXJ2/ZiN8ob06O0UAjJTlva1RGhOVVBZyI2tJOA2vJ/zPPovcdiReyOeSu3neqYqr3IUVx</D></RSAKeyValue>";
	string MonoXml1024 = "<RSAKeyValue><Modulus>i+O73a/wy4fMi6V9dF9Us4oyAUvNwrJcOS69r7edDjw0MDd5vz8PcaAdL3BOwMEmItI1i8Z39HxqvuuwdB0GGCNKGh4Ft680nIAgsLugztJ4Shjm+xgFM3cFSGxJpSWkWlTpXs2ARhOSIbr5/+UwON4EvpkYxC5GbRIsrqDHHV8=</Modulus><Exponent>EQ==</Exponent><P>88rUpdL/RNdeuYTReHSK+MmRWnc2S79Z67uHtMIWO0BsG/gMUIKnFSpJiEaFpnHXe881cdUvNvo9NGuTz5Jh1Q==</P><Q>kuT52+3qfpuQ539A08+DiPSfyNHsq2vwOg8vxd3pBfVUyLSPkaLHOmXumk7BDvWAsQXQPOblsgeByunEO1ooYw==</Q><DP>ZGKTy89aDUmfeZEK9V0qKjThf5p/xNZSM+POd5s2VKIOZd6bqK5EzHrS++DNnuOU9r67p1fIJbJVb/APr9Lc/Q==</DP><DQ>ikDrKVhkOuzEnaTxuErWJovhrfLAoVaHvixpMrK9MsjIRG20TNVwNvaGNuC1s7niiH3xKkK6EPgBr+sTCq81EQ==</DQ><InverseQ>N3di8j2v4ZxE0Y/WFST/wSuAvTfTevx3F6I9jjYReUH8Yy88aKoKr+mgRSl+94/glwbCQ3Q+jP79puiBmcRrPA==</InverseQ><D>OZoCD/0m6mUX/UQkmVRuK89+AIigBOAH2097SFqqFOudBMuMe+zKH7pmXtPkMUB5HWWdk+hPgsnRmeiT85N6+kB7xMfgGI5AsJga19OAi2yI3RnXlTq0FMcYHZCvRE8AXUxVQTO7/n3iq3r8TEDaPmcTpn6eooXvGoA6jRCHLYk=</D></RSAKeyValue>";
	string MonoXml1536 = "<RSAKeyValue><Modulus>jzYjq6zGOWfmpnzDf5SNrSifeYbFFgbh0/tO2UkLWBpbFqHdLo8CZJt1j3UtSN82ZuV6IBMJnmIsxOH3JQ677IPb76Hz/F0V07jCM88kcz3RRtCahXj/14p2EwTY3297yylWx8+zGmpQDPOxejbaGyjn2TyPx6Nq54SVSAfBUgcXuwwpksogwv/neZLyOlu/JBG5Ej11FOfJ8vi7gb/OCqK1EdzRthf9e4FS5KoxC81TiTUXyfesYpSsHI6fQ5i1</Modulus><Exponent>EQ==</Exponent><P>r0edPMfgFzP3gOrrFIS83DteP5B9l0Y0c9ONSdM3il1Gm/NtU1YTmEHAdAaNHS/p8xqO2ojnYLTK7EUADQeeEUnl3f+BYcYkVSOy6Fq8KKJlDP/PUNF97Yp42TgUL2bD</P><Q>0SngrKVGg1QV5paow1TV6IBTaxVflPcHRdkk7H3WkzSz8XGmpVWYumRWQoDT7NwdsMZnvXImlavUX4v0fNqTvHwN1HwTU8g64RFqfePZK48fN3EjIZDKcTSEQxrFdDsn</Q><DP>e7oUo1/pecpUWwAtd+U6BN6c4ZMredc0FYZFnYYJFl/1m0JrSeJoLz14yl76Mrho59aC9Jzfj46tW3wACTKNsdnPb4cuJuY3w6C6heWx/pC/zPDOk2azPkOgmVTC9EiJ</DP><DQ>uI5c1JHUzjsiYgx2yngICWIrfJpjZVJv1DgRhV/5kPJEa6CD+0uGwpTElQhChbMpQaABLrAD7Xl/JyEjBMDctV5mjk9cWPv3t4fWbxRWNX5IuHLiw0OFcvIaWVPbZo6L</DQ><InverseQ>p52gqtyki7MRn0fFTOkSCee/2QQAZyM2cYt3DtgDQbRcqrMT6Q9WXgyXOGdadqFFvUpGxysCQ/rppLttCG9S8wG8lD9I6hulda3TkiKCsYNuNLco0epmjXpWc0YmCcys</InverseQ><D>OvgszjgVYu6MJm+byx8NKS7YQRlgNj8RsaO3Di0iyey8GGDEfJU9OHw/d05d8NRhseYFHEQTBPs/nF0LaZynu73TCFG+0VOBdUwTuvrw5CiDWWT0VRO0pAvWRBEOAaZfiC2ViL8qyz8MYj4MM8wOUMOOx+Oy9LGa9MJOcLSkJKixqWLqxv17PVIoyMiHE+SW2m5uMleT32TVU+Mzz3lec2n1RWRGS4P14EyOf1l8WqmRlMk8jXm/xVf8w9xgbwtF</D></RSAKeyValue>";
	string MonoXml2048 = "<RSAKeyValue><Modulus>ppiz9wru8QdjxD1IZ8Ouc4rpKMjR51jDq/mPu7gILbto1sD+01r/P84zFd2n9Jbv+vHJ8ClTr/9vh3dy6OzN/ASUoAMzmGnHQPsFHyaZ7TxDmF2EaF45F0rDbsPsOxlHUKkc33EuHwzXRZRdD4c2vCY8D+qrdpldkC4KH4oWjBHPk2MlqM1mPGJIvzz9nANhEl9YHHHZmAV2auZIWMWyO9zPHoE0YYMeFTHIuOZmxbu6iKX0pWcHzukx6FCqxN9zOAz5xl9D0f//0vhsiZUINM4y4ypS9C+jF8WAUthj29R3s1Hxr5I32kr3+nhtkHu7ljqLzYqCBq6hixZEhHlUlQ==</Modulus><Exponent>EQ==</Exponent><P>w5gi5/IS9jFz8tAVpUgmCBBvUCMWG6VmzvrKxf+Z6VQ/Uz3iirTDyMHbjfThlRLik39EbCs2woN9HYeIbEoQRKvbPVewvQZzV6nD5vAIR/wMzL/T14kStaMI7oYUXTBsTrfz/YwNFXPgANV1QEjFMT+S7zGacduP9PG3Kx5dqu8=</P><Q>2gv5mTL6RDAitvOdFoI8Ke/PrQFne1Gx8oKrMBVoRUL9Jzzp4oWlNoDjU4N4USET3KsuR8OBgH8KH/caJC1NuDG+CQ/Bkf/iB82LuoOvaBSAKFCXRMng6LyoDFxb1ptwFavFogTrxPs3GpHYzIq4quhuP18X9bjHkiNzZ3qqCLs=</Q><DP>ihEJlKrgNVAVnFal3hTPjTjHC2QPmwtXoSmAE0sDO0qHDZUYYekCq9QiglKBHe82hju30+JExYn99roF8hYpmeK44AGp7tdgeh17OZpgMtAJCP/g1GDB6aBCikCG2F5qketC0ReQw9lS02l/8SRO9ZZJmcipQU+w6SMX4jONaZk=</DP><DQ>TPUq6sayrqeTyBm++OKr0pDf4rUzlO+oN3lpmIAGrwiVlWDLBKejuOIyHXmx/oQlILTjKGMepdJ8C0gnWA/9UBGOXY0XJHht5KLmBZfjjiVaaJTqGEdAUiR3i+Rcphi+JcQnophTNnbIJ37E/OWqlqxjJWzbR6qgyiqhM5S0e40=</DQ><InverseQ>dYOYJxCnJUZLastml8Fu9/HoW+DoX3OQ/I2b3aRBYByh7dpeRPaMdtycHgSNtMAocYDXMypl+f92vHFXENc4WFtEVAQSkxgD2IWTgQEj92kmaCA8Jrs8boH3eXfLb6ZlHf1fas5lIreq5Gzj/0IC0zDlYKBWOQf/3d+mxI9kbyw=</InverseQ><D>f2W2y/lNTueIhwGv1uDu0NOjPU5GKWH/CwoinpvKBNqbdwwOKSd35X+QeiH46Dcv/CJPIRCLSlnr4BAMlABhSD+88tVFkqs+BIO4n1nBALWdGillQMCGAsCzkPAtHiJjtib39ilfgSfv6ensz6OiU6TEhKRlAFcpbkFTCQ8+axtxer2M3TiaAarE3njI65lgOwmu+djnqDyKWzfGcA37QafIQPOnWiMIgtBWfKetnKXtk1ReqdDw2IckUNBud4EeRStM7jy7MlVNc0TXiTDK7ow+Vd2EIPGpQXkav167V4F4Lc7VYrGDrFTV2fM66uBBcmgaH3gUOVGq0qUttAuZLQ==</D></RSAKeyValue>";

	// same keypairs but without CRT
	string MonoXml384woCRT = "<RSAKeyValue><Modulus>i9COhlY/aGJD+B0fjUtOR+bbDkKyrwDzXzvwbBE8TJmtFYNrjfLHOj/gjdKCEcqb</Modulus><Exponent>EQ==</Exponent><D>GKxVYwApTqft4H2cJ/46/aE1tzjyWx5IzfYoG1Qmnzr+u1/rf+EGkyIzGWIEcyXR</D></RSAKeyValue>";
	string MonoXml448woCRT = "<RSAKeyValue><Modulus>rGhbXQawiO5muvnbq7FuVZc3QuFp2CZNqHekmRA/fdUzmUP6jFH7usaVh1kZ+Fk0em0fJzjOYgU=</Modulus><Exponent>EQ==</Exponent><D>ZWqQGJqF9jHiE6IIv1lP9hy3GEhcYQd4+a/KOvAKrWm95W6eF+aDWFgzao9lR1/BLP86HhcucSk=</D></RSAKeyValue>";
	string MonoXml512woCRT = "<RSAKeyValue><Modulus>jq1Bst+LWM5ScL29w+C/GVULDAebcT0Kl9ERGp/I2A2qJ4jHuzr7ltoQqtVGRquQwwDZQ/o1fH99bWraz5Fi7w==</Modulus><Exponent>EQ==</Exponent><D>bRsUH1+my3CZZUXNWY2/T5tizPbCKWrp+5/f5y7k4XK2d7xCN+qSZxn+nT0patdhewBMFOI9/xp5zD4U/seSIQ==</D></RSAKeyValue>";
	string MonoXml576woCRT = "<RSAKeyValue><Modulus>h18QgR8YiVXKVmcTRuPfHljm9Yz5vPt9UXg25GU2WztihEihEyZYMJaRzEb8/iS7eGg7jLXW8SrxhyLUnXfgWvtn/9uNGinj</Modulus><Exponent>EQ==</Exponent><D>b3t3AOxujzeXknMA0Pfk69C+M6FGIyl2Qxe0vBcdtItCEpYpGKlxv+l1rPu1/VOy4F8Q3u+5dMuhc2OlLfX7F6WkL5+dN7pB</D></RSAKeyValue>";
	string MonoXml640woCRT = "<RSAKeyValue><Modulus>4YELOUiQPs7Nw6AM2m14LxuKDk1dXGYhvKczD6mB2//U4sitp+MFReJ0ot840hQNwwCssw8hyyniFqIvSjKJknv+HFL4Lc3AyCP3pL5iFf0=</Modulus><Exponent>EQ==</Exponent><D>rHG9SewyEelwLC8Y40SnMxUPOB0LKIpWCLwX/OsI84dmj04qcVNAQxB0O5hmt5GXswJd1VPOdRmf/YrMiLZhIHFbLgdrQP+gyWGCSu3koz0=</D></RSAKeyValue>";
	string MonoXml704woCRT = "<RSAKeyValue><Modulus>mUDm0P3S9oPB06NJT5oDFS5+Xy/ACBvGdkvNjCiZo9bOLvVpOsiih9ipWZw9zQInjfP0uhGqDBkl9jw61yEHIqed151b9hkO2cmfb2o3GvPkbXkoMcoxBQ==</Modulus><Exponent>EQ==</Exponent><D>WiYta+CaNqfbbW8cELTywSpodDo0uXnACVnENFQeJCQA0FQfyDnJBJ2Qy0vuhbaAbz2qNXZcYp6QxbeBQQZGEB//nahkNns7KxKXDEFMMU1ghU9eoubr8Q==</D></RSAKeyValue>";
	string MonoXml768woCRT = "<RSAKeyValue><Modulus>pnVBGKuCV1sdnZP7yHMKlMnyc+4MZicSRDu6ESWPKUx/Ab2fbwwOoHDE5pSW/wCGziaEA2Agfwt6YKAr/JyPmcBcHichUjhzjDvExBrdd3jmQ1WjjNzV829+qSEq0kUl</Modulus><Exponent>EQ==</Exponent><D>kt/uJNORH+b8A4KS3gsndDm3z7Ps0prx//h3ABIU6DRwEJhBYfuUb3KPnkbflbUqONprCS1FlC0mo+Hxald/r99a5PE3JWiVTN4GHuZ3zYEviEvRtNsOG7XYKydTUO3F</D></RSAKeyValue>";
	string MonoXml832woCRT = "<RSAKeyValue><Modulus>3dVclQYr1ks5x8VXLcO0H0BRNlmg/XT01IFso0aQY5AwU/kFbDUwTSrmSZCuRBk2blp14BYvpwpviuP9WkIcjhTSuPwPD1XWSl33WHSWEc6uYEHxjn0gieHK3SiyVAgOrcVXtOaNJck=</Modulus><Exponent>EQ==</Exponent><D>aGRnzaiNGYzP5YoK6FwYhy1TZN7iWSf6vlsF8nuPPemAJ4Q+yYJxFUFdT8udTTkKjkiv8BrhBpTUlAjP9ge+BNftSibZ4zp8wd1pxewIPFYnhimAVIFasWv/1NnIP8QwH0bQQOshCxE=</D></RSAKeyValue>";
	string MonoXml896woCRT = "<RSAKeyValue><Modulus>8SR9zUf+X33JO4Gen620JE7yqQhvP0RSnU+XGE5fSHTPmy/HtuP5/IsCbfDsKOa5aAa4uj6PpxsG/6GgrbgmkxSfJmj8OfYP7Eo7wjXheJmv87Iw8y0rJ3PhkKbF82GFU/CmYQO3A77QVK74v9NMRw==</Modulus><Exponent>EQ==</Exponent><D>Ko34FSrSiVJuzkQM7wCJM5V2HdRP3fz/hSwpuP7FhUHKSI/2ETdKO6APfNAprN1sA0x687Cv8E+4mTmZ7zr/6qhiXIxKO1CYJIssptb/IfAC9McjnXeRfvn7XtHwprzmFRCfTE06ZPWD4QXJG22WIQ==</D></RSAKeyValue>";
	string MonoXml960woCRT = "<RSAKeyValue><Modulus>tsPl3wQ+EYH4tuzDokt/CknZUVNZMr9AEwZCGMzpXORmkHaBA5Dh5N24OtQwO1YQ4gXCosU9YpdQujIvki48txiL+r6/KtK1hIOm4h/LCZRZ0ztYZvufH2ysv6Sv22iEbv8/ABOmaAu7A0RueGI1dX8aZ+dIlqzz</Modulus><Exponent>EQ==</Exponent><D>rAOrLEA6atSt2VdOtt2kvmOfW5m9XPA8TiQB+TlUGzFReOgA9Ewf5nZTCjEeVfamel/GPtfBTbuXRdTf41eV1Ci1WHuXJ2/ZiN8ob06O0UAjJTlva1RGhOVVBZyI2tJOA2vJ/zPPovcdiReyOeSu3neqYqr3IUVx</D></RSAKeyValue>";
	string MonoXml1024woCRT = "<RSAKeyValue><Modulus>i+O73a/wy4fMi6V9dF9Us4oyAUvNwrJcOS69r7edDjw0MDd5vz8PcaAdL3BOwMEmItI1i8Z39HxqvuuwdB0GGCNKGh4Ft680nIAgsLugztJ4Shjm+xgFM3cFSGxJpSWkWlTpXs2ARhOSIbr5/+UwON4EvpkYxC5GbRIsrqDHHV8=</Modulus><Exponent>EQ==</Exponent><D>OZoCD/0m6mUX/UQkmVRuK89+AIigBOAH2097SFqqFOudBMuMe+zKH7pmXtPkMUB5HWWdk+hPgsnRmeiT85N6+kB7xMfgGI5AsJga19OAi2yI3RnXlTq0FMcYHZCvRE8AXUxVQTO7/n3iq3r8TEDaPmcTpn6eooXvGoA6jRCHLYk=</D></RSAKeyValue>";
	string MonoXml1536woCRT = "<RSAKeyValue><Modulus>jzYjq6zGOWfmpnzDf5SNrSifeYbFFgbh0/tO2UkLWBpbFqHdLo8CZJt1j3UtSN82ZuV6IBMJnmIsxOH3JQ677IPb76Hz/F0V07jCM88kcz3RRtCahXj/14p2EwTY3297yylWx8+zGmpQDPOxejbaGyjn2TyPx6Nq54SVSAfBUgcXuwwpksogwv/neZLyOlu/JBG5Ej11FOfJ8vi7gb/OCqK1EdzRthf9e4FS5KoxC81TiTUXyfesYpSsHI6fQ5i1</Modulus><Exponent>EQ==</Exponent><D>OvgszjgVYu6MJm+byx8NKS7YQRlgNj8RsaO3Di0iyey8GGDEfJU9OHw/d05d8NRhseYFHEQTBPs/nF0LaZynu73TCFG+0VOBdUwTuvrw5CiDWWT0VRO0pAvWRBEOAaZfiC2ViL8qyz8MYj4MM8wOUMOOx+Oy9LGa9MJOcLSkJKixqWLqxv17PVIoyMiHE+SW2m5uMleT32TVU+Mzz3lec2n1RWRGS4P14EyOf1l8WqmRlMk8jXm/xVf8w9xgbwtF</D></RSAKeyValue>";
	string MonoXml2048woCRT = "<RSAKeyValue><Modulus>ppiz9wru8QdjxD1IZ8Ouc4rpKMjR51jDq/mPu7gILbto1sD+01r/P84zFd2n9Jbv+vHJ8ClTr/9vh3dy6OzN/ASUoAMzmGnHQPsFHyaZ7TxDmF2EaF45F0rDbsPsOxlHUKkc33EuHwzXRZRdD4c2vCY8D+qrdpldkC4KH4oWjBHPk2MlqM1mPGJIvzz9nANhEl9YHHHZmAV2auZIWMWyO9zPHoE0YYMeFTHIuOZmxbu6iKX0pWcHzukx6FCqxN9zOAz5xl9D0f//0vhsiZUINM4y4ypS9C+jF8WAUthj29R3s1Hxr5I32kr3+nhtkHu7ljqLzYqCBq6hixZEhHlUlQ==</Modulus><Exponent>EQ==</Exponent><D>f2W2y/lNTueIhwGv1uDu0NOjPU5GKWH/CwoinpvKBNqbdwwOKSd35X+QeiH46Dcv/CJPIRCLSlnr4BAMlABhSD+88tVFkqs+BIO4n1nBALWdGillQMCGAsCzkPAtHiJjtib39ilfgSfv6ensz6OiU6TEhKRlAFcpbkFTCQ8+axtxer2M3TiaAarE3njI65lgOwmu+djnqDyKWzfGcA37QafIQPOnWiMIgtBWfKetnKXtk1ReqdDw2IckUNBud4EeRStM7jy7MlVNc0TXiTDK7ow+Vd2EIPGpQXkav167V4F4Lc7VYrGDrFTV2fM66uBBcmgaH3gUOVGq0qUttAuZLQ==</D></RSAKeyValue>";

	// import/export XML keypairs
	// so we know that Windows (original MS Framework) can use keypairs generated by Mono
	[Test]
	public void MonoXmlImportExport () 
	{
		rsa = new RSACryptoServiceProvider ();

		rsa.FromXmlString (MonoXml384);
		Assert.AreEqual (MonoXml384, rsa.ToXmlString (true), "Mono-Xml384");

		rsa.FromXmlString (MonoXml448);
		Assert.AreEqual (MonoXml448, rsa.ToXmlString (true), "Mono-Xml448");

		rsa.FromXmlString (MonoXml512);
		Assert.AreEqual (MonoXml512, rsa.ToXmlString (true), "Mono-Xml512");

		rsa.FromXmlString (MonoXml576);
		Assert.AreEqual (MonoXml576, rsa.ToXmlString (true), "Mono-Xml576");

		rsa.FromXmlString (MonoXml640);
		Assert.AreEqual (MonoXml640, rsa.ToXmlString (true), "Mono-Xml640");

		rsa.FromXmlString (MonoXml704);
		Assert.AreEqual (MonoXml704, rsa.ToXmlString (true), "Mono-Xml704");

		rsa.FromXmlString (MonoXml768);
		Assert.AreEqual (MonoXml768, rsa.ToXmlString (true), "Mono-Xml768");

		rsa.FromXmlString (MonoXml832);
		Assert.AreEqual (MonoXml832, rsa.ToXmlString (true), "Mono-Xml832");

		rsa.FromXmlString (MonoXml896);
		Assert.AreEqual (MonoXml896, rsa.ToXmlString (true), "Mono-Xml896");

		rsa.FromXmlString (MonoXml960);
		Assert.AreEqual (MonoXml960, rsa.ToXmlString (true), "Mono-Xml960");

		rsa.FromXmlString (MonoXml1024);
		Assert.AreEqual (MonoXml1024, rsa.ToXmlString (true), "Mono-Xml1024");

		rsa.FromXmlString (MonoXml1536);
		Assert.AreEqual (MonoXml1536, rsa.ToXmlString (true), "Mono-Xml1536");

		rsa.FromXmlString (MonoXml2048);
		Assert.AreEqual (MonoXml2048, rsa.ToXmlString (true), "Mono-Xml2048");
	}

	[Test]
	public void MonoXmlImportWithoutCRT () 
	{
		rsa = new RSACryptoServiceProvider ();
		rsa.FromXmlString (MonoXml384woCRT);
		rsa.FromXmlString (MonoXml448woCRT);
		rsa.FromXmlString (MonoXml512woCRT);
		rsa.FromXmlString (MonoXml576woCRT);
		rsa.FromXmlString (MonoXml640woCRT);
		rsa.FromXmlString (MonoXml704woCRT);
		rsa.FromXmlString (MonoXml768woCRT);
		rsa.FromXmlString (MonoXml832woCRT);
		rsa.FromXmlString (MonoXml896woCRT);
		rsa.FromXmlString (MonoXml960woCRT);
		rsa.FromXmlString (MonoXml1024woCRT);
		rsa.FromXmlString (MonoXml1536woCRT);
		rsa.FromXmlString (MonoXml2048woCRT);
	}

	// The .NET framework can import keypairs with private key BUT without
	// CRT but once imported they cannot be used or exported back
	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ExportWithoutCRT () 
	{
		try {
			rsa = new RSACryptoServiceProvider ();
			rsa.FromXmlString (MonoXml384woCRT);
		}
		catch {
		}
		// exception is HERE!
		rsa.ToXmlString (true);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ExportWithoutCRT_2 () 
	{
		try {
			rsa = new RSACryptoServiceProvider ();
			rsa.FromXmlString (MonoXml384woCRT);
		}
		catch {
		}
		// exception is HERE!
		rsa.ExportParameters (true);
	}

	// Validate that we can sign with every keypair and verify the signature
	// With Windows this means that we can use Mono keypairs to sign and verify.
	// For Mono this doesn't mean much.
	[Test]
	public void MonoSignature () 
	{
		rsa = new RSACryptoServiceProvider ();

		rsa.FromXmlString (MonoXml384);
		SignAndVerify ("Mono-384", rsa);

		rsa.FromXmlString (MonoXml448);
		SignAndVerify ("Mono-448", rsa);

		rsa.FromXmlString (MonoXml512);
		SignAndVerify ("Mono-512", rsa);

		rsa.FromXmlString (MonoXml576);
		SignAndVerify ("Mono-576", rsa);

		rsa.FromXmlString (MonoXml640);
		SignAndVerify ("Mono-640", rsa);

		rsa.FromXmlString (MonoXml704);
		SignAndVerify ("Mono-704", rsa);

		rsa.FromXmlString (MonoXml768);
		SignAndVerify ("Mono-768", rsa);

		rsa.FromXmlString (MonoXml832);
		SignAndVerify ("Mono-832", rsa);

		rsa.FromXmlString (MonoXml896);
		SignAndVerify ("Mono-896", rsa);

		rsa.FromXmlString (MonoXml960);
		SignAndVerify ("Mono-960", rsa);

		rsa.FromXmlString (MonoXml1024);
		SignAndVerify ("Mono-1024", rsa);

		rsa.FromXmlString (MonoXml1536);
		SignAndVerify ("Mono-1536", rsa);

		rsa.FromXmlString (MonoXml2048);
		SignAndVerify ("Mono-2048", rsa);
	}

	// Validate that we can verify a signature made with Mono
	// With Windows this means that we can verify Mono signatures.
	// For Mono this doesn't mean much.
	[Test]
	public void MonoVerify () 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		rsa = new RSACryptoServiceProvider ();

		rsa.FromXmlString (MonoXml384);
		byte[] sign384 = { 0x6B, 0xEF, 0x8A, 0x2E, 0x2E, 0xD5, 0xB6, 0x19, 0x2D, 0x9C, 0x48, 0x75, 0xA8, 0x54, 0xAD, 0x61, 0xD1, 0xCC, 0xF3, 0x9A, 0x3E, 0x4E, 0xE2, 0xF1, 0x44, 0x1D, 0xC4, 0x3A, 0x30, 0xF4, 0x9B, 0x2D, 0x88, 0xA7, 0xB8, 0xEC, 0x2D, 0x17, 0x4E, 0x66, 0x6C, 0x4C, 0x5A, 0xB5, 0x44, 0x4B, 0xAF, 0x06 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign384), "Mono-384-Verify");
		sign384[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign384), "Mono-384-VerBad");

		rsa.FromXmlString (MonoXml448);
		byte[] sign448 = { 0x9F, 0x62, 0xDF, 0xD4, 0x8B, 0x3E, 0x85, 0xEC, 0xF9, 0xF2, 0x17, 0x1C, 0x2B, 0x18, 0x37, 0xDA, 0xCA, 0x74, 0x5F, 0x85, 0x70, 0x41, 0x44, 0xB3, 0xE5, 0xA3, 0xC8, 0xC7, 0x37, 0x9E, 0x52, 0x16, 0x18, 0x2C, 0xE3, 0x87, 0x1A, 0x34, 0x13, 0x4A, 0x5D, 0xBB, 0x79, 0x27, 0x1C, 0x2A, 0xD2, 0x96, 0x79, 0xC4, 0x26, 0x51, 0x1B, 0x24, 0x12, 0xBC };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign448), "Mono-448-Verify");
		sign448[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign448), "Mono-448-VerBad");

		rsa.FromXmlString (MonoXml512);
		byte[] sign512 = { 0x33, 0xBB, 0x7E, 0x0B, 0xC0, 0xB2, 0x9A, 0xC2, 0x2F, 0xF4, 0xBE, 0x1B, 0xDF, 0xD6, 0x79, 0xC1, 0x38, 0x47, 0xEA, 0x05, 0xB2, 0xC0, 0xFA, 0xF8, 0xC9, 0xDC, 0x6D, 0x56, 0xBF, 0xD3, 0xBF, 0xAA, 0xB8, 0x1E, 0x74, 0xE4, 0xF3, 0x38, 0x45, 0xA9, 0x34, 0xD1, 0x5C, 0x0D, 0x0F, 0x56, 0x70, 0x0C, 0x93, 0x6B, 0xD6, 0x80, 0x17, 0xAD, 0x80, 0xF9, 0xD8, 0xFD, 0x1E, 0x6F, 0xF3, 0x5C, 0xB2, 0x78 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign512), "Mono-512-Verify");
		sign512[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign512), "Mono-512-VerBad");

		rsa.FromXmlString (MonoXml576);
		byte[] sign576 = { 0x61, 0x8C, 0xD0, 0xE3, 0x63, 0x95, 0x7C, 0xB6, 0xD8, 0x84, 0x4D, 0xD1, 0x04, 0xDF, 0x1F, 0x4A, 0xCF, 0x69, 0x95, 0x7B, 0x69, 0x8E, 0x09, 0x2A, 0x0F, 0x1B, 0x23, 0xA7, 0x20, 0x31, 0x95, 0x93, 0xD1, 0x67, 0xBC, 0x27, 0x80, 0x41, 0x60, 0xD1, 0xFE, 0x62, 0xB2, 0x17, 0xC9, 0x57, 0x4F, 0x03, 0x40, 0xDB, 0xB3, 0x7E, 0xD2, 0x0C, 0xEA, 0x7D, 0x72, 0x76, 0x64, 0x4F, 0x51, 0x79, 0x43, 0xCC, 0x8F, 0x36, 0x1E, 0x81, 0x43, 0x20, 0xB9, 0xAD };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign576), "Mono-576-Verify");
		sign576[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign576), "Mono-576-VerBad");

		rsa.FromXmlString (MonoXml640);
		byte[] sign640 = { 0x4A, 0x6A, 0xED, 0xEC, 0x96, 0x9C, 0x89, 0xD1, 0x41, 0x2E, 0x16, 0x0D, 0xBA, 0xB0, 0x48, 0x92, 0xF9, 0xA6, 0x33, 0x26, 0x0D, 0x6C, 0x99, 0xA9, 0x4E, 0x3B, 0x68, 0x82, 0xAB, 0x94, 0x33, 0x89, 0xEC, 0x8A, 0xCC, 0x32, 0xDD, 0x45, 0x9C, 0x16, 0x5E, 0xCE, 0x5F, 0xF3, 0xDC, 0x5F, 0x09, 0xC7, 0x69, 0xC7, 0xEA, 0x74, 0xAB, 0x79, 0xA7, 0x55, 0xD9, 0xF8, 0xDF, 0x8F, 0x8C, 0x9A, 0xBA, 0xA9, 0x56, 0x86, 0x96, 0x64, 0xE9, 0xC0, 0x21, 0x8C, 0x34, 0x91, 0x31, 0xC2, 0x80, 0xC7, 0x47, 0x6F };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign640), "Mono-640-Verify");
		sign640[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign640), "Mono-640-VerBad");

		rsa.FromXmlString (MonoXml704);
		byte[] sign704 = { 0x94, 0xC5, 0x45, 0xCD, 0x9C, 0xA9, 0xBC, 0xDF, 0x9D, 0x04, 0xCE, 0xFD, 0x21, 0xEB, 0x3F, 0xBE, 0x32, 0x56, 0xC3, 0x6B, 0xEF, 0x4E, 0x8F, 0xA9, 0x02, 0x14, 0xC4, 0xF1, 0xFA, 0x9B, 0x35, 0xFE, 0x36, 0x34, 0x03, 0x31, 0xC1, 0xC8, 0xBF, 0xA1, 0x41, 0x03, 0xCB, 0xE1, 0xB9, 0x81, 0x76, 0x60, 0xC9, 0xD1, 0xB4, 0x04, 0x98, 0xA5, 0xDF, 0x4F, 0x37, 0x60, 0xB8, 0x15, 0xF8, 0x22, 0xB7, 0x54, 0x32, 0x87, 0x19, 0x9B, 0xB9, 0xDF, 0xB9, 0x2D, 0x25, 0xA2, 0xAF, 0x04, 0xC1, 0xE0, 0xF0, 0x0B, 0xF2, 0xCC, 0x0E, 0x1F, 0x40, 0xF6, 0x5C, 0xCD };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign704), "Mono-704-Verify");
		sign704[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign704), "Mono-704-VerBad");

		rsa.FromXmlString (MonoXml768);
		byte[] sign768 = { 0x28, 0x99, 0xE0, 0xD2, 0xCF, 0xD3, 0x2B, 0x1B, 0xA2, 0x06, 0xC5, 0x17, 0x86, 0x07, 0xC8, 0x49, 0x77, 0x33, 0xEC, 0xFE, 0xC9, 0x15, 0x99, 0x90, 0x4C, 0x6C, 0xED, 0x2C, 0x32, 0xF8, 0xAB, 0x0A, 0xB6, 0xEB, 0x15, 0x08, 0x4A, 0xC1, 0xDD, 0xAD, 0x88, 0x47, 0xAD, 0x3D, 0xA2, 0x4B, 0x94, 0x7E, 0x37, 0x1F, 0x33, 0xFC, 0xC3, 0xFE, 0xC2, 0x27, 0x45, 0x74, 0x8E, 0x3C, 0xC8, 0x68, 0x8C, 0xF4, 0x77, 0xCC, 0xD0, 0x79, 0x37, 0x7E, 0x26, 0x1B, 0xDE, 0xBF, 0x16, 0x3E, 0xAE, 0xB9, 0xEB, 0xA0, 0x00, 0xCE, 0x51, 0x8A, 0x69, 0x12, 0xF5, 0xBE, 0x39, 0x1C, 0x0F, 0x0F, 0xD8, 0x13, 0xDD, 0x6B };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign768), "Mono-768-Verify");
		sign768[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign768), "Mono-768-VerBad");

		rsa.FromXmlString (MonoXml832);
		byte[] sign832 = { 0x8D, 0x56, 0xB2, 0x3C, 0x45, 0x47, 0xB8, 0x6F, 0x56, 0x7D, 0x85, 0x4F, 0x3C, 0x14, 0xAC, 0x61, 0x08, 0x8C, 0x6A, 0xF3, 0xAF, 0xCA, 0x65, 0xCC, 0xC3, 0x6F, 0x53, 0x6F, 0x84, 0xC1, 0x9F, 0xFD, 0x66, 0x83, 0xA1, 0x3B, 0xFF, 0x61, 0x41, 0xDB, 0x2C, 0xE5, 0xD5, 0x13, 0x3E, 0x15, 0x7A, 0xBD, 0x1F, 0x44, 0x4C, 0x4F, 0x10, 0xB7, 0x4A, 0x4B, 0x9D, 0xD1, 0xF5, 0xC2, 0x4E, 0x4D, 0xC8, 0xF9, 0x51, 0xC9, 0xF3, 0x04, 0x69, 0x02, 0xAE, 0x7E, 0xC3, 0x76, 0x56, 0x08, 0x8E, 0xF7, 0xA1, 0x25, 0x1B, 0x97, 0x65, 0x8E, 0x3A, 0x98, 0x52, 0xDB, 0x3E, 0xF4, 0x59, 0x5F, 0xEE, 0x6D, 0x78, 0x22, 0x37, 0x2E, 0x79, 0x69, 0x6D, 0x8F, 0xF9, 0xDF };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign832), "Mono-832-Verify");
		sign832[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign832), "Mono-832-VerBad");

		rsa.FromXmlString (MonoXml896);
		byte[] sign896 = { 0xAB, 0xD9, 0xE4, 0x5C, 0xAB, 0xF7, 0xE1, 0x06, 0x7F, 0x0C, 0xD6, 0x03, 0xB5, 0xA1, 0xDB, 0x22, 0x3E, 0x85, 0xBE, 0xCD, 0x54, 0x12, 0xD5, 0x11, 0x71, 0xCC, 0xEE, 0x71, 0x4B, 0xA8, 0xB5, 0xAE, 0x04, 0x82, 0x78, 0x72, 0xE8, 0x7B, 0x77, 0x42, 0x58, 0x14, 0xA3, 0xF0, 0xFC, 0xE9, 0x15, 0x98, 0x2E, 0x34, 0x57, 0xAA, 0x95, 0x6F, 0x2E, 0x9B, 0x5B, 0x88, 0xF0, 0x51, 0x26, 0x35, 0x1F, 0x0C, 0x59, 0x51, 0x73, 0x0E, 0xD3, 0x5D, 0x02, 0xAD, 0xB7, 0x1C, 0x94, 0x57, 0x1E, 0xA6, 0x01, 0x19, 0x73, 0x29, 0xBE, 0xDF, 0x77, 0xF2, 0x13, 0x28, 0xBD, 0x50, 0x89, 0x39, 0x6A, 0x1B, 0xEF, 0x66, 0x29, 0x71, 0x55, 0x44, 0x3A, 0x36, 0x89, 0xC1, 0xFE, 0x25, 0x32, 0x9F, 0x65, 0x76, 0xEA, 0x71 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign896), "Mono-896-Verify");
		sign896[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign896), "Mono-896-VerBad");

		rsa.FromXmlString (MonoXml960);
		byte[] sign960 = { 0x7A, 0x13, 0x06, 0xF1, 0xB5, 0x14, 0x78, 0x33, 0xA6, 0x14, 0x40, 0xDC, 0x22, 0xB6, 0xF3, 0xBB, 0xDC, 0xCD, 0x53, 0xAF, 0x05, 0xC5, 0x84, 0x0C, 0xD2, 0x15, 0x5A, 0x04, 0xFC, 0x30, 0x57, 0x64, 0xF4, 0x8B, 0xD6, 0x5D, 0x5B, 0x3D, 0xFC, 0x82, 0x6B, 0xB9, 0xE4, 0xB9, 0x56, 0xB3, 0xCF, 0x2A, 0x6D, 0x14, 0x83, 0x31, 0x9E, 0xFD, 0x55, 0x4B, 0x2C, 0xBD, 0x5A, 0xA8, 0xAC, 0xD6, 0x5D, 0xFA, 0x58, 0xF8, 0x3E, 0x60, 0x32, 0x11, 0xA4, 0x09, 0xC2, 0x01, 0xE7, 0x14, 0xEF, 0xBA, 0x51, 0x57, 0x86, 0xA5, 0x86, 0x28, 0x63, 0x68, 0x33, 0xBF, 0x34, 0xF9, 0x1D, 0x36, 0xC3, 0x6E, 0xCF, 0x97, 0xA5, 0x19, 0xB4, 0x5F, 0x7B, 0x70, 0xB0, 0x72, 0xF2, 0xF2, 0xFF, 0x05, 0xB7, 0x31, 0x5A, 0x5D, 0xC6, 0x88, 0x88, 0x21, 0x97, 0x40, 0x0E, 0x0E };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign960), "Mono-960-Verify");
		sign960[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign960), "Mono-960-VerBad");

		rsa.FromXmlString (MonoXml1024);
		byte[] sign1024 = { 0x26, 0x98, 0x40, 0x5F, 0x30, 0x4E, 0x90, 0x20, 0x32, 0x10, 0x64, 0xCF, 0x03, 0xA8, 0x1E, 0x53, 0x20, 0x19, 0x59, 0xCB, 0x08, 0x5F, 0x8D, 0x45, 0x51, 0xEE, 0xDD, 0x71, 0x2E, 0x21, 0x86, 0xB0, 0xC6, 0xE2, 0x6F, 0x2A, 0xF2, 0x8E, 0xBD, 0xDE, 0xAD, 0xA0, 0x56, 0x7E, 0xED, 0x38, 0x4F, 0x8D, 0x3A, 0xC6, 0x8A, 0x15, 0x34, 0x71, 0xDE, 0xC5, 0x60, 0x32, 0x95, 0x38, 0xD7, 0x69, 0x0F, 0x3B, 0xDF, 0xF8, 0x4C, 0x2D, 0x83, 0x58, 0x7E, 0x36, 0x4B, 0x10, 0x4A, 0x8B, 0x23, 0x70, 0x09, 0xBE, 0xCF, 0x02, 0x2E, 0x97, 0xC4, 0x0F, 0x94, 0x42, 0x0B, 0xFA, 0x1F, 0x16, 0x97, 0x25, 0x1C, 0x14, 0x2B, 0x82, 0xD0, 0x7A, 0xC1, 0x2C, 0x2F, 0x72, 0x9C, 0xD9, 0xEE, 0x90, 0x2F, 0x5B, 0xC5, 0xB1, 0x34, 0x73, 0x10, 0xEC, 0x79, 0x97, 0x9A, 0xDA, 0xE2, 0xB4, 0xAE, 0x99, 0xE9, 0xA2, 0xFF, 0xC6 };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign1024), "Mono-1024-Verify");
		sign1024[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign1024), "Mono-1024-VerBad");

		rsa.FromXmlString (MonoXml1536);
		byte[] sign1536 = { 0x33, 0xE2, 0xA4, 0xCE, 0x18, 0xE6, 0xF3, 0x46, 0x0E, 0x32, 0xD1, 0xA6, 0x8A, 0xC3, 0xCA, 0x4B, 0x36, 0x4E, 0x4C, 0xAE, 0x39, 0x95, 0x5A, 0x05, 0x37, 0xBA, 0x0F, 0x19, 0xDC, 0x94, 0x6A, 0x78, 0xDA, 0xEA, 0xF0, 0xA2, 0x80, 0x47, 0xD5, 0xB9, 0xC3, 0x53, 0xC9, 0xDA, 0x0B, 0x29, 0xCA, 0x61, 0x37, 0x7C, 0xD5, 0x5D, 0x99, 0x58, 0xAD, 0x0F, 0xA8, 0xEF, 0x17, 0xFD, 0xA8, 0x55, 0x79, 0xEF, 0x07, 0xD1, 0x63, 0xE0, 0x2C, 0xEF, 0x14, 0x42, 0x72, 0x2D, 0x71, 0xA3, 0xBB, 0x29, 0x87, 0xF8, 0xCC, 0xFB, 0x70, 0xCC, 0x13, 0x70, 0x24, 0xC3, 0x2A, 0x2B, 0xD2, 0x1C, 0x34, 0xD7, 0x85, 0xBC, 0xA4, 0x4E, 0x7B, 0x7E, 0x1C, 0x5B, 0x03, 0x06, 0xB2, 0x01, 0xBF, 0x73, 0x30, 0x77, 0xEB, 0x03, 0x17, 0x24, 0xFE, 0x46, 0xC7, 0x9B, 0xEB, 0x75, 0xF6, 0x56, 0x43, 0x1E, 0x0D, 0x56, 0x05, 0x37, 0x78, 0xB3, 0x76, 0x93, 0x76, 0xFA, 0x73, 0xCD, 0xE5, 0xB1, 0x3C, 0x60, 0xB7, 0xCC, 0x1E, 0x98, 0x89, 0xD8, 0xB4, 0x0A, 0xD3, 0x52, 0xCD, 0xEF, 0xC1, 0xBE, 0xFC, 0xA8, 0x2C, 0xE6, 0x01, 0xD3, 0xB3, 0x05, 0x5C, 0x12, 0x48, 0xD8, 0x20, 0xF9, 0x3B, 0xAD, 0x97, 0xD4, 0xD1, 0x13, 0xD6, 0xA5, 0x31, 0x4E, 0x52, 0x13, 0xBD, 0x5C, 0x00, 0x5C, 0x2A, 0x86, 0xFC, 0x98, 0x8B, 0x93, 0xAE, 0x5A };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign1536), "Mono-1536-Verify");
		sign1536[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign1536), "Mono-1536-VerBad");

		rsa.FromXmlString (MonoXml2048);
		byte[] sign2048 = { 0xA5, 0x70, 0x6B, 0x3C, 0x5E, 0x5D, 0x49, 0x7C, 0xCB, 0xEE, 0xE3, 0x23, 0xF5, 0xD7, 0xEE, 0xF3, 0xA8, 0x8A, 0xED, 0x47, 0x5F, 0x2A, 0x03, 0x72, 0x41, 0x02, 0x1E, 0x5D, 0x93, 0x3B, 0x27, 0x4B, 0x2D, 0x7A, 0x21, 0x50, 0x7B, 0xDC, 0xFB, 0x0F, 0xCB, 0xEB, 0x8E, 0xB5, 0x4C, 0x44, 0x90, 0x39, 0xF0, 0xCB, 0x4A, 0x5E, 0xD7, 0x67, 0x5C, 0x46, 0xC3, 0x3C, 0x94, 0xDC, 0x33, 0x36, 0x36, 0xA5, 0xF3, 0xCE, 0x1F, 0xA7, 0x8F, 0x79, 0xB8, 0x60, 0x94, 0x0F, 0x7A, 0x87, 0x18, 0x12, 0xCD, 0x21, 0x54, 0x05, 0x53, 0xA0, 0x88, 0x1F, 0x61, 0x1F, 0xAB, 0xEC, 0x6D, 0xCF, 0x10, 0xE0, 0x8D, 0x14, 0x5C, 0x6A, 0x46, 0x8C, 0xB9, 0xB6, 0x52, 0x38, 0x1F, 0xAE, 0xF1, 0xB8, 0xB5, 0x9B, 0x3C, 0xE1, 0x6E, 0xBE, 0x21, 0x1B, 0x01, 0x1E, 0xD9, 0x1E, 0x97, 0x78, 0x47, 0xC9, 0x86, 0xC4, 0xE7, 0x58, 0xF8, 0xEB, 0xAC, 0x22, 0x38, 0xD4, 0x2A, 0xE8, 0x1B, 0x40, 0x5F, 0xAF, 0x35, 0xFA, 0x13, 0x30, 0x0E, 0x5C, 0x4C, 0xF5, 0xF1, 0xB3, 0x31, 0x6C, 0x1D, 0x96, 0xFD, 0xAB, 0xC4, 0x0E, 0x16, 0x0A, 0xF9, 0x28, 0x49, 0x59, 0xF1, 0xB6, 0x35, 0x2D, 0x21, 0x69, 0x4F, 0xD0, 0x5B, 0xB7, 0x7E, 0xC3, 0x00, 0xC7, 0xDA, 0x56, 0x48, 0xA0, 0x93, 0x05, 0xB4, 0x6D, 0xEE, 0x2D, 0x6A, 0x60, 0xF6, 0x91, 0x7C, 0xDB, 0xD8, 0xC3, 0xFD, 0x33, 0xBC, 0xC9, 0x68, 0x73, 0xC6, 0x64, 0x49, 0x80, 0x3C, 0xD5, 0x4A, 0xE1, 0x28, 0x5F, 0xE9, 0x2A, 0xA7, 0x1B, 0xA6, 0x38, 0x2A, 0xAE, 0x0A, 0xA8, 0xCC, 0x96, 0x9A, 0xEA, 0xE5, 0xAD, 0xB4, 0xD8, 0x70, 0x2F, 0xFA, 0xAD, 0x17, 0x03, 0x4D, 0xDA, 0x2E, 0x1B, 0x4D, 0x88, 0x82, 0x3E, 0x04, 0x6A, 0xAF, 0x38, 0xC1, 0x4F, 0x1B, 0x9D, 0x07, 0x1A, 0x67, 0x6A };
		Assert.IsTrue (rsa.VerifyHash (hash, sha1OID, sign2048), "Mono-2048-Verify");
		sign2048[0] = 0x00;
		Assert.IsFalse (rsa.VerifyHash (hash, sha1OID, sign2048), "Mono-2048-VerBad");
	}

	// Key Pair Persistence Tests
	// References
	// a.	.Net Framework Cryptography Frequently Asked Questions, Question 8
	//	http://www.gotdotnet.com/team/clr/cryptofaq.htm
	// b.	Generating Keys for Encryption and Decryption
	//	http://msdn.microsoft.com/library/en-us/cpguide/html/cpcongeneratingkeysforencryptiondecryption.asp

	[Test]
	public void Persistence_PersistKeyInCsp_False () 
	{
		CspParameters csp = new CspParameters (1, null, "Persistence_PersistKeyInCsp_False");
		// MS generates (or load) keypair here
		// Mono load (if it exists) the keypair here
		RSACryptoServiceProvider rsa1 = new RSACryptoServiceProvider (minKeySize, csp);
		// Mono will generate the keypair here (if it doesn't exists)
		string first = rsa1.ToXmlString (true);

		// persistance is "on" by default when a CspParameters is supplied
		Assert.IsTrue (rsa1.PersistKeyInCsp, "PersistKeyInCsp");

		// this means nothing if we don't call Clear !!!
		rsa1.PersistKeyInCsp = false;
		Assert.IsFalse (rsa1.PersistKeyInCsp, "PersistKeyInCsp");

		// reload using the same container name
		RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider (minKeySize, csp);
		string second = rsa2.ToXmlString (true);

		Assert.AreEqual (first, second, "Key Pair Same Container");
	}

	[Test]
	public void Persistence_PersistKeyInCsp_True () 
	{
		CspParameters csp = new CspParameters (1, null, "Persistence_PersistKeyInCsp_True");
		// MS generates (or load) keypair here
		// Mono load (if it exists) the keypair here
		RSACryptoServiceProvider rsa1 = new RSACryptoServiceProvider (minKeySize, csp);
		// Mono will generate the keypair here (if it doesn't exists)
		string first = rsa1.ToXmlString (true);

		// persistance is "on" by default
		Assert.IsTrue (rsa1.PersistKeyInCsp, "PersistKeyInCsp");

		// reload using the same container name
		RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider (minKeySize, csp);
		string second = rsa2.ToXmlString (true);

		Assert.AreEqual (first, second, "Key Pair Same Container");
	}

	[Test]
	public void Persistence_Delete () 
	{
		CspParameters csp = new CspParameters (1, null, "Persistence_Delete");
		// MS generates (or load) keypair here
		// Mono load (if it exists) the keypair here
		RSACryptoServiceProvider rsa1 = new RSACryptoServiceProvider (minKeySize, csp);
		// Mono will generate the keypair here (if it doesn't exists)
		string original = rsa1.ToXmlString (true);

		// note: Delete isn't well documented but can be done by 
		// flipping the PersistKeyInCsp to false and back to true.
		rsa1.PersistKeyInCsp = false;
		rsa1.Clear ();

		// recreate using the same container name
		RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider (minKeySize, csp);
		string newKeyPair = rsa2.ToXmlString (true);

		Assert.IsTrue ((original != newKeyPair), "Key Pair Deleted");
	}

	[Test]
	public void UseMachineKeyStore_Default ()
	{
		Assert.IsFalse (RSACryptoServiceProvider.UseMachineKeyStore, "UseMachineKeyStore(Default)");
	}

	[Test]
	[Category("AndroidSdksNotWorking")]
	public void UseMachineKeyStore () 
	{
		// note only applicable when CspParameters isn't used - which don't
		// help much as you can't know the generated key container name
		try {
			RSACryptoServiceProvider.UseMachineKeyStore = true;
			CspParameters csp = new CspParameters (1, null, "UseMachineKeyStore");
			csp.KeyContainerName = "UseMachineKeyStore";
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider (csp);
			string machineKeyPair = rsa.ToXmlString (true);
			rsa.Clear ();

			RSACryptoServiceProvider.UseMachineKeyStore = false;
			csp = new CspParameters (1, null, "UseMachineKeyStore");
			csp.Flags |= CspProviderFlags.UseMachineKeyStore;
			rsa = new RSACryptoServiceProvider (csp);

			Assert.IsTrue (machineKeyPair != rsa.ToXmlString (true), "UseMachineKeyStore");
		}
		catch (CryptographicException ce) {
			// only root can create the required directory (if inexistant)
			// afterward anyone can use (read from) it
			if (!(ce.InnerException is UnauthorizedAccessException) && !(ce.InnerException is IOException ioe && ioe.HResult == 30 /* Read-only file system */))
				throw;
		}
		catch (UnauthorizedAccessException) {
		}
	}

	[Test]
	public void PKCS1 () 
	{
		byte[] data = new byte [8];
		rsa = new RSACryptoServiceProvider (minKeySize);
		byte[] encdata = rsa.Encrypt (data, false);
		byte[] decdata = rsa.Decrypt (encdata, false);
		Assert.AreEqual (BitConverter.ToString (data), BitConverter.ToString (decdata), "PKCS1");
	}

	[Test]
	public void OAEP () 
	{
		byte[] data = new byte [8];
		rsa = new RSACryptoServiceProvider (minKeySize);
		try {
			byte[] encdata = rsa.Encrypt (data, true);
			byte[] decdata = rsa.Decrypt (encdata, true);
			Assert.AreEqual (BitConverter.ToString (data), BitConverter.ToString (decdata), "PKCS1");
		}
		catch (CryptographicException) {
			// will fail on MS runtime before Windows XP
		}
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void Bug79269 ()
	{
		rsa = new RSACryptoServiceProvider ();
		rsa.FromXmlString ("<RSAKeyValue><Modulus>iSObDmmhDgrl4NiLaviFcpv4NdysBWJcqiVz3AQbPdajtXaQQ8VJdfRkixah132yKOFGCWZhHS3EuPMh8dcNwGwta2nh+m2IV6ktzI4+mZ7CSNAsmlDY0JI+H8At1vKvNArlC5jkVGuliYroJeSU/NLPLNYgspi7TtXGy9Rfug8=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>");
		Assert.IsTrue (rsa.PublicOnly, "PublicOnly");
		string b64 = @"YgyAhscnTTIcDeLJTZcOYYyHVxNhV6d03jeZYjq0sPMEsfCCbE/NcFyYHD9BTuiduqPplCLbGpfZIZYJ6vAP9m5z4Q9eEw79kmEFCsm8wSKEo/gKiptVpwQ78VOPrWd/wEkTTeeg2nVim3JIsTKGFlV7rKxIWQhGN9aAqgP8nZI=";
		byte [] bytes = Convert.FromBase64String (b64);
		rsa.Decrypt (bytes, true);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void Bug79320_OAEP ()
	{
		string s = "hdphq/mn8goBi43YGPkmOfPj5vXjHrKPJkT4mLT3l+XzLttHMLC4/yBYkuzlXtbrl2jtAJRb6oA8UcQFalUMnCa09LDZrgNU2yySn7YbiG8raSq7u2nfDCbPu+c8T9fyHxrCHrX0zeqqImX33csIn6rIrQZ8HKcMsoQso4qtS2A=";
		byte [] bytes = Convert.FromBase64String (s);
		RSACryptoServiceProvider r = new RSACryptoServiceProvider ();
		r.FromXmlString ("<RSAKeyValue><Modulus>iSObDmmhDgrl4NiLaviFcpv4NdysBWJcqiVz3AQbPdajtXaQQ8VJdfRkixah132yKOFGCWZhHS3EuPMh8dcNwGwta2nh+m2IV6ktzI4+mZ7CSNAsmlDY0JI+H8At1vKvNArlC5jkVGuliYroJeSU/NLPLNYgspi7TtXGy9Rfug8=</Modulus><Exponent>EQ==</Exponent><P>pd4svtxrnTWXVSb151/JFgT9szI6dxQ5pAFPd4A4yuxLLEay2W2z7d9LVk5siMFhZ10uTJGWzNP5pSgLT8wdww==</P><Q>06j6m4cGRc3uoKVuFFGA19JG3Bi4tDBEQHebEc/Y3+eThrOasYIRrQmGUfqWnd9eFitO9GOaVJ0muNDV7NOxxQ==</Q><DP>OoqmYXr4zhLqHg3AM4s36adomZlBz6zJDLUrGx4yKYCTAJFsTL1OkDCxLYUXP1NPjeSm7dkIDA6UWGh8doRGvQ==</DP><DQ>PkDCLb5NI5br1OVcnJBxMGsFyEOBnmiMi2545x8DjSX+Nq1LnZ6555ljvcIsTIz9jgy83nel3KaxCS5dCWtwhQ==</DQ><InverseQ>OrFYaG7wTqim/bub4qY0CvIfhsjG4/4MEabg0UFTf/+tekKas7DDKg2TD5BS2q0O3XEt7xIfp0S6dpOAnrlyGQ==</InverseQ><D>IESc9FUW1iCuj0ICr8IBSCSy3383iMvZkXI5YPHoSskXdf3Hl3m27pPbbAVTQcM4+o9bxfn4u5JMZ8C8sV/G/8Cfl4ss1NVMbZOecvVObRqRpqXaveq5fN2X0EklH1wzm5w3O8cMXdbC/hc0gKUqaMjFVn1zpf3zVjpOkY0eGRE=</D></RSAKeyValue>");
		Assert.IsNotNull (r.Decrypt (bytes, true));
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void Bug79320_PKCS1 ()
	{
		string s = "hdphq/mn8goBi43YGPkmOfPj5vXjHrKPJkT4mLT3l+XzLttHMLC4/yBYkuzlXtbrl2jtAJRb6oA8UcQFalUMnCa09LDZrgNU2yySn7YbiG8raSq7u2nfDCbPu+c8T9fyHxrCHrX0zeqqImX33csIn6rIrQZ8HKcMsoQso4qtS2A=";
		byte [] bytes = Convert.FromBase64String (s);
		RSACryptoServiceProvider r = new RSACryptoServiceProvider ();
		r.FromXmlString ("<RSAKeyValue><Modulus>iSObDmmhDgrl4NiLaviFcpv4NdysBWJcqiVz3AQbPdajtXaQQ8VJdfRkixah132yKOFGCWZhHS3EuPMh8dcNwGwta2nh+m2IV6ktzI4+mZ7CSNAsmlDY0JI+H8At1vKvNArlC5jkVGuliYroJeSU/NLPLNYgspi7TtXGy9Rfug8=</Modulus><Exponent>EQ==</Exponent><P>pd4svtxrnTWXVSb151/JFgT9szI6dxQ5pAFPd4A4yuxLLEay2W2z7d9LVk5siMFhZ10uTJGWzNP5pSgLT8wdww==</P><Q>06j6m4cGRc3uoKVuFFGA19JG3Bi4tDBEQHebEc/Y3+eThrOasYIRrQmGUfqWnd9eFitO9GOaVJ0muNDV7NOxxQ==</Q><DP>OoqmYXr4zhLqHg3AM4s36adomZlBz6zJDLUrGx4yKYCTAJFsTL1OkDCxLYUXP1NPjeSm7dkIDA6UWGh8doRGvQ==</DP><DQ>PkDCLb5NI5br1OVcnJBxMGsFyEOBnmiMi2545x8DjSX+Nq1LnZ6555ljvcIsTIz9jgy83nel3KaxCS5dCWtwhQ==</DQ><InverseQ>OrFYaG7wTqim/bub4qY0CvIfhsjG4/4MEabg0UFTf/+tekKas7DDKg2TD5BS2q0O3XEt7xIfp0S6dpOAnrlyGQ==</InverseQ><D>IESc9FUW1iCuj0ICr8IBSCSy3383iMvZkXI5YPHoSskXdf3Hl3m27pPbbAVTQcM4+o9bxfn4u5JMZ8C8sV/G/8Cfl4ss1NVMbZOecvVObRqRpqXaveq5fN2X0EklH1wzm5w3O8cMXdbC/hc0gKUqaMjFVn1zpf3zVjpOkY0eGRE=</D></RSAKeyValue>");
		Assert.IsNotNull (r.Decrypt (bytes, true));
	}

#if !MOBILE
	[Test]
	[Category ("NotWorking")]
	public void CspKeyContainerInfo_NewKeypair ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		CspKeyContainerInfo info = rsa.CspKeyContainerInfo;
		Assert.IsTrue (info.Accessible, "Accessible");
// FIXME	AssertNotNull ("CryptoKeySecurity", info.CryptoKeySecurity);
		Assert.IsTrue (info.Exportable, "Exportable");
		Assert.IsFalse (info.HardwareDevice, "HardwareDevice");
		Assert.IsNotNull (info.KeyContainerName, "KeyContainerName");
		Assert.AreEqual (KeyNumber.Exchange, info.KeyNumber, "KeyNumber");
		Assert.IsFalse (info.MachineKeyStore, "MachineKeyStore");
		Assert.IsFalse (info.Protected, "Protected");
		Assert.IsTrue (info.RandomlyGenerated, "RandomlyGenerated");
		Assert.IsFalse (info.Removable, "Removable");
		Assert.IsNotNull (info.UniqueKeyContainerName, "UniqueKeyContainerName");
	}

	[Test]
	[Category ("NotWorking")]
	public void CspKeyContainerInfo_ImportedKeypair ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		RSAParameters rsap = AllTests.GetRsaKey (true);
		rsa.ImportParameters (rsap);
		CspKeyContainerInfo info = rsa.CspKeyContainerInfo;
		Assert.IsTrue (info.Accessible, "Accessible");
// FIXME	AssertNotNull ("CryptoKeySecurity", info.CryptoKeySecurity);
		Assert.IsTrue (info.Exportable, "Exportable");
		Assert.IsFalse (info.HardwareDevice, "HardwareDevice");
		Assert.IsNotNull (info.KeyContainerName, "KeyContainerName");
		Assert.AreEqual (KeyNumber.Exchange, info.KeyNumber, "KeyNumber");
		Assert.IsFalse (info.MachineKeyStore, "MachineKeyStore");
		Assert.IsFalse (info.Protected, "Protected");
		Assert.IsTrue (info.RandomlyGenerated, "RandomlyGenerated");
		Assert.IsFalse (info.Removable, "Removable");
		Assert.IsNotNull (info.UniqueKeyContainerName, "UniqueKeyContainerName");
	}

	[Test]
	[Category ("NotWorking")]
	// This case wasn't fixed in Nov CTP
	public void CspKeyContainerInfo_ImportedPublicKey ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		RSAParameters rsap = AllTests.GetRsaKey (false);
		rsa.ImportParameters (rsap);
		CspKeyContainerInfo info = rsa.CspKeyContainerInfo;
		Assert.IsFalse (info.Accessible, "Accessible");
		// info.CryptoKeySecurity throws a CryptographicException at this stage
		// info.Exportable throws a CryptographicException at this stage
		Assert.IsFalse (info.HardwareDevice, "HardwareDevice");
		Assert.IsNotNull (info.KeyContainerName, "KeyContainerName");
		Assert.AreEqual (KeyNumber.Exchange, info.KeyNumber, "KeyNumber");
		Assert.IsFalse (info.MachineKeyStore, "MachineKeyStore");
		// info.Protected throws a CryptographicException at this stage
		Assert.IsTrue (info.RandomlyGenerated, "RandomlyGenerated");
		Assert.IsFalse (info.Removable, "Removable");
		// info.UniqueKeyContainerName throws a CryptographicException at this stage
	}
#endif
	[Test]
	public void ExportCspBlob_Full () 
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		RSAParameters rsap = AllTests.GetRsaKey (true);
		rsa.ImportParameters (rsap);

		byte[] keypair = rsa.ExportCspBlob (true);
		Assert.AreEqual ("07-02-00-00-00-A4-00-00-52-53-41-32-00-04-00-00-11-00-00-00-CB-BD-1D-09-FD-E5-F8-46-59-8F-2A-CA-98-72-53-E2-7F-68-C1-F6-41-9A-7A-52-1F-A5-61-7B-2D-B1-AA-E0-4E-39-98-45-45-B2-34-88-74-53-09-06-9D-64-6A-EE-84-25-3A-D9-B7-B4-E6-3E-72-37-C7-DF-A3-E0-B8-AF-7F-80-8B-5B-8A-9D-71-19-46-EC-E1-60-0D-52-ED-76-48-CD-6F-EB-CE-48-EA-61-AB-02-5C-03-AF-BA-DF-B8-1F-F5-54-74-F0-B6-D6-40-A4-43-10-D4-EE-07-8D-36-F7-71-A8-9D-2B-AC-38-23-9C-CE-82-06-09-2F-F8-BB-99-65-FB-58-2A-BA-41-75-39-1F-9D-45-76-21-25-5B-2D-0A-04-AA-E7-FA-28-7E-3B-1E-5D-6E-23-F0-4E-12-32-F6-84-3D-9E-1A-B8-93-A4-FD-F4-AE-44-9F-EB-99-01-60-B5-A1-10-0B-81-08-C9-B3-B9-B1-81-AE-CF-EE-03-15-46-AF-00-E7-41-A4-A5-16-04-4D-52-52-33-CE-CF-B5-04-32-B4-A3-0D-EA-92-2E-B4-66-16-B4-40-98-86-9D-8B-02-35-20-0F-5A-D0-B1-66-88-D0-42-6C-3F-35-D9-D1-AA-EA-33-12-34-F6-53-F4-27-F0-B1-7F-C9-81-C9-67-1F-3C-C9-AD-74-41-47-0A-91-FC-38-2B-20-2E-9A-2E-69-F7-C1-59-FF-14-74-B7-EA-75-09-49-D9-3E-B1-F2-51-B0-54-9B-AC-D0-A4-83-01-DC-DD-07-EB-5A-9A-D6-FC-23-40-E2-E4-37-03-BA-3E-A6-4C-49-54-3D-DA-36-98-5A-24-F9-39-FE-07-98-84-95-A4-99-1B-0D-22-7A-E4-8A-FD-31-E9-42-E3-8A-6F-8F-3F-80-F9-A7-46-31-4C-A9-56-05-5C-1C-99-7E-8A-3A-BD-AD-61-A9-4C-86-78-F8-B7-51-03-75-F0-0A-FF-90-02-1E-47-F7-39-80-79-25-FB-ED-66-36-EA-98-E6-56-96-0F-2E-9E-4E-2B-40-BF-67-A9-67-B1-83-58-4F-15-32-86-C9-9F-11-E2-39-CD-7F-07-93-50-88-53-34-F7-71-F2-80-B3-23-94-AE-DB-5B-26-8D-19-01-63-BB-DA-4F-6C-B0-C1-B3-1C-5B-C8-4B-3A-46-E4-3F-88-B8-B7-C2-28-94-AF-94-48-CC-CE-A1-E1-FB-DD-3E-A1-74-4F-ED-26-99-93-0A-D2-F5-BE-C3-8D-D3-2C-09-6C-C5-68-AF-6A-E5-44-94-52-A9-F1-76-1C-11-BC-CF-AC-50-59-AD-F3-7F-C9-DA-3E-C9-2E-B8-DC-7F-E3-39-A9-82-C0-A2-0D-87-D6-69-26-39-5C-5E-74-65-C4-6B-8D-99-C7-B3-10-94-B2-41-96-57-01-38-81-84-27-B4-68-06-1E-25-31-3F-F8-CD-C1-30-DB-88-B9-C4-89-F2-FA-41-53-FC-DA-A5", BitConverter.ToString (keypair));
	}

	[Test]
	public void ExportCspBlob_PublicOnly ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		RSAParameters rsap = AllTests.GetRsaKey (true);
		rsa.ImportParameters (rsap);

		byte[] pubkey = rsa.ExportCspBlob (false);
		Assert.AreEqual ("06-02-00-00-00-A4-00-00-52-53-41-31-00-04-00-00-11-00-00-00-CB-BD-1D-09-FD-E5-F8-46-59-8F-2A-CA-98-72-53-E2-7F-68-C1-F6-41-9A-7A-52-1F-A5-61-7B-2D-B1-AA-E0-4E-39-98-45-45-B2-34-88-74-53-09-06-9D-64-6A-EE-84-25-3A-D9-B7-B4-E6-3E-72-37-C7-DF-A3-E0-B8-AF-7F-80-8B-5B-8A-9D-71-19-46-EC-E1-60-0D-52-ED-76-48-CD-6F-EB-CE-48-EA-61-AB-02-5C-03-AF-BA-DF-B8-1F-F5-54-74-F0-B6-D6-40-A4-43-10-D4-EE-07-8D-36-F7-71-A8-9D-2B-AC-38-23-9C-CE-82-06-09-2F-F8-BB", BitConverter.ToString (pubkey));
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ExportCspBlob_MissingPrivateKey ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		RSAParameters rsap = AllTests.GetRsaKey (false);
		rsa.ImportParameters (rsap);

		rsa.ExportCspBlob (true);
	}

	[Test]
	public void ExportCspBlob_MissingPrivateKey_PublicOnly ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		RSAParameters rsap = AllTests.GetRsaKey (false);
		rsa.ImportParameters (rsap);

		byte[] pubkey = rsa.ExportCspBlob (false);
		Assert.AreEqual ("06-02-00-00-00-A4-00-00-52-53-41-31-00-04-00-00-11-00-00-00-CB-BD-1D-09-FD-E5-F8-46-59-8F-2A-CA-98-72-53-E2-7F-68-C1-F6-41-9A-7A-52-1F-A5-61-7B-2D-B1-AA-E0-4E-39-98-45-45-B2-34-88-74-53-09-06-9D-64-6A-EE-84-25-3A-D9-B7-B4-E6-3E-72-37-C7-DF-A3-E0-B8-AF-7F-80-8B-5B-8A-9D-71-19-46-EC-E1-60-0D-52-ED-76-48-CD-6F-EB-CE-48-EA-61-AB-02-5C-03-AF-BA-DF-B8-1F-F5-54-74-F0-B6-D6-40-A4-43-10-D4-EE-07-8D-36-F7-71-A8-9D-2B-AC-38-23-9C-CE-82-06-09-2F-F8-BB", BitConverter.ToString (pubkey));
	}

	[Test]
	public void ImportCspBlob_Keypair ()
	{
		byte[] blob = new byte [596] { 0x07, 0x02, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 0x00, 0x04, 0x00, 0x00, 0x11, 
			0x00, 0x00, 0x00, 0xCB, 0xBD, 0x1D, 0x09, 0xFD, 0xE5, 0xF8, 0x46, 0x59, 0x8F, 0x2A, 0xCA, 0x98, 0x72, 0x53, 0xE2, 0x7F, 0x68, 0xC1, 
			0xF6, 0x41, 0x9A, 0x7A, 0x52, 0x1F, 0xA5, 0x61, 0x7B, 0x2D, 0xB1, 0xAA, 0xE0, 0x4E, 0x39, 0x98, 0x45, 0x45, 0xB2, 0x34, 0x88, 0x74, 
			0x53, 0x09, 0x06, 0x9D, 0x64, 0x6A, 0xEE, 0x84, 0x25, 0x3A, 0xD9, 0xB7, 0xB4, 0xE6, 0x3E, 0x72, 0x37, 0xC7, 0xDF, 0xA3, 0xE0, 0xB8, 
			0xAF, 0x7F, 0x80, 0x8B, 0x5B, 0x8A, 0x9D, 0x71, 0x19, 0x46, 0xEC, 0xE1, 0x60, 0x0D, 0x52, 0xED, 0x76, 0x48, 0xCD, 0x6F, 0xEB, 0xCE, 
			0x48, 0xEA, 0x61, 0xAB, 0x02, 0x5C, 0x03, 0xAF, 0xBA, 0xDF, 0xB8, 0x1F, 0xF5, 0x54, 0x74, 0xF0, 0xB6, 0xD6, 0x40, 0xA4, 0x43, 0x10, 
			0xD4, 0xEE, 0x07, 0x8D, 0x36, 0xF7, 0x71, 0xA8, 0x9D, 0x2B, 0xAC, 0x38, 0x23, 0x9C, 0xCE, 0x82, 0x06, 0x09, 0x2F, 0xF8, 0xBB, 0x99, 
			0x65, 0xFB, 0x58, 0x2A, 0xBA, 0x41, 0x75, 0x39, 0x1F, 0x9D, 0x45, 0x76, 0x21, 0x25, 0x5B, 0x2D, 0x0A, 0x04, 0xAA, 0xE7, 0xFA, 0x28, 
			0x7E, 0x3B, 0x1E, 0x5D, 0x6E, 0x23, 0xF0, 0x4E, 0x12, 0x32, 0xF6, 0x84, 0x3D, 0x9E, 0x1A, 0xB8, 0x93, 0xA4, 0xFD, 0xF4, 0xAE, 0x44, 
			0x9F, 0xEB, 0x99, 0x01, 0x60, 0xB5, 0xA1, 0x10, 0x0B, 0x81, 0x08, 0xC9, 0xB3, 0xB9, 0xB1, 0x81, 0xAE, 0xCF, 0xEE, 0x03, 0x15, 0x46, 
			0xAF, 0x00, 0xE7, 0x41, 0xA4, 0xA5, 0x16, 0x04, 0x4D, 0x52, 0x52, 0x33, 0xCE, 0xCF, 0xB5, 0x04, 0x32, 0xB4, 0xA3, 0x0D, 0xEA, 0x92, 
			0x2E, 0xB4, 0x66, 0x16, 0xB4, 0x40, 0x98, 0x86, 0x9D, 0x8B, 0x02, 0x35, 0x20, 0x0F, 0x5A, 0xD0, 0xB1, 0x66, 0x88, 0xD0, 0x42, 0x6C, 
			0x3F, 0x35, 0xD9, 0xD1, 0xAA, 0xEA, 0x33, 0x12, 0x34, 0xF6, 0x53, 0xF4, 0x27, 0xF0, 0xB1, 0x7F, 0xC9, 0x81, 0xC9, 0x67, 0x1F, 0x3C, 
			0xC9, 0xAD, 0x74, 0x41, 0x47, 0x0A, 0x91, 0xFC, 0x38, 0x2B, 0x20, 0x2E, 0x9A, 0x2E, 0x69, 0xF7, 0xC1, 0x59, 0xFF, 0x14, 0x74, 0xB7, 
			0xEA, 0x75, 0x09, 0x49, 0xD9, 0x3E, 0xB1, 0xF2, 0x51, 0xB0, 0x54, 0x9B, 0xAC, 0xD0, 0xA4, 0x83, 0x01, 0xDC, 0xDD, 0x07, 0xEB, 0x5A, 
			0x9A, 0xD6, 0xFC, 0x23, 0x40, 0xE2, 0xE4, 0x37, 0x03, 0xBA, 0x3E, 0xA6, 0x4C, 0x49, 0x54, 0x3D, 0xDA, 0x36, 0x98, 0x5A, 0x24, 0xF9, 
			0x39, 0xFE, 0x07, 0x98, 0x84, 0x95, 0xA4, 0x99, 0x1B, 0x0D, 0x22, 0x7A, 0xE4, 0x8A, 0xFD, 0x31, 0xE9, 0x42, 0xE3, 0x8A, 0x6F, 0x8F, 
			0x3F, 0x80, 0xF9, 0xA7, 0x46, 0x31, 0x4C, 0xA9, 0x56, 0x05, 0x5C, 0x1C, 0x99, 0x7E, 0x8A, 0x3A, 0xBD, 0xAD, 0x61, 0xA9, 0x4C, 0x86, 
			0x78, 0xF8, 0xB7, 0x51, 0x03, 0x75, 0xF0, 0x0A, 0xFF, 0x90, 0x02, 0x1E, 0x47, 0xF7, 0x39, 0x80, 0x79, 0x25, 0xFB, 0xED, 0x66, 0x36, 
			0xEA, 0x98, 0xE6, 0x56, 0x96, 0x0F, 0x2E, 0x9E, 0x4E, 0x2B, 0x40, 0xBF, 0x67, 0xA9, 0x67, 0xB1, 0x83, 0x58, 0x4F, 0x15, 0x32, 0x86, 
			0xC9, 0x9F, 0x11, 0xE2, 0x39, 0xCD, 0x7F, 0x07, 0x93, 0x50, 0x88, 0x53, 0x34, 0xF7, 0x71, 0xF2, 0x80, 0xB3, 0x23, 0x94, 0xAE, 0xDB, 
			0x5B, 0x26, 0x8D, 0x19, 0x01, 0x63, 0xBB, 0xDA, 0x4F, 0x6C, 0xB0, 0xC1, 0xB3, 0x1C, 0x5B, 0xC8, 0x4B, 0x3A, 0x46, 0xE4, 0x3F, 0x88, 
			0xB8, 0xB7, 0xC2, 0x28, 0x94, 0xAF, 0x94, 0x48, 0xCC, 0xCE, 0xA1, 0xE1, 0xFB, 0xDD, 0x3E, 0xA1, 0x74, 0x4F, 0xED, 0x26, 0x99, 0x93, 
			0x0A, 0xD2, 0xF5, 0xBE, 0xC3, 0x8D, 0xD3, 0x2C, 0x09, 0x6C, 0xC5, 0x68, 0xAF, 0x6A, 0xE5, 0x44, 0x94, 0x52, 0xA9, 0xF1, 0x76, 0x1C, 
			0x11, 0xBC, 0xCF, 0xAC, 0x50, 0x59, 0xAD, 0xF3, 0x7F, 0xC9, 0xDA, 0x3E, 0xC9, 0x2E, 0xB8, 0xDC, 0x7F, 0xE3, 0x39, 0xA9, 0x82, 0xC0, 
			0xA2, 0x0D, 0x87, 0xD6, 0x69, 0x26, 0x39, 0x5C, 0x5E, 0x74, 0x65, 0xC4, 0x6B, 0x8D, 0x99, 0xC7, 0xB3, 0x10, 0x94, 0xB2, 0x41, 0x96, 
			0x57, 0x01, 0x38, 0x81, 0x84, 0x27, 0xB4, 0x68, 0x06, 0x1E, 0x25, 0x31, 0x3F, 0xF8, 0xCD, 0xC1, 0x30, 0xDB, 0x88, 0xB9, 0xC4, 0x89, 
			0xF2, 0xFA, 0x41, 0x53, 0xFC, 0xDA, 0xA5 };
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.ImportCspBlob (blob);

		byte[] keypair = rsa.ExportCspBlob (true);
		for (int i=0; i < blob.Length; i++)
			Assert.AreEqual (blob [i], keypair [i], i.ToString ());
	}

	[Test]
	public void ImportCspBlob_Signature_Keypair ()
	{
		// from bug #5299
		byte[] blob = new byte [] {  
			0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 
			0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0xCB, 0xF3, 
			0xF0, 0x0C, 0xD7, 0xC1, 0xA8, 0x06, 0x1A, 0xF5, 0x49, 0x4E, 0x7A, 
			0x02, 0x4A, 0x46, 0xB2, 0x8F, 0xE0, 0x01, 0x5C, 0x89, 0x01, 0x2D, 
			0x42, 0x5A, 0xEA, 0x16, 0x11, 0x66, 0x48, 0x26, 0x29, 0xAE, 0x2A, 
			0xAA, 0xD4, 0x3C, 0x27, 0xF6, 0x48, 0x0E, 0x09, 0x25, 0xD0, 0x63, 
			0x80, 0x74, 0xDA, 0x1B, 0x57, 0x1D, 0x62, 0x83, 0xB9, 0x58, 0x5D, 
			0x09, 0x4D, 0x0B, 0x1F, 0x3F, 0xC8, 0xB8, 0x99, 0x3B, 0x9A, 0x57, 
			0x16, 0x18, 0xE8, 0x73, 0x2F, 0x34, 0x96, 0x5C, 0xE6, 0x8F, 0x1E, 
			0xF8, 0x46, 0x4A, 0x90, 0x16, 0x3F, 0x40, 0x18, 0x53, 0x62, 0x7F, 
			0x24, 0xB2, 0x3A, 0xFB, 0xA2, 0x80, 0x39, 0x1C, 0x08, 0x6D, 0xFC, 
			0x6B, 0xCA, 0x0A, 0x14, 0xDD, 0xA9, 0x41, 0x57, 0x25, 0x49, 0x3A, 
			0x27, 0x9C, 0x25, 0xC4, 0x5E, 0xC5, 0x7B, 0x83, 0x1B, 0x9D, 0xDF, 
			0x03, 0x24, 0x94, 0x5D, 0x90, 0xD5, 0xEC, 0x7B, 0xD5, 0xCF, 0x66, 
			0x3D, 0x93, 0x63, 0xD5, 0x1A, 0x9A, 0x9B, 0x46, 0x78, 0x89, 0xD1, 
			0xC3, 0x38, 0x9A, 0x69, 0x61, 0xFE, 0xA2, 0x0C, 0xD9, 0x73, 0x81, 
			0x11, 0x28, 0x71, 0x06, 0x69, 0x3D, 0x1A, 0x4E, 0xF4, 0x9D, 0x8A, 
			0x5C, 0x9A, 0x2F, 0x71, 0x9F, 0x2B, 0x9F, 0xE6, 0xB1, 0xBF, 0x4E, 
			0x8C, 0xFA, 0x93, 0x04, 0x29, 0xD2, 0x4A, 0x73, 0x85, 0x7A, 0x91, 
			0x8D, 0x19, 0xCA, 0x1F, 0x2E, 0x5D, 0xD1, 0xAD, 0x70, 0xB5, 0x64, 
			0x69, 0x5B, 0x09, 0x84, 0x35, 0xB1, 0x31, 0x15, 0x40, 0x6C, 0x0D, 
			0x31, 0xF3, 0xEA, 0xE4, 0xD7, 0x6D, 0x42, 0xD0, 0xBB, 0x84, 0xDB, 
			0x73, 0x10, 0x16, 0x0B, 0xDC, 0xE2, 0x62, 0x32, 0xEB, 0x23, 0x19, 
			0x38, 0x64, 0x6A, 0x3D, 0x5E, 0x65, 0xAD, 0x7B, 0xEC, 0xB6, 0xC0, 
			0x00, 0x4A, 0x4F, 0x98, 0x35, 0xF3, 0xD7, 0x73, 0xD3, 0x31, 0xDE, 
			0xB6, 0x85, 0xDA, 0x4C, 0x3D, 0x79, 0x5A, 0x96, 0x07, 0x63, 0x70, 
			0x34, 0x45, 0xF4, 0x78, 0x25, 0x78, 0x92, 0x3C, 0x56, 0x38, 0xF9, 
			0xFA, 0x0D, 0xF4, 0x00, 0xD3, 0xD7, 0xBB, 0xA3, 0x97, 0xB4, 0x26, 
			0xEB, 0x25, 0xA3, 0x1E, 0x34, 0xFC, 0x0E, 0x7F, 0x6A, 0x12, 0x25, 
			0x25, 0x50, 0x26, 0x8D, 0x10, 0x80, 0xC7, 0xEB, 0x4B, 0x4B, 0x74, 
			0x09, 0x89, 0x2F, 0xBB, 0x03, 0x02, 0x17, 0xFD, 0x13, 0x71, 0x33, 
			0xB4, 0x46, 0x49, 0xDD, 0xBE, 0x34, 0x01, 0x37, 0xC3, 0x63, 0x4D, 
			0xBE, 0x76, 0xAD, 0x62, 0x11, 0x6C, 0xDD, 0x64, 0x47, 0x73, 0x95, 
			0x92, 0x58, 0xBF, 0xFF, 0xE3, 0x20, 0xD2, 0xB1, 0xEC, 0xA8, 0x03, 
			0xB8, 0xA7, 0x0E, 0xA5, 0xAE, 0xAF, 0x47, 0x45, 0xED, 0x5F, 0xE1, 
			0x0B, 0xA8, 0xA3, 0x03, 0xB7, 0x93, 0xA6, 0xD8, 0xAC, 0x71, 0xAB, 
			0x77, 0x8B, 0xEC, 0x6F, 0x16, 0x0E, 0x1A, 0x2B, 0x2D, 0x31, 0xBD, 
			0x69, 0xD4, 0x9E, 0x9E, 0x0F, 0xA2, 0xED, 0x94, 0x59, 0x1D, 0x61, 
			0x0E, 0xE5, 0xD3, 0x19, 0x2B, 0xAD, 0x70, 0x90, 0xAF, 0x51, 0x7F, 
			0x56, 0x53, 0xC2, 0x86, 0xB4, 0x24, 0xBC, 0xD0, 0x63, 0xAC, 0x4B, 
			0xE0, 0xE0, 0x6C, 0xF5, 0xF6, 0x21, 0x7D, 0xE9, 0x7C, 0x45, 0x13, 
			0xE7, 0x87, 0x11, 0x09, 0xCC, 0xA6, 0xB1, 0xCD, 0x49, 0x10, 0x33, 
			0xBC, 0x07, 0xC8, 0x56, 0xA9, 0x19, 0xC1, 0x86, 0xDF, 0x63, 0xDF, 
			0xE6, 0x6D, 0xFB, 0x46, 0x31, 0x93, 0x36, 0x3C, 0x8E, 0x6E, 0xB0, 
			0xC7, 0x66, 0xFC, 0x6C, 0x85, 0x5B, 0xF4, 0xEE, 0x1D, 0x3B, 0xE3, 
			0x8B, 0xF5, 0xB9, 0x88, 0x48, 0x2F, 0x77, 0x56, 0x82, 0x85, 0x7C, 
			0xF9, 0xE9, 0x15, 0xA5, 0x8E, 0x46, 0xEA, 0x08, 0xB9, 0xD0, 0x8F, 
			0x0F, 0x28, 0x1C, 0x96, 0xA1, 0xB3, 0x00, 0x6E, 0x9B, 0x81, 0xBD, 
			0xD4, 0x54, 0xA4, 0xFD, 0xD5, 0xA7, 0x4B, 0x2E, 0x17, 0x10, 0xED, 
			0xD2, 0xAA, 0x38, 0x2E, 0x24, 0x7C, 0x59, 0xF4, 0x2D, 0x08, 0x3B, 
			0x15, 0x05, 0x6A, 0xD8, 0x61, 0x8A, 0xAC, 0xCD, 0x5E, 0x77, 0x4C, 
			0x8E, 0x0C, 0xEE, 0xD8, 0xEF, 0xD0, 0xBC, 0x1B, 0x14, 0x17, 0xE3, 
			0x38, 0x27, 0xA1, 0x70, 0x5B, 0x5C, 0xC3, 0xE3, 0x91, 0x1E, 0x01, 
			0xE3, 0x9A, 0x16, 0x1A, 0x5C, 0x4D, 0xD9, 0x3B, 0x36, 0x7F, 0x0B, 
			0x93, 0x16
		};
		rsa = new RSACryptoServiceProvider ();
		rsa.ImportCspBlob (blob);

		byte[] keypair = rsa.ExportCspBlob (false);
		Assert.AreEqual (keypair [5], blob [5], "0x24 signature");
	}

	[Test]
	public void ExportCspBlob_PublicKey ()
	{
		byte[] blob = new byte [148] { 0x06, 0x02, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0xCB, 0xBD, 0x1D, 0x09, 0xFD, 0xE5, 0xF8, 0x46, 0x59, 0x8F, 0x2A, 0xCA, 0x98, 0x72, 0x53, 0xE2, 0x7F, 0x68, 0xC1, 0xF6, 0x41, 0x9A, 0x7A, 0x52, 0x1F, 0xA5, 0x61, 0x7B, 0x2D, 0xB1, 0xAA, 0xE0, 0x4E, 0x39, 0x98, 0x45, 0x45, 0xB2, 0x34, 0x88, 0x74, 0x53, 0x09, 0x06, 0x9D, 0x64, 0x6A, 0xEE, 0x84, 0x25, 0x3A, 0xD9, 0xB7, 0xB4, 0xE6, 0x3E, 0x72, 0x37, 0xC7, 0xDF, 0xA3, 0xE0, 0xB8, 0xAF, 0x7F, 0x80, 0x8B, 0x5B, 0x8A, 0x9D, 0x71, 0x19, 0x46, 0xEC, 0xE1, 0x60, 0x0D, 0x52, 0xED, 0x76, 0x48, 0xCD, 0x6F, 0xEB, 0xCE, 0x48, 0xEA, 0x61, 0xAB, 0x02, 0x5C, 0x03, 0xAF, 0xBA, 0xDF, 0xB8, 0x1F, 0xF5, 0x54, 0x74, 0xF0, 0xB6, 0xD6, 0x40, 0xA4, 0x43, 0x10, 0xD4, 0xEE, 0x07, 0x8D, 0x36, 0xF7, 0x71, 0xA8, 0x9D, 0x2B, 0xAC, 0x38, 0x23, 0x9C, 0xCE, 0x82, 0x06, 0x09, 0x2F, 0xF8, 0xBB };
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.ImportCspBlob (blob);

		byte[] pubkey = rsa.ExportCspBlob (false);
		for (int i = 0; i < blob.Length; i++)
			Assert.AreEqual (blob [i], pubkey [i], i.ToString ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ImportCspBlob_Null ()
	{
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.ImportCspBlob (null);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ImportCspBlob_Bad ()
	{
		byte[] blob = new byte [148]; // valid size for public key
		rsa = new RSACryptoServiceProvider (minKeySize);
		rsa.ImportCspBlob (blob);
	}

	[Test] //bug 38054
	public void NonExportableKeysAreNonExportable ()
	{
		var cspParams = new CspParameters();
		cspParams.KeyContainerName = "TestRSAKey";
		cspParams.Flags = CspProviderFlags.UseNonExportableKey;
		var rsa = new RSACryptoServiceProvider(cspParams);
		Assert.Throws<CryptographicException>(() => rsa.ExportParameters(true));
	}
}

}
