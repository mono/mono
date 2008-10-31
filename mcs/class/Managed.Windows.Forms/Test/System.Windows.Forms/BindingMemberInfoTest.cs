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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com


using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingMemberInfoTest : TestHelper {

		[Test]
		public void CtorNullTest ()
		{
			BindingMemberInfo bmi = new BindingMemberInfo (null);

			Assert.AreEqual (bmi.BindingMember, String.Empty, "CTORNULL1");
			Assert.AreEqual (bmi.BindingField, String.Empty, "CTORNULL2");
			Assert.AreEqual (bmi.BindingPath, String.Empty, "CTORNULL3");
		}

		[Test]
		public void CtorMemberOnly ()
		{
			BindingMemberInfo bmi = new BindingMemberInfo ("Member");

			Assert.AreEqual (bmi.BindingMember, "Member", "CTORMEMBER1");
			Assert.AreEqual (bmi.BindingField, "Member", "CTORMEMBER2");
			Assert.AreEqual (bmi.BindingPath, String.Empty, "CTORMEMBER3");
		}

		[Test]
		public void CtorMemberAndPathOnly ()
		{
			BindingMemberInfo bmi = new BindingMemberInfo ("Member.Path");

			Assert.AreEqual (bmi.BindingMember, "Member.Path", "CTMAF1");
			Assert.AreEqual (bmi.BindingPath, "Member", "CTMAF2");
			Assert.AreEqual (bmi.BindingField, "Path", "CTMAF3");
		}

		[Test]
		public void CtorAll ()
		{
			BindingMemberInfo bmi = new BindingMemberInfo ("Member.Path.Field");

			Assert.AreEqual (bmi.BindingMember, "Member.Path.Field", "CTALL1");
			Assert.AreEqual (bmi.BindingPath, "Member.Path", "CTALL2");
			Assert.AreEqual (bmi.BindingField, "Field", "CTALL3");
		}

		[Test]
		public void CtorEmpty ()
		{
			BindingMemberInfo bmi = new BindingMemberInfo ("...");

			Assert.AreEqual (bmi.BindingMember, "...", "CTEMPTY1");
			Assert.AreEqual (bmi.BindingPath, "..", "CTEMPTY2");
			Assert.AreEqual (bmi.BindingField, String.Empty, "CTEMPTY3");
		}

		[Test]
		public void CtorSpecialChars ()
		{
			BindingMemberInfo bmi = new BindingMemberInfo (",/';.[]-=!.$%&*~");

			Assert.AreEqual (bmi.BindingMember, ",/';.[]-=!.$%&*~", "CTORSPECIAL1");
			Assert.AreEqual (bmi.BindingPath, ",/';.[]-=!", "CTORSPECIAL2");
			Assert.AreEqual (bmi.BindingField, "$%&*~", "CTORSPECIAL3");
		}

		[Test]
		public void EqualsTest ()
		{
			BindingMemberInfo a = new BindingMemberInfo ("A.B.C");
			BindingMemberInfo b = new BindingMemberInfo ("A.B.C");

			Assert.AreEqual (a, b, "EQUALS1");

			b = new BindingMemberInfo ("A.B");
			Assert.IsFalse (a.Equals (b), "EQUALS2");
		}
	}
}


