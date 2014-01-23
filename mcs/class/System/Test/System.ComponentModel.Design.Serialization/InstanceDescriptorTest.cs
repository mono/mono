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
using System.Threading;

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
		public void Constructor0_Arguments_Mismatch ()
		{
			try {
				new InstanceDescriptor (ci, null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Length mismatch
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}
		}

		[Test]
		public void Constructor0_MemberInfo_Type ()
		{
			Type type = typeof (Uri);
			InstanceDescriptor id = new InstanceDescriptor (type,
				new object [] { url });
			Assert.AreEqual (1, id.Arguments.Count, "#1");
			Assert.IsTrue (id.IsComplete, "#2");
			Assert.AreSame (type, id.MemberInfo, "#3");
			Assert.IsNull (id.Invoke (), "#4");
		}

		[Test]
		public void Constructor_Null_ICollection ()
		{
			InstanceDescriptor id = new InstanceDescriptor (null, new object[] { });
			Assert.AreEqual (0, id.Arguments.Count, "#1");
			Assert.IsTrue (id.IsComplete, "#2");
			Assert.IsNull (id.MemberInfo, "#3");
			Assert.IsNull (id.Invoke (), "#4");
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
			Assert.AreEqual (0, id.Arguments.Count, "#1");
			Assert.IsTrue (id.IsComplete, "#2");
			Assert.IsNull (id.MemberInfo, "#3");
			Assert.IsNull (id.Invoke (), "#4");
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

		[Test]
		public void Field_Arguments_Empty ()
		{
			FieldInfo fi = typeof (Uri).GetField ("SchemeDelimiter");

			InstanceDescriptor id = new InstanceDescriptor (fi, new object [0]);
			Assert.AreEqual (0, id.Arguments.Count, "#1");
			Assert.IsTrue (id.IsComplete, "#2");
			Assert.AreSame (fi, id.MemberInfo, "#3");
			Assert.IsNotNull (id.Invoke (), "#4");
		}

		[Test]
		public void Field_Arguments_Mismatch ()
		{
			FieldInfo fi = typeof (Uri).GetField ("SchemeDelimiter");

			try {
				new InstanceDescriptor (fi, new object [] { url });
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Parameter must be static
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Field_Arguments_Null ()
		{
			FieldInfo fi = typeof (Uri).GetField ("SchemeDelimiter");

			InstanceDescriptor id = new InstanceDescriptor (fi, null);
			Assert.AreEqual (0, id.Arguments.Count, "#1");
			Assert.IsTrue (id.IsComplete, "#2");
			Assert.AreSame (fi, id.MemberInfo, "#3");
			Assert.IsNotNull (id.Invoke (), "#4");
		}

		[Test]
		public void Field_MemberInfo_NonStatic ()
		{
			FieldInfo fi = typeof (InstanceField).GetField ("Name");

			try {
				new InstanceDescriptor (fi, null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Parameter must be static
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Property_Arguments_Mismatch ()
		{
#if MOBILE
			// ensure the property is not linked out of the application since it make the test fails
			Assert.NotNull (Thread.CurrentPrincipal, "pre-test");
#endif
			PropertyInfo pi = typeof (Thread).GetProperty ("CurrentPrincipal");

			InstanceDescriptor id = new InstanceDescriptor (pi, new object [] { url });
			Assert.AreEqual (1, id.Arguments.Count, "#1");
			object [] arguments = new object [id.Arguments.Count];
			id.Arguments.CopyTo (arguments, 0);
			Assert.AreSame (url, arguments [0], "#2");
			Assert.IsTrue (id.IsComplete, "#3");
			Assert.AreSame (pi, id.MemberInfo, "#4");
			try {
				id.Invoke ();
				Assert.Fail ("#5");
			} catch (TargetParameterCountException) {
			}
		}

		[Test]
		public void Property_Arguments_Null ()
		{
#if MOBILE
			// ensure the property is not linked out of the application since it make the test fails
			Assert.NotNull (Thread.CurrentPrincipal, "pre-test");
#endif
			PropertyInfo pi = typeof (Thread).GetProperty ("CurrentPrincipal");

			InstanceDescriptor id = new InstanceDescriptor (pi, null);
			Assert.AreEqual (0, id.Arguments.Count, "#1");
			Assert.IsTrue (id.IsComplete, "#2");
			Assert.AreSame (pi, id.MemberInfo, "#3");
			Assert.IsNotNull (id.Invoke (), "#4");
		}

		[Test]
		public void Property_MemberInfo_NonStatic ()
		{
			PropertyInfo pi = typeof (Uri).GetProperty ("Host");

			try {
				new InstanceDescriptor (pi, null);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Parameter must be static
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			try {
				new InstanceDescriptor (pi, null, false);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Parameter must be static
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}
		}

		[Test]
		public void Property_MemberInfo_WriteOnly ()
		{
			PropertyInfo pi = typeof (WriteOnlyProperty).GetProperty ("Name");

			try {
				new InstanceDescriptor (pi, null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Parameter must be readable
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		class WriteOnlyProperty
		{
			public static string Name {
				set {
				}
			}
		}

		class InstanceField
		{
			public string Name;
		}
	}
}
