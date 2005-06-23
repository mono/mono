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
	[TestFixture]
	public class StackTest: Assertion
	{
                private Stack stack1;
                private Stack stack2;
                private Stack stackInt;

                public void TestConstructor()
                {
                        AssertEquals(false, stack1 == null);
                }
                
                public void TestICollectionConstructor1()
                {
                        Stack stackTest = new Stack(new int[] {0, 1, 2, 3, 4});

                        for (int i = 4; i >= 0; i--)
                                AssertEquals(i, stackTest.Pop());

                        AssertEquals(0, stackTest.Count);
                }

		public void TestICollectionConstructor2()
		{
			bool exceptionThrown = false;
			try {
				Stack stackTest = new Stack(null);
			} catch (ArgumentNullException e) {
				exceptionThrown = true;
				AssertEquals("ParamName must be \"col\"","col",e.ParamName);
			}
			Assert("null argument must throw ArgumentNullException", exceptionThrown);
					
		}

                public void TestIntConstructor1()
                {
                        Stack stackTest = new Stack(50);

                        Assert(stackTest != null);
                }

		public void TestIntConstructor2()
		{
			bool exceptionThrown = false;
			try {
				Stack stackTest = new Stack(-1);
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				exceptionThrown = true;
				AssertEquals("ParamName must be \"initialCapacity\"","initialCapacity",e.ParamName);
			}
			Assert("negative argument must throw ArgumentOutOfRangeException", exceptionThrown);
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

                public void TestGetEnumerator1()
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

		public void TestGetEnumerator2()
		{
			
			IEnumerator e = stackInt.GetEnumerator();
			try 
			{
				// Tests InvalidOperationException if enumerator is uninitialized
				Object o = e.Current;
				Fail("InvalidOperationException should be thrown");
			} catch (InvalidOperationException) {}
		}

		public void TestGetEnumerator3()
		{
			
			IEnumerator e = stack1.GetEnumerator();
			e.MoveNext();
			try 
			{
				// Tests InvalidOperationException if enumeration has ended
				Object o = e.Current;
				Fail("InvalidOperationException should be thrown");
			} catch (InvalidOperationException) {}
		}
	
		public void TestEnumeratorReset1() 
		{
			IEnumerator e = stackInt.GetEnumerator();

			e.MoveNext();
			AssertEquals("current value", 4, e.Current);
			e.MoveNext();

			e.Reset();

			e.MoveNext();
			AssertEquals("current value after reset", 4, e.Current);
		}

		public void TestEnumeratorReset2() 
		{
			IEnumerator e = stackInt.GetEnumerator();

			e.MoveNext();
			AssertEquals("current value", 4, e.Current);

			// modifies underlying the stack. Reset must throw InvalidOperationException
			stackInt.Push(5);
			
			try 
			{
				e.Reset();
				Fail("InvalidOperationException should be thrown");
			} 
			catch (InvalidOperationException) {}
		}

		public void TestEnumeratorMoveNextException() 
		{
			IEnumerator e = stackInt.GetEnumerator();

			// modifies underlying the stack. MoveNext must throw InvalidOperationException
			stackInt.Push(5);
			
			try 
			{
				e.MoveNext();
				Fail("InvalidOperationException should be thrown");
			} 
			catch (InvalidOperationException) {}
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

			stackInt.Push(null);

			Assert(stackInt.Contains(toLocate));

			Assert("must contain null", stackInt.Contains(null));

			stackInt.Pop();

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
                        } catch (ArgumentNullException e) {
				AssertEquals("ParamName must be \"array\"","array",e.ParamName);
			}

                        try
                        {
                                stackInt.CopyTo(arr, -1);
                                Fail("Should throw an ArgumentOutOfRangeException");
                        } 
			catch (ArgumentOutOfRangeException e) 
			{
				AssertEquals("ParamName must be \"index\"","index",e.ParamName);
			}

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
		
		public void TestPop()
		{
			for (int i = 4; i >= 0; i--) 
			{
				AssertEquals(i, stackInt.Pop());
			}
			try {
				stackInt.Pop();
				Fail("must throw InvalidOperationException");
			} catch (InvalidOperationException){
			}
		}

                public void TestToArray()
                {
                        object[] arr = stackInt.ToArray();

                        AssertEquals(stackInt.Count, arr.Length);                       

                        for (int i = 0; i < 5; i++)
                                AssertEquals(arr[i], stackInt.Pop());
                }
		
		public void TestResize()
		{
			Stack myStack = new Stack(20);

			for (int i = 0; i < 500; i++) 
			{
				myStack.Push(i);
				AssertEquals("push count test",i+1, myStack.Count);
			}
			
			for (int i = 499; i >= 0; i--) 
			{
				AssertEquals(i, myStack.Pop());
				AssertEquals("pop count test",i, myStack.Count);
			}
		}

		public void TestEmptyCopyTo ()
		{
			Stack stack = new Stack ();
			string [] arr = new string [0];
			stack.CopyTo (arr, 0);
		}

		[Test]	
		public void TestICollectionCtorUsesEnum ()
		{
			BitArray x = new BitArray (10, true);
			Stack s = new Stack (x);
		}

		[SetUp]
                protected  void SetUp()
                {
                        stack1 = new Stack();
                        stack2 = new Stack();

                        stackInt = new Stack();
    
                        for (int i = 0; i < 5; i++)
                                stackInt.Push(i);
                }
        }
}
