//
// StackTest.cs
//
// Author:
//  Chris Hynes <chrish@assistedsolutions.com>
//
// (C) 2001 Chris Hynes
//

using System;

using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Collections
{
	public class StackTest: TestCase
	{
                private Stack stack1;
                private Stack stack2;
                private Stack stackInt;

                public void TestConstructor()
                {
                        AssertEquals(false, stack1 == null);
                }
                
                public void TestICollectionConstructor()
                {
                        Stack stackTest = new Stack(new int[] {0, 1, 2, 3, 4});

                        for (int i = 4; i >= 0; i--)
                                AssertEquals(i, stackTest.Pop());

                        AssertEquals(0, stackTest.Count);
                }

                public void TestIntConstructor()
                {
                        Stack stackTest = new Stack(50);

                        AssertEquals(false, stackTest == null);
                }

                public void TestCount()
                {
                        Stack stackTest = new Stack();

                        stackTest.Push(50);
                        stackTest.Push(5);
                        stackTest.Push(0);
                        stackTest.Push(50);

                        AssertEquals(4, stackTest.Count);
                }

                public void TestIsSyncronized()
                {
                        AssertEquals(false, stack1.IsSynchronized);
                        AssertEquals(true, Stack.Synchronized(stack1).IsSynchronized);
                }

                public void TestSyncRoot()
                {
                        AssertEquals(false, stack1.SyncRoot == null);
                }

                public void TestGetEnumerator()
                {
                        stackInt.Pop();

                        int j = 3;

                        foreach (int i in stackInt)
                        {
                                AssertEquals(j--, i);
                        }

                        stackInt.Clear();

                        IEnumerator e = stackInt.GetEnumerator();

                        AssertEquals(false, e.MoveNext());
                }

                public void TestClear()
                {
                        stackInt.Clear();

                        AssertEquals(0, stackInt.Count);
                }

                public void TestClone()
                {
                        Stack clone = (Stack)stackInt.Clone();

                        while (stackInt.Count > 0)
                        {
                                AssertEquals(stackInt.Pop(), clone.Pop());
                        }
                }

                public void TestContains()
                {
                        string toLocate = "test";

                        stackInt.Push(toLocate);

                        stackInt.Push("chaff");

                        Assert(stackInt.Contains(toLocate));

                        stackInt.Pop();

                        Assert(stackInt.Contains(toLocate));

                        stackInt.Pop();

                        Assert(!stackInt.Contains(toLocate));
			
			stackInt.Push(null);
			Assert(stackInt.Contains(null));
			stackInt.Pop();
			Assert(!stackInt.Contains(null));
			
			
                }

                public void TestCopyTo()
                {
                        int[] arr = new int[stackInt.Count - 1];
                        int[,] arrMulti;

                        try 
                        {
                                stackInt.CopyTo(null, 0);
                                Fail("Should throw an ArgumentNullException");
                        } 
                        catch (ArgumentNullException) {}

                        try
                        {
                                stackInt.CopyTo(arr, -1);
                                Fail("Should throw an ArgumentOutOfRangeException");
                        } 
                        catch (ArgumentOutOfRangeException) {}

                        try
                        {
                                stackInt.CopyTo(arrMulti = new int[1, 1], 1);
                                Fail("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        try
                        {
                                stackInt.CopyTo(arr = new int[2], 3);
                                Fail("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        try
                        {
                                stackInt.CopyTo(arr = new int[3], 2);
                                Fail("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        try
                        {
                                stackInt.CopyTo(arr = new int[2], 3);
                                Fail("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        arr = new int[stackInt.Count];

                        stackInt.CopyTo(arr, 0);

                        int j = 4;

                        for (int i = 0; i < 4; i++)
                        {
                               AssertEquals(j--, arr[i]);
                        }
                }

                public void TestSyncronized()
                {
                        Stack syncStack = Stack.Synchronized(stackInt);

                        syncStack.Push(5);

                        for (int i = 5; i >= 0; i--)
                                AssertEquals(i, syncStack.Pop());
                }

                public void TestPushPeekPop()
                {
                        stackInt.Pop();

                        int topVal = (int)stackInt.Peek();

                        AssertEquals(3, topVal);

                        AssertEquals(4, stackInt.Count);

                        AssertEquals(topVal, stackInt.Pop());

                        AssertEquals(2, stackInt.Pop());

                        Stack test = new Stack();
                        test.Push(null);

                        AssertEquals(null, test.Pop());
                }

                public void TestToArray()
                {
                        object[] arr = stackInt.ToArray();

                        AssertEquals(stackInt.Count, arr.Length);                       

                        for (int i = 0; i < 5; i++)
                                AssertEquals(arr[i], stackInt.Pop());
                }

                protected override void SetUp()
                {
                        stack1 = new Stack();
                        stack2 = new Stack();

                        stackInt = new Stack();
    
                        for (int i = 0; i < 5; i++)
                                stackInt.Push(i);
                }
        }
}
