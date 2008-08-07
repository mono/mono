//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class FrameDimensionTest {

		[Test]
		public void Empty ()
		{
			FrameDimension fd = new FrameDimension (Guid.Empty);
			Assert.AreEqual ("00000000-0000-0000-0000-000000000000", fd.Guid.ToString (), "Guid");
			Assert.AreEqual (Guid.Empty.GetHashCode (), fd.GetHashCode (), "GetHashCode");
			Assert.AreEqual ("[FrameDimension: 00000000-0000-0000-0000-000000000000]", fd.ToString (), "ToString");

			Assert.IsTrue (fd.Equals (new FrameDimension (Guid.Empty)), "Equals(Empty)");
			Assert.IsFalse (fd.Equals (null), "Equals(null)");
		}

		[Test]
		public void WellKnownValues ()
		{
			Assert.AreEqual ("7462dc86-6180-4c7e-8e3f-ee7333a7a483", FrameDimension.Page.Guid.ToString (), "Page-Guid");
			Assert.AreEqual ("Page", FrameDimension.Page.ToString (), "Page-ToString");
			Assert.IsTrue (Object.ReferenceEquals (FrameDimension.Page, FrameDimension.Page), "Page-ReferenceEquals");

			Assert.AreEqual ("84236f7b-3bd3-428f-8dab-4ea1439ca315", FrameDimension.Resolution.Guid.ToString (), "Resolution-Guid");
			Assert.AreEqual ("Resolution", FrameDimension.Resolution.ToString (), "Resolution-ToString");
			Assert.IsTrue (Object.ReferenceEquals (FrameDimension.Resolution, FrameDimension.Resolution), "Resolution-ReferenceEquals");

			Assert.AreEqual ("6aedbd6d-3fb5-418a-83a6-7f45229dc872", FrameDimension.Time.Guid.ToString (), "Time-Guid");
			Assert.AreEqual ("Time", FrameDimension.Time.ToString (), "Time-ToString");
			Assert.IsTrue (Object.ReferenceEquals (FrameDimension.Time, FrameDimension.Time), "Page-ReferenceEquals");
		}

		[Test]
		public void Equals ()
		{
			FrameDimension fd = new FrameDimension (new Guid ("7462dc86-6180-4c7e-8e3f-ee7333a7a483"));
			// equals
			Assert.IsTrue (fd.Equals (FrameDimension.Page), "Page");
			// but ToString differs!
			Assert.AreEqual ("[FrameDimension: 7462dc86-6180-4c7e-8e3f-ee7333a7a483]", fd.ToString (), "ToString");
		}
	}
}
