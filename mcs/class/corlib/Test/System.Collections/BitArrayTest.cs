//
// BitArrayTest.cs - NUnit Test Cases for the System.Collections.BitArray class
// 
// Author: David Menestrina (dmenest@yahoo.com)
//

using NUnit.Framework;
using System.Collections;
using System;

namespace MonoTests.System.Collections
{
[TestFixture]
public class BitArrayTest
{
  private BitArray testBa;
  private bool [] testPattern;
  private BitArray op1;
  private BitArray op2;

  private void verifyPattern(BitArray ba, bool[] pattern)
  {
    Assert.AreEqual (ba.Length, pattern.Length);
    for (int i = 0; i < pattern.Length; i++)
      Assert.AreEqual (ba[i], pattern[i]);
  }

  [SetUp]
  public void SetUp()
  {
    testPattern = new bool[70];

    int i;
    for(i = 0; i < testPattern.Length/2; i++)
      testPattern[i] = ((i % 2) == 0);
    for(; i < testPattern.Length; i++)
      testPattern[i] = ((i % 2) != 0);

    testBa = new BitArray(70);
    for(i = 0; i < testBa.Length/2; i++)
      testBa[i] = ((i % 2) == 0);
    for(; i < testBa.Length; i++)
      testBa[i] = ((i % 2) != 0);

    // for TestAnd, TestOr, TestNot, TestXor
    op1 = new BitArray(new int[] { 0x33333333, 0x33333333 });
    op2 = new BitArray(new int[] { 0x66666666, 0x66666666 });
  }
  
  [Test]
  public void TestBoolConstructor()
  {
    BitArray ba = new BitArray(testPattern);
    verifyPattern(ba, testPattern);
  }

  [Test]
  public void TestCopyConstructor() 
  {
    BitArray ba = new BitArray(testBa);

    verifyPattern(ba, testPattern);
  }

  [Test]
  public void TestByteConstructor()
  {
    byte [] byteArr = new byte[] { 0xaa, 0x55, 0xaa, 0x55, 0x80 };
    BitArray ba = new BitArray(byteArr);
    
    Assert.AreEqual (ba.Length, byteArr.Length * 8, "Lengths not equal");
    
    // spot check
    Assert.IsTrue (ba[7], "7 not true");
    Assert.IsTrue (!ba[6], "6 not false");
    Assert.IsTrue (!ba[15], "15 not false");
    Assert.IsTrue (ba[14], "14 not true");
    Assert.IsTrue (ba[39], "39 not true");
    Assert.IsTrue (!ba[35], "35 not false");

  }

  [Test]
  public void TestIntConstructor()
  {
    int [] intArr = new int[] { ~0x55555555, 0x55555551 };
    BitArray ba = new BitArray(intArr);
    
    Assert.AreEqual (ba.Length, intArr.Length * 32);
    
    // spot check
    
    Assert.IsTrue (ba[31]);
    Assert.IsTrue (!ba[30]);
    Assert.IsTrue (!ba[63]);
    Assert.IsTrue (ba[62]);
    Assert.IsTrue (ba[32]);
    Assert.IsTrue (!ba[35]);
  }

  [Test]
  public void TestValConstructor()
  {
    BitArray ba = new BitArray(64, false);

    Assert.AreEqual (ba.Length, 64);
    foreach (bool b in ba)
      Assert.IsTrue (!b);

    ba = new BitArray(64, true);

    Assert.AreEqual (ba.Length, 64);
    foreach (bool b in ba)
      Assert.IsTrue (b);
  }

  [Test]
  public void TestClone()
  {
    BitArray ba = (BitArray)testBa.Clone();

    verifyPattern(ba, testPattern);

    // ensure that changes in ba don't get propagated to testBa
    ba[0] = false;
    ba[1] = false;
    ba[2] = false;
    
    verifyPattern(testBa, testPattern);
  }
  
  [Test]
  public void TestSetLength()
  {
    int origLen = testBa.Length;
    testBa.Length += 33;

    Assert.AreEqual (origLen + 33, testBa.Length);
    for (int i = origLen; i < testBa.Length; i++)
      testBa[i] = true;

    testBa.Length -= 33;
    verifyPattern(testBa, testPattern);
  }

