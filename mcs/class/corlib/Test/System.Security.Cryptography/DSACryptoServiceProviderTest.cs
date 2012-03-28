//
// DSACryptoServiceProviderTest.cs, NUnit Test Cases for DSACryptoServiceProvider
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
public class DSACryptoServiceProviderTest {

	protected DSACryptoServiceProvider dsa;
	protected DSACryptoServiceProvider disposed;

//	static string xmlPrivate = "<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter><X>fAOytZttUZFzt/AvwRinmvYKL7E=</X></DSAKeyValue>";
//	static string xmlPublic = "<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter></DSAKeyValue>";

	static int minKeySize = 512;
	private DSACryptoServiceProvider smallDsa;

	private bool machineKeyStore;

	public DSACryptoServiceProviderTest () 
	{
		disposed = new DSACryptoServiceProvider (minKeySize);
		// FX 2.0 beta 1 bug - we must use the key before clearing it
		// http://lab.msdn.microsoft.com/ProductFeedback/viewfeedback.aspx?feedbackid=dc970a7f-b82f-45e5-9d37-fb0ed72e6b41
		int ks = disposed.KeySize;
		disposed.Clear ();
		// do not generate a new keypair for each test
		smallDsa = new DSACryptoServiceProvider (minKeySize);
	}

	[SetUp]
	public void Setup () 
	{
		machineKeyStore = DSACryptoServiceProvider.UseMachineKeyStore;
	}

	[TearDown]
	public void TearDown () 
	{
		DSACryptoServiceProvider.UseMachineKeyStore = machineKeyStore;
	}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// may also help for DSA descendants
	public void AssertEquals (string message, DSAParameters expectedKey, DSAParameters actualKey, bool checkPrivateKey) 
	{
		Assert.AreEqual (expectedKey.Counter, actualKey.Counter, message + " Counter");
		AssertEquals (message + " G", expectedKey.G, actualKey.G);
		AssertEquals (message + " J", expectedKey.J, actualKey.J);
		AssertEquals (message + " P", expectedKey.P, actualKey.P);
		AssertEquals (message + " Q", expectedKey.Q, actualKey.Q);
		AssertEquals (message + " Seed", expectedKey.Seed, actualKey.Seed);
		AssertEquals (message + " Y", expectedKey.Y, actualKey.Y);
		if (checkPrivateKey)
			AssertEquals (message + " X", expectedKey.X, actualKey.X);
	}

