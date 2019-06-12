
//
// GCTest.cs - NUnit test cases for System.GC
//
// Authors:
// 	Nathan Ricci (naricc@microsoft.com)
//
// Copyright (C) 2019 Microsoft Corporation
//


using NUnit.Framework;
using System;


namespace MonoTests.System.GarbageCollector {

	public interface SizedObject 
	{
		long ExpectedSize();
	}

	public class NoPointer : SizedObject {
		public long ExpectedSize() {
			return 2 * IntPtr.Size;
		}
	}

	public class TwoPointer : SizedObject {
		public object field1;
		public object field2;

		public long ExpectedSize() 
		{
			return 4*IntPtr.Size;
		}

	}

	public class FourPointer : SizedObject {
		public object field1;
		public object field2;
		public object field3;
		public object field4;

		public long ExpectedSize() {
			return 6*IntPtr.Size;
		}
	}

	public class EightPointer : SizedObject {
		public object field1;
		public object field2;
		public object fiedl3;
		public object field4;
		public object field5;
		public object field6;
		public object field7;
		public object field8;

		public long ExpectedSize() {
			return (long)(10*IntPtr.Size);
		}
		
	}

	[TestFixture]
	public class ThreadAllocationCounterTest
	{
		private static SizedObject makeRandomSizedObject;

		[Test]
		public static void TestGetBytesAllocatedForCurrentThread()
		{
			Func<SizedObject>[] objectAllocators = {
				() => new NoPointer(),
				// () => new TwoPointer(),
				// () => new FourPointer(),
				// () => new EightPointer()
			};

			Random r = new  Random();

			long expectedSize = 0;
			long bytesBeforeAlloc = GC.GetAllocatedBytesForCurrentThread();

			for (int i = 0; i < 1000000; i++) {
				expectedSize += objectAllocators[r.Next(0, objectAllocators.Length) ]().ExpectedSize();
			}

			long bytesAfterAlloc = GC.GetAllocatedBytesForCurrentThread();

			Assert.AreEqual(expectedSize, bytesAfterAlloc - bytesBeforeAlloc);
		}

	}
}
