//
// System.Runtime.InteropServices.GCHandle Test Cases
//
// Authors:
// 	Paolo Molaro (lupus@ximian.com)
//
// (c) 2005 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class GCHandleTest : Assertion
	{
		static GCHandle handle;

		[Test]
		public void DefaultZeroValue ()
		{
			AssertEquals (false, handle.IsAllocated);
		}

		[Test]
		public void AllocNull ()
		{
			IntPtr ptr = (IntPtr)GCHandle.Alloc(null);
			GCHandle gch = (GCHandle)ptr;
		}

	}
}