	[Test]
	public void ConstructorEmpty () 
	{
		// under Mono:: a new key pair isn't generated
		dsa = new DSACryptoServiceProvider ();
		// test default key size
		Assert.AreEqual (1024, dsa.KeySize, "DSA ConstructorEmpty");
		Assert.IsFalse (dsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (dsa.PublicOnly, "PublicOnly");
	}

	[Test]
	public void ConstructorKeySize () 
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		// test default key size
		Assert.AreEqual (minKeySize, dsa.KeySize, "DSA ConstructorKeySize");
		Assert.IsFalse (dsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (dsa.PublicOnly, "PublicOnly");
	}

	[Test]
	[Category ("TargetJvmNotSupported")]
	public void ConstructorCspParameters () 
	{
		CspParameters csp = new CspParameters (13, null, "Mono1024");
		// under MS a new keypair will only be generated the first time
		dsa = new DSACryptoServiceProvider (csp);
		// test default key size
		Assert.AreEqual (1024, dsa.KeySize, "DSA ConstructorCspParameters");
		Assert.IsTrue (dsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (dsa.PublicOnly, "PublicOnly");
	}

	[Test]
	[Category ("TargetJvmNotSupported")]
	public void ConstructorKeySizeCspParameters () 
	{
		CspParameters csp = new CspParameters (13, null, "Mono512");
		dsa = new DSACryptoServiceProvider (minKeySize, csp);
		Assert.AreEqual (minKeySize, dsa.KeySize, "DSA ConstructorCspParameters");
		Assert.IsTrue (dsa.PersistKeyInCsp, "PersistKeyInCsp");
		Assert.IsFalse (dsa.PublicOnly, "PublicOnly");
	}

	[Test]
	[Ignore ("Much too long (with MS as Mono doesn't generates the keypair unless it need it)")]
	public void KeyGeneration ()
	{
		// Test every valid key size
		KeySizes LegalKeySize = dsa.LegalKeySizes [0];
		for (int i = LegalKeySize.MinSize; i <= LegalKeySize.MaxSize; i += LegalKeySize.SkipSize) {
			dsa = new DSACryptoServiceProvider (i);
			Assert.AreEqual (i, dsa.KeySize, "DSA.KeySize");
			Assert.IsFalse (dsa.PublicOnly, "PublicOnly");
		}
	}

	[Test]
	public void LimitedKeyGeneration () 
	{
		// Test smallest valid key size (performance issue)
		using (dsa = new DSACryptoServiceProvider (minKeySize)) {	// MS generates keypair here
			Assert.AreEqual (minKeySize, dsa.KeySize, "BeforeMonoKeyGeneration.KeySize");
			byte[] hash = new byte [20];
			dsa.CreateSignature (hash);				// mono generates keypair here
			Assert.AreEqual (minKeySize, dsa.KeySize, "AfterMonoKeyGeneration.KeySize");
			Assert.IsFalse (dsa.PublicOnly, "PublicOnly");
		}
		// here Dispose is called (with true)
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void TooSmallKeyPair () 
	{
		dsa = new DSACryptoServiceProvider (384);

		// in 2.0 MS delay the creation of the key pair until it is required
		// (same trick that Mono almost always used ;-) but they also delay
		// the parameter validation (what Mono didn't). So here we must "get"
		// the key (export) to trigger the exception
		dsa.ToXmlString (true);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void TooBigKeyPair () 
	{
		dsa = new DSACryptoServiceProvider (2048);

		// in 2.0 MS delay the creation of the key pair until it is required
		// (same trick that Mono almost always used ;-) but they also delay
		// the parameter validation (what Mono didn't). So here we must "get"
		// the key (export) to trigger the exception
		dsa.ToXmlString (true);
	}

	[Test]
	public void Properties () 
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		Assert.AreEqual (1, dsa.LegalKeySizes.Length, "LegalKeySize");
		Assert.AreEqual (minKeySize, dsa.LegalKeySizes[0].MinSize, "LegalKeySize.MinSize");
		Assert.AreEqual (1024, dsa.LegalKeySizes[0].MaxSize, "LegalKeySize.MaxSize");
		Assert.AreEqual (64, dsa.LegalKeySizes[0].SkipSize, "LegalKeySize.SkipSize");
		Assert.IsNull (dsa.KeyExchangeAlgorithm, "KeyExchangeAlgorithm");
		Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#dsa-sha1", dsa.SignatureAlgorithm);
		dsa.Clear ();
		Assert.AreEqual (1, dsa.LegalKeySizes.Length, "LegalKeySize(disposed)");
		Assert.AreEqual (minKeySize, dsa.LegalKeySizes[0].MinSize, "LegalKeySize.MinSize(disposed)");
		Assert.AreEqual (1024, dsa.LegalKeySizes[0].MaxSize, "LegalKeySize.MaxSize(disposed)");
		Assert.AreEqual (64, dsa.LegalKeySizes[0].SkipSize, "LegalKeySize.SkipSize(disposed)");
		Assert.IsNull (dsa.KeyExchangeAlgorithm, "KeyExchangeAlgorithm(disposed)");
		Assert.AreEqual ("http://www.w3.org/2000/09/xmldsig#dsa-sha1", dsa.SignatureAlgorithm);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void CreateSignatureDisposed () 
	{
		byte[] hash = new byte [20];
		disposed.CreateSignature (hash);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void CreateSignatureInvalidHashLength () 
	{
		byte[] hash = new byte [19];
		smallDsa.CreateSignature (hash);
	}

	[Test]
	public void CreateSignature () 
	{
		byte[] hash = new byte [20];
		// for Mono, no keypair has yet been generated before calling CreateSignature
		smallDsa.CreateSignature (hash);
	}

	[Test]
	public void SignData () 
	{
		byte[] data = new byte [128];
		byte[] signature = smallDsa.SignData (data);
		Assert.IsTrue (smallDsa.VerifyData (data, signature), "SignData");
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void SignDataDisposed () 
	{
		byte[] data = new byte [20];
		disposed.SignData (data);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void SignHashDisposed () 
	{
		byte[] hash = new byte [20];
		disposed.SignHash (hash, "SHA1");
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void SignHashInvalidAlgorithm () 
	{
		byte[] hash = new byte [16];
		smallDsa.SignHash (hash, "MD5");
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void VerifyDataDisposed () 
	{
		byte[] data = new byte [20];
		byte[] sign = new byte [40];
		disposed.VerifyData (data, sign);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void VerifyHashInvalidAlgorithm () 
	{
		byte[] hash = new byte [16];
		byte[] sign = new byte [40];
		smallDsa.VerifyHash (hash, "MD5", sign);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void VerifyHashDisposed () 
	{
		byte[] hash = new byte [20];
		byte[] sign = new byte [40];
		disposed.VerifyHash (hash, "SHA1", sign);
	}

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void VerifySignatureDisposed () 
	{
		byte[] hash = new byte [20];
		byte[] sign = new byte [40];
		disposed.VerifySignature (hash, sign);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void VerifySignatureInvalidHashLength () 
	{
		byte[] hash = new byte [19];
		byte[] sign = new byte [40];
		smallDsa.VerifySignature (hash, sign);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void VerifySignatureInvalidSignatureLength () 
	{
		byte[] hash = new byte [20];
		byte[] sign = new byte [39];
		smallDsa.VerifySignature (hash, sign);
	}

	[Test]
	public void VerifySignatureWithoutKey () 
	{
		byte[] hash = new byte [20];
		byte[] sign = new byte [40];
		DSACryptoServiceProvider emptyDSA = new DSACryptoServiceProvider (minKeySize); 
		// Mono hasn't generated a keypair - but it's impossible to 
		// verify a signature based on a new just generated keypair
		Assert.IsFalse (emptyDSA.VerifySignature (hash, sign));
	}
		
#if !NET_2_1
	[Test]
	[Category ("NotWorking")]
	public void ImportDisposed ()
	{
		DSACryptoServiceProvider import = new DSACryptoServiceProvider (minKeySize);
		import.Clear ();
		import.ImportParameters (AllTests.GetKey (false));
		// no exception from Fx 2.0 +
	}
#endif

	[Test]
	[ExpectedException (typeof (ObjectDisposedException))]
	public void ExportDisposed () 
	{
		DSAParameters param = disposed.ExportParameters (false);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void DSAImportMissingP () 
	{
		DSAParameters input = AllTests.GetKey (false);
		input.P = null;
		dsa = new DSACryptoServiceProvider (1024);
		dsa.ImportParameters (input);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void DSAImportMissingQ () 
	{
		DSAParameters input = AllTests.GetKey (false);
		input.Q = null;
		dsa = new DSACryptoServiceProvider (1024);
		dsa.ImportParameters (input);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void DSAImportMissingG () 
	{
		DSAParameters input = AllTests.GetKey (false);
		input.G = null;
		dsa = new DSACryptoServiceProvider (1024);
		dsa.ImportParameters (input);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void DSAImportMissingY () 
	{
		DSAParameters input = AllTests.GetKey (false);
		input.Y = null;
		dsa = new DSACryptoServiceProvider (1024);
		dsa.ImportParameters (input);
	}

	[Test]
	public void DSAImportMissingJ () 
	{
		DSAParameters input = AllTests.GetKey (false);
		input.J = null;
		dsa = new DSACryptoServiceProvider (1024);
		dsa.ImportParameters (input);
		Assert.AreEqual (1024, dsa.KeySize, "MissingJ.KeySize");
	}

	[Test]
	public void DSAImportMissingSeed () 
	{
		DSAParameters input = AllTests.GetKey (false);
		input.Seed = null;
		dsa = new DSACryptoServiceProvider (1024);
		dsa.ImportParameters (input);
		Assert.AreEqual (1024, dsa.KeySize, "MissingSeed.KeySize");
	}

	// all keypairs generated by CryptoAPI on Windows
	string CapiXml512 = "<DSAKeyValue><P>tWcdz18IhYs8k1vagB94wR/sgHSANmmVlSstfMBWbIwthyT2AV64LkMLeCOdflYFEXgpmpTx5UIGGgEx8/EEuw==</P><Q>6E7WKEXRY+ASYA6nkBUP25fgz88=</Q><G>QPykdzvQ997BxXBZ+xFM10jzvZ3G73T9Aj6y2bMNdKd3/GfZwbvQui83YoRfFnYumJmV6c4cfCN1Tuf6n6jCuA==</G><Y>jrNR5BBEzW9aD9WjeJ7us0ityN8mpcqBVbHCAAWIjc0BvlY1AacPQ8xN3kKyHHKpPgMwNuFXHwks6VO68Y0etw==</Y><J>x+c7udpLkcx0I6LX2+HjLn/VHTdPr2XH9Dk4fBUnVrrjYKPVYQCIavHKFCY=</J><Seed>7qAsSVCQRpmFIfhyhYdtMybIvLI=</Seed><PgenCounter>AYU=</PgenCounter><X>DSWG2qNYTBMXD6k5j9AeCaxSVp4=</X></DSAKeyValue>";
	string CapiXml576 = "<DSAKeyValue><P>/UpVL7nsgTsT+HzOidLQgSi9Rk8sn+jBl/RH9OzoSJhmLKM4cCl/mz3LAZNWqDS9kqDlTohYfPTVdsKA79U3uHx+xeUpk6UH</P><Q>mR7th6PKwfpwBBzp1MUGy36jxuc=</Q><G>oWjhXviie9LYZxoSCZuc45kQG+1iIJm3Omeik06b0jM9ZtVtHCXjuHRohEZfuBck8HRUxreSRT895kXzhN3/hYcqLnGMPbiZ</G><Y>kC1DnEs3hW30JtYLRQ3d46sp++6oNIjz9ncAZz6VEQg6GqjLieigAg/5ikzHjUS3Ps9xVOgGGpjSFTMSYrEl/K3obPNNFPWi</Y><J>AAAAAad4wklEWIys2shSp+zpfpu0K+/li8cJMK8K5oneqwedsMPT/ciDPVgrJYtS1WrwDoIHIAo=</J><Seed>LuRNAo/Lj+cbWgE6JB57C+8tB1I=</Seed><PgenCounter>cQ==</PgenCounter><X>h3vfdRLRPghNCXKzITtcpJo3W+U=</X></DSAKeyValue>";
	string CapiXml640 = "<DSAKeyValue><P>1SjEwMEXKJFpmQip/SFhqQHoi3SWgXAbMA6ZMVVL/34O9CqUY2shx9E839i4mTh+euvyF9A6UhUuVRFJ8HMFpPeBqbbzrVtynU2USW74h2U=</P><Q>rYvCtKnZiH28AoErTfKCAex11YM=</Q><G>K5iAfQ50aEpTtkuHN/WQCa7bysGBT3vbPkRVUcYX0GAxZSIrXqvQJVh74FQnttk0AkSSqc1Bj3aH8DG0Ie3pqFDfyWoj4ZHh7DaqjHIUlTs=</G><Y>aqEzlNdCktrAI4PF1DNYFkPTFbjO+ittp78dThOJOaDitM0OhkdujSzwvwRQ25phVbsxlVpGesxpqI3dA6Y13v2Xbek9P+1WkS1iPZt88f8=</Y><J>AAAAATpvI+WRt+RdcSC/hd9pO+yvd8fYY+NLh+F951dEuZfulWl09ARuzwc5BHu3x2IO/NbUc3Eru82RBBmhzA==</J><Seed>QMvwX0OS+ANLT263MGWecWwQOfs=</Seed><PgenCounter>Ag==</PgenCounter><X>Gu8/RsqX34sOnfNU71OZCmL2pMw=</X></DSAKeyValue>";
	string CapiXml704 = "<DSAKeyValue><P>v8CmmIBAKfifPNhkHtkBk9FdS5U4gq8Ex/L+0tVR1Izh7t9C2d0C1ZNbaTOTDGKV7TKOLIOXQ/sYVa3Sa033cWrfk/Z1veHd+49Xk7JjEiKnMipfXtXp8Q==</P><Q>qctfxht2I8ZbZF6f+jT4DNON00U=</Q><G>nQ4ao+mLFUD8XRr4bhVbk5siffsbCauzOQNZlWQMfHjuSTfD8+MKzhgXb/A+a0CQRBXvYu+2R8VmyRET93IlwkppXgxldSwUd3xlng/3O9ogbF06zHVjHg==</G><Y>d82MybaKc5FselhGYpGm/XROSyAsSs/blDzQAnjyPfKWdbXSaHLjyeficMmipi60/nH5ShnCnMrc22Bm9hOpMaMfhidMX6Uh5YB9EKy7Cdb6587/y6EiLA==</Y><J>AAAAASEbOtcd7wjAzuiD9qyOTA4xEKFPgOup5SuDH6FZ9FXAH+NePtGeBpXKIaLj2shrkKhsJmY5pVza/yVE9L74ajqcK2kw</J><Seed>8W3wrLCmjs81ekM3Jz3D9fxfAuw=</Seed><PgenCounter>wA==</PgenCounter><X>UWQ3ZFd/Eg2OzY9Fin/aCGLHNbQ=</X></DSAKeyValue>";
	string CapiXml768 = "<DSAKeyValue><P>wOca55yVYJveGqx8w3acAV/LNyLmo9eAXKxnIXyEsjB+LoGjJRvJbqMy0NP9VR6qv12iZaVs/32A2y6iHMeVnQWxSnu9ieeM+Gwwh4Xg1beOeHsI4FKBH+ZTgYiNRGXT</P><Q>uPrZhRYtl7IOc38ZQnKd3StxCO8=</Q><G>S8cg4PHBvNOQ/c7zYYJTS991U5O2OibCsaOLUKFlUhkf+9DUDvJIauSzeWbQVkb6ta5oOapRbUNPfjY6OlZWHMnlBXAUpme2UBoz1IMUY2xM6Q4JKrxMv6NLCHoMhnP6</G><Y>rb/X/sFw7f0JOnSD2ygUzFFU57iIxYwRl8t8lUZ2lJ0oBdw2zy75Oud4S7vB56Xo0JJAEFSUQFobpAWpxqZ/5qLP0PzhxhjwN+Dv9S5hpcaicJrIbHKdx8A7O6P5QcR2</Y><J>AAAAAQr2+OH8gZynYhX5wG8T9iWuMu6LsLuQsbYoDcbCbMdwDRr1bNYjR1NPZNfuDtVlzXFY1nOSG67wW7HZIWYMYBBCRKbE5oK16O8VI04=</J><Seed>xjnQGEAsKqfluAuJCCs8hG3pDI8=</Seed><PgenCounter>nA==</PgenCounter><X>erfJ6egI1OnM1z1IRFhJaFN+B3w=</X></DSAKeyValue>";
	string CapiXml832 = "<DSAKeyValue><P>pbJGnzQS8MnlCD9/odKjNmFsFxdOpsaVld2k3yNxbvJGRNPhAm3jXXL+1lAlSanpsBDRzEZfd8QQO1W5dZ3J0D9nLY15Pf49UhMvduYvEzgqofdMWxi0Wc+lfsivsa1/7tgfzVE2Jc0=</P><Q>7XixpXVHZk2vrfb8wng8Ek80sWk=</Q><G>RQKfUz7so8P76ax7pz4l8mSBwlXZbdwc0+ASgXa7lS2o1r/RsNrrrAWrLvMKQP6Nzg48uuDiI4QCoyFoi3ToiqPlV63fl4JSxQgHSCRh2VHg+iG+4fujqAPEfDWsQZGH84/jZLSpw+4=</G><Y>CakyawTgUySuImok8j6zg6y6xizp1NlPBlVmIzggP8rpaATvnapyk2T8jPsMHFsNWFiRbCQapXBvVp2Vqc2w+VOuTi2C0evjbcGT3ZdEAJoxyVkwy3I8P2hHtXtwy1c67kOADAAZZ0c=</Y><J>sp/uZOuB11am784e30URbuizTkAPUXDvQT6FyZGEIEreZGavkCOhOs8+vZ2eevYSPsr8eFC8XaMK+mOa13AQb60kYPuGPAgpw8OJnL/gwMemEbHs</J><Seed>zXDB6meB3EeaXj4xYLDxXdvJzxo=</Seed><PgenCounter>Af4=</PgenCounter><X>6APt5R3qKDQSvI04R6ibq0gRHMo=</X></DSAKeyValue>";
	string CapiXml896 = "<DSAKeyValue><P>tdSKUdac07x6tSb7I/HuZeLTzdP5Ft0ngwcabST3LVPvC9VlczcYidHAMObeTuuL5eu+OGFr8//FmzANMkl2QlBu6L79dNraHb2TeUAApt6YYMfLJgzqXpHRzlxR33Uhox4PY3OAS1W1yNU3DvPQow==</P><Q>0eJqRrUp4/23i5geDgMmjFViKQ0=</Q><G>nWUPHcBQUpy1Wnxz4y+EI/LcfrAMP/quhxJRdcxiTpdsI/OHZXaBnieHjcrGsuOgBAfifapiEUj3mWN2wk5H3wZpxi6TDIXrweWcWIuWpB33Xr1ZHM5TYPmrFfimbav/xJh3Cd9j8wi1eackodrEQw==</G><Y>nja1XNP1H/fNsL5nKymZelvlBP7NPOTVaFp+2L3a6mVamGdl37CHMHlLzXIXYMsaAyf6pGMivwjj2caSu7wwzNdAbavWnagXSSU6pXNnAMHWS266j642xc030/pavBLU/hCV7N5VZ/EapLBUEqqpcg==</Y><J>3cghxRYCKhtnphhsgfHs90NqrPkMx4iIXMNcomAmrJdthiGPTfADM+4AykVs3KDUcEQtkU8eUiZsiDqDdKkCxFwtpdvpp7bViKPjnzl0j7yxf3Fcb2GRW033Rqo=</J><Seed>zrLLYcXgs+SWccZ5R0EeaO41l+w=</Seed><PgenCounter>Byc=</PgenCounter><X>Vbs8rxlLRHVAm5x1UdQXblNT610=</X></DSAKeyValue>";
	string CapiXml960 = "<DSAKeyValue><P>8I6EDGOIko1Fm6r6nBWqLS7A4RhYwfdUilJINLJpsvZ7z0qa4MqqLFB57H3Iok/YLe8Z4SSqrLJDqH4Rxl28rNAOcVBuTiCHx5GNuetfeqF2knFLCg4caJIwI51AbzSJEiBp7tlGyNHdu46XOMFukdC8jkozTbzJ</P><Q>rS9PUEwcN0vYzIcmgon339LAqrE=</Q><G>X33SudRcEMlrtB5xDndktK4UNJOVPuT14ypK4JLB8DTQRqYMkHUqNnTRCYMurOJGsbNP4ebjqp4eCMyQ+zHmEKNNi6AZulFzUvzkdbfBkN8HJYJQC9tdp9U445BUF5lnIsKvv18d9gFbcUvL8aFRmNVY0M9NtSdb</G><Y>fjKb8Z7LZ1ylNo1Yzc904XRFf/AsnORT2l4+1ruPZPPlaRkGRfF8sD/B/6We+rYOJg68F8ngBmCwkzy9QfV4lkeB6n1sco03h7Zd/B6cMjC6tqAThdh70xk9CTyk46fL61qN032HfdF1ca2OqkuDdnfpbIBg0EUt</Y><J>AAAAAWOWpJWt08loRNc8cCf2Dl94/2aRbebis+SxRwqSiF77iEsMlsndA+hLX+6UOE4fFuFm7x38o6iUB1poYI0TaxQrDeaFjovIiPOovcofuP8efHiIeMR6cnugvYWXkC3ZsZASK0g=</J><Seed>iS3sRFr6k97343rVRZLsZVN0Brw=</Seed><PgenCounter>UA==</PgenCounter><X>B5SpH6Fd1S/3GuiHgkZJZLKQcFk=</X></DSAKeyValue>";
	string CapiXml1024 = "<DSAKeyValue><P>rHUuNzvl+8HhyXeXhVePoCZPX7oBr0gIWgMBU067L+hN3AvLw2372iSgnb8cT0wbFjgcJYUfmJcsQfGJCz+ngGeVKFkqPZCrlG6VuAY3mYZ5VZY68q4EtsM4YtqjRNLDIiTdAtHYdMdWnraK58Z/Z688Drbkp0m4PgDEj+VvgcU=</P><Q>+1Qjw//auFyRrXKm3I/ehy5uph0=</Q><G>g++njdKHRxHdhIUjbd2wEsV+q0KJRiiXCbHyxBPb0L4Ahv8vG2SxQftdHwBIEuh8cOKG9/bPmvsmjPdTigbZUKhA4QmH4hhTVa0f1dtJepylCJcwvEmFDG38sw2J95XSAodPBfeuLxOBYrpLXZry5fi6P0m3BgSEqSHjCQfHQok=</G><Y>Ee57vjhzaJi+iHTfVvURccywtV7sauK997+wsk3XH6GC85htoAG42qeHKB93wtA67IuIh6gH6Mt6+RA6jQ1P+OEoCTGHUR2Fu0qhBHGU/AsNXKrrtaDc0a+abcEeZxU1ENhz7D2eOxm/ohp/If2mt2FZ/VtUOt9eOEtOCtRbyaI=</Y><J>r6nCxn/CLCgxXOgQH5c4kjyMoFg/XUB7xNFFHfI6MXI26h3astfAkHzcSJvl5lmM9iLHxgOMIviYXfWFLD++M3rglElCw7BnHccKWtQGxmg1ToFOAjC7UaCejLcDI0i7HX6WdO0YY1ZcqA2U</J><Seed>SpWGXImjbDvjqh5E64HkgSLEMtM=</Seed><PgenCounter>Bc0=</PgenCounter><X>f4PsM9PtzRlhvoLemk9NS5KZq3Q=</X></DSAKeyValue>";

	// import/export XML keypairs
	// so we know that Mono can use keypairs generated by CryptoAPI
	[Test]
	public void CapiXmlImportExport () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider ();

		dsa.FromXmlString (CapiXml512);
		Assert.AreEqual (CapiXml512, dsa.ToXmlString (true), "Capi-Xml512");

		dsa.FromXmlString (CapiXml576);
		Assert.AreEqual (CapiXml576, dsa.ToXmlString (true), "Capi-Xml576");

		dsa.FromXmlString (CapiXml640);
		Assert.AreEqual (CapiXml640, dsa.ToXmlString (true), "Capi-Xml640");

		dsa.FromXmlString (CapiXml704);
		Assert.AreEqual (CapiXml704, dsa.ToXmlString (true), "Capi-Xml704");

		dsa.FromXmlString (CapiXml768);
		Assert.AreEqual (CapiXml768, dsa.ToXmlString (true), "Capi-Xml768");

		dsa.FromXmlString (CapiXml832);
		Assert.AreEqual (CapiXml832, dsa.ToXmlString (true), "Capi-Xml832");

		dsa.FromXmlString (CapiXml896);
		Assert.AreEqual (CapiXml896, dsa.ToXmlString (true), "Capi-Xml896");

		dsa.FromXmlString (CapiXml960);
		Assert.AreEqual (CapiXml960, dsa.ToXmlString (true), "Capi-Xml960");

		dsa.FromXmlString (CapiXml1024);
		Assert.AreEqual (CapiXml1024, dsa.ToXmlString (true), "Capi-Xml1024");
	}

	private void SignAndVerify (string msg, DSACryptoServiceProvider dsa)
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		byte[] sign1 = dsa.CreateSignature (hash);
		byte[] sign2 = dsa.SignData (hash, 0, hash.Length);
		byte[] sign3 = dsa.SignData (new MemoryStream (hash));

		// we don't need the private key to verify
		DSAParameters param = dsa.ExportParameters (false);
		DSACryptoServiceProvider key = (DSACryptoServiceProvider) DSA.Create ();
		key.ImportParameters (param);
		// the signature is never the same so the only way to know if 
		// it worked is to verify it ourselve (i.e. can't compare)
		bool ok = key.VerifySignature (hash, sign1);
		Assert.IsTrue (ok, msg + "-CreateSignature-VerifySignature");

		ok = key.VerifyHash (hash, null, sign1);
		Assert.IsTrue (ok, msg + "-CreateSignature-VerifyHash");

		ok = key.VerifyData (hash, sign2);
		Assert.IsTrue (ok, msg + "-SignData(byte[])-VerifyData");

		ok = key.VerifyData (hash, sign3);
		Assert.IsTrue (ok, msg + "-SignData(Stream)-VerifyData");
	}

	// Validate that we can sign with every keypair and verify the signature
	// With Mono this means that we can use CAPI keypair to sign and verify.
	// For Windows this doesn't mean much.
	[Test]
	public void CapiSignature () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider ();

		dsa.FromXmlString (CapiXml512);
		SignAndVerify ("Capi-512", dsa);

		dsa.FromXmlString (CapiXml576);
		SignAndVerify ("Capi-576", dsa);

		dsa.FromXmlString (CapiXml640);
		SignAndVerify ("Capi-640", dsa);

		dsa.FromXmlString (CapiXml704);
		SignAndVerify ("Capi-704", dsa);

		dsa.FromXmlString (CapiXml768);
		SignAndVerify ("Capi-768", dsa);

		dsa.FromXmlString (CapiXml832);
		SignAndVerify ("Capi-832", dsa);

		dsa.FromXmlString (CapiXml896);
		SignAndVerify ("Capi-896", dsa);

		dsa.FromXmlString (CapiXml960);
		SignAndVerify ("Capi-960", dsa);

		dsa.FromXmlString (CapiXml1024);
		SignAndVerify ("Capi-1024", dsa);
	}

	// Validate that we can verify a signature made with CAPI
	// With Mono this means that we can verify CAPI signatures.
	// For Windows this doesn't mean much.
	[Test]
	public void CapiVerify () 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider ();

		dsa.FromXmlString (CapiXml512);
		byte[] sign512 = { 0xCA, 0x11, 0xA4, 0xCD, 0x5B, 0xBA, 0xA1, 0xC9, 0x8C, 0xEF, 0x9A, 0xB8, 0x84, 0x09, 0x96, 0xD0, 0x1B, 0x39, 0x6D, 0x1C, 0xE1, 0xB2, 0x0E, 0xD3, 0xCE, 0xCF, 0x6A, 0x48, 0xDC, 0x22, 0x40, 0xDC, 0xCD, 0x61, 0x25, 0x7F, 0x9E, 0x1B, 0x79, 0x89 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign512), "Capi-512-Verify");
		sign512[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign512), "Capi-512-VerBad");

		dsa.FromXmlString (CapiXml576);
		byte[] sign576 = { 0x10, 0x77, 0xE9, 0x4C, 0x29, 0xB0, 0xF4, 0x0E, 0x3B, 0xB7, 0x8E, 0x3A, 0x40, 0x22, 0x63, 0x70, 0xF3, 0xA5, 0xB7, 0x4A, 0x5C, 0x85, 0xB5, 0xF3, 0x4B, 0x1C, 0x4A, 0x92, 0xDD, 0x1D, 0xED, 0x63, 0x26, 0xC2, 0x42, 0x20, 0xBE, 0x33, 0x55, 0x57 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign576), "Capi-576-Verify");
		sign576[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign576), "Capi-576-VerBad");

		dsa.FromXmlString (CapiXml640);
		byte[] sign640 = { 0x4C, 0x04, 0xAC, 0xE0, 0x84, 0x04, 0x5A, 0x1D, 0x9D, 0x61, 0xA1, 0x62, 0xBE, 0x11, 0xEA, 0x0D, 0x1C, 0x21, 0xC7, 0x55, 0x2C, 0x7C, 0x84, 0x4F, 0x22, 0xE9, 0xA1, 0xF1, 0x2C, 0x83, 0x13, 0x90, 0xAE, 0x36, 0xFD, 0x59, 0x32, 0x21, 0xAE, 0x0F };
		Assert.IsTrue (dsa.VerifySignature (hash, sign640), "Capi-640-Verify");
		sign640[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign640), "Capi-640-VerBad");

		dsa.FromXmlString (CapiXml704);
		byte[] sign704 = { 0xA2, 0x75, 0x32, 0xE0, 0x4B, 0xCA, 0x92, 0x51, 0x84, 0xAC, 0x7C, 0xDE, 0x97, 0xB8, 0xC3, 0x25, 0xD7, 0xF8, 0xA7, 0xE0, 0x76, 0x42, 0x7E, 0x5E, 0x5E, 0x3F, 0x82, 0xDB, 0x87, 0xBF, 0xC9, 0xCC, 0xD9, 0xA2, 0x8E, 0xA2, 0xFE, 0xD3, 0x48, 0x30 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign704), "Capi-704-Verify");
		sign704[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign704), "Capi-704-VerBad");

		dsa.FromXmlString (CapiXml768);
		byte[] sign768 = { 0x92, 0x27, 0x89, 0x4B, 0xB2, 0xDF, 0xE9, 0x98, 0x5A, 0xC5, 0x78, 0x5E, 0xBD, 0x51, 0x6D, 0x10, 0x30, 0xEC, 0x14, 0x95, 0x6E, 0xEB, 0xA6, 0x5F, 0x3E, 0x47, 0x47, 0x86, 0x19, 0xD0, 0xF2, 0x9B, 0x70, 0x98, 0x97, 0x07, 0x04, 0x0C, 0x13, 0xC6 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign768), "Capi-768-Verify");
		sign768[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign768), "Capi-768-VerBad");

		dsa.FromXmlString (CapiXml832);
		byte[] sign832 = { 0xC7, 0x10, 0x86, 0x86, 0x4A, 0x19, 0xBC, 0x8E, 0xC5, 0x0E, 0x53, 0xC0, 0x9E, 0x70, 0x2C, 0xFD, 0x4B, 0x9B, 0xBD, 0x79, 0x46, 0x8E, 0x9F, 0x64, 0x41, 0xF9, 0xBB, 0xDD, 0x3B, 0x93, 0x63, 0x82, 0x7B, 0x9B, 0x5B, 0x12, 0x9B, 0xAA, 0x90, 0xAD };
		Assert.IsTrue (dsa.VerifySignature (hash, sign832), "Capi-832-Verify");
		sign832[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign832), "Capi-832-VerBad");

		dsa.FromXmlString (CapiXml896);
		byte[] sign896 = { 0x7F, 0x0F, 0x5F, 0xC4, 0x44, 0x38, 0x65, 0xD7, 0x0B, 0x03, 0xD1, 0xAC, 0x77, 0xA2, 0xA2, 0x47, 0x37, 0x37, 0x42, 0xA2, 0x97, 0x23, 0xDA, 0x7F, 0xEC, 0xD5, 0x78, 0x3D, 0x5E, 0xDA, 0xA0, 0x02, 0xD6, 0x2D, 0x4B, 0xFA, 0x79, 0x7B, 0x7A, 0x87 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign896), "Capi-896-Verify");
		sign896[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign896), "Capi-896-VerBad");

		dsa.FromXmlString (CapiXml960);
		byte[] sign960 = { 0x63, 0x77, 0x39, 0xE5, 0x03, 0xD2, 0x33, 0xF5, 0xFE, 0x16, 0xE4, 0x7E, 0x49, 0x4E, 0x72, 0xA0, 0x1B, 0x8D, 0x4D, 0xEC, 0x55, 0x15, 0x72, 0x1C, 0x22, 0x37, 0x4B, 0x64, 0xEB, 0x02, 0x3E, 0xC5, 0xCF, 0x32, 0x07, 0xC5, 0xD3, 0xA2, 0x02, 0x0A };
		Assert.IsTrue (dsa.VerifySignature (hash, sign960), "Capi-960-Verify");
		sign960[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign960), "Capi-960-VerBad");

		dsa.FromXmlString (CapiXml1024);
		byte[] sign1024 = { 0x1C, 0x5A, 0x9D, 0x82, 0xDD, 0xE0, 0xF5, 0x65, 0x74, 0x48, 0xFB, 0xD6, 0x27, 0x92, 0x98, 0xEA, 0xC6, 0x7F, 0x4F, 0x7C, 0x49, 0xFC, 0xCF, 0x46, 0xEB, 0x62, 0x27, 0x05, 0xFC, 0x9C, 0x40, 0x74, 0x5B, 0x13, 0x7E, 0x76, 0x9C, 0xE4, 0xF6, 0x53 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign1024), "Capi-1024-Verify");
		sign1024[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign1024), "Capi-1024-VerBad");
	}

	// all keypairs generated by Mono on Windows
	string MonoXml512 = "<DSAKeyValue><P>zh9f0jwaZU7uk9A2YZkKDiAt3Rk/UFieBrs9CtvrWDNjmZyB8OZtwdt5qoZxxk/ZQdhnQPyY4Wfb2AXfcNw1oQ==</P><Q>qRl9bdSR4U2HEJ75f8v/h9ofAXs=</Q><G>aZZELrF/gAWcJnITv2pCzpwbP4d1OppRyEko5Ub6HJYL4Ez4TPTtveW+6ni90Kqq6Hbfc1x26MaU+c7s8GsTBw==</G><Y>j4sRCjiN0vdibzB0EuHFdDmB1nb8YmjEMtLzU2A7icqVbo+obwAIofNSaEW5bSc1nSGL1AFD2WIa49U6yewxXg==</Y><J>AAAAATgMk/+kNvZLy1QCT75cuivedK6Iz+bZ2g/ncnvBjjb5oM9PFxAZU+MNI57g</J><Seed>tEXsCuDb0lrfypB5kTI5iTWPqSA=</Seed><PgenCounter>xg==</PgenCounter><X>nd+0VT69+NJ0l4DuQDnOwCDR30A=</X></DSAKeyValue>";
	string MonoXml576 = "<DSAKeyValue><P>+IVgSLz5ciuK8w5Wq56zX5ZDU/A4XrwTUngLmG4SyjtHLZUVS4npZ/gaJk02oSl4E1WUKL8yKDy1p3jewbH5bAScuprQpXr5</P><Q>01NXNM+IZtv6fl8Zr64daxJHAqk=</Q><G>ZrM3QjPyuINgj38UEqi1APX3zz7jsoCUNnslCjVgzhS0q9L44FcKPxW9WW+uh/N2PxcSqDMCd3yFvbzzgHG8+Te12UT00BHW</G><Y>CUHNKbeRzIBQncxOhaC7pKz0Gscx5zhO++BTvGRDKkOUBbvO+kf52Y+q1Hmp0OS+I3tDFU80V7Lmt5UpPw4DyzSuqukkI6Uy</Y><J>AAAAAS0PAQxeB0hDGlnmieeuKG1X8KsogV8rdq8fI9Glhu44LfGZmzBNsBCrqHSMBVVFWTerdjg=</J><Seed>eUE4tvz2nInFOGDhfMFO90p+d1o=</Seed><PgenCounter>8Q==</PgenCounter><X>iAWUFn8TSUh5GYNLB2YLyzg6KxE=</X></DSAKeyValue>";
	string MonoXml640 = "<DSAKeyValue><P>7aE8g2fajUuMRVNVdblVtOqoFjWao0gPvS9z/rZAtfcJhZ/Q75PFUUKhBz/zMClT0O6GY695LErNUQvIH9bfgrM5Mywk5tRDV2FN61kWg00=</P><Q>ppaB4BvQvi/PtUi60y6cO55BGS8=</Q><G>g+ImQPN/hbRDu/A23Wv9MMVHRUgMGaPySPeOtdKad96iycTS0ddC/Odo7fvg11+ukiauD7Sec4UQPNGFHoT0vozAcg6SDmX4KgM4M5QDsl8=</G><Y>lsItHPOgvebvDU7o+Cns17Q1+EZ3ySG2DXQ5RyZ8zB4UmBfsC8Lnz5EliGZqej/D5OJSAjNfIx9hh+Jd6ENBNNyWDZnLIwYi5WuwhxFZ+vY=</Y><J>AAAAAW0sBVV2lXw4OIn3UIqYPzE+ith31rrZ0OnTlqYElQqSXub7Sf20ILEIYGQ3CZ2so4t1qmebmjK+cwAGdA==</J><Seed>fecSUnEdYuJJP2pIGw89Apu74Qs=</Seed><PgenCounter>Rg==</PgenCounter><X>nDBo1S2rT/XfwdHFVa7LcVoNCuE=</X></DSAKeyValue>";
	string MonoXml704 = "<DSAKeyValue><P>lWVVsk//enAqLcmFBuw68MRuF4OBENHlnS9Fcy6idae43Di/yRkNxURLcjKAgDne6a0fPvAXMa7iyOm8r1GO6DtdR3xepApufnhQrn982lZY2D2Uzufkxw==</P><Q>3tFuyT6SXpfM746TBeufyWcVxjU=</Q><G>d5x4zOlwNSNk+nzF6rMl3iU81NHm7Org+ZjV00haTRcud1nSn9qlPMmh1Iyu2eDb/hVFYSHVriR6JL9uhnHRFgDmG0Gq3rLYjP9HwSc0bSZJhpnvMh+plg==</G><Y>gMyp9cb1b+PiJVjHm1JXyMRpPQaqXAwv9aZ/3ALUMla5xWbs8eOX0xKb03F9SiOyrrOLl7FU1NV3aPAqlxRVAEoDWfs59kwFuvtxTNCPMA9yUe9LIPmESg==</Y><J>q6TNHPV5sVaSSavn0P3kpQ/dEnw6r+Kf6vA8f1A6XwsufncwAAjLgkDD5tglPcUzuqSBNbpGnZgXHpD7mXvORGzJEm4=</J><Seed>U4J0gynSch6aQjtYthQvOrZ5FDo=</Seed><PgenCounter>2g==</PgenCounter><X>yYzu1dTYzh9veG5e+Uph0fJdOLo=</X></DSAKeyValue>";
	string MonoXml768 = "<DSAKeyValue><P>rpEwOnT4y+tJvCg9LvuQ+PxB12r1ZghEaF7fiGh46ip2GsMfotb24tE58YpzTqiaJT3H/w5Ps7MZKE42322dX1oV9kRSmr/EYYS9GdUbnKCtfa+sA1x0FwDZgqaCZ3pV</P><Q>5eRCKf4gMUCAToisFaJsn0xQ6IM=</Q><G>bKVUkTMOgonH9RzGI1FXbLgemhVq2qSpwVce7f3qjGDPnw7uzXfLPqvMc3WYRGLB99gps+8SNdJ7pdxIprAyGG0ovl0Fq1X7Xwk9RQBqdbxnx+tLCXhkgLV/OpEnJGqs</G><Y>eX3lNXsbmfeg2Ac4dzTqQMowXV22ydnoXtOE2EggOE60WcHxiNql0yLUvFfRIdf0PnJa7/EfqzAxc1U6dHGljNWwH8mvsJXKQ/ps5fa8v5UveAzg73chUparNM9d7WAp</Y><J>wmR011Gh0XpjfIpTczt/J4pQq5L9IqrhxeQHPgRTC5/xL4Lh8cheAlVDrF8RqgnE6VgSCTOOfeq+7ySLNTdrWA3xweyS5s+lpqQEHA==</J><Seed>yWTv1DNaxQxqdKMdwPCzpP1dz6Q=</Seed><PgenCounter>PQ==</PgenCounter><X>5bS76Q72Mkw8/c9q8VZJY8XQ3vs=</X></DSAKeyValue>";
	string MonoXml832 = "<DSAKeyValue><P>/8mh+RcX8QTo1cxuXYuSs4FihZ9IiKocX/M1gHCtsK9vWHBH+JoRPJ0huuQLp0qZzK0BncPyJdkST+HCnOuWEzBBunf2cebtAl6Bm3LYldzCdig7ptrlHsVBSOzSnBsEfl6VAGvFEw0=</P><Q>3j2ihzDNHb2OIm/Ly+928iz6r2M=</Q><G>ctmj3nFuBvbVYJo9sxmOvmnRsd+Z4bVc+ieb450uRdA+IsXSqfTN1phHUt7DQmFxBALi0ygg7PX1V+0JkZf4Ui1/lMPdznA6H+UJzV0GgJpCNwsYfrZbNFddm+wunRMuz6uTB57UKxM=</G><Y>cI8rpi0L89rBytXQCl/HFlsoTrogdzcj7/yYgzDRqnuINUA7lUJ1Vzj803p9IKQg6pNb0jRKwHcYnHAg/Lb3yWJcQHsZWvdfAG3vR/P8LM15BiXnAGuur7X8zRa1E9/vqcn77GCsuuI=</Y><J>AAAAASakjFeNPQ1kUYa8c77CkoEGZCAfG5+31W/00Tnu8QkgZMjGaUp/4vzvzgAn+KuZh0lRYbxi4IHpCEvgji7Cr+w3AFLdtRK63TwvJFkWLqx8N64MhA==</J><Seed>cblDQQ0wbr/Tpb/Pu3IcICApHx8=</Seed><PgenCounter>jQ==</PgenCounter><X>uVVaaWQdHK6O4mlrLXUouHTQQbo=</X></DSAKeyValue>";
	string MonoXml896 = "<DSAKeyValue><P>x0mhnUZKcKqmCMpbvWfF67pBkQeJSm9I8S0BZISyeSpt9CS9qSnZlCyA+bRPA6BURXl6cPkDKMosHvkN40TWR8nj0zx5ggKmWFDZC7FaIXlvrhs4ALwNMC6Ji376r0EhZIpl+rGaOFM9uz+ya7fasQ==</P><Q>7PE0XlOwB9mi8PKk6HjW5bsU2Ds=</Q><G>r1DKk6xGIRWkE0PdC6kqsAECOZ2Q1VKZow+5xLCMAUlH+172fl4ViXw1Ihk6LTj5BYCb5NLxTO26naUoxQfY5FT/C3e9k17UWVBI7cPpNJNmIQGY2N/enI01tGWSZO5THyaXI0mvG2mI0Io8kG+E3Q==</G><Y>N+v4DSXxsSteaPQxu6eOBdhWpd2N1eV/A0RZYAcoWXb2uF8L3foq4Ake+VXgLRWDExkhBQurWBQxrD4vJbdgHeI/yKkmHL6IATDkuv0KPkZ8gMqJJPszk5UhxHxTo9+NiYKlGZO6qhJYV9MRbcpQqQ==</Y><J>11EYG/P5XTPQRJSyHboxKKol7zCm/0BnWvjsdBaPC3bHh2g6GR6uOE0uPZ+FJLVvP8iqClvWbxQF8pm/yKRaZa3SaJZjk1DNuEQZOkkxWqkti8MC2mIAXdCwlRA=</J><Seed>8nTVIayazWuhjpKfKkYnDz/Y3Yk=</Seed><PgenCounter>sAE=</PgenCounter><X>0nvKIkkjg3yHb0rw9eLM2nPExdk=</X></DSAKeyValue>";
	string MonoXml960 = "<DSAKeyValue><P>rvLpyJmoSI71LUh2slfZmfafcV/iJj2PbR+hN80NL4YAyw22T8zG4s2q5Ez1eBN+34ob48+f2UJsvSDJrFCFWHvrifgGWkUmssDBA8iIP2oAZ8jy9YdJ0QoOa2DPi9IsK6Xo0p1IsexsPpxzHzAahs86e1tBSqob</P><Q>/lEFkFT1So7tFkYuLb03uWAlekE=</Q><G>WhRVK0LJInEG5DM42yxmrgq4J8DS4uofTyD3VEAHlKWVmE7okNDrqGI+kVvMOtePVrHsl6asIlSIk+U27QW6F8opsS2vb+ior/WcFT4HjcKgGScFk4ZTVijFFejM4pLV3FuR83nSVxn4p/HzheLX6sCISWD4Pdrp</G><Y>jHV8rnyQZLWf/6DfDUpWqRjvmU2F4EqiExYJwmfwkauwar8uzfmLCzrpMYvhF1NQ8OVyP7vYkgF2QFuMBG7X0HFNLji4mrF7DNkeUB+2LAkecpCWCgpIbPoOPXVQqe/bJgYf+8ttozbQmJTAqaGrJ2tTxweqiYCm</Y><J>sBtkEkGWVMPWdkg1WH25Q+gISLVadvRnXn4dC6n/FTqFHunYTWyh+FuLGA1l899fubwBZNrMGVhLhWdEv0wan6v0SPHQ2iz4EW5LeO/sEyXUfxhoVT7MwkMwTcMYhRmkwCFfmg==</J><Seed>aY7TaL00Rm9qsE3eU2cu3auWHVs=</Seed><PgenCounter>dw==</PgenCounter><X>kUX5eEVEaWpgm5DtHifImDLuSb8=</X></DSAKeyValue>";
	string MonoXml1024 = "<DSAKeyValue><P>rD4HJ6rTF7dEz2RvHhnqREpu8fW0Fs8181CHqEqgUuAKG3x8euMTHlIt+kZ95RT1chLXGBRhOWT2YTgq04cTczatZhrxSWnI8hz1eHbZvRAGQ6ZP2FMvvWyJMgWnU7D5aShUM3LOqM1TgHumGwSYXhBvbj42Jjzbn80ULPkLmOU=</P><Q>pwiHLGzrt+FSJ0+IwTiS7/N3/Ls=</Q><G>OWdsYYZ0u/p6vvBrI0X+KTubx2dXG396w01dYIshLAwA4EsoDskusyr8mG4bvxFKLzGCt2ZjC7QImSKO8Y72TmNeYmmbeDc9IYENk7LuBzd8WboCRIt2gZ/KTh26TkiN8072ZVOqRZn3I35SFkUUn6PVng6EXkuhJ8EzLyvXev4=</G><Y>EcNLWKf9FJX5OUB0ON7aNASSH1SxzqvYCK8uP4H3y2LyRo7fi1JvDi/BUSyhCxfvRMG8O5QUi2yAXfNFWqEVnGUxlwNUzpx4UwKF+vYZAcnU8Wb0RJ++VT+O2NN9wou1Ys4p/gWYNsi4fk0nm8Hjx9ae5VyMXSQHEhQuUxwxWw0=</Y><J>AAAAAQf7wobrNXeeDP58+AEbzO7Gl5pTvqHfipR8ttv5yuOTahOrQFOKbH3ZpVLDF6jJsj2TMJLKsFaZsOcuEgVONT8R0nOAWbskUMxYV72492RwWylTKolkcbRyhonRbpOUFdE6xrztzijDeYRObA==</J><Seed>jiFLa4UqlTqHrUk69fHpjjc9Fe0=</Seed><PgenCounter>JQE=</PgenCounter><X>i+yoBF9nTiYNVeONGl8+/7gf1LQ=</X></DSAKeyValue>";

	// import/export XML keypairs
	// so we know that Windows (original MS Framework) can use keypairs generated by Mono
	[Test]
	public void MonoXmlImportExport () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider ();

		dsa.FromXmlString (MonoXml512);
		Assert.AreEqual (MonoXml512, dsa.ToXmlString (true), "Mono-Xml512");

		dsa.FromXmlString (MonoXml576);
		Assert.AreEqual (MonoXml576, dsa.ToXmlString (true), "Mono-Xml576");

		dsa.FromXmlString (MonoXml640);
		Assert.AreEqual (MonoXml640, dsa.ToXmlString (true), "Mono-Xml640");

		dsa.FromXmlString (MonoXml704);
		Assert.AreEqual (MonoXml704, dsa.ToXmlString (true), "Mono-Xml704");

		dsa.FromXmlString (MonoXml768);
		Assert.AreEqual (MonoXml768, dsa.ToXmlString (true), "Mono-Xml768");

		dsa.FromXmlString (MonoXml832);
		Assert.AreEqual (MonoXml832, dsa.ToXmlString (true), "Mono-Xml832");

		dsa.FromXmlString (MonoXml896);
		Assert.AreEqual (MonoXml896, dsa.ToXmlString (true), "Mono-Xml896");

		dsa.FromXmlString (MonoXml960);
		Assert.AreEqual (MonoXml960, dsa.ToXmlString (true), "Mono-Xml960");

		dsa.FromXmlString (MonoXml1024);
		Assert.AreEqual (MonoXml1024, dsa.ToXmlString (true), "Mono-Xml1024");
	}

	// Validate that we can sign with every keypair and verify the signature
	// With Windows this means that we can use Mono keypairs to sign and verify.
	// For Mono this doesn't mean much.
	[Test]
	public void MonoSignature () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider ();

		dsa.FromXmlString (MonoXml512);
		SignAndVerify ("Mono-512", dsa);

		dsa.FromXmlString (MonoXml576);
		SignAndVerify ("Mono-576", dsa);

		dsa.FromXmlString (MonoXml640);
		SignAndVerify ("Mono-640", dsa);

		dsa.FromXmlString (MonoXml704);
		SignAndVerify ("Mono-704", dsa);

		dsa.FromXmlString (MonoXml768);
		SignAndVerify ("Mono-768", dsa);

		dsa.FromXmlString (MonoXml832);
		SignAndVerify ("Mono-832", dsa);

		dsa.FromXmlString (MonoXml896);
		SignAndVerify ("Mono-896", dsa);

		dsa.FromXmlString (MonoXml960);
		SignAndVerify ("Mono-960", dsa);

		dsa.FromXmlString (MonoXml1024);
		SignAndVerify ("Mono-1024", dsa);
	}

	// Validate that we can verify a signature made with Mono
	// With Windows this means that we can verify Mono signatures.
	// For Mono this doesn't mean much.
	[Test]
	public void MonoVerify () 
	{
		byte[] hash = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13 };
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider ();

		dsa.FromXmlString (MonoXml512);
		byte[] sign512 = { 0x53, 0x44, 0x0A, 0xD7, 0x43, 0x3C, 0x1F, 0xC7, 0xCE, 0x9C, 0xAE, 0xDC, 0xFC, 0x61, 0xD5, 0xCE, 0xC9, 0x5C, 0x8D, 0x13, 0x0F, 0x66, 0x15, 0xCB, 0x9F, 0x94, 0x19, 0x18, 0x63, 0x40, 0x6D, 0x3E, 0x16, 0xA8, 0x3E, 0x9B, 0x8A, 0xC2, 0xA5, 0x38 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign512), "Mono-512-Verify");
		sign512[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign512), "Mono-512-VerBad");

		dsa.FromXmlString (MonoXml576);
		byte[] sign576 = { 0xAC, 0xC8, 0xA3, 0x22, 0x3D, 0x77, 0xE1, 0x13, 0xFD, 0x65, 0x72, 0xE0, 0xA5, 0xF5, 0x94, 0xD6, 0x70, 0x70, 0x40, 0xA9, 0x10, 0x1E, 0x1D, 0xEA, 0x51, 0x68, 0x32, 0x2A, 0x24, 0x47, 0x22, 0x8D, 0xC0, 0xD0, 0x8A, 0x5A, 0x0E, 0x98, 0x5F, 0x2C };
		Assert.IsTrue (dsa.VerifySignature (hash, sign576), "Mono-576-Verify");
		sign576[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign576), "Mono-576-VerBad");

		dsa.FromXmlString (MonoXml640);
		byte[] sign640 = { 0x5E, 0x6E, 0x1F, 0x38, 0xCA, 0x0D, 0x01, 0x01, 0xBE, 0x5E, 0x8B, 0xAB, 0xCE, 0xD2, 0x42, 0x80, 0x4A, 0xBD, 0x74, 0x60, 0x56, 0x4B, 0x16, 0x4A, 0x71, 0xBC, 0xC3, 0x82, 0xE1, 0x54, 0x0D, 0xB0, 0x67, 0xB6, 0xB6, 0x71, 0x46, 0xA9, 0x01, 0x44 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign640), "Mono-640-Verify");
		sign640[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign640), "Mono-640-VerBad");

		dsa.FromXmlString (MonoXml704);
		byte[] sign704 = { 0x27, 0x33, 0x45, 0xC5, 0x41, 0xC7, 0xD6, 0x5D, 0x68, 0x32, 0x50, 0x4C, 0xB3, 0x4B, 0x59, 0xCF, 0x49, 0xD1, 0x61, 0x82, 0xCF, 0x73, 0x20, 0x20, 0x3E, 0x09, 0xA3, 0x49, 0xAA, 0x22, 0x1D, 0x1D, 0x27, 0xBA, 0x9F, 0xDD, 0xE2, 0x7D, 0xCD, 0x82 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign704), "Mono-704-Verify");
		sign704[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign704), "Mono-704-VerBad");

		dsa.FromXmlString (MonoXml768);
		byte[] sign768 = { 0xCA, 0x53, 0x91, 0x99, 0xAE, 0x1B, 0x97, 0xE5, 0x3B, 0x08, 0x78, 0x92, 0xD1, 0x2E, 0x0D, 0xAC, 0xB7, 0x82, 0xFB, 0xA3, 0x84, 0xEE, 0x9B, 0x5E, 0x12, 0x6C, 0x16, 0x6D, 0x97, 0xC1, 0xCF, 0x9A, 0xA9, 0xCF, 0x6A, 0x6E, 0x08, 0x45, 0xA7, 0x19 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign768), "Mono-768-Verify");
		sign768[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign768), "Mono-768-VerBad");

		dsa.FromXmlString (MonoXml832);
		byte[] sign832 = { 0x7F, 0x1C, 0xC5, 0xA4, 0xDB, 0x95, 0x27, 0xD3, 0x23, 0x6E, 0xCE, 0xBC, 0xC0, 0x9D, 0x82, 0x02, 0x6E, 0xA0, 0x80, 0x5D, 0x53, 0x54, 0x3D, 0x1B, 0x1C, 0x54, 0xDD, 0x1F, 0xD5, 0x7E, 0x07, 0x60, 0xDD, 0x2A, 0xB2, 0x96, 0x3C, 0x36, 0xB3, 0x60 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign832), "Mono-832-Verify");
		sign832[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign832), "Mono-832-VerBad");

		dsa.FromXmlString (MonoXml896);
		byte[] sign896 = { 0x36, 0x90, 0xA2, 0x4C, 0xDA, 0xDC, 0x6C, 0xF3, 0x83, 0xFE, 0xA8, 0x14, 0xFB, 0x01, 0x69, 0x5F, 0xFA, 0xFA, 0x71, 0xA6, 0x6F, 0xBE, 0x96, 0xB7, 0x11, 0xE7, 0xDE, 0xC7, 0x71, 0x10, 0x83, 0xEE, 0x34, 0x18, 0x4E, 0x88, 0xC1, 0xE0, 0xF9, 0x0A };
		Assert.IsTrue (dsa.VerifySignature (hash, sign896), "Mono-896-Verify");
		sign896[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign896), "Mono-896-VerBad");

		dsa.FromXmlString (MonoXml960);
		byte[] sign960 = { 0xCE, 0xE8, 0x52, 0x32, 0x9B, 0x30, 0xB5, 0x22, 0x6C, 0x21, 0x34, 0xC6, 0x09, 0xD8, 0xA8, 0x6D, 0x00, 0x87, 0x0F, 0x87, 0x21, 0x50, 0x18, 0x99, 0xED, 0x2A, 0xD3, 0xA5, 0x82, 0x8D, 0x38, 0x63, 0x21, 0xDA, 0xE1, 0x94, 0x65, 0xE1, 0x6E, 0x72 };
		Assert.IsTrue (dsa.VerifySignature (hash, sign960), "Mono-960-Verify");
		sign960[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign960), "Mono-960-VerBad");

		dsa.FromXmlString (MonoXml1024);
		byte[] sign1024 = { 0x67, 0x73, 0x4D, 0x6C, 0x0E, 0xB2, 0x85, 0xC6, 0x97, 0x5F, 0x09, 0x42, 0xEA, 0xDB, 0xC6, 0xE1, 0x6D, 0x84, 0xA0, 0x15, 0x63, 0x6C, 0xE3, 0x83, 0xA5, 0xCB, 0x58, 0xC6, 0x51, 0x3C, 0xB4, 0xD2, 0x6B, 0xA0, 0x70, 0xCE, 0xD9, 0x8D, 0x96, 0xCA };
		Assert.IsTrue (dsa.VerifySignature (hash, sign1024), "Mono-1024-Verify");
		sign1024[0] = 0x00;
		Assert.IsFalse (dsa.VerifySignature (hash, sign1024), "Mono-1024-VerBad");
	}

	// Key Pair Persistence Tests
	// References
	// a.	.Net Framework Cryptography Frequently Asked Questions, Question 8
	//	http://www.gotdotnet.com/team/clr/cryptofaq.htm
	// b.	Generating Keys for Encryption and Decryption
	//	http://msdn.microsoft.com/library/en-us/cpguide/html/cpcongeneratingkeysforencryptiondecryption.asp

	[Test]
	[Category ("TargetJvmNotSupported")]
	public void Persistence_PersistKeyInCsp_False () 
	{
		CspParameters csp = new CspParameters (3, null, "Persistence_PersistKeyInCsp_False");
		// MS generates (or load) keypair here
		// Mono load (if it exists) the keypair here
		DSACryptoServiceProvider dsa1 = new DSACryptoServiceProvider (minKeySize, csp);
		// Mono will generate the keypair here (if it doesn't exists)
		string first = dsa1.ToXmlString (true);

		// persistance is "on" by default when a CspParameters is supplied
		Assert.IsTrue (dsa1.PersistKeyInCsp, "PersistKeyInCsp");

		// this means nothing if we don't call Clear !!!
		dsa1.PersistKeyInCsp = false;
		Assert.IsFalse (dsa1.PersistKeyInCsp, "PersistKeyInCsp");

		// reload using the same container name
		DSACryptoServiceProvider dsa2 = new DSACryptoServiceProvider (minKeySize, csp);
		string second = dsa2.ToXmlString (true);

		Assert.AreEqual (first, second, "Key Pair Same Container");
	}

	[Test]
	[Category ("TargetJvmNotSupported")]
	public void Persistence_PersistKeyInCsp_True () 
	{
		CspParameters csp = new CspParameters (3, null, "Persistence_PersistKeyInCsp_True");
		// MS generates (or load) keypair here
		// Mono load (if it exists) the keypair here
		DSACryptoServiceProvider dsa1 = new DSACryptoServiceProvider (minKeySize, csp);
		// Mono will generate the keypair here (if it doesn't exists)
		string first = dsa1.ToXmlString (true);

		// persistance is "on" by default
		Assert.IsTrue (dsa1.PersistKeyInCsp, "PersistKeyInCsp");

		// reload using the same container name
		DSACryptoServiceProvider dsa2 = new DSACryptoServiceProvider (minKeySize, csp);
		string second = dsa2.ToXmlString (true);

		Assert.AreEqual (first, second, "Key Pair Same Container");
	}

	[Test]
	[Category ("TargetJvmNotSupported")]
	public void Persistence_Delete () 
	{
		CspParameters csp = new CspParameters (3, null, "Persistence_Delete");
		// MS generates (or load) keypair here
		// Mono load (if it exists) the keypair here
		DSACryptoServiceProvider dsa1 = new DSACryptoServiceProvider (minKeySize, csp);
		// Mono will generate the keypair here (if it doesn't exists)
		string original = dsa1.ToXmlString (true);

		// note: Delete isn't well documented but can be done by 
		// flipping the PersistKeyInCsp to false and back to true.
		dsa1.PersistKeyInCsp = false;
		dsa1.Clear ();

		// recreate using the same container name
		DSACryptoServiceProvider dsa2 = new DSACryptoServiceProvider (minKeySize, csp);
		string newKeyPair = dsa2.ToXmlString (true);

		Assert.IsTrue (original != newKeyPair, "Key Pair Deleted");
	}

	[Test]
	public void PersistKey_True () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider (minKeySize);
		string key = dsa.ToXmlString (true);
		dsa.PersistKeyInCsp = true;
		Assert.AreEqual (key, dsa.ToXmlString (true), "PersistKeyInCsp-True");
	}

	[Test]
	public void PersistKey_False () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider (minKeySize);
		string key = dsa.ToXmlString (true);
		dsa.PersistKeyInCsp = false;
		Assert.AreEqual (key, dsa.ToXmlString (true), "PersistKeyInCsp-False");
	}

	[Test]
	public void PersistKey_FalseTrue () 
	{
		DSACryptoServiceProvider dsa = new DSACryptoServiceProvider (minKeySize);
		string key = dsa.ToXmlString (true);
		dsa.PersistKeyInCsp = false;
		dsa.PersistKeyInCsp = true;
		Assert.AreEqual (key, dsa.ToXmlString (true), "PersistKeyInCsp-FalseTrue");
	}

	[Test]
	public void UseMachineKeyStore_Default () 
	{
		Assert.IsFalse (DSACryptoServiceProvider.UseMachineKeyStore);
	}

	[Test]
	[Category ("TargetJvmNotSupported")]
	public void UseMachineKeyStore () 
	{
		// note only applicable when CspParameters isn't used - which don't
		// help much as you can't know the generated key container name
		try {
			DSACryptoServiceProvider.UseMachineKeyStore = true;
			CspParameters csp = new CspParameters (13, null, "UseMachineKeyStore");
			csp.KeyContainerName = "UseMachineKeyStore";
			DSACryptoServiceProvider dsa = new DSACryptoServiceProvider (csp);
			string machineKeyPair = dsa.ToXmlString (true);
			dsa.Clear ();

			DSACryptoServiceProvider.UseMachineKeyStore = false;
			csp = new CspParameters (13, null, "UseMachineKeyStore");
			csp.Flags |= CspProviderFlags.UseMachineKeyStore;
			dsa = new DSACryptoServiceProvider (csp);

			Assert.IsTrue (machineKeyPair != dsa.ToXmlString (true), "UseMachineKeyStore");
		}
		catch (CryptographicException ce) {
			// only root can create the required directory (if inexistant)
			// afterward anyone can use (read from) it
			if (!(ce.InnerException is UnauthorizedAccessException))
				throw;
		}
		catch (UnauthorizedAccessException) {
		}
	}
		
