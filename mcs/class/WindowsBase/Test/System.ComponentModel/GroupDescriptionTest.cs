//
// GroupDescriptionTest.cs
//
// Author:
//       Antonius Riha <antoniusriha@gmail.com>
//
// Copyright (c) 2014 Antonius Riha
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class GroupDescriptionTest
	{
		[Test]
		public void NamesMatch ()
		{
			var gd = new ConcreteGroupDescription ();
			var obj = new object ();
			Assert.IsTrue (gd.NamesMatch (obj, obj), "A1");
			Assert.IsFalse (gd.NamesMatch (new object (), new object ()), "A2");
		}

		[Test]
		public void ShouldSerializeGroupNames ()
		{
			var g = new ConcreteGroupDescription ();
			g.GroupNames.Add ("name");
			Assert.IsTrue (g.ShouldSerializeGroupNames (), "#A1");
		}

		[Test]
		public void ShouldSerializeGroupNamesEmpty ()
		{
			var g = new ConcreteGroupDescription ();
			Assert.IsFalse (g.ShouldSerializeGroupNames (), "#A1");
		}

		class ConcreteGroupDescription : GroupDescription
		{
			public override object GroupNameFromItem (object item, int level, CultureInfo culture)
			{
				throw new NotSupportedException ();
			}
		}
	}
}
