//
// ContextStackTest.cs - Unit tests for 
//	System.ComponentModel.Design.Serialization.ContextStack
//
// Author:
//	Ivan N. Zlatev <contact@i-nz.net>
//
// Copyright (C) 2007 Ivan N. Zlatev <contact@i-nz.net>
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
using System.ComponentModel.Design.Serialization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.Design.Serialization
{
	[TestFixture]
	public class ContextStackTest
	{
		[Test]
		public void IntegrityTest ()
		{
			ContextStack stack = new ContextStack ();

			string one = "one";
			string two = "two";
			stack.Push (two);
			stack.Push (one);
			Assert.AreSame (one, stack [typeof (string)], "#1");
			Assert.AreSame (one, stack [0], "#2");
			Assert.AreSame (one, stack.Current, "#3");

			Assert.AreSame (one, stack.Pop (), "#4");

			Assert.AreSame (two, stack [typeof (string)], "#5");
			Assert.AreSame (two, stack [0], "#6");
			Assert.AreSame (two, stack.Current, "#7");

#if NET_2_0
			string three = "three";
			stack.Append (three);

			Assert.AreSame (two, stack[typeof (string)], "#8");
			Assert.AreSame (two, stack[0], "#9");
			Assert.AreSame (two, stack.Current, "#10");

			Assert.AreSame (two, stack.Pop (), "#11");

			Assert.AreSame (three, stack[typeof (string)], "#12");
			Assert.AreSame (three, stack[0], "#13");
			Assert.AreSame (three, stack.Current, "#14");
			Assert.AreSame (three, stack.Pop (), "#15");
#else
			Assert.AreSame (two, stack.Pop (), "#15");
#endif

			Assert.IsNull (stack.Pop (), "#16");
			Assert.IsNull (stack.Current, "#17");
		}

#if NET_2_0
		[Test]
		public void Append_Context_Null ()
		{
			ContextStack stack = new ContextStack ();
			try {
				stack.Append (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("context", ex.ParamName, "#5");
			}
		}
#endif

		[Test] // Item (Int32)
		public void Indexer1 ()
		{
			ContextStack stack = new ContextStack ();
			string one = "one";
			string two = "two";

			stack.Push (one);
			stack.Push (two);

			Assert.AreSame (two, stack [0], "#1");
			Assert.AreSame (one, stack [1], "#2");
			Assert.IsNull (stack [2], "#3");
			Assert.AreSame (two, stack.Pop (), "#4");
			Assert.AreSame (one, stack [0], "#5");
			Assert.IsNull (stack [1], "#6");
			Assert.AreSame (one, stack.Pop (), "#7");
			Assert.IsNull (stack [0], "#8");
			Assert.IsNull (stack [1], "#9");
		}

		[Test] // Item (Int32)
		public void Indexer1_Level_Negative ()
		{
			ContextStack stack = new ContextStack ();
			stack.Push (new Foo ());

			try {
				object context = stack [-1];
				Assert.Fail ("#A1:" + context);
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.AreEqual (new ArgumentOutOfRangeException ("level").Message, ex.Message, "#A4");
				Assert.AreEqual ("level", ex.ParamName, "#A5");
			}

			try {
				object context = stack [-5];
				Assert.Fail ("#B1:" + context);
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.AreEqual (new ArgumentOutOfRangeException ("level").Message, ex.Message, "#B4");
				Assert.AreEqual ("level", ex.ParamName, "#B5");
			}
		}

		[Test] // Item (Type)
		public void Indexer2 ()
		{
			ContextStack stack = new ContextStack ();

			Foo foo = new Foo ();
			FooBar foobar = new FooBar ();

			stack.Push (foobar);
			stack.Push (foo);
			Assert.AreSame (foo, stack [typeof (Foo)], "#1");
			Assert.AreSame (foo, stack [typeof (IFoo)], "#2");
			Assert.AreSame (foo, stack.Pop (), "#3");
			Assert.AreSame (foobar, stack [typeof (Foo)], "#4");
			Assert.AreSame (foobar, stack [typeof (FooBar)], "#5");
			Assert.AreSame (foobar, stack [typeof (IFoo)], "#6");
			Assert.IsNull (stack [typeof (string)], "#7");
		}

		[Test] // Item (Type)
		public void Indexer2_Type_Null ()
		{
			ContextStack stack = new ContextStack ();
			stack.Push (new Foo ());
			try {
				object context = stack [(Type) null];
				Assert.Fail ("#1:" + context);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("type", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Push_Context_Null ()
		{
			ContextStack stack = new ContextStack ();
			try {
				stack.Push (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("context", ex.ParamName, "#5");
			}
		}

		public interface IFoo
		{
		}

		public class Foo : IFoo
		{
		}

		public class FooBar : Foo
		{
		}
	}
}

#endif