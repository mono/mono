//
// DSATest.cs - NUnit Test Cases for DSA
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

/*public class NonAbstractDSAForUnitTests : DSA {
	protected DSAParameters dsa;

	// not tested here - but we must implemented all abstract properties
	public override string KeyExchangeAlgorithm 
	{
		get { return null; }
	}

	// not tested here - but we must implemented all abstract properties
	public override string SignatureAlgorithm 
	{
		get { return null; }
	}

	// not tested here - but we must implemented all abstract methods
	public override byte[] CreateSignature (byte[] rgbHash) 
	{
		return null;
	}

	// basic implementation for tests
	public override DSAParameters ExportParameters (bool includePrivateParameters) 
	{
		DSAParameters dsaParams = dsa;
		if (!includePrivateParameters)
			dsaParams.X = null;
		return dsaParams;
	}

	// basic implementation for tests
	public override void ImportParameters (DSAParameters parameters) 
	{
		dsa = parameters;
	}

	// not tested here - but we must implemented all abstract methods
	public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
	{
		return false;
	}

	protected override void Dispose (bool disposing) {}
}*/

public class DSATest : TestCase {
	protected DSA dsa;

	static string xmlPrivate = "<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter><X>fAOytZttUZFzt/AvwRinmvYKL7E=</X></DSAKeyValue>";

	static string xmlPublic = "<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter></DSAKeyValue>";

	protected override void SetUp () 
	{
		//dsa = new NonAbstractDSAForUnitTests ();
		dsa = new DSACryptoServiceProvider ();
	}

	protected override void TearDown () {}

	public void AssertEquals (string msg, byte[] array1, byte[] array2) 
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	// may also help for DSA descendants
	public void AssertEquals (string message, DSAParameters expectedKey, DSAParameters actualKey, bool checkPrivateKey) 
	{
		AssertEquals( message + " Counter", expectedKey.Counter, actualKey.Counter );
		AssertEquals( message + " G", expectedKey.G, actualKey.G );
		AssertEquals( message + " J", expectedKey.J, actualKey.J );
		AssertEquals( message + " P", expectedKey.P, actualKey.P );
		AssertEquals( message + " Q", expectedKey.Q, actualKey.Q );
		AssertEquals( message + " Seed", expectedKey.Seed, actualKey.Seed );
		AssertEquals( message + " Y", expectedKey.Y, actualKey.Y );
		if (checkPrivateKey)
			AssertEquals( message + " X", expectedKey.X, actualKey.X );
	}

	// LAMESPEC: ImportParameters inverse the byte arrays inside DSAParameters !!!
	// importing and exporting a DSA key (including private key)
	public void TestDSAImportPrivateExportPrivate() 
	{
		DSAParameters input = AllTests.GetKey (true);
		dsa.ImportParameters (input);
		string xmlDSA = dsa.ToXmlString (true);
		dsa.FromXmlString (xmlDSA);
		AssertEquals ("DSA Import Private Export Private (xml)", xmlPrivate, xmlDSA);
		DSAParameters output = dsa.ExportParameters (true);
		AssertEquals ("DSA Import Private Export Private (binary)", AllTests.GetKey (true), output, true);
	}

	// importing and exporting a DSA key (without private key)
	public void TestDSAImportPrivateExportPublic() 
	{
		DSAParameters input = AllTests.GetKey (true);
		dsa.ImportParameters (input);
		string xmlDSA = dsa.ToXmlString (false);
		dsa.FromXmlString (xmlDSA);
		AssertEquals ("DSA Import Private Export Public (xml)", xmlPublic, xmlDSA);
		DSAParameters output = dsa.ExportParameters (false);
		AssertEquals ("DSA Import Private Export Public (binary)", AllTests.GetKey (true), output, false);
	}

	// importing and exporting a DSA key (including private key)
	public void TestDSAImportPublicExportPrivate() 
	{
		DSAParameters input = AllTests.GetKey (false);
		dsa.ImportParameters (input);
		string xmlDSA = null;
		try {
			xmlDSA = dsa.ToXmlString (true);
			Fail ("Expected CryptographicException but got no exception");
		}
		catch (CryptographicException) {
			// expected as we cannot get a private key 
			// from a public key (at least not in our lifetime)
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got " + e.ToString ());
		}
	}

	// importing and exporting a DSA key (without private key)
	public void TestDSAImportPublicExportPublic() 
	{
		DSAParameters input = AllTests.GetKey (false);
		dsa.ImportParameters (input);
		string xmlDSA = dsa.ToXmlString (false);
		dsa.FromXmlString (xmlDSA);
		AssertEquals ("DSA Import Public Export Public (xml)", xmlPublic, xmlDSA);
		DSAParameters output = dsa.ExportParameters (false);
		AssertEquals ("DSA Import Public Export Public (binary)", AllTests.GetKey (false), output, true);
	}
}

}
