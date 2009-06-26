//
// BitVector32Test.cs - NUnit Test Cases for System.Net.BitVector32
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
			Assert.AreEqual (31, b.Data, "Data");
			Assert.IsTrue (b.Equals (b), "Equals(self)");
			Assert.IsTrue (b[31], "31");
			Assert.IsFalse (b[32], "32");
			Assert.AreEqual (b.ToString (), "BitVector32{00000000000000000000000000011111}", b.ToString ());

			BitVector32 b2 = new BitVector32 (b);
			Assert.IsTrue (b.Equals (b2), "Equals(b2)");
			Assert.AreEqual (b.GetHashCode (), b2.GetHashCode (), "GetHashCode==");

			b2[32] = true;
			Assert.IsFalse (b.Equals (b2), "Equals(b32)");
			Assert.IsFalse (b.GetHashCode () == b2.GetHashCode (), "GetHashCode!=");
		}

		[Test]
		public void Constructors_MaxValue ()
		{
			BitVector32 b = new BitVector32 (Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, b.Data, "Data");
			Assert.AreEqual ("BitVector32{01111111111111111111111111111111}", BitVector32.ToString (b), "ToString(BitVector)");
		}

		[Test]
		public void Constructors_MinValue ()
		{
			BitVector32 b = new BitVector32 (Int32.MinValue);
			Assert.AreEqual (Int32.MinValue, b.Data, "Data");
			Assert.AreEqual ("BitVector32{10000000000000000000000000000000}", BitVector32.ToString (b), "ToString(BitVector)");
		}
		
		[Test]
		public void Indexers ()
		{
			BitVector32 b = new BitVector32 (7);
			Assert.IsTrue (b [0], "#1");
			Assert.IsTrue (b [1], "#2");
			Assert.IsTrue (b [2], "#3");
			Assert.IsTrue (b [4], "#4");
			Assert.IsTrue (!b [8], "#5");
			Assert.IsTrue (!b [16], "#6");
			b [8] = true;
			Assert.IsTrue (b [4], "#7");
			Assert.IsTrue (b [8], "#8");
			Assert.IsTrue (!b [16], "#9");
			b [8] = false;
			Assert.IsTrue (b [4], "#10");
			Assert.IsTrue (!b [8], "#11");
			Assert.IsTrue (!b [16], "#12");

			BitVector32.Section s = BitVector32.CreateSection (31);
			s = BitVector32.CreateSection (64, s);
			// Print (s);
			
			// b = new BitVector32 (0x777777);
			BitVector32 b1 = new BitVector32 (0xffff77);
			BitVector32 b2 = new BitVector32 (b1 [s]);
			//Console.WriteLine (b1.ToString ());
			//Console.WriteLine (b2.ToString ());
			Assert.AreEqual (123, b1 [s], "#14");
			
			// b1 [s] = 15;
			//Console.WriteLine (b1.ToString ());
		}

		[Test]
		public void CreateMask ()
		{
			Assert.AreEqual (1, BitVector32.CreateMask (), "#1");
			Assert.AreEqual (1, BitVector32.CreateMask (0), "#2");
			Assert.AreEqual (2, BitVector32.CreateMask (1), "#3");
			Assert.AreEqual (32, BitVector32.CreateMask (16), "#4");
			Assert.AreEqual (-2, BitVector32.CreateMask (Int32.MaxValue), "#5");
			Assert.AreEqual (-4, BitVector32.CreateMask (-2), "#6");
			Assert.AreEqual (2, BitVector32.CreateMask (Int32.MinValue + 1), "#7");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateMask_MinValue ()
		{
			BitVector32.CreateMask (Int32.MinValue);
		}
		
		[Test]
		public void CreateSection ()
		{
			BitVector32.Section s = BitVector32.CreateSection (1);
			Assert.AreEqual ((short) 1, s.Mask, "#1");

			s = BitVector32.CreateSection (2);
			Assert.AreEqual ((short) 3, s.Mask, "#2");

			s = BitVector32.CreateSection (3);
			Assert.AreEqual ((short) 3, s.Mask, "#3");

			s = BitVector32.CreateSection (5);
			Assert.AreEqual ((short) 7, s.Mask, "#4");
			
			s = BitVector32.CreateSection (20);
			Assert.AreEqual ((short) 0x1f, s.Mask, "#4");

			s = BitVector32.CreateSection (Int16.MaxValue);
			Assert.AreEqual ((short) 0x7fff, s.Mask, "#5");

			s = BitVector32.CreateSection (Int16.MaxValue - 100);
			Assert.AreEqual ((short) 0x7fff, s.Mask, "#6");

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (0);
				Assert.Fail ("#7");
			} catch (ArgumentException) {}

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (-1);
				Assert.Fail ("#8");
			} catch (ArgumentException) {}

			try {
				BitVector32.Section s2 = BitVector32.CreateSection (Int16.MinValue);
				Assert.Fail ("#9");
			} catch (ArgumentException) {}
			
			s = BitVector32.CreateSection (20);
			Assert.AreEqual ((short) 0x1f, s.Mask, "#10a");
			Assert.AreEqual ((short) 0x00, s.Offset, "#10b");			
			s = BitVector32.CreateSection (120, s);
			Assert.AreEqual ((short) 0x7f, s.Mask, "#10c");
			Assert.AreEqual ((short) 0x05, s.Offset, "#10d");					
			s = BitVector32.CreateSection (1000, s);
			Assert.AreEqual ((short) 0x3ff, s.Mask, "#10e");
			Assert.AreEqual ((short) 0x0c, s.Offset, "#10f");			
		}

		[Test]
		public void Section ()
		{
			BitVector32.Section s1 = BitVector32.CreateSection (20);
			Assert.AreEqual (31, s1.Mask, "1.Mask");
			Assert.AreEqual (0, s1.Offset, "1.Offset");
			Assert.AreEqual ("Section{0x1f, 0x0}", BitVector32.Section.ToString (s1), "ToString(Section)");

			BitVector32.Section s2 = BitVector32.CreateSection (20);
			Assert.IsTrue (s1.Equals (s2), "s1==s2");
			Assert.IsTrue (s2.Equals ((object)s1), "s2==s1");
			Assert.AreEqual (s1.GetHashCode (), s2.GetHashCode (), "GetHashCode");
			Assert.AreEqual ("Section{0x1f, 0x0}", s2.ToString (), "ToString()");
		}

		[Test]
		public void SectionCorrectSize ()
		{
			BitVector32.Section s1 = BitVector32.CreateSection (32767);
			BitVector32.Section s2 = BitVector32.CreateSection (32767, s1);
			BitVector32.Section s3 = BitVector32.CreateSection (3, s2);
			BitVector32 v1 = new BitVector32 (0);
			v1[s3] = 3;
			Assert.AreEqual (v1[s3], 3);
		}

		[Test]
		public void SectionIncorrectSize ()
		{
			BitVector32.Section s1 = BitVector32.CreateSection (32767);
			BitVector32.Section s2 = BitVector32.CreateSection (32767, s1);
			try {
				BitVector32.Section s3 = BitVector32.CreateSection (4, s2);
				Assert.Fail("Illegal section created");
			} catch (ArgumentException) {}
		}

                [Test]
                public void NegativeIndexer ()
                {
                        BitVector32 bv = new BitVector32 (-1);
#if NET_2_0
			Assert.IsTrue (bv [Int32.MinValue], "Int32.MinValue");
#else
			Assert.IsFalse (bv [Int32.MinValue], "Int32.MinValue");
#endif
                }

                [Test]
                public void TestSectionIndexer ()
                {
                        BitVector32 bv = new BitVector32 (-1);
                        BitVector32.Section sect = BitVector32.CreateSection (1);
                        sect = BitVector32.CreateSection (Int16.MaxValue, sect);
                        sect = BitVector32.CreateSection (Int16.MaxValue, sect);
                        sect = BitVector32.CreateSection (1, sect);
			Assert.AreEqual (1, bv[sect], "bv[sect]");
                        bv [sect] = 0; 

                        Assert.AreEqual (Int32.MaxValue, bv.Data, "#12a");
                }

                [Test, ExpectedException (typeof (ArgumentException))]
                public void TestCreateSection1 ()
                {
                        BitVector32.Section section = BitVector32.CreateSection (Int16.MaxValue);
                        section = BitVector32.CreateSection (0, section);
                }

                [Test, ExpectedException (typeof (ArgumentException))]
                public void TestCreateSection2 ()
                {
                        BitVector32.Section section = BitVector32.CreateSection (Int16.MaxValue);
                        section = BitVector32.CreateSection (-1, section);
                }

                [Test, ExpectedException (typeof (ArgumentException))]
                public void TestCreateSection3 ()
                {
                        BitVector32.Section section = BitVector32.CreateSection (Int16.MaxValue);
                        section = BitVector32.CreateSection (Int16.MinValue, section);
                }

		private void Print (BitVector32.Section s)
		{
			Console.WriteLine (s.ToString () + " : "+ s.Mask + " : " + s.Offset);
		}
	}        
}
