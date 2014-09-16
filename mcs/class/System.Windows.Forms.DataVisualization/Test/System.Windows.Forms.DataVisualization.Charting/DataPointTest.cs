//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com) 
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

using System;
using System.Windows.Forms.DataVisualization.Charting;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.DataVisualization.Charting
{
	[TestFixture]
	public class DataPointTest
	{
		[Test]
		public void Constructor1 ()
		{
			DataPoint dp = new DataPoint ();

			Assert.AreEqual (false, dp.IsEmpty, "A1");
			Assert.AreEqual ("DataPoint", dp.Name, "A2");
			Assert.AreEqual (0, dp.XValue, "A3");
			Assert.AreEqual (new double[] { 0.0d }, dp.YValues, "A4");
		}

		[Test]
		public void Constructor2 ()
		{
			DataPoint dp = new DataPoint (1d, 2d);

			Assert.AreEqual (false, dp.IsEmpty, "A1");
			Assert.AreEqual ("DataPoint", dp.Name, "A2");
			Assert.AreEqual (1d, dp.XValue, "A3");
			Assert.AreEqual (new double[] { 2d }, dp.YValues, "A4");
		}

		[Test]
		public void Constructor3 ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (false, dp.IsEmpty, "A1");
			Assert.AreEqual ("DataPoint", dp.Name, "A2");
			Assert.AreEqual (1d, dp.XValue, "A3");
			Assert.AreEqual (new double[] { 2d, 3d }, dp.YValues, "A4");
		}

		[Test]
		public void IsEmptyProperty ()
		{
			DataPoint dp = new DataPoint (1d, 2d);

			Assert.AreEqual (false, dp.IsEmpty, "A1");
			Assert.AreEqual (1d, dp.XValue, "A2");
			Assert.AreEqual (new double[] { 2d }, dp.YValues, "A3");

			dp.IsEmpty = true;

			Assert.AreEqual (true, dp.IsEmpty, "A4");
			Assert.AreEqual (1d, dp.XValue, "A5");
			Assert.AreEqual (new double[] { 2d }, dp.YValues, "A6");

			dp.XValue = 6d;
			dp.YValues = new double[] { 7d };

			Assert.AreEqual (true, dp.IsEmpty, "A7");
		}

		[Test]
		public void NameProperty ()
		{
			DataPoint dp = new DataPoint (1d, 2d);
			Assert.AreEqual ("DataPoint", dp.Name, "A1");

			dp.Name = "Point";
			Assert.AreEqual ("DataPoint", dp.Name, "A2");
		}

		[Test]
		public void XValueProperty ()
		{
			DataPoint dp = new DataPoint (1d, 2d);
			Assert.AreEqual (1d, dp.XValue, "A1");

			dp.XValue = 2d;
			Assert.AreEqual (2d, dp.XValue, "A2");
		}

		[Test]
		public void YValueProperty ()
		{
			DataPoint dp = new DataPoint (1d, 2d);
			Assert.AreEqual (new double[] { 2d }, dp.YValues, "A1");

			dp.YValues = new double[] { 2d, 3d };
			Assert.AreEqual (new double[] { 2d, 3d }, dp.YValues, "A2");
		}

		[Test]
		public void CloneMethod ()
		{
			DataPoint dp = new DataPoint ();

			Assert.AreEqual (false, dp.IsEmpty, "A1");
			Assert.AreEqual ("DataPoint", dp.Name, "A2");
			Assert.AreEqual (0, dp.XValue, "A3");
			Assert.AreEqual (new double[] { 0.0d }, dp.YValues, "A4");

			DataPoint dp2 = (DataPoint)dp.Clone ();
			Assert.AreEqual (false, dp2.IsEmpty, "A5");
			Assert.AreEqual ("DataPoint", dp2.Name, "A6");
			Assert.AreEqual (0, dp2.XValue, "A7");
			Assert.AreEqual (new double[] { 0.0d }, dp2.YValues, "A8");
		}

		[Test]
		public void GetValueByNameMethod ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (1d, dp.GetValueByName ("X"), "A1");
			Assert.AreEqual (2d, dp.GetValueByName ("Y"), "A2");
			Assert.AreEqual (2d, dp.GetValueByName ("Y1"), "A3");
			Assert.AreEqual (3d, dp.GetValueByName ("Y2"), "A4");

			Assert.AreEqual (1d, dp.GetValueByName ("x"), "A5");
			Assert.AreEqual (2d, dp.GetValueByName ("y"), "A6");
			Assert.AreEqual (2d, dp.GetValueByName ("y1"), "A7");
			Assert.AreEqual (3d, dp.GetValueByName ("y2"), "A8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetValueByNameMethodAE ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (1d, dp.GetValueByName ("X1"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetValueByNameMethodAE2 ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (1d, dp.GetValueByName ("Y4"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetValueByNameMethodANE ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (1d, dp.GetValueByName (null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetValueByNameMethodAE3 ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (1d, dp.GetValueByName (string.Empty), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetValueByNameMethodAE4 ()
		{
			DataPoint dp = new DataPoint (1d, new double[] { 2d, 3d });

			Assert.AreEqual (1d, dp.GetValueByName ("Y0"), "A1");
		}
	}
}
