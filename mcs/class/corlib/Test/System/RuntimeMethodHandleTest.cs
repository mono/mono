//
// RuntimeMethodHandleTest.cs - Unit tests for System.RuntimeMethodHandle
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2007 Gert Driesen
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class RuntimeMethodHandleTest
	{
		[Test]
		public void Equals_Object ()
		{
			Type type = typeof (RuntimeMethodHandleTest);
			RuntimeMethodHandle rmhA1 = type.GetMethod ("Equals_Object").MethodHandle;
			RuntimeMethodHandle rmhA2 = type.GetMethod ("Equals_Object").MethodHandle;
			RuntimeMethodHandle rmhB1 = type.GetMethod ("Equals_RuntimeMethodHandle").MethodHandle;
			RuntimeMethodHandle rmhB2 = type.GetMethod ("Equals_RuntimeMethodHandle").MethodHandle;

			Assert.IsTrue (rmhA1.Equals((object) rmhA1), "#1");
			Assert.IsTrue (rmhA1.Equals ((object) rmhA2), "#2");
			Assert.IsTrue (rmhB1.Equals ((object) rmhB1), "#3");
			Assert.IsTrue (rmhB2.Equals ((object) rmhB1), "#4");
			Assert.IsFalse (rmhA1.Equals ((object) rmhB1), "#5");
			Assert.IsFalse (rmhB1.Equals ((object) rmhA1), "#6");
			Assert.IsFalse (rmhA1.Equals (null), "#7");
			Assert.IsFalse (rmhA1.Equals (5), "#8");
		}

		[Test]
		public void Equals_RuntimeMethodHandle ()
		{
			Type type = typeof (RuntimeMethodHandleTest);
			RuntimeMethodHandle rmhA1 = type.GetMethod ("Equals_Object").MethodHandle;
			RuntimeMethodHandle rmhA2 = type.GetMethod ("Equals_Object").MethodHandle;
			RuntimeMethodHandle rmhB1 = type.GetMethod ("Equals_RuntimeMethodHandle").MethodHandle;
			RuntimeMethodHandle rmhB2 = type.GetMethod ("Equals_RuntimeMethodHandle").MethodHandle;

			Assert.IsTrue (rmhA1.Equals (rmhA1), "#1");
			Assert.IsTrue (rmhA1.Equals (rmhA2), "#2");
			Assert.IsTrue (rmhB1.Equals (rmhB1), "#3");
			Assert.IsTrue (rmhB2.Equals (rmhB1), "#4");
			Assert.IsFalse (rmhA1.Equals (rmhB1), "#5");
			Assert.IsFalse (rmhB1.Equals (rmhA1), "#6");
		}

#if NET_2_0
#pragma warning disable 1718
		[Test]
		public void Operators ()
		{
			Type type = typeof (RuntimeMethodHandleTest);
			RuntimeMethodHandle rmhA1 = type.GetMethod ("Operators").MethodHandle;
			RuntimeMethodHandle rmhA2 = type.GetMethod ("Operators").MethodHandle;
			RuntimeMethodHandle rmhB1 = type.GetMethod ("Equals_Object").MethodHandle;
			RuntimeMethodHandle rmhB2 = type.GetMethod ("Equals_Object").MethodHandle;

			// equality
			Assert.IsTrue (rmhA1 == rmhA1, "#A1");
			Assert.IsTrue (rmhA1 == rmhA2, "#A2");
			Assert.IsTrue (rmhB1 == rmhB1, "#A3");
			Assert.IsTrue (rmhB2 == rmhB1, "#A4");
			Assert.IsFalse (rmhA1 == rmhB1, "#A5");
			Assert.IsFalse (rmhB1 == rmhA1, "#A6");

			// inequality
			Assert.IsFalse (rmhA1 != rmhA1, "#B1");
			Assert.IsFalse (rmhA1 != rmhA2, "#B2");
			Assert.IsFalse (rmhB1 != rmhB1, "#B3");
			Assert.IsFalse (rmhB2 != rmhB1, "#B4");
			Assert.IsTrue (rmhA1 != rmhB1, "#B5");
			Assert.IsTrue (rmhB1 != rmhA1, "#B6");
		}
#pragma warning restore 1718
#endif

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void Serialization_Of_Empty_Handle ()
		{
			RuntimeMethodHandle handle = new RuntimeMethodHandle ();
			new BinaryFormatter ().Serialize (Stream.Null, handle);
		}
	}
}
