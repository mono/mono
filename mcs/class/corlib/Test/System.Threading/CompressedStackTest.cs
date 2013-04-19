//
// CompressedStackTest.cs - NUnit Test Cases for CompressedStack
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOBILE

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading {

	// NOTES
	// The tests will fails on 2.0 beta 1 (and a few CTP afterwards) because it relies
	// on a LinkDemand for ECMA key (and identity permissions now support unrestricted).

	[TestFixture]
	public class CompressedStackTest {

		static bool success;

		static void Callback (object o)
		{
			success = (bool) o;
		}

		[SetUp]
		public void SetUp ()
		{
			success = false;
			Thread.CurrentThread.SetCompressedStack (null);
		}

		[Test]
		public void Capture ()
		{
			CompressedStack cs1 = CompressedStack.Capture ();
			Assert.IsNotNull (cs1, "Capture-1");

			CompressedStack cs2 = CompressedStack.Capture ();
			Assert.IsNotNull (cs2, "Capture-2");

			Assert.IsFalse (cs1.Equals (cs2), "cs1.Equals (cs2)");
			Assert.IsFalse (cs2.Equals (cs1), "cs2.Equals (cs1)");
			Assert.IsFalse (cs1.GetHashCode () == cs2.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void CreateCopy ()
		{
			CompressedStack cs1 = CompressedStack.Capture ();
			CompressedStack cs2 = cs1.CreateCopy ();
			Assert.IsFalse (cs1.Equals (cs2), "cs1.Equals (cs2)");
			Assert.IsFalse (cs2.Equals (cs1), "cs2.Equals (cs1)");
			Assert.IsFalse (cs1.GetHashCode () == cs2.GetHashCode (), "GetHashCode");
			Assert.IsFalse (Object.ReferenceEquals (cs1, cs2), "ReferenceEquals");
		}

		[Test]
		public void GetCompressedStack ()
		{
			CompressedStack cs1 = CompressedStack.GetCompressedStack ();
			Assert.IsNotNull (cs1, "GetCompressedStack");

			CompressedStack cs2 = CompressedStack.Capture ();
			Assert.IsNotNull (cs2, "Capture");

			Assert.IsFalse (cs1.Equals (cs2), "cs1.Equals (cs2)");
			Assert.IsFalse (cs2.Equals (cs1), "cs2.Equals (cs1)");
			Assert.IsFalse (cs1.GetHashCode () == cs2.GetHashCode (), "GetHashCode");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_Null ()
		{
			StreamingContext sc = new StreamingContext ();
			CompressedStack cs = CompressedStack.Capture ();
			cs.GetObjectData (null, sc);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Run_Null ()
		{
			CompressedStack.Run (null, new ContextCallback (Callback), true);
		}

		[Test]
		public void Run_Capture ()
		{
			Assert.IsFalse (success, "pre-check");
			CompressedStack.Run (CompressedStack.Capture (), new ContextCallback (Callback), true);
			Assert.IsTrue (success, "post-check");
		}

		[Test]
		public void Run_GetCompressedStack ()
		{
			Assert.IsFalse (success, "pre-check");
			CompressedStack.Run (CompressedStack.GetCompressedStack (), new ContextCallback (Callback), true);
			Assert.IsTrue (success, "post-check");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Run_Thread ()
		{
			// this is because Thread.CurrentThread.GetCompressedStack () returns null for an empty
			// compressed stack while CompressedStack.GetCompressedStack () return "something" empty ;-)
			CompressedStack.Run (Thread.CurrentThread.GetCompressedStack (), new ContextCallback (Callback), true);
		}
	}
}

#endif
