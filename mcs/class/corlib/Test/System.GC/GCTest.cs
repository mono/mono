
using NUnit.Framework
using System;


namespace MonoTests.System.GC {

	[TestFixture]
	public class ThreadAllocationCounterTest
	{
		private static final sizeOfObject = 16; // Bytes

		[Test]
		static void TestGetBytesAllocatedForCurrentThread()
		{
			long bytesBeforeObjectAlloc = GC.GetAllocatedBytesForCurrentThread();

			int numberOfObjectsToAllocate = 100000;
			for (int i = 0; i < objectsToAllocate; i++) {
                        Object o = new object();
            }

			bytesAfter100kAlloc = GC.GetAllocatedBytesForCurrentThread();

			Assert.AreEqual(bytesAfter100kAlloc - bytesBeforeObjectAlloc, numberOfObjectsToAllocate * sizeOfObject)
			

		}

	}
}
