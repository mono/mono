
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


namespace MonoTests.System.GC {

	public interface SizedObject 
	{
		int ExpectedSize();
	}

	public class NoPointer : SizedObject {
		public int ExpectedSize() {
			return 2 * IntPtr.Size;
		}
	}

	public class TwoPointer : SizedObject {
		public object field1;
		public object field2;

		public int ExpectedSize() 
		{
			return 4*IntPtr.Size;
		}

	}

	public class FourPointer : SizedObject {
		public object field1;
		public object field2;
		public object field3;
		public object field4;

		public int ExpectedSize() {
			return 6*IntPtr.Size();
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

		public int ExpectedSize() {
			10*IntPtr.Size();
		}
		
	}

	[TestFixture]
	public class ThreadAllocationCounterTest
	{
		private static SizedObject makeRandomSizedObject;

		[Test]
		public static void TestGetBytesAllocatedForCurrentThread()
		{
			Action<SizedObject>[] objectAllocators = {
				() => new TwoPointerObject(),
				() => new FourPointerObject(),
				() => new EightPointerObject()
			};

			Random r = new  Random();

			long expectedSize = 0;
			long bytesBeforeAlloc = GC.GetAllocatedBytesForCurrentThread();

			for (int i = 0; i < 10000000; i++) {
				expectedSize += objectAllocators[r.next() %  objectAlloctors.Size ]().ExpectedSize();
			}

			bytesAfterAlloc = GC.GetAllocatedBytesForCurrentThread();

			Assert.AreEqual(bytesAfterAlloc - bytesBeforeObjectAlloc, expectedSize);
		}

	}
}
