// ArraySegmentTest.cs - NUnit Test Cases for the System.ArraySegment class
//
// Ankit Jain  <jankit@novell.com>
// Raja R Harinath  <rharinath@novell.com>
// Jensen Somers <jensen.somers@gmail.com>
// 
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// 

#if NET_2_0
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MonoTests.System
{

[TestFixture]
public class ArraySegmentTest
{
	public ArraySegmentTest() {}

	[Test]
	public void CtorTest1 () 
	{
		byte [] b_arr = new byte [4096];
		Array arr;

		ArraySegment<byte> seg = new ArraySegment<byte> (b_arr, 0, b_arr.Length);
		Assert.AreEqual (seg.Count, b_arr.Length, "#1");
		Assert.AreEqual (seg.Offset, 0, "#2");
		
		arr = seg.Array;
		Assert.AreEqual (arr.Length, 4096, "#5");
		
		seg = new ArraySegment<byte> (b_arr, 100, b_arr.Length - 100);
		Assert.AreEqual (seg.Count, b_arr.Length - 100, "#3");
		Assert.AreEqual (seg.Offset, 100, "#4");
		
		arr = seg.Array;
		Assert.AreEqual (arr.Length, 4096, "#5");
	}

	[Test]
	public void CtorTest2 ()
	{
		byte [] b_arr = new byte [4096];
		ArraySegment<byte> seg = new ArraySegment<byte> (b_arr);
		Assert.AreEqual (seg.Count, b_arr.Length, "#6");
		Assert.AreEqual (seg.Offset, 0, "#7");
		
		Array arr = seg.Array;
		Assert.AreEqual (arr.Length, 4096, "#8");
	}

	[Test]
	public void CtorTest3 ()
	{
		EmptyArraySegTest (0);
		EmptyArraySegTest (10);
	}

	private void EmptyArraySegTest (int len)
	{
		byte [] b_arr = new byte [len];

		ArraySegment<byte> seg = new ArraySegment<byte> (b_arr, 0, b_arr.Length);

		Assert.AreEqual (seg.Count, b_arr.Length, "#1 [array len {0}] ", len);
		Assert.AreEqual (seg.Offset, 0, "#2 [array len {0}] ", len);
		Array arr = seg.Array;
		Assert.AreEqual (arr.Length, len, "#3 [array len {0}] ", len);
		
		seg = new ArraySegment<byte> (b_arr, b_arr.Length, 0);
		Assert.AreEqual (seg.Count, 0, "#4 [array len {0}] ", len);
		Assert.AreEqual (seg.Offset, b_arr.Length, "#5 [array len {0}] ", len);
		arr = seg.Array;
		Assert.AreEqual (arr.Length, len, "#6 [array len {0}] ", len);

		seg = new ArraySegment<byte> (b_arr);
		Assert.AreEqual (seg.Count, b_arr.Length, "#7 [array len {0}] ", len);
		Assert.AreEqual (seg.Offset, 0, "#8 [array len {0}] ", len);
		arr = seg.Array;
		Assert.AreEqual (arr.Length, len, "#9 [array len {0}] ", len);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CtorErrorTest ()
	{
		byte [] arr = new byte [4096];	
		ArraySegment<byte> seg = new ArraySegment<byte> (arr, 1, arr.Length);
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CtorErrorTest2 ()
	{
		byte [] arr = new byte [4096];	
		ArraySegment<byte> seg = new ArraySegment<byte> (arr, 0, arr.Length + 2);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void CtorErrorTest3 ()
	{
		byte [] arr = new byte [4096];	
		ArraySegment<byte> seg = new ArraySegment<byte> (arr, -1, arr.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void CtorErrorTest4 ()
	{
		byte [] arr = new byte [4096];	
		ArraySegment<byte> seg = new ArraySegment<byte> (arr, 2, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CtorErrorTest5 ()
	{
		byte [] arr = new byte [4096];
		ArraySegment<byte> seg = new ArraySegment<byte> (arr, 0, arr.Length + 2);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void CtorNullTest1 ()
	{
		ArraySegment<byte> seg = new ArraySegment<byte> (null, 0 , 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void CtorNullTest2 ()
	{
		ArraySegment<byte> seg = new ArraySegment<byte> (null);
	}

#pragma warning disable 1718
	[Test]
	public void TestArraySegmentEqual ()
	{
	    string[] myArr_1 = { "The", "good" };
	    string[] myArr_2 = { "The", "good" };
	    
	    ArraySegment<string> myArrSeg_1 = new ArraySegment<string>(myArr_1);
	    ArraySegment<string> myArrSeg_2 = new ArraySegment<string>(myArr_2);
	    
	    // Should return true.
	    Assert.AreEqual(myArrSeg_1.Equals(myArrSeg_1), true);
	    Assert.AreEqual(myArrSeg_1 == myArrSeg_1, true);
	    
	    // Should return false. Allthough the strings are the same.
	    Assert.AreEqual(myArrSeg_1.Equals(myArrSeg_2), false);
	    Assert.AreEqual(myArrSeg_1 == myArrSeg_2, false);
	    
	    // Should return true.
	    Assert.AreEqual(myArrSeg_1 != myArrSeg_2, true);
	}
#pragma warning restore 1718
}

}
#endif
