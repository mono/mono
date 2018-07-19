//
// WeakReferenceTest.cs - NUnit Test Cases for WeakReference
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System {

	[TestFixture]
	public class WeakReferenceTest {

		[Test]
		public void WeakReference_Object_Null ()
		{
			WeakReference wr = new WeakReference (null);
			Assert.IsFalse (wr.IsAlive, "IsAlive");
			Assert.IsNull (wr.Target, "Target");
			Assert.IsFalse (wr.TrackResurrection, "TrackResurrection");
		}

		[Test]
		public void WeakReference_Object_Null_TrackResurrection_True ()
		{
			WeakReference wr = new WeakReference (null, true);
			Assert.IsFalse (wr.IsAlive, "IsAlive");
			Assert.IsNull (wr.Target, "Target");
			Assert.IsTrue (wr.TrackResurrection, "TrackResurrection");
		}

		[Test]
		public void WeakReference_Object_Null_TrackResurrection_False ()
		{
			WeakReference wr = new WeakReference (null, false);
			Assert.IsFalse (wr.IsAlive, "IsAlive");
			Assert.IsNull (wr.Target, "Target");
			Assert.IsFalse (wr.TrackResurrection, "TrackResurrection");
		}

		[Test]
		public void WeakReference_Object ()
		{
			using (Stream s = Stream.Null) {
				WeakReference wr = new WeakReference (s);
				Assert.IsTrue (wr.IsAlive, "IsAlive");
				Assert.AreSame (s, wr.Target, "Target");
				Assert.IsFalse (wr.TrackResurrection, "TrackResurrection");
			}
		}

		[Test]
		public void WeakReference_Object_TrackResurrection_True ()
		{
			using (Stream s = Stream.Null) {
				WeakReference wr = new WeakReference (s, true);
				Assert.IsTrue (wr.IsAlive, "IsAlive");
				Assert.AreSame (s, wr.Target, "Target");
				Assert.IsTrue (wr.TrackResurrection, "TrackResurrection");
			}
		}

		[Test]
		public void WeakReference_Object_TrackResurrection_False ()
		{
			using (Stream s = Stream.Null) {
				WeakReference wr = new WeakReference (s, false);
				Assert.IsTrue (wr.IsAlive, "IsAlive");
				Assert.AreSame (s, wr.Target, "Target");
				Assert.IsFalse (wr.TrackResurrection, "TrackResurrection");
			}
		}

		class Foo {
			WeakReference wr;

			public static bool failed;

			public Foo () {
				wr = new WeakReference (new object ());
			}

			~Foo () {
				try {
					var b = wr.IsAlive;
				} catch (Exception) {
					failed = true;
				}
			}
		}

		[Test]
		public void WeakReference_IsAlive_Finalized ()
		{
			var f = new Foo ();
			f = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			Assert.IsFalse (Foo.failed);
		}

		[Test]
		public void WeakReferenceT_TryGetTarget_NullTarget ()
		{
			var r = new WeakReference <object> (null);
			object obj;
			Assert.IsFalse (r.TryGetTarget (out obj), "#1");
		}
	}
}

