//
// MetaHeader class testing unit
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
	public class MetaHeaderTest {

		[Test]
		public void DefaultValues ()
		{
			MetaHeader mh = new MetaHeader ();
			Assert.AreEqual (0, mh.HeaderSize, "HeaderSize");
			Assert.AreEqual (0, mh.MaxRecord, "MaxRecord");
			Assert.AreEqual (0, mh.NoObjects, "NoObjects");
			Assert.AreEqual (0, mh.NoParameters, "NoParameters");
			Assert.AreEqual (0, mh.Size, "Size");
			Assert.AreEqual (0, mh.Type, "Type");
			Assert.AreEqual (0, mh.Version, "Version");
		}

		[Test]
		public void Min ()
		{
			MetaHeader mh = new MetaHeader ();
			mh.HeaderSize = short.MinValue;
			Assert.AreEqual (short.MinValue, mh.HeaderSize, "HeaderSize");
			mh.MaxRecord = int.MinValue;
			Assert.AreEqual (int.MinValue, mh.MaxRecord, "MaxRecord");
			mh.NoObjects = short.MinValue;
			Assert.AreEqual (short.MinValue, mh.NoObjects, "NoObjects");
			mh.NoParameters = short.MinValue;
			Assert.AreEqual (short.MinValue, mh.NoParameters, "NoParameters");
			mh.Size = int.MinValue;
			Assert.AreEqual (int.MinValue, mh.Size, "Size");
			mh.Type = short.MinValue;
			Assert.AreEqual (short.MinValue, mh.Type, "Type");
			mh.Version = short.MinValue;
			Assert.AreEqual (short.MinValue, mh.Version, "Version");
		}

		[Test]
		public void Max ()
		{
			MetaHeader mh = new MetaHeader ();
			mh.HeaderSize = short.MaxValue;
			Assert.AreEqual (short.MaxValue, mh.HeaderSize, "HeaderSize");
			mh.MaxRecord = int.MaxValue;
			Assert.AreEqual (int.MaxValue, mh.MaxRecord, "MaxRecord");
			mh.NoObjects = short.MaxValue;
			Assert.AreEqual (short.MaxValue, mh.NoObjects, "NoObjects");
			mh.NoParameters = short.MaxValue;
			Assert.AreEqual (short.MaxValue, mh.NoParameters, "NoParameters");
			mh.Size = int.MaxValue;
			Assert.AreEqual (int.MaxValue, mh.Size, "Size");
			mh.Type = short.MaxValue;
			Assert.AreEqual (short.MaxValue, mh.Type, "Type");
			mh.Version = short.MaxValue;
			Assert.AreEqual (short.MaxValue, mh.Version, "Version");
		}
	}
}
