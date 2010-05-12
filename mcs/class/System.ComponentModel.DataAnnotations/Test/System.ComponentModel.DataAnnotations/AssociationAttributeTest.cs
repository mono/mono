//
// AssociationAttributeTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[TestFixture]
	public class AssociationAttributeTest
	{
		[Test]
		public void Constructor ()
		{
			AssociationAttribute attr;

			attr = new AssociationAttribute (null, "key1,key2", "key3,key4");
			Assert.AreEqual (null, attr.Name, "#A1-1");
			Assert.AreEqual ("key1,key2", attr.ThisKey, "#A1-2");
			Assert.AreEqual ("key3,key4", attr.OtherKey, "#A1-3");
			Assert.IsNotNull (attr.OtherKeyMembers, "#A2-1");

			int count = 0;
			var list = new List<string> ();
			foreach (string m in attr.OtherKeyMembers) {
				count++;
				list.Add (m);
			}
			Assert.AreEqual (2, count, "#A2-2");
			Assert.AreEqual ("key3", list [0], "#A2-3");
			Assert.AreEqual ("key4", list [1], "#A2-4");

			Assert.IsNotNull (attr.ThisKeyMembers, "#A3-1");

			count = 0;
			list = new List<string> ();
			foreach (string m in attr.ThisKeyMembers) {
				count++;
				list.Add (m);
			}
			Assert.AreEqual (2, count, "#A3-2");
			Assert.AreEqual ("key1", list [0], "#A3-3");
			Assert.AreEqual ("key2", list [1], "#A3-4");

			attr = new AssociationAttribute ("name", null, "key3,key4");
			Assert.AreEqual ("name", attr.Name, "#B1-1");
			Assert.AreEqual (null, attr.ThisKey, "#B1-2");
			Assert.AreEqual ("key3,key4", attr.OtherKey, "#B1-3");
			Assert.IsNotNull (attr.OtherKeyMembers, "#B2-1");

			count = 0;
			list = new List<string> ();
			foreach (string m in attr.OtherKeyMembers) {
				count++;
				list.Add (m);
			}
			Assert.AreEqual (2, count, "#B2-2");
			Assert.AreEqual ("key3", list [0], "#B2-3");
			Assert.AreEqual ("key4", list [1], "#B2-4");

			// this is just sad...
			try {
				var m = attr.ThisKeyMembers;
				Assert.Fail ("#B2-5");
			} catch (NullReferenceException) {
				// success
			}

			attr = new AssociationAttribute ("name", " key1  ,   key 2  ,, ,key    3  ", "       ");
			Assert.IsNotNull (attr.ThisKeyMembers, "#C1");

			count = 0;
			list = new List<string> ();
			foreach (string m in attr.ThisKeyMembers) {
				count++;
				list.Add (m);
			}

			// It seems all the whitespace is removed from key names
			Assert.AreEqual (5, count, "#C2-1");
			Assert.AreEqual ("key1", list [0], "#C2-2");
			Assert.AreEqual ("key2", list [1], "#C2-3");
			Assert.AreEqual (String.Empty, list [2], "#C2-4");
			Assert.AreEqual (String.Empty, list [3], "#C2-5");
			Assert.AreEqual ("key3", list [4], "#C2-6");

			Assert.IsNotNull (attr.OtherKeyMembers, "#C3");
			count = 0;
			list = new List<string> ();
			foreach (string m in attr.OtherKeyMembers) {
				count++;
				list.Add (m);
			}
			Assert.AreEqual (1, count, "#C4-1");
			Assert.AreEqual (String.Empty, list [0], "#C4-2");
		}
	}
#endif
}
