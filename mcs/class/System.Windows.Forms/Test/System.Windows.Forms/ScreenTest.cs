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
// Copyright (c) 2012 SIL International (http://sil.org)
//
// Authors:
//	Stephen McConnel (stephen_mcconnel@sil.org)
//

using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ScreenTest
	{
		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			// If there is only one screen, then FromRectangle always returns that screen,
			// so this test would not test anything (and would fail on the second Assert
			// below).
			int screenCount = Screen.AllScreens.Length;
			if (screenCount == 1)
				Assert.Ignore ("These tests require at least 2 screens");
		}

		static Rectangle GetLowestScreenBounds ()
		{
			Rectangle lowestScreenBounds = new Rectangle (int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
			foreach (Screen scrn in Screen.AllScreens) {
				if (scrn.Bounds.X < lowestScreenBounds.X || scrn.Bounds.Y < lowestScreenBounds.Y)
					lowestScreenBounds = scrn.Bounds;
			}
			return lowestScreenBounds;
		}

		[Test]
		public void FromRectangleTest_ContainedWithinLowestScreen ()
		{
			var lowestScreenBounds = GetLowestScreenBounds ();

			// If a rectangle is contained within the lowest screen, then the lowest screen
			// should be found for that rectangle.
			Rectangle testRect1 = new Rectangle (lowestScreenBounds.X + lowestScreenBounds.Width / 4,
				lowestScreenBounds.Width / 2,
				lowestScreenBounds.Y + lowestScreenBounds.Height / 4,
				lowestScreenBounds.Height / 2);
			Screen scrn1 = Screen.FromRectangle (testRect1);
			Assert.AreEqual (lowestScreenBounds, scrn1.Bounds,
				"Wrong screen was found for rectangle contained in the first screen");
		}

		[Test]
		public void FromRectangleTest_SlightOverlapWithLowestScreen ()
		{
			var lowestScreenBounds = GetLowestScreenBounds ();

			// If a rectangle overlaps only slightly within the lowest screen, then the lowest screen
			// should not be found for that rectangle.  (This is where the original implementation
			// fails.)
			Rectangle testRect2 = new Rectangle (lowestScreenBounds.X + lowestScreenBounds.Width - 15,
				lowestScreenBounds.Width / 2,
				lowestScreenBounds.Y + lowestScreenBounds.Height - 15,
				lowestScreenBounds.Height / 2);
			Screen scrn2 = Screen.FromRectangle (testRect2);
			Assert.AreNotEqual (lowestScreenBounds, scrn2.Bounds,
				"Wrong screen was found for rectangle slightly overlapping the first screen");
		}

		[Test]
		public void FromRectangleTest_MostlyOverlapWithLowestScreen ()
		{
			var lowestScreenBounds = GetLowestScreenBounds ();

			// If a rectangle overlaps mostly within the lowest screen, then the lowest screen
			// should be found for that rectangle.
			Rectangle testRect3 = new Rectangle (lowestScreenBounds.X + (lowestScreenBounds.Width / 2) + 15,
				lowestScreenBounds.Width / 2,
				lowestScreenBounds.Y + (lowestScreenBounds.Height / 2) + 15,
				lowestScreenBounds.Height / 2);
			Screen scrn3 = Screen.FromRectangle (testRect3);
			Assert.AreEqual (lowestScreenBounds, scrn3.Bounds,
				"Wrong screen was found for rectangle mostly overlapping the first screen");
		}
	}
}
