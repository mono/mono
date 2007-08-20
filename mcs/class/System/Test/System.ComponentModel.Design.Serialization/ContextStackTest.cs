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

using NUnit.Framework;

using System.ComponentModel.Design.Serialization;

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
			Assert.AreEqual (one, stack[typeof (string)], "#1");
			Assert.AreEqual (one, stack[0], "#2");
			Assert.AreEqual (one, stack.Current, "#3");

			Assert.AreEqual (one, stack.Pop (), "#4");

			Assert.AreEqual (two, stack[typeof (string)], "#5");
			Assert.AreEqual (two, stack[0], "#6");
			Assert.AreEqual (two, stack.Current, "#7");

#if NET_2_0
			string three = "three";
			stack.Append (three);

			Assert.AreEqual (two, stack[typeof (string)], "#8");
			Assert.AreEqual (two, stack[0], "#9");
			Assert.AreEqual (two, stack.Current, "#10");

			Assert.AreEqual (two, stack.Pop (), "#11");

			Assert.AreEqual (three, stack[typeof (string)], "#12");
			Assert.AreEqual (three, stack[0], "#13");
			Assert.AreEqual (three, stack.Current, "#14");
#endif
		}
	}
}
