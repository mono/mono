//
// BitVector32Test.cs - NUnit Test Cases for System.Net.BitVector32
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized
{
	public class BitVector32Test : TestCase
	{
		public BitVector32Test () :
			base ("[MonoTests.System.Net.BitVector32Test]") {}

		public BitVector32Test (string name) : base (name) {}

		protected override void SetUp () {}

		protected override void TearDown () {}

		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (BitVector32Test));
			}
		}
		
		public void TestConstructors ()
		{
			BitVector32 b = new BitVector32 (31);
		}
		
		public void TestIndexers ()
		{
			BitVector32 b = new BitVector32 (7);
			Assert ("#1", b [0]);
			Assert ("#2", b [1]);
			Assert ("#3", b [2]);
			Assert ("#4", b [4]);
			Assert ("#5", !b [8]);
			Assert ("#6", !b [16]);
			b [8] = true;
			Assert ("#7", b [4]);
			Assert ("#8", b [8]);
			Assert ("#9", !b [16]);
			b [8] = false;
			Assert ("#10", b [4]);
			Assert ("#11", !b [8]);
			Assert ("#12", !b [16]);

			BitVector32.Section s = BitVector32.CreateSection (31);
			s = BitVector32.CreateSection (64, s);
			// Print (s);
			
			// b = new BitVector32 (0x777777);
			BitVector32 b1 = new BitVector32 (0xffff77);
			BitVector32 b2 = new BitVector32 (b1 [s]);
			//Console.WriteLine (b1.ToString ());
			//Console.WriteLine (b2.ToString ());
			AssertEquals ("#14", 123, b1 [s]);
			
			// b1 [s] = 15;
			//Console.WriteLine (b1.ToString ());
		}

		public void TestCreateMask ()
		{
			AssertEquals ("#1", 1, BitVector32.CreateMask ());
			AssertEquals ("#2", 1, BitVector32.CreateMask (0));
			AssertEquals ("#3", 2, BitVector32.CreateMask (1));
			AssertEquals ("#4", 32, BitVector32.CreateMask (16));
			AssertEquals ("#6", -2, BitVector32.CreateMask (Int32.MaxValue));
			AssertEquals ("#5", -4, BitVector32.CreateMask (-2));
			try {
				BitVector32.CreateMask (Int32.MinValue);
				Fail ("#7");
			} catch (InvalidOperationException) {}			
		}
		
		public void TestCreateSection ()
		{
			BitVector32.Section s = BitVector32.CreateSection (1);
			AssertEquals ("#1", (short) 1, s.Mask);

			s = BitVector32.CreateSection (2);
			AssertEquals ("#2", (short) 3, s.Mask);

			s = BitVector32.CreateSection (3);
			AssertEquals ("#3", (short) 3, s.Mask);

			s = BitVector32.CreateSection (5);
			AssertEquals ("#4", (short) 7, s.Mask);
			
			s = BitVector32.CreateSection (20);
			AssertEquals ("#4", (short) 0x1f, s.Mask);

			s = BitVector32.CreateSection (Int16.MaxValue);
			AssertEquals ("#5", (short) 0x7fff, s.Mask);

			s = BitVector32.CreateSection (Int16.MaxValue - 100);
			AssertEquals ("#6", (short) 0x7fff, s.Mask);

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (0);
				Fail ("#7");
			} catch (ArgumentException) {}

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (-1);
				Fail ("#8");
			} catch (ArgumentException) {}

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (Int16.MinValue);
				Fail ("#9");
			} catch (ArgumentException) {}
			
			s = BitVector32.CreateSection (20);
			AssertEquals ("#10a", (short) 0x1f, s.Mask);
			AssertEquals ("#10b", (short) 0x00, s.Offset);			
			s = BitVector32.CreateSection (120, s);
			AssertEquals ("#10c", (short) 0x7f, s.Mask);
			AssertEquals ("#10d", (short) 0x05, s.Offset);					
			s = BitVector32.CreateSection (1000, s);
			AssertEquals ("#10e", (short) 0x3ff, s.Mask);
			AssertEquals ("#10f", (short) 0x0c, s.Offset);			
		}


		private void Print (BitVector32.Section s)
		{
			Console.WriteLine (s.ToString () + " : "+ s.Mask + " : " + s.Offset);
		}
	}        
}