#if !NET_2_1
	[Test]
	[Category ("NotWorking")]
	public void CspKeyContainerInfo_NewKeypair ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		CspKeyContainerInfo info = dsa.CspKeyContainerInfo;
		Assert.IsTrue (info.Accessible, "Accessible");
		// FIXME	AssertNotNull ("CryptoKeySecurity", info.CryptoKeySecurity);
		Assert.IsTrue (info.Exportable, "Exportable");
		Assert.IsFalse (info.HardwareDevice, "HardwareDevice");
		Assert.IsNotNull (info.KeyContainerName, "KeyContainerName");
		Assert.AreEqual (KeyNumber.Signature, info.KeyNumber, "KeyNumber");
		Assert.IsFalse (info.MachineKeyStore, "MachineKeyStore");
		Assert.IsFalse (info.Protected, "Protected");
		Assert.IsNotNull (info.ProviderName, "ProviderName");
		Assert.AreEqual (13, info.ProviderType, "ProviderType");
		Assert.IsTrue (info.RandomlyGenerated, "RandomlyGenerated");
		Assert.IsFalse (info.Removable, "Removable");
		Assert.IsNotNull (info.UniqueKeyContainerName, "UniqueKeyContainerName");
	}

	[Test]
	[Category ("NotWorking")]
	public void CspKeyContainerInfo_ImportedKeypair ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		DSAParameters rsap = AllTests.GetKey (true);
		dsa.ImportParameters (rsap);
		CspKeyContainerInfo info = dsa.CspKeyContainerInfo;
		Assert.IsTrue (info.Accessible, "Accessible");
