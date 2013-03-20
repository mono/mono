//
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
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
using System.ComponentModel;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class DataObjectMethodAttributeTests
	{
		[Test]
		public void Ctor () {
			DataObjectMethodAttribute attr = new DataObjectMethodAttribute (DataObjectMethodType.Fill);
			Assert.IsFalse (attr.IsDefault);
		}

		[Test]
		public void MatchTest () {
			DataObjectMethodAttribute a1 = new DataObjectMethodAttribute (DataObjectMethodType.Fill);
			DataObjectMethodAttribute a2 = new DataObjectMethodAttribute (DataObjectMethodType.Delete, true);

			Assert.IsFalse (a1.Match (a2), "#1");

			DataObjectMethodAttribute a3 = new DataObjectMethodAttribute (DataObjectMethodType.Delete);

			Assert.IsTrue (a2.Match (a3), "#2");
		}

		[Test]
		public void EqualsTest () {
			DataObjectMethodAttribute a1 = new DataObjectMethodAttribute (DataObjectMethodType.Fill);
			DataObjectMethodAttribute a2 = new DataObjectMethodAttribute (DataObjectMethodType.Delete);

			Assert.IsFalse (a1.Equals (a2), "#1");

			DataObjectMethodAttribute a3 = new DataObjectMethodAttribute (DataObjectMethodType.Delete);

			Assert.IsTrue (a2.Equals (a3), "#2");
		}
	}
}

#endif
