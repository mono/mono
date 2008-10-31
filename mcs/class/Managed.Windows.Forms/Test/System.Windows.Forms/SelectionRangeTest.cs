//
// SelectionRangeTest.cs
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
// Copyright (c) 2008 Andy Hume
//
// Authors:
//   	Andy Hume  <andyhume32@yahoo.co.uk>

using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class SelectionRangeTest : TestHelper
	{

		[Test]
		public void DefaultConstructor ()
		{
			SelectionRange sr = new SelectionRange ();
			Assert.AreEqual (DateTime.MinValue, sr.Start, "Start");
			// "9999-12-31 00:00:00", note not 23:59:59.
			Assert.AreEqual (DateTime.MaxValue.Date, sr.End, "End");

#if NET_2_0
			Assert.AreEqual (DateTimeKind.Unspecified, sr.Start.Kind, "Start Kind");
			Assert.AreEqual (DateTimeKind.Unspecified, sr.End.Kind, "End Kind");
#endif
		}

		[Test]
		public void DefaultConstructor_ToString ()
		{
			SelectionRange sr = new SelectionRange ();
			// "9999-12-31 00:00:00", note not 23:59:59.
			Assert.AreEqual (string.Format ("SelectionRange: Start: {0}, End: {1}", new DateTime (1, 1, 1).ToString (), new DateTime (9999, 12, 31).ToString ()),
			   sr.ToString (), "ToString");
		}

		[Test]
		public void TwoDatesConstructor ()
		{
			SelectionRange sr = new SelectionRange (new DateTime (2001, 1, 11), new DateTime (2008, 2, 17));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
		}

		[Test]
		public void TwoDatesConstructor_Backwards () // start > end
		{
			SelectionRange sr = new SelectionRange (new DateTime (2008, 2, 17), new DateTime (2001, 1, 11));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
		}

		[Test]
		public void TwoDatesConstructor_WithTime ()
		{
			// Apparenly any time value is stripped, found while testing PropertyGrid.
			SelectionRange sr = new SelectionRange (new DateTime (2001, 1, 11, 13, 14, 15), new DateTime (2008, 2, 17));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
		}

		[Test]
		public void TwoDatesConstructor_WithTime2 ()
		{
			// Apparenly any time value is stripped, found while testing PropertyGrid.
			SelectionRange sr = new SelectionRange (new DateTime (2001, 1, 11), new DateTime (2008, 2, 17, 1, 2, 3));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
#if NET_2_0
			Assert.AreEqual (DateTimeKind.Unspecified, sr.Start.Kind, "Start Kind");
			Assert.AreEqual (DateTimeKind.Unspecified, sr.End.Kind, "End Kind");
#endif
		}

#if NET_2_0
		[Test]
		public void TwoDatesConstructor_WithTimeWithKindLocal ()
		{
			// Apparenly any time value is stripped, found while testing PropertyGrid.
			SelectionRange sr = new SelectionRange (new DateTime (2001, 1, 11, 13, 14, 15, DateTimeKind.Local), new DateTime (2008, 2, 17));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
			//
			Assert.AreEqual (DateTimeKind.Local, sr.Start.Kind, "Start Kind");
			Assert.AreEqual (DateTimeKind.Unspecified, sr.End.Kind, "End Kind");
		}

		[Test]
		public void TwoDatesConstructor_WithTime2WithKindUtc ()
		{
			// Apparenly any time value is stripped, found while testing PropertyGrid.
			SelectionRange sr = new SelectionRange (new DateTime (2001, 1, 11), new DateTime (2008, 2, 17, 1, 2, 3, DateTimeKind.Utc));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
			//
			Assert.AreEqual (DateTimeKind.Unspecified, sr.Start.Kind, "Start Kind");
			Assert.AreEqual (DateTimeKind.Utc, sr.End.Kind, "End Kind");
		}

		[Test]
		public void TwoDatesConstructor_WithTwoTimeWithTwoKinds ()
		{
			// Apparenly any time value is stripped, found while testing PropertyGrid.
			SelectionRange sr = new SelectionRange (
			    new DateTime (2001, 1, 11, 1, 2, 3, DateTimeKind.Utc),
			    new DateTime (2008, 2, 17, 1, 2, 3, DateTimeKind.Local));
			Assert.AreEqual (new DateTime (2001, 1, 11), sr.Start, "Start");
			Assert.AreEqual (new DateTime (2008, 2, 17), sr.End, "End");
			//
			Assert.AreEqual (DateTimeKind.Utc, sr.Start.Kind, "Start Kind");
			Assert.AreEqual (DateTimeKind.Local, sr.End.Kind, "End Kind");
		}
#endif

	}
}