  [Test]
  public void TestAnd()
  {
    BitArray result = op1.And(op2);
    Assert.AreEqual (result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert.IsTrue (!result[i++]);
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (!result[i++]);
      Assert.IsTrue (!result[i++]);
    }
  }

  [Test]
  public void TestOr()
  {
    BitArray result = op1.Or(op2);
    Assert.AreEqual (result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (!result[i++]);
    }
  }

  [Test]
  public void TestNot()
  {
    BitArray result = op1.Not();
    Assert.AreEqual (result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert.IsTrue (!result[i++]);
      Assert.IsTrue (!result[i++]);
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (result[i++]);
    }
  }

  [Test]
  public void TestXor()
  {
    BitArray result = op1.Xor(op2);
    Assert.AreEqual (result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (!result[i++]);
      Assert.IsTrue (result[i++]);
      Assert.IsTrue (!result[i++]);
    }
  }

  [Test]
  public void TestSetAll()
  {
    testBa.SetAll(false);
    foreach(bool b in testBa)
      Assert.IsTrue (!b);
    testBa.SetAll(true);
    foreach(bool b in testBa)
      Assert.IsTrue (b);
  }

  [Test]
  public void TestCopyToBool()
  {
    try {
	    bool[] barray = new bool[testBa.Length + 10];
	    
	    testBa.CopyTo(barray, 5);

	    for (int i = 0; i < testBa.Length; i++)
	      Assert.AreEqual (testBa[i], barray[i+5]);
    }
    catch(Exception e){
	Assert.Fail ("Unexpected exception thrown: " + e.ToString());
    }
  }

  [Test]
  public void CopyToEmptyEmpty () {
	  BitArray bitarray = new BitArray(0);

	  int[] intarray = new int[0];

	  bitarray.CopyTo(intarray, 0);
  }

  [Test]
  public void TestCopyToByte()
  {
    try {
	    testBa.Length = 34;
	    byte[] barray = new byte[5 + 10];
	    
	    testBa.CopyTo(barray, 5);

	    for (int i = 5; i < 9; i++)
	      Assert.AreEqual (0x55, barray[i] & 0xff);

	    // FIXME: MS fails on the next line.  This is because
	    // we truncated testBa.Length, and MS's internal array still
	    // has the old bits set.  CopyTo() doesn't say specifically
	    // whether the "junk" bits (bits past Length, but within Length
	    // rounded up to 32) will be copied as 0, or if those bits are
	    // undefined.
	    //Assert.AreEqual (0x01, barray[9] & 0xff);
    }
    catch(Exception e){
	Assert.Fail ("Unexpected exception thrown: " + e.ToString());
    }
  }

  [Test]
  public void TestCopyToInt()
  {
    try {
	    testBa.Length = 34;
	    int[] iarray = new int[2 + 10];
	    
	    testBa.CopyTo(iarray, 5);

	    Assert.AreEqual (0x55555555, iarray[5]);
	    // FIXME:  Same thing here as in TestCopyToByte
	    //Assert.AreEqual (0x01, iarray[6]);
    }
    catch(Exception e){
	Assert.Fail ("Unexpected exception thrown: " + e.ToString());
    }
  }

  [Test]
  public void TestEnumerator()
  {
    
    try {
	    IEnumerator e = testBa.GetEnumerator();
	    
	    for (int i = 0; e.MoveNext(); i++)
	      Assert.AreEqual (e.Current, testPattern[i]);

	    Assert.IsTrue (!e.MoveNext());
	    // read, to make sure reading isn't considered a write.
	    bool b = testBa[0];

	    e.Reset();
	    for (int i = 0; e.MoveNext(); i++)
	      Assert.AreEqual (e.Current, testPattern[i]);

	    try
	    {
	      e.Reset();
	      testBa[0] = !testBa[0];
	      e.MoveNext();
	      Assert.Fail ("IEnumerator.MoveNext() should throw when collection modified.");
	    }
	    catch (InvalidOperationException)
	    {
	    }
    }
    catch(Exception ex){
	Assert.Fail ("Unexpected exception thrown: " + ex.ToString());
    }
  }
}

}