// FIXME	AssertNotNull ("CryptoKeySecurity", info.CryptoKeySecurity);
		Assert.IsTrue (info.Exportable, "Exportable");
		Assert.IsFalse (info.HardwareDevice, "HardwareDevice");
		Assert.IsNotNull (info.KeyContainerName, "KeyContainerName");
		Assert.AreEqual (KeyNumber.Signature, info.KeyNumber, "KeyNumber");
		Assert.IsFalse (info.MachineKeyStore, "MachineKeyStore");
		Assert.IsFalse (info.Protected, "Protected");
		Assert.IsNotNull (info.ProviderName, "ProviderName");
		Assert.AreEqual (13, info.ProviderType, "ProviderType");
		Assert.IsTrue (info.RandomlyGenerated, "RandomlyGenerated");
		Assert.IsFalse (info.Removable, "Removable");
		Assert.IsNotNull (info.UniqueKeyContainerName, "UniqueKeyContainerName");
	}

	[Test]
	[Category ("NotWorking")]
	// This case wasn't fixed in Nov CTP
	public void CspKeyContainerInfo_ImportedPublicKey ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		DSAParameters rsap = AllTests.GetKey (false);
		dsa.ImportParameters (rsap);
		CspKeyContainerInfo info = dsa.CspKeyContainerInfo;
		Assert.IsFalse (info.Accessible, "Accessible");
		// info.CryptoKeySecurity throws a CryptographicException at this stage
		// info.Exportable throws a CryptographicException at this stage
		Assert.IsFalse (info.HardwareDevice, "HardwareDevice");
		Assert.IsNotNull (info.KeyContainerName, "KeyContainerName");
		Assert.AreEqual (KeyNumber.Signature, info.KeyNumber, "KeyNumber");
		Assert.IsFalse (info.MachineKeyStore, "MachineKeyStore");
		// info.Protected throws a CryptographicException at this stage
		Assert.IsNotNull (info.ProviderName, "ProviderName");
		Assert.AreEqual (13, info.ProviderType, "ProviderType");
		Assert.IsTrue (info.RandomlyGenerated, "RandomlyGenerated");
		Assert.IsFalse (info.Removable, "Removable");
		// info.UniqueKeyContainerName throws a CryptographicException at this stage
	}
