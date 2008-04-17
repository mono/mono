//
// DSATest.cs - NUnit Test Cases for DSA
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005, 2008 Novell Inc. (http://www.novell.com)
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

#if NET_2_0

	public class NonAbstractDSAForUnitTests : DSA {
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
		public override byte [] CreateSignature (byte [] rgbHash)
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
			dsa.P = parameters.P;
			dsa.Q = parameters.Q;
			dsa.G = parameters.G;
			dsa.J = parameters.J;
			dsa.Y = parameters.Y;
			if (parameters.X != null) {
				// watch out for private key zeroification
				dsa.X = (byte []) parameters.X.Clone ();
			}
			dsa.Seed = parameters.Seed;
			dsa.Counter = parameters.Counter;
		}

		// not tested here - but we must implemented all abstract methods
		public override bool VerifySignature (byte [] rgbHash, byte [] rgbSignature)
		{
			return false;
		}

		protected override void Dispose (bool disposing) { }
	}
#endif

	[TestFixture]
	public class DSATest {

		protected DSA dsa;

		static string xmlPrivate = "<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter><X>fAOytZttUZFzt/AvwRinmvYKL7E=</X></DSAKeyValue>";

		static string xmlPublic = "<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter></DSAKeyValue>";

		[SetUp]
		public void SetUp ()
		{
#if NET_2_0
			dsa = new NonAbstractDSAForUnitTests ();
#else
			dsa = new DSACryptoServiceProvider ();
#endif
		}

		public void AssertEquals (string msg, byte [] array1, byte [] array2)
		{
			Assert.AreEqual (array1, array2, msg);
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

		// LAMESPEC: ImportParameters inverse the byte arrays inside DSAParameters !!!
		// importing and exporting a DSA key (including private key)
		[Test]
		public void DSAImportPrivateExportPrivate ()
		{
			DSAParameters input = AllTests.GetKey (true);
			dsa.ImportParameters (input);
			string xmlDSA = dsa.ToXmlString (true);
			dsa.FromXmlString (xmlDSA);
			Assert.AreEqual (xmlPrivate, xmlDSA, "DSA Import Private Export Private (xml)");
			DSAParameters output = dsa.ExportParameters (true);
			AssertEquals ("DSA Import Private Export Private (binary)", AllTests.GetKey (true), output, true);
		}

		// importing and exporting a DSA key (without private key)
		[Test]
		public void DSAImportPrivateExportPublic ()
		{
			DSAParameters input = AllTests.GetKey (true);
			dsa.ImportParameters (input);
			string xmlDSA = dsa.ToXmlString (false);
			dsa.FromXmlString (xmlDSA);
			Assert.AreEqual (xmlPublic, xmlDSA, "DSA Import Private Export Public (xml)");
			DSAParameters output = dsa.ExportParameters (false);
			AssertEquals ("DSA Import Private Export Public (binary)", AllTests.GetKey (true), output, false);
		}

		// importing and exporting a DSA key (including private key)
		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (CryptographicException))]
