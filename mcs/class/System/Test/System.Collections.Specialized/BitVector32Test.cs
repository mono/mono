//
// BitVector32Test.cs - NUnit Test Cases for System.Net.BitVector32
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized
{
	[TestFixture]
	public class BitVector32Test
	{
		[Test]
		public void Constructors ()
		{
			BitVector32 b = new BitVector32 (31);
		}
		
		[Test]
		public void Indexers ()
		{
			BitVector32 b = new BitVector32 (7);
			Assertion.Assert ("#1", b [0]);
			Assertion.Assert ("#2", b [1]);
			Assertion.Assert ("#3", b [2]);
			Assertion.Assert ("#4", b [4]);
			Assertion.Assert ("#5", !b [8]);
			Assertion.Assert ("#6", !b [16]);
			b [8] = true;
			Assertion.Assert ("#7", b [4]);
			Assertion.Assert ("#8", b [8]);
			Assertion.Assert ("#9", !b [16]);
			b [8] = false;
			Assertion.Assert ("#10", b [4]);
			Assertion.Assert ("#11", !b [8]);
			Assertion.Assert ("#12", !b [16]);

			BitVector32.Section s = BitVector32.CreateSection (31);
			s = BitVector32.CreateSection (64, s);
			// Print (s);
			
			// b = new BitVector32 (0x777777);
			BitVector32 b1 = new BitVector32 (0xffff77);
			BitVector32 b2 = new BitVector32 (b1 [s]);
			//Console.WriteLine (b1.ToString ());
			//Console.WriteLine (b2.ToString ());
			Assertion.AssertEquals ("#14", 123, b1 [s]);
			
			// b1 [s] = 15;
			//Console.WriteLine (b1.ToString ());
		}

		[Test]
		public void CreateMask ()
		{
			Assertion.AssertEquals ("#1", 1, BitVector32.CreateMask ());
			Assertion.AssertEquals ("#2", 1, BitVector32.CreateMask (0));
			Assertion.AssertEquals ("#3", 2, BitVector32.CreateMask (1));
			Assertion.AssertEquals ("#4", 32, BitVector32.CreateMask (16));
			Assertion.AssertEquals ("#6", -2, BitVector32.CreateMask (Int32.MaxValue));
			Assertion.AssertEquals ("#5", -4, BitVector32.CreateMask (-2));
			try {
				BitVector32.CreateMask (Int32.MinValue);
				Assertion.Fail ("#7");
			} catch (InvalidOperationException) {}			
		}
		
		[Test]
		public void CreateSection ()
		{
			BitVector32.Section s = BitVector32.CreateSection (1);
			Assertion.AssertEquals ("#1", (short) 1, s.Mask);

			s = BitVector32.CreateSection (2);
			Assertion.AssertEquals ("#2", (short) 3, s.Mask);

			s = BitVector32.CreateSection (3);
			Assertion.AssertEquals ("#3", (short) 3, s.Mask);

			s = BitVector32.CreateSection (5);
			Assertion.AssertEquals ("#4", (short) 7, s.Mask);
			
			s = BitVector32.CreateSection (20);
			Assertion.AssertEquals ("#4", (short) 0x1f, s.Mask);

			s = BitVector32.CreateSection (Int16.MaxValue);
			Assertion.AssertEquals ("#5", (short) 0x7fff, s.Mask);

			s = BitVector32.CreateSection (Int16.MaxValue - 100);
			Assertion.AssertEquals ("#6", (short) 0x7fff, s.Mask);

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (0);
				Assertion.Fail ("#7");
			} catch (ArgumentException) {}

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (-1);
				Assertion.Fail ("#8");
			} catch (ArgumentException) {}

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (Int16.MinValue);
				Assertion.Fail ("#9");
			} catch (ArgumentException) {}
			
			s = BitVector32.CreateSection (20);
			Assertion.AssertEquals ("#10a", (short) 0x1f, s.Mask);
			Assertion.AssertEquals ("#10b", (short) 0x00, s.Offset);			
			s = BitVector32.CreateSection (120, s);
			Assertion.AssertEquals ("#10c", (short) 0x7f, s.Mask);
			Assertion.AssertEquals ("#10d", (short) 0x05, s.Offset);					
			s = BitVector32.CreateSection (1000, s);
			Assertion.AssertEquals ("#10e", (short) 0x3ff, s.Mask);
			Assertion.AssertEquals ("#10f", (short) 0x0c, s.Offset);			
		}

		private void Print (BitVector32.Section s)
		{
			Console.WriteLine (s.ToString () + " : "+ s.Mask + " : " + s.Offset);
		}
	}        
}