#endif

	[Test]
	public void ExportCspBlob_Full ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		DSAParameters dsap = AllTests.GetKey (true);
		dsa.ImportParameters (dsap);

		byte[] keypair = dsa.ExportCspBlob (true);
		Assert.AreEqual ("07-02-00-00-00-22-00-00-44-53-53-32-00-04-00-00-D3-7B-C6-ED-3F-72-44-BD-22-7D-F2-D4-62-FE-7C-E3-75-8F-9C-DE-69-C0-3A-0C-E6-26-5A-46-3D-EF-0D-90-20-C6-F6-45-7C-73-E8-B9-7D-86-27-F7-97-76-C8-1C-8E-8B-CE-26-93-E6-21-53-20-93-58-25-45-1C-46-66-17-10-98-03-96-D0-E5-C8-30-80-72-B6-49-B3-82-95-B3-8D-3A-A5-E5-60-C6-42-3A-33-70-67-30-C5-1C-32-D9-EB-CF-C1-36-B3-F3-24-07-39-86-0D-E9-12-80-73-26-A7-8C-8B-8A-40-AA-51-43-8F-20-DE-D2-9C-F3-B3-51-73-83-62-A0-11-C9-50-93-E1-F0-64-BE-D0-9E-E0-5B-13-47-AA-56-65-62-47-FD-3A-94-B7-1B-E5-35-95-86-14-64-C0-D6-07-96-4C-55-1E-0A-4C-10-C2-B5-E6-FB-74-F9-A5-72-E0-42-96-62-0B-EF-B7-52-36-7D-E3-01-12-85-E6-FE-92-75-40-C2-A6-D0-9D-16-6F-C1-C7-A7-DF-48-80-A2-5D-A0-FD-84-BE-06-AC-CB-32-22-82-D2-D7-7C-69-FC-BC-94-78-2B-11-5B-1C-1E-AF-DB-7A-AA-31-F3-D8-74-84-00-3F-9D-B9-4B-B2-68-7E-F4-1B-C2-83-73-21-78-0F-D5-0F-B0-EB-76-41-F1-23-7A-6A-78-CC-4F-3D-B1-2F-0A-F6-9A-A7-18-C1-2F-F0-B7-73-91-51-6D-9B-B5-B2-03-7C-E0-00-00-00-9B-AF-1B-E9-C1-C7-35-F5-E2-EB-C9-EE-F6-BA-25-6D-6F-39-83-B9", BitConverter.ToString (keypair));
	}

	[Test]
	public void ExportCspBlob_PublicOnly ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		DSAParameters dsap = AllTests.GetKey (true);
		dsa.ImportParameters (dsap);

		byte[] pubkey = dsa.ExportCspBlob (false);
		Assert.AreEqual ("06-02-00-00-00-22-00-00-44-53-53-31-00-04-00-00-D3-7B-C6-ED-3F-72-44-BD-22-7D-F2-D4-62-FE-7C-E3-75-8F-9C-DE-69-C0-3A-0C-E6-26-5A-46-3D-EF-0D-90-20-C6-F6-45-7C-73-E8-B9-7D-86-27-F7-97-76-C8-1C-8E-8B-CE-26-93-E6-21-53-20-93-58-25-45-1C-46-66-17-10-98-03-96-D0-E5-C8-30-80-72-B6-49-B3-82-95-B3-8D-3A-A5-E5-60-C6-42-3A-33-70-67-30-C5-1C-32-D9-EB-CF-C1-36-B3-F3-24-07-39-86-0D-E9-12-80-73-26-A7-8C-8B-8A-40-AA-51-43-8F-20-DE-D2-9C-F3-B3-51-73-83-62-A0-11-C9-50-93-E1-F0-64-BE-D0-9E-E0-5B-13-47-AA-56-65-62-47-FD-3A-94-B7-1B-E5-35-95-86-14-64-C0-D6-07-96-4C-55-1E-0A-4C-10-C2-B5-E6-FB-74-F9-A5-72-E0-42-96-62-0B-EF-B7-52-36-7D-E3-01-12-85-E6-FE-92-75-40-C2-A6-D0-9D-16-6F-C1-C7-A7-DF-48-80-A2-5D-A0-FD-84-BE-06-AC-CB-32-22-82-D2-D7-7C-69-FC-BC-94-78-2B-11-5B-1C-1E-AF-DB-7A-AA-31-F3-D8-74-84-00-3F-9D-B9-4B-B2-68-7E-F4-1B-C2-83-73-21-78-0F-D5-0F-B0-EB-76-41-F1-23-7A-6A-78-CC-4F-3D-BB-C7-03-89-C4-0B-66-86-80-D5-AA-34-E8-14-71-F9-29-BE-B9-EE-34-B1-5F-A2-C8-4D-CD-CF-0E-8E-A4-2E-D4-65-8C-27-FF-C1-41-26-F9-0E-E5-11-C9-CC-3E-45-87-EC-49-BA-7C-83-91-DE-70-E8-27-1C-47-EB-1D-E2-37-62-2F-AA-5B-30-80-8B-80-00-55-F4-64-C2-BE-5A-D3-54-4A-E7-0B-95-00-F4-BA-72-CD-F8-22-E6-30-4E-F6-BD-BE-3F-00-52-7F-E2-57-5F-C0-BE-82-C0-50-07-1C-7D-89-56-49-CE-28-52-8C-11-B1-D1-51-51-12-B2-E0-00-00-00-9B-AF-1B-E9-C1-C7-35-F5-E2-EB-C9-EE-F6-BA-25-6D-6F-39-83-B9", BitConverter.ToString (pubkey));
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ExportCspBlob_MissingPrivateKey ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		DSAParameters dsap = AllTests.GetKey (false);
		dsa.ImportParameters (dsap);

		dsa.ExportCspBlob (true);
	}

	[Test]
	public void ExportCspBlob_MissingPrivateKey_PublicOnly ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		DSAParameters dsap = AllTests.GetKey (false);
		dsa.ImportParameters (dsap);

		byte[] pubkey = dsa.ExportCspBlob (false);
		Assert.AreEqual ("06-02-00-00-00-22-00-00-44-53-53-31-00-04-00-00-D3-7B-C6-ED-3F-72-44-BD-22-7D-F2-D4-62-FE-7C-E3-75-8F-9C-DE-69-C0-3A-0C-E6-26-5A-46-3D-EF-0D-90-20-C6-F6-45-7C-73-E8-B9-7D-86-27-F7-97-76-C8-1C-8E-8B-CE-26-93-E6-21-53-20-93-58-25-45-1C-46-66-17-10-98-03-96-D0-E5-C8-30-80-72-B6-49-B3-82-95-B3-8D-3A-A5-E5-60-C6-42-3A-33-70-67-30-C5-1C-32-D9-EB-CF-C1-36-B3-F3-24-07-39-86-0D-E9-12-80-73-26-A7-8C-8B-8A-40-AA-51-43-8F-20-DE-D2-9C-F3-B3-51-73-83-62-A0-11-C9-50-93-E1-F0-64-BE-D0-9E-E0-5B-13-47-AA-56-65-62-47-FD-3A-94-B7-1B-E5-35-95-86-14-64-C0-D6-07-96-4C-55-1E-0A-4C-10-C2-B5-E6-FB-74-F9-A5-72-E0-42-96-62-0B-EF-B7-52-36-7D-E3-01-12-85-E6-FE-92-75-40-C2-A6-D0-9D-16-6F-C1-C7-A7-DF-48-80-A2-5D-A0-FD-84-BE-06-AC-CB-32-22-82-D2-D7-7C-69-FC-BC-94-78-2B-11-5B-1C-1E-AF-DB-7A-AA-31-F3-D8-74-84-00-3F-9D-B9-4B-B2-68-7E-F4-1B-C2-83-73-21-78-0F-D5-0F-B0-EB-76-41-F1-23-7A-6A-78-CC-4F-3D-BB-C7-03-89-C4-0B-66-86-80-D5-AA-34-E8-14-71-F9-29-BE-B9-EE-34-B1-5F-A2-C8-4D-CD-CF-0E-8E-A4-2E-D4-65-8C-27-FF-C1-41-26-F9-0E-E5-11-C9-CC-3E-45-87-EC-49-BA-7C-83-91-DE-70-E8-27-1C-47-EB-1D-E2-37-62-2F-AA-5B-30-80-8B-80-00-55-F4-64-C2-BE-5A-D3-54-4A-E7-0B-95-00-F4-BA-72-CD-F8-22-E6-30-4E-F6-BD-BE-3F-00-52-7F-E2-57-5F-C0-BE-82-C0-50-07-1C-7D-89-56-49-CE-28-52-8C-11-B1-D1-51-51-12-B2-E0-00-00-00-9B-AF-1B-E9-C1-C7-35-F5-E2-EB-C9-EE-F6-BA-25-6D-6F-39-83-B9", BitConverter.ToString (pubkey));
	}

	[Test]
	public void ImportCspBlob_Keypair ()
	{
		byte[] blob = new byte [336] { 0x07, 0x02, 0x00, 0x00, 0x00, 0x22, 0x00, 0x00, 0x44, 0x53, 0x53, 0x32, 0x00, 0x04, 0x00, 0x00, 0xD3,
			0x7B, 0xC6, 0xED, 0x3F, 0x72, 0x44, 0xBD, 0x22, 0x7D, 0xF2, 0xD4, 0x62, 0xFE, 0x7C, 0xE3, 0x75, 0x8F, 0x9C, 0xDE, 0x69, 0xC0, 0x3A, 
			0x0C, 0xE6, 0x26, 0x5A, 0x46, 0x3D, 0xEF, 0x0D, 0x90, 0x20, 0xC6, 0xF6, 0x45, 0x7C, 0x73, 0xE8, 0xB9, 0x7D, 0x86, 0x27, 0xF7, 0x97, 
			0x76, 0xC8, 0x1C, 0x8E, 0x8B, 0xCE, 0x26, 0x93, 0xE6, 0x21, 0x53, 0x20, 0x93, 0x58, 0x25, 0x45, 0x1C, 0x46, 0x66, 0x17, 0x10, 0x98, 
			0x03, 0x96, 0xD0, 0xE5, 0xC8, 0x30, 0x80, 0x72, 0xB6, 0x49, 0xB3, 0x82, 0x95, 0xB3, 0x8D, 0x3A, 0xA5, 0xE5, 0x60, 0xC6, 0x42, 0x3A, 
			0x33, 0x70, 0x67, 0x30, 0xC5, 0x1C, 0x32, 0xD9, 0xEB, 0xCF, 0xC1, 0x36, 0xB3, 0xF3, 0x24, 0x07, 0x39, 0x86, 0x0D, 0xE9, 0x12, 0x80, 
			0x73, 0x26, 0xA7, 0x8C, 0x8B, 0x8A, 0x40, 0xAA, 0x51, 0x43, 0x8F, 0x20, 0xDE, 0xD2, 0x9C, 0xF3, 0xB3, 0x51, 0x73, 0x83, 0x62, 0xA0, 
			0x11, 0xC9, 0x50, 0x93, 0xE1, 0xF0, 0x64, 0xBE, 0xD0, 0x9E, 0xE0, 0x5B, 0x13, 0x47, 0xAA, 0x56, 0x65, 0x62, 0x47, 0xFD, 0x3A, 0x94, 
			0xB7, 0x1B, 0xE5, 0x35, 0x95, 0x86, 0x14, 0x64, 0xC0, 0xD6, 0x07, 0x96, 0x4C, 0x55, 0x1E, 0x0A, 0x4C, 0x10, 0xC2, 0xB5, 0xE6, 0xFB, 
			0x74, 0xF9, 0xA5, 0x72, 0xE0, 0x42, 0x96, 0x62, 0x0B, 0xEF, 0xB7, 0x52, 0x36, 0x7D, 0xE3, 0x01, 0x12, 0x85, 0xE6, 0xFE, 0x92, 0x75, 
			0x40, 0xC2, 0xA6, 0xD0, 0x9D, 0x16, 0x6F, 0xC1, 0xC7, 0xA7, 0xDF, 0x48, 0x80, 0xA2, 0x5D, 0xA0, 0xFD, 0x84, 0xBE, 0x06, 0xAC, 0xCB, 
			0x32, 0x22, 0x82, 0xD2, 0xD7, 0x7C, 0x69, 0xFC, 0xBC, 0x94, 0x78, 0x2B, 0x11, 0x5B, 0x1C, 0x1E, 0xAF, 0xDB, 0x7A, 0xAA, 0x31, 0xF3, 
			0xD8, 0x74, 0x84, 0x00, 0x3F, 0x9D, 0xB9, 0x4B, 0xB2, 0x68, 0x7E, 0xF4, 0x1B, 0xC2, 0x83, 0x73, 0x21, 0x78, 0x0F, 0xD5, 0x0F, 0xB0, 
			0xEB, 0x76, 0x41, 0xF1, 0x23, 0x7A, 0x6A, 0x78, 0xCC, 0x4F, 0x3D, 0xB1, 0x2F, 0x0A, 0xF6, 0x9A, 0xA7, 0x18, 0xC1, 0x2F, 0xF0, 0xB7, 
			0x73, 0x91, 0x51, 0x6D, 0x9B, 0xB5, 0xB2, 0x03, 0x7C, 0xE0, 0x00, 0x00, 0x00, 0x9B, 0xAF, 0x1B, 0xE9, 0xC1, 0xC7, 0x35, 0xF5, 0xE2, 
			0xEB, 0xC9, 0xEE, 0xF6, 0xBA, 0x25, 0x6D, 0x6F, 0x39, 0x83, 0xB9 };
		dsa = new DSACryptoServiceProvider (minKeySize);
		dsa.ImportCspBlob (blob);

		byte[] keypair = dsa.ExportCspBlob (true);
		for (int i = 0; i < blob.Length; i++)
			Assert.AreEqual (blob[i], keypair[i], i.ToString ());
	}

	[Test]
	public void ExportCspBlob_PublicKey ()
	{
		byte[] blob = new byte [444] { 0x06, 0x02, 0x00, 0x00, 0x00, 0x22, 0x00, 0x00, 0x44, 0x53, 0x53, 0x31, 0x00, 0x04, 0x00, 0x00, 0xD3, 
			0x7B, 0xC6, 0xED, 0x3F, 0x72, 0x44, 0xBD, 0x22, 0x7D, 0xF2, 0xD4, 0x62, 0xFE, 0x7C, 0xE3, 0x75, 0x8F, 0x9C, 0xDE, 0x69, 0xC0, 0x3A, 
			0x0C, 0xE6, 0x26, 0x5A, 0x46, 0x3D, 0xEF, 0x0D, 0x90, 0x20, 0xC6, 0xF6, 0x45, 0x7C, 0x73, 0xE8, 0xB9, 0x7D, 0x86, 0x27, 0xF7, 0x97, 
			0x76, 0xC8, 0x1C, 0x8E, 0x8B, 0xCE, 0x26, 0x93, 0xE6, 0x21, 0x53, 0x20, 0x93, 0x58, 0x25, 0x45, 0x1C, 0x46, 0x66, 0x17, 0x10, 0x98, 
			0x03, 0x96, 0xD0, 0xE5, 0xC8, 0x30, 0x80, 0x72, 0xB6, 0x49, 0xB3, 0x82, 0x95, 0xB3, 0x8D, 0x3A, 0xA5, 0xE5, 0x60, 0xC6, 0x42, 0x3A, 
			0x33, 0x70, 0x67, 0x30, 0xC5, 0x1C, 0x32, 0xD9, 0xEB, 0xCF, 0xC1, 0x36, 0xB3, 0xF3, 0x24, 0x07, 0x39, 0x86, 0x0D, 0xE9, 0x12, 0x80, 
			0x73, 0x26, 0xA7, 0x8C, 0x8B, 0x8A, 0x40, 0xAA, 0x51, 0x43, 0x8F, 0x20, 0xDE, 0xD2, 0x9C, 0xF3, 0xB3, 0x51, 0x73, 0x83, 0x62, 0xA0, 
			0x11, 0xC9, 0x50, 0x93, 0xE1, 0xF0, 0x64, 0xBE, 0xD0, 0x9E, 0xE0, 0x5B, 0x13, 0x47, 0xAA, 0x56, 0x65, 0x62, 0x47, 0xFD, 0x3A, 0x94, 
			0xB7, 0x1B, 0xE5, 0x35, 0x95, 0x86, 0x14, 0x64, 0xC0, 0xD6, 0x07, 0x96, 0x4C, 0x55, 0x1E, 0x0A, 0x4C, 0x10, 0xC2, 0xB5, 0xE6, 0xFB, 
			0x74, 0xF9, 0xA5, 0x72, 0xE0, 0x42, 0x96, 0x62, 0x0B, 0xEF, 0xB7, 0x52, 0x36, 0x7D, 0xE3, 0x01, 0x12, 0x85, 0xE6, 0xFE, 0x92, 0x75, 
			0x40, 0xC2, 0xA6, 0xD0, 0x9D, 0x16, 0x6F, 0xC1, 0xC7, 0xA7, 0xDF, 0x48, 0x80, 0xA2, 0x5D, 0xA0, 0xFD, 0x84, 0xBE, 0x06, 0xAC, 0xCB, 
			0x32, 0x22, 0x82, 0xD2, 0xD7, 0x7C, 0x69, 0xFC, 0xBC, 0x94, 0x78, 0x2B, 0x11, 0x5B, 0x1C, 0x1E, 0xAF, 0xDB, 0x7A, 0xAA, 0x31, 0xF3, 
			0xD8, 0x74, 0x84, 0x00, 0x3F, 0x9D, 0xB9, 0x4B, 0xB2, 0x68, 0x7E, 0xF4, 0x1B, 0xC2, 0x83, 0x73, 0x21, 0x78, 0x0F, 0xD5, 0x0F, 0xB0, 
			0xEB, 0x76, 0x41, 0xF1, 0x23, 0x7A, 0x6A, 0x78, 0xCC, 0x4F, 0x3D, 0xBB, 0xC7, 0x03, 0x89, 0xC4, 0x0B, 0x66, 0x86, 0x80, 0xD5, 0xAA, 
			0x34, 0xE8, 0x14, 0x71, 0xF9, 0x29, 0xBE, 0xB9, 0xEE, 0x34, 0xB1, 0x5F, 0xA2, 0xC8, 0x4D, 0xCD, 0xCF, 0x0E, 0x8E, 0xA4, 0x2E, 0xD4, 
			0x65, 0x8C, 0x27, 0xFF, 0xC1, 0x41, 0x26, 0xF9, 0x0E, 0xE5, 0x11, 0xC9, 0xCC, 0x3E, 0x45, 0x87, 0xEC, 0x49, 0xBA, 0x7C, 0x83, 0x91, 
			0xDE, 0x70, 0xE8, 0x27, 0x1C, 0x47, 0xEB, 0x1D, 0xE2, 0x37, 0x62, 0x2F, 0xAA, 0x5B, 0x30, 0x80, 0x8B, 0x80, 0x00, 0x55, 0xF4, 0x64, 
			0xC2, 0xBE, 0x5A, 0xD3, 0x54, 0x4A, 0xE7, 0x0B, 0x95, 0x00, 0xF4, 0xBA, 0x72, 0xCD, 0xF8, 0x22, 0xE6, 0x30, 0x4E, 0xF6, 0xBD, 0xBE, 
			0x3F, 0x00, 0x52, 0x7F, 0xE2, 0x57, 0x5F, 0xC0, 0xBE, 0x82, 0xC0, 0x50, 0x07, 0x1C, 0x7D, 0x89, 0x56, 0x49, 0xCE, 0x28, 0x52, 0x8C, 
			0x11, 0xB1, 0xD1, 0x51, 0x51, 0x12, 0xB2, 0xE0, 0x00, 0x00, 0x00, 0x9B, 0xAF, 0x1B, 0xE9, 0xC1, 0xC7, 0x35, 0xF5, 0xE2, 0xEB, 0xC9, 
			0xEE, 0xF6, 0xBA, 0x25, 0x6D, 0x6F, 0x39, 0x83, 0xB9 };
		dsa = new DSACryptoServiceProvider (minKeySize);
		dsa.ImportCspBlob (blob);

		byte[] pubkey = dsa.ExportCspBlob (false);
		for (int i = 0; i < blob.Length; i++)
			Assert.AreEqual (blob[i], pubkey[i], i.ToString ());
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ImportCspBlob_Null ()
	{
		dsa = new DSACryptoServiceProvider (minKeySize);
		dsa.ImportCspBlob (null);
	}

	[Test]
	[ExpectedException (typeof (CryptographicException))]
	public void ImportCspBlob_Bad ()
	{
		byte[] blob = new byte [148]; // valid size for public key
		dsa = new DSACryptoServiceProvider (minKeySize);
		dsa.ImportCspBlob (blob);
	}
}

}