#endif
		public void DSAImportPublicExportPrivate ()
		{
			DSAParameters input = AllTests.GetKey (false);
			dsa.ImportParameters (input);
			string xmlDSA = dsa.ToXmlString (true);
		}

		// importing and exporting a DSA key (without private key)
		[Test]
		public void DSAImportPublicExportPublic ()
		{
			DSAParameters input = AllTests.GetKey (false);
			dsa.ImportParameters (input);
			string xmlDSA = dsa.ToXmlString (false);
			dsa.FromXmlString (xmlDSA);
			Assert.AreEqual (xmlPublic, xmlDSA, "DSA Import Public Export Public (xml)");
			DSAParameters output = dsa.ExportParameters (false);
			AssertEquals ("DSA Import Public Export Public (binary)", AllTests.GetKey (false), output, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlStringNull ()
		{
			dsa.FromXmlString (null);
		}

		[Test]
		public void ToXmlStringWithoutSeed ()
		{
			DSA d = DSA.Create ();
			d.FromXmlString ("<DSAKeyValue><P>vb95327o8+f5lbrS9qSXxLQYTkcP/WTlJnI0fuw/vFaf7DFQe/ORdTqpa0I3okDOcRiUihzr0y58aQarlNf58MMhMcx/XqRzB2UOVZ/bt2EpfAC3CISwXHlHFoW6+dCHpc72aJOXpreWV6k0oZUg71tKMsPVUP1I8xgELArxAUE=</P><Q>5ul/yRjQ8hFv4w94ZHsP337ebjk=</Q><G>NunCU4DkWaq6IKKhRPCMBBmMgILU8Zqd3aHe0UyKZLYFSOjcKkOIPJ9iWtfDtErHcxb3yjHRV6/EndR+wX8rNsTjYDeUGg5vC6IV4Es+rRCmhVXQ7Y2N+bAH71VxPRbNC90NjgYqKwXZHf2l6c+W4XRvRvNiM5puwz+ubWcm5AA=</G><Y>hQinH+upZPNtTS2o7bi03EOybn9eHC8U61/Rax+oe00YPG+0Md7Okup6CMxZmww0n2F8W7YRZeI7Pltm8TlpmUdMmGSAiILUX585vFM19GR4XeSecqpj1BFO/x4T9tGeakoWxquEjFl4JqEuvDQwnvM76jWDmkUTI4U8kJPnHcw=</Y><J>0l0NjQKpwTJt+h8qmlXhbt4jL+OnaSZkM1zdyIPmOpNavJz7slGtoDAneoQ8STNiT+RrNqGdPbs5glAP8sXS0mdKJ6dGQuySGwGZTP9cWCq81YjRJJ74QuPJUYUruuhN0RTkiukqGzkJYQtA</J></DSAKeyValue>");
			d.ToXmlString (false);
		}

		[Test]
		public void ToXmlStringWithZeroCounter ()
		{
			DSA d = DSA.Create ();
			// <PgenCounter>AA==</PgenCounter> == 0
			d.FromXmlString ("<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>AA==</PgenCounter><X>fAOytZttUZFzt/AvwRinmvYKL7E=</X></DSAKeyValue>");
			d.ToXmlString (false);
		}

		[Test]
		public void FromXmlString_InvalidTop ()
		{
			string xml = "<a><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><J>AAAAAQ6LSuRiYdsocZ6rgyqIOpE1/uCO1PfEn758Lg2VW6OHJTYHNC30s0gSTG/Jt3oHYX+S8vrtNYb8kRJ/ipgcofGq2Qo/cYKP7RX2K6EJwSfWInhsNMr1JmzuK0lUKkXXXVo15fL8O2/16uEWMg==</J><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter></a>";
			dsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, dsa.ToXmlString (false), "InvalidTop");
		}

		[Test]
		public void FromXmlString_Embedded ()
		{
			// from bug #355464
			string xml = "<SigningKey version=\"1.0\">" + xmlPublic + "</SigningKey>";
			dsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, dsa.ToXmlString (false), "Embedded");
		}

		[Test]
		public void FromXmlString_EmbeddedTwoLevelWithExtraElement ()
		{
			string xml = "<b><u>" + xmlPublic + "</u></b><i>why not ?</i>";
			dsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, dsa.ToXmlString (false), "Deep");
		}

		[Test]
		public void FromXmlString_TwoKeys ()
		{
			DSA second = DSA.Create ();
			string xml = "<two>" + xmlPublic + second.ToXmlString (false) + "</two>";
			dsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, dsa.ToXmlString (false), "TwoKeys");
		}

		[Test]
		public void FromXmlString_InvalidXml ()
		{
			string xml = "<open>" + xmlPublic + "</close>";
			dsa.FromXmlString (xml);
			Assert.AreEqual (xmlPublic, dsa.ToXmlString (false), "Embedded");
		}

		[Test]
		public void ImportExportWithoutJ ()
		{
			DSA d = DSA.Create ();
			DSAParameters input = AllTests.GetKey (false);
			input.J = null;
			// J is calculated (usually pre-calculated)
			d.ImportParameters (input);
			input = d.ExportParameters (false);
			// if J isn't imported, then it's not exportable and not part of the xml
			Assert.IsNull (input.J, "exported-J");
			Assert.AreEqual ("<DSAKeyValue><P>s/Oc0t4gj0NRqkCKi4ynJnOAEukNhjkHJPOzNsHP69kyHMUwZ3AzOkLGYOWlOo2zlYKzSbZygDDI5dCWA5gQF2ZGHEUlWJMgUyHmkybOi44cyHaX9yeGfbnoc3xF9sYgkA3vPUZaJuYMOsBp3pyPdeN8/mLU8n0ivURyP+3Ge9M=</P><Q>qkcTW+Ce0L5k8OGTUMkRoGKDc1E=</Q><G>PU/MeGp6I/FBduuwD9UPeCFzg8Ib9H5osku5nT8AhHTY8zGqetuvHhxbESt4lLz8aXzX0oIiMsusBr6E/aBdooBI36fHwW8WndCmwkB1kv7mhRIB4302UrfvC2KWQuBypfl0++a1whBMCh5VTJYH1sBkFIaVNeUbt5Q6/UdiZVY=</G><Y>shJRUdGxEYxSKM5JVol9HAdQwIK+wF9X4n9SAD++vfZOMOYi+M1yuvQAlQvnSlTTWr7CZPRVAICLgDBbqi9iN+Id60ccJ+hw3pGDfLpJ7IdFPszJEeUO+SZBwf8njGXULqSODs/NTciiX7E07rm+KflxFOg0qtWAhmYLxIkDx7s=</Y><Seed>uYM5b20luvbuyevi9TXHwekbr5s=</Seed><PgenCounter>4A==</PgenCounter></DSAKeyValue>", d.ToXmlString (false), "xml");
		}

		[Test]
		public void ImportExportWithoutY ()
		{
			DSA d = DSA.Create ();
			DSAParameters input = AllTests.GetKey (true);
			input.Y = null;
			// Y is calculated from X
			d.ImportParameters (input);
			Assert.AreEqual (xmlPrivate, d.ToXmlString (true), "xmlPrivate");
			Assert.AreEqual (xmlPublic, d.ToXmlString (false), "xmlPublic");
		}
	}
}
