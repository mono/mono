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

public class BitArrayTest : TestCase 
{
  private BitArray testBa;
  private bool [] testPattern;
  private BitArray op1;
  private BitArray op2;

  private void verifyPattern(BitArray ba, bool[] pattern)
  {
    AssertEquals(ba.Length, pattern.Length);
    for (int i = 0; i < pattern.Length; i++)
      AssertEquals(ba[i], pattern[i]);
  }

  protected override void SetUp()
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
  
  public void TestBoolConstructor()
  {
    BitArray ba = new BitArray(testPattern);
    verifyPattern(ba, testPattern);
  }

  public void TestCopyConstructor() 
  {
    BitArray ba = new BitArray(testBa);

    verifyPattern(ba, testPattern);
  }

  public void TestByteConstructor()
  {
    byte [] byteArr = new byte[] { 0xaa, 0x55, 0xaa, 0x55, 0x80 };
    BitArray ba = new BitArray(byteArr);
    
    AssertEquals("Lengths not equal", ba.Length, byteArr.Length * 8);
    
    // spot check
    Assert("7 not true", ba[7]);
    Assert("6 not false", !ba[6]);
    Assert("15 not false", !ba[15]);
    Assert("14 not true", ba[14]);
    Assert("39 not true", ba[39]);
    Assert("35 not false", !ba[35]);

  }

  public void TestIntConstructor()
  {
    int [] intArr = new int[] { ~0x55555555, 0x55555551 };
    BitArray ba = new BitArray(intArr);
    
    AssertEquals(ba.Length, intArr.Length * 32);
    
    // spot check
    
    Assert(ba[31]);
    Assert(!ba[30]);
    Assert(!ba[63]);
    Assert(ba[62]);
    Assert(ba[32]);
    Assert(!ba[35]);
  }

  public void TestValConstructor()
  {
    BitArray ba = new BitArray(64, false);

    AssertEquals(ba.Length, 64);
    foreach (bool b in ba)
      Assert(!b);

    ba = new BitArray(64, true);

    AssertEquals(ba.Length, 64);
    foreach (bool b in ba)
      Assert(b);
  }

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
  
  public void TestSetLength()
  {
    int origLen = testBa.Length;
    testBa.Length += 33;

    AssertEquals(origLen + 33, testBa.Length);
    for (int i = origLen; i < testBa.Length; i++)
      testBa[i] = true;

    testBa.Length -= 33;
    verifyPattern(testBa, testPattern);
  }

  public void TestAnd()
  {
    BitArray result = op1.And(op2);
    AssertEquals(result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert(!result[i++]);
      Assert(result[i++]);
      Assert(!result[i++]);
      Assert(!result[i++]);
    }
  }

  public void TestOr()
  {
    BitArray result = op1.Or(op2);
    AssertEquals(result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert(result[i++]);
      Assert(result[i++]);
      Assert(result[i++]);
      Assert(!result[i++]);
    }
  }

  public void TestNot()
  {
    BitArray result = op1.Not();
    AssertEquals(result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert(!result[i++]);
      Assert(!result[i++]);
      Assert(result[i++]);
      Assert(result[i++]);
    }
  }

  public void TestXor()
  {
    BitArray result = op1.Xor(op2);
    AssertEquals(result.Length, op1.Length);
    for (int i = 0; i < result.Length; )
    {
      Assert(result[i++]);
      Assert(!result[i++]);
      Assert(result[i++]);
      Assert(!result[i++]);
    }
  }

  public void TestSetAll()
  {
    testBa.SetAll(false);
    foreach(bool b in testBa)
      Assert(!b);
    testBa.SetAll(true);
    foreach(bool b in testBa)
      Assert(b);
  }

  public void TestCopyToBool()
  {
    try {
	    bool[] barray = new bool[testBa.Length + 10];
	    
	    testBa.CopyTo(barray, 5);

	    for (int i = 0; i < testBa.Length; i++)
	      AssertEquals(testBa[i], barray[i+5]);
    }
    catch(Exception e){
	Fail("Unexpected exception thrown: " + e.ToString());
    }
  }

  public void TestCopyToByte()
  {
    try {
	    testBa.Length = 34;
	    byte[] barray = new byte[5 + 10];
	    
	    testBa.CopyTo(barray, 5);

	    for (int i = 5; i < 9; i++)
	      AssertEquals(0x55, barray[i] & 0xff);

	    // FIXME: MS fails on the next line.  This is because
	    // we truncated testBa.Length, and MS's internal array still
	    // has the old bits set.  CopyTo() doesn't say specifically
	    // whether the "junk" bits (bits past Length, but within Length
	    // rounded up to 32) will be copied as 0, or if those bits are
	    // undefined.
	    //AssertEquals(0x01, barray[9] & 0xff);
    }
    catch(Exception e){
	Fail("Unexpected exception thrown: " + e.ToString());
    }
  }

  public void TestCopyToInt()
  {
    try {
	    testBa.Length = 34;
	    int[] iarray = new int[2 + 10];
	    
	    testBa.CopyTo(iarray, 5);

	    AssertEquals(0x55555555, iarray[5]);
	    // FIXME:  Same thing here as in TestCopyToByte
	    //AssertEquals(0x01, iarray[6]);
    }
    catch(Exception e){
	Fail("Unexpected exception thrown: " + e.ToString());
    }
  }

  public void TestEnumerator()
  {
    
    try {
	    IEnumerator e = testBa.GetEnumerator();
	    
	    for (int i = 0; e.MoveNext(); i++)
	      AssertEquals(e.Current, testPattern[i]);

	    Assert(!e.MoveNext());
	    // read, to make sure reading isn't considered a write.
	    bool b = testBa[0];

	    e.Reset();
	    for (int i = 0; e.MoveNext(); i++)
	      AssertEquals(e.Current, testPattern[i]);

	    try
	    {
	      e.Reset();
	      testBa[0] = !testBa[0];
	      e.MoveNext();
	      Fail("IEnumerator.MoveNext() should throw when collection modified.");
	    }
	    catch (InvalidOperationException)
	    {
	    }
    }
    catch(Exception ex){
	Fail("Unexpected exception thrown: " + ex.ToString());
    }
  }
}

}
