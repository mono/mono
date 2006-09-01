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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using NUnit.Framework;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Workflow.Activities;

namespace MonoTests.System.Workflow.ComponentModel
{

	[TestFixture]
	public class ActivityCollectionTest
	{
		[Test]
		public void TestAddRemove ()
		{
			ActivityCollection ac = new ActivityCollection (new ParallelActivity ());
			CodeActivity ca1 = new CodeActivity ();
			CodeActivity ca2 = new CodeActivity ();
			CodeActivity ca3 = new CodeActivity ();
			ac.Add (ca1);
			Assert.AreEqual (1, ac.Count, "C1#1");
			ac.Add (ca2);
			Assert.AreEqual (2, ac.Count, "C1#2");
			ac.Add (ca3);
			Assert.AreEqual (3, ac.Count, "C1#3");
			ac.Remove (ca2);
			Assert.AreEqual (2, ac.Count, "C1#4");
			ac.RemoveAt (0);
			Assert.AreEqual (1, ac.Count, "C1#5");
			Assert.AreEqual (ca3, ac[0], "C1#6");
		}

		[Test]
		public void TestClearContains ()
		{
			ActivityCollection ac = new ActivityCollection (new ParallelActivity ());
			CodeActivity ca1 = new CodeActivity ();
			CodeActivity ca2 = new CodeActivity ();
			CodeActivity ca3 = new CodeActivity ();
			ac.Add (ca1);
			ac.Add (ca2);
			Assert.AreEqual (true, ac.Contains (ca2), "C2#1");
			Assert.AreEqual (false, ac.Contains (ca3), "C2#2");
			Assert.AreEqual (2, ac.Count, "C2#3");
			ac.Clear ();
			Assert.AreEqual (false, ac.Contains (ca3), "C2#4");
			Assert.AreEqual (0, ac.Count, "C2#5");
		}


		// Exceptions

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestConstructorNullException ()
		{
			ActivityCollection ac = new ActivityCollection (null);
		}
	}
}

