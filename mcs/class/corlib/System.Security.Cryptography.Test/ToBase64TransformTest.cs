// 
// ToBase64TransformTest
// 
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
// 

using System;
using System.Security.Cryptography;

using NUnit.Framework;


namespace System.Security.Cryptography.Test {

	/// <summary>
	///  ToBase64Transform test suite.
	/// </summary>
	public class ToBase64TransformTest {

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite("ToBase64Transform tests");
				suite.AddTest(ToBase64TransformTestCase.Suite);
				return suite;
			}
		}


		public class ToBase64TransformTestCase : TestCase {

			public ToBase64TransformTestCase (String name) : base(name)
			{
			}

			protected override void SetUp ()
			{
			}

			public static ITest Suite
			{
				get {
					Console.WriteLine("Testing " + (new ToBase64Transform ()));
					return new TestSuite(typeof(ToBase64TransformTestCase));
				}
			}




			//
			// Tests
			//

			public void TestProperties ()
			{
				ToBase64Transform encoder = new ToBase64Transform ();

				Assert ("Wrong input block size!", encoder.InputBlockSize == 3);
				Assert ("Wrong output block size!", encoder.OutputBlockSize == 4);
				Assert ("Should be unable to transform multiple blocks!", !encoder.CanTransformMultipleBlocks);

			}


/*
// FIXME: Well, according to Beta2 docs an exception should be thrown here,
// since "data size is not valid". Nothing happens in Beta2 though.
// So just ignore this for now.

			public void TestException ()
			{
				ToBase64Transform encoder = new ToBase64Transform ();

				byte [] res = new byte [4];
				bool thrown = false;
				try {
					encoder.TransformBlock (new byte [] {1,2,3,4,5},0,5,res,0);
				} catch (CryptographicException) {
					thrown = true;
				}
				Assert (thrown);
			}
*/


			public void TestFullBlockEncoding ()
			{
				ToBase64Transform encoder = new ToBase64Transform ();
				byte [] res = new byte [encoder.OutputBlockSize];

				foreach (Base64TestCase testCase in fullBlockTests) {
					Base64TestCase tmp = new Base64TestCase ();
					tmp.flat = testCase.flat;
					int n = encoder.TransformBlock (testCase.GetFlat(),0,3,res,0);
					AssertEquals(n, encoder.OutputBlockSize);
					tmp.SetEncoded(res);
					AssertEquals(testCase,tmp);
				}

			}



			public void TestFinalBlockEncoding ()
			{
				ToBase64Transform encoder = new ToBase64Transform ();

				foreach (Base64TestCase testCase in finalBlockTests) {
					Base64TestCase tmp = new Base64TestCase ();
					tmp.flat = testCase.flat;
					byte [] res = encoder.TransformFinalBlock (testCase.GetFlat(),0,testCase.flat.Length);
					tmp.SetEncoded(res);
					AssertEquals(testCase,tmp);
				}

			}


			Base64TestCase [] fullBlockTests = {
				new Base64TestCase("ABC","QUJD"),
				new Base64TestCase("123","MTIz"),
				new Base64TestCase(new byte [] {125,24,215},"fRjX")
			};


			Base64TestCase [] finalBlockTests = {
				new Base64TestCase("AB","QUI="),
				new Base64TestCase("1","MQ=="),
				new Base64TestCase(new byte [] {125,24},"fRg=")
			};






			//
			// Test helper
			//
			private class Base64TestCase {
				public string flat;
				public string encoded;

				public Base64TestCase () : this ("","")
				{
				}

				public Base64TestCase (Base64TestCase that)
				{
					this.flat = that.flat;
					this.encoded = that.encoded;
				}

				public Base64TestCase (byte [] flat, string encoded)
				{
					SetFlat (flat);
					this.encoded = encoded;
				}

				public Base64TestCase (string flat, string encoded)
				{
					this.flat = flat;
					this.encoded = encoded;
				}

				public void SetEncoded (byte [] encoded)
				{
					String enc="";

					for (int i = 0; i < encoded.Length; i++)
						enc += (char) encoded[i];

					this.encoded = enc;
				}

				public void SetFlat (byte [] flat)
				{
					String flt="";

					for (int i=0; i < flat.Length; i++)
						flt += (char) flat[i];

					this.flat = flt;
				}

				public byte [] GetFlat ()
				{
					byte [] res = new byte [flat.Length];
					for (int i = 0; i < flat.Length; i++)
						res [i] = (byte) flat [i];
					return res;
				}

				public static bool operator == (Base64TestCase t1, Base64TestCase t2)
				{
					return (t1.flat.Equals (t2.flat) &&
					        t1.encoded.Equals (t2.encoded));
				}

				public static bool operator != (Base64TestCase t1, Base64TestCase t2)
				{
					return !(t1 == t2);
				}

				public override bool Equals (object o)
				{
					return (o is Base64TestCase)
					       && (this == (o as Base64TestCase));
				}

				public override int GetHashCode ()
				{
					return flat.GetHashCode () ^ (encoded.GetHashCode () << 2);
				}

				public override string ToString ()
				{
					return "Flat = " + flat + ", encoded = " + encoded;
				}
			}

		} // ToBase64TransformTestCase




	} // ToBase64TransformTest

} // System.Security.Cryptography.Test
