// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
// ***********************************************************************

using System;

namespace NUnit.Framework.Constraints.Tests
{
	[TestFixture]
	public class RangeTests
	{
		[Test]
		public void InRangeSucceeds()
		{
			Assert.That( 7, Is.InRange(5, 10) );
			Assert.That(0.23, Is.InRange(-1.0, 1.0));
			Assert.That(DateTime.Parse("12-December-2008"),
				Is.InRange(DateTime.Parse("1-October-2008"), DateTime.Parse("31-December-2008")));
		}

		[Test]
		public void InRangeFails()
		{
			string expectedMessage = string.Format("  Expected: in range (5,10){0}  But was:  12{0}",
                    Env.NewLine);

			Assert.That(
				new TestDelegate( FailingInRangeMethod ),
				Throws.TypeOf(typeof(AssertionException)).With.Message.EqualTo(expectedMessage));
		}

		private void FailingInRangeMethod()
		{
			Assert.That(12, Is.InRange(5, 10));
		}

		[Test]
		public void NotInRangeSucceeds()
		{
			Assert.That(12, Is.Not.InRange(5, 10));
			Assert.That(2.57, Is.Not.InRange(-1.0, 1.0));
		}

		[Test]
		public void NotInRangeFails()
		{
			string expectedMessage = string.Format("  Expected: not in range (5,10){0}  But was:  7{0}",
                    Env.NewLine);

			Assert.That(
				new TestDelegate(FailingNotInRangeMethod),
				Throws.TypeOf(typeof(AssertionException)).With.Message.EqualTo(expectedMessage));
		}

		private void FailingNotInRangeMethod()
		{
			Assert.That(7, Is.Not.InRange(5, 10));
		}
	}
}
