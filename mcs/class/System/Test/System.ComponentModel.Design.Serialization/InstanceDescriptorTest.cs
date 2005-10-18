//
// InstanceDescriptorTest.cs - Unit tests for 
//	System.ComponentModel.Design.Serialization.InstanceDescriptor
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace MonoTests.System.ComponentModel.Design.Serialization {

	[TestFixture]
	public class InstanceDescriptorTest {

		private const string url = "http://www.mono-project.com/";
		private ConstructorInfo ci;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			ci = typeof (Uri).GetConstructor (new Type[1] { typeof (string) });
		}

		[Test]
		public void Constructor_Null_ICollection ()
		{
			InstanceDescriptor id = new InstanceDescriptor (null, new object[] { });
			Assert.AreEqual (0, id.Arguments.Count, "Arguments");
			Assert.IsTrue (id.IsComplete, "IsComplete");
			Assert.IsNull (id.MemberInfo, "MemberInfo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_MemberInfo_Null ()
		{
			new InstanceDescriptor (ci, null);
			// mismatch for required parameters
		}

		[Test]
		public void Constructor_MemberInfo_ICollection ()
		{
			InstanceDescriptor id = new InstanceDescriptor (ci, new object[] { url });
			Assert.AreEqual (1, id.Arguments.Count, "Arguments");
			Assert.IsTrue (id.IsComplete, "IsComplete");
			Assert.AreSame (ci, id.MemberInfo, "MemberInfo");
			Uri uri = (Uri) id.Invoke ();
			Assert.AreEqual (url, uri.AbsoluteUri, "Invoke");
		}

		[Test]
		public void Constructor_Null_ICollection_Boolean ()
		{
			InstanceDescriptor id = new InstanceDescriptor (null, new object[] { }, true);
			Assert.AreEqual (0, id.Arguments.Count, "Arguments");
			Assert.IsTrue (id.IsComplete, "IsComplete");
			Assert.IsNull (id.MemberInfo, "MemberInfo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_MemberInfo_Null_Boolean ()
		{
			new InstanceDescriptor (ci, null, false);
			// mismatch for required parameters
		}

		[Test]
		public void Constructor_MemberInfo_ICollection_Boolean ()
		{
			InstanceDescriptor id = new InstanceDescriptor (ci, new object[] { url }, false);
			Assert.AreEqual (1, id.Arguments.Count, "Arguments");
			Assert.IsFalse (id.IsComplete, "IsComplete");
			Assert.AreSame (ci, id.MemberInfo, "MemberInfo");
			Uri uri = (Uri) id.Invoke ();
			Assert.AreEqual (url, uri.AbsoluteUri, "Invoke");
		}
	}
}
