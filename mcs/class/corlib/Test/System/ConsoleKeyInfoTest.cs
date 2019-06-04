//
// ConsoleKeyInfoTest.cs
//
// Authors:
//  Alexander Köplinger (alkpli@microsoft.com)
//
// (c) 2017 Microsoft
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
using System.IO;
using System.Text;

namespace MonoTests.System
{
	[TestFixture]
	public class ConsoleKeyInfoTest
	{
		[Test]
		public void CtorTest()
		{
			var cki = new ConsoleKeyInfo ('A', ConsoleKey.A, true, false, true);

			Assert.AreEqual ('A', cki.KeyChar, "#1");
			Assert.AreEqual (ConsoleKey.A, cki.Key, "#2");
			Assert.AreEqual ((ConsoleModifiers.Shift | ConsoleModifiers.Control) , cki.Modifiers, "#3");
		}

		[Test]
		public void EqualTest()
		{
			var ckiA = new ConsoleKeyInfo ('a', ConsoleKey.A, false, false, false);
			var ckiB = new ConsoleKeyInfo ('b', ConsoleKey.B, false, false, false);
			var ckiA2 = new ConsoleKeyInfo ('a', ConsoleKey.A, false, false, false);

			Assert.IsFalse (ckiA == ckiB, "#1");
			Assert.IsTrue (ckiA != ckiB, "#2");
			Assert.IsFalse (ckiA.Equals (ckiB), "#3");

			Assert.IsTrue (ckiA == ckiA2, "#4");
			Assert.IsFalse (ckiA != ckiA2, "#5");
			Assert.IsTrue (ckiA.Equals (ckiA2), "#6");
		}
	}
}
