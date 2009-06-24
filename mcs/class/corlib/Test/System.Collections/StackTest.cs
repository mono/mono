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
	public class StackTest
	{
                private Stack stack1;
                private Stack stack2;
                private Stack stackInt;

		[Test]
                public void TestConstructor()
                {
                        Assert.AreEqual (stack1 == null, false);
                }
                
		[Test]
                public void TestICollectionConstructor1()
                {
                        Stack stackTest = new Stack(new int[] {0, 1, 2, 3, 4});

                        for (int i = 4; i >= 0; i--)
                                Assert.AreEqual (stackTest.Pop(), i);

                        Assert.AreEqual (stackTest.Count, 0);
                }

		[Test]
		public void TestICollectionConstructor2()
		{
			bool exceptionThrown = false;
			try {
				Stack stackTest = new Stack(null);
			} catch (ArgumentNullException e) {
				exceptionThrown = true;
				Assert.AreEqual ("col", e.ParamName, "ParamName must be \"col\"");
			}
			Assert.IsTrue (exceptionThrown, "null argument must throw ArgumentNullException");
					
		}

		[Test]
                public void TestIntConstructor1()
                {
                        Stack stackTest = new Stack(50);

                        Assert.IsTrue (stackTest != null);
                }

		[Test]
		public void TestIntConstructor2()
		{
			bool exceptionThrown = false;
			try {
				Stack stackTest = new Stack(-1);
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				exceptionThrown = true;
				Assert.AreEqual ("initialCapacity", e.ParamName, "ParamName must be \"initialCapacity\"");
			}
			Assert.IsTrue (exceptionThrown, "negative argument must throw ArgumentOutOfRangeException");
		}

		[Test]
                public void TestCount()
                {
                        Stack stackTest = new Stack();

                        stackTest.Push(50);
                        stackTest.Push(5);
                        stackTest.Push(0);
                        stackTest.Push(50);

                        Assert.AreEqual (stackTest.Count, 4);
                }

		[Test]
                public void TestIsSyncronized()
                {
                        Assert.AreEqual (stack1.IsSynchronized, false);
                        Assert.AreEqual (Stack.Synchronized(stack1).IsSynchronized, true);
                }

		[Test]
                public void TestSyncRoot()
                {
                        Assert.AreEqual (stack1.SyncRoot == null, false);
                }

		[Test]
                public void TestGetEnumerator1()
                {
                        stackInt.Pop();

                        int j = 3;

                        foreach (int i in stackInt)
                        {
                                Assert.AreEqual (i, j--);
                        }

                        stackInt.Clear();

                        IEnumerator e = stackInt.GetEnumerator();

                        Assert.AreEqual (e.MoveNext(), false);
                }

		[Test]
		public void TestGetEnumerator2()
		{
			
			IEnumerator e = stackInt.GetEnumerator();
			try 
			{
				// Tests InvalidOperationException if enumerator is uninitialized
				Object o = e.Current;
				Assert.Fail ("InvalidOperationException should be thrown");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void TestGetEnumerator3()
		{
			
			IEnumerator e = stack1.GetEnumerator();
			e.MoveNext();
			try 
			{
				// Tests InvalidOperationException if enumeration has ended
				Object o = e.Current;
				Assert.Fail ("InvalidOperationException should be thrown");
			} catch (InvalidOperationException) {}
		}
	
		[Test]
		public void TestEnumeratorReset1() 
		{
			IEnumerator e = stackInt.GetEnumerator();

			e.MoveNext();
			Assert.AreEqual (4, e.Current, "current value");
			e.MoveNext();

			e.Reset();

			e.MoveNext();
			Assert.AreEqual (4, e.Current, "current value after reset");
		}

		[Test]
		public void TestEnumeratorReset2() 
		{
			IEnumerator e = stackInt.GetEnumerator();

			e.MoveNext();
			Assert.AreEqual (4, e.Current, "current value");

			// modifies underlying the stack. Reset must throw InvalidOperationException
			stackInt.Push(5);
			
			try 
			{
				e.Reset();
				Assert.Fail ("InvalidOperationException should be thrown");
			} 
			catch (InvalidOperationException) {}
		}

		[Test]
		public void TestEnumeratorMoveNextException() 
		{
			IEnumerator e = stackInt.GetEnumerator();

			// modifies underlying the stack. MoveNext must throw InvalidOperationException
			stackInt.Push(5);
			
			try 
			{
				e.MoveNext();
				Assert.Fail ("InvalidOperationException should be thrown");
			} 
			catch (InvalidOperationException) {}
		}


		[Test]
                public void TestClear()
                {
                        stackInt.Clear();

                        Assert.AreEqual (stackInt.Count, 0);
                }

		[Test]
                public void TestClone()
                {
                        Stack clone = (Stack)stackInt.Clone();

                        while (stackInt.Count > 0)
                        {
                                Assert.AreEqual (clone.Pop(), stackInt.Pop());
                        }
                }

		[Test]
                public void TestContains()
                {
                        string toLocate = "test";


                        stackInt.Push(toLocate);

                        stackInt.Push("chaff");

			stackInt.Push(null);

			Assert.IsTrue (stackInt.Contains(toLocate));

			Assert.IsTrue (stackInt.Contains(null), "must contain null");

			stackInt.Pop();

                        stackInt.Pop();

                        Assert.IsTrue (stackInt.Contains(toLocate));

                        stackInt.Pop();

                        Assert.IsTrue (!stackInt.Contains(toLocate));
			
			stackInt.Push(null);
			Assert.IsTrue (stackInt.Contains(null));
			stackInt.Pop();
			Assert.IsTrue (!stackInt.Contains(null));
			
			
                }

		[Test]
                public void TestCopyTo()
                {
                        int[] arr = new int[stackInt.Count - 1];
                        int[,] arrMulti;

                        try 
                        {
                                stackInt.CopyTo(null, 0);
                                Assert.Fail ("Should throw an ArgumentNullException");
                        } catch (ArgumentNullException e) {
				Assert.AreEqual ("array", e.ParamName, "ParamName must be \"array\"");
			}

                        try
                        {
                                stackInt.CopyTo(arr, -1);
                                Assert.Fail ("Should throw an ArgumentOutOfRangeException");
                        } 
			catch (ArgumentOutOfRangeException e) 
			{
				Assert.AreEqual ("index", e.ParamName, "ParamName must be \"index\"");
			}

                        try
                        {
                                stackInt.CopyTo(arrMulti = new int[1, 1], 1);
                                Assert.Fail ("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        try
                        {
                                stackInt.CopyTo(arr = new int[2], 3);
                                Assert.Fail ("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        try
                        {
                                stackInt.CopyTo(arr = new int[3], 2);
                                Assert.Fail ("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        try
                        {
                                stackInt.CopyTo(arr = new int[2], 3);
                                Assert.Fail ("Should throw an ArgumentException");
                        } 
                        catch (ArgumentException) {}

                        arr = new int[stackInt.Count];

                        stackInt.CopyTo(arr, 0);

                        int j = 4;

                        for (int i = 0; i < 4; i++)
                        {
                               Assert.AreEqual (arr[i], j--);
                        }
                }

		[Test]
                public void TestSyncronized()
                {
                        Stack syncStack = Stack.Synchronized(stackInt);

                        syncStack.Push(5);

                        for (int i = 5; i >= 0; i--)
                                Assert.AreEqual (syncStack.Pop(), i);
                }

		[Test]
                public void TestPushPeekPop()
                {
                        stackInt.Pop();

                        int topVal = (int)stackInt.Peek();

                        Assert.AreEqual (topVal, 3);

                        Assert.AreEqual (stackInt.Count, 4);

                        Assert.AreEqual (stackInt.Pop(), topVal);

                        Assert.AreEqual (stackInt.Pop(), 2);

                        Stack test = new Stack();
                        test.Push(null);

                        Assert.AreEqual (test.Pop(), null);

                }
		
		[Test]
		public void TestPop()
		{
			for (int i = 4; i >= 0; i--) 
			{
				Assert.AreEqual (stackInt.Pop(), i);
			}
			try {
				stackInt.Pop();
				Assert.Fail ("must throw InvalidOperationException");
			} catch (InvalidOperationException){
			}
		}

		[Test]
                public void TestToArray()
                {
                        object[] arr = stackInt.ToArray();

                        Assert.AreEqual (arr.Length, stackInt.Count);                       

                        for (int i = 0; i < 5; i++)
                                Assert.AreEqual (stackInt.Pop(), arr[i]);
                }
		
		[Test]
		public void TestResize()
		{
			Stack myStack = new Stack(20);

			for (int i = 0; i < 500; i++) 
			{
				myStack.Push(i);
				Assert.AreEqual (i+1, myStack.Count, "push count test");
			}
			
			for (int i = 499; i >= 0; i--) 
			{
				Assert.AreEqual (myStack.Pop(), i);
				Assert.AreEqual (i, myStack.Count, "pop count test");
			}
		}

		[Test]
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
