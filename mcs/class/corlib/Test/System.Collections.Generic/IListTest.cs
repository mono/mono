//
// MonoTests.System.Collections.Generic.Test.IListTest
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
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

#if NET_2_0

using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic
{
	[TestFixture]
	public class IListTest
	{
		[Test] // bug #80260
		public void CastToArray ()
		{
			IList<string> list = new string[] { "foo", "bar" };
			string[] array = (string[]) list;
			Assert.IsNotNull (array, "#1");
			Assert.AreEqual (2, array.Length, "#2");
			Assert.AreEqual ("foo", array [0], "#3");
			Assert.AreEqual ("bar", array [1], "#4");
		}
	}
}
#endif

