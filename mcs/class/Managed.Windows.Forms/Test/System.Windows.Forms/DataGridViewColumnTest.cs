//
// DataGridViewColumnTest.cs - Unit tests for 
// System.Windows.Forms.DataGridViewColumn
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

#if NET_2_0

using System;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewColumnTest
	{
		[Test] // bug #80746
		[Category ("NotWorking")]
		public void HeaderText ()
		{
			DataGridViewColumn dvc = new DataGridViewColumn ();
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#A1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#A2");
			dvc.Name = "A";
			dvc.HeaderText = "B";
			Assert.AreEqual ("B", dvc.HeaderText, "#B1");
			Assert.AreEqual ("B", dvc.HeaderCell.Value, "#B2");
			Assert.AreEqual ("A", dvc.Name, "#B3");
			dvc.HeaderText = "C";
			Assert.AreEqual ("C", dvc.HeaderText, "#C1");
			Assert.AreEqual ("C", dvc.HeaderCell.Value, "#C2");
			Assert.AreEqual ("A", dvc.Name, "#C3");
			dvc.HeaderText = string.Empty;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#D1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#D2");
			Assert.AreEqual ("A", dvc.Name, "#D3");
			dvc.HeaderText = "E";
			Assert.AreEqual ("E", dvc.HeaderText, "#E1");
			Assert.AreEqual ("E", dvc.HeaderCell.Value, "#E2");
			Assert.AreEqual ("A", dvc.Name, "#E3");
			dvc.HeaderText = null;
			Assert.AreEqual (string.Empty, dvc.HeaderText, "#F1");
			Assert.IsNull (dvc.HeaderCell.Value, "#F2");
			Assert.AreEqual ("A", dvc.Name, "#F3");
		}

		[Test]
		[Category ("NotWorking")]
		public void Name ()
		{
			DataGridViewColumn dvc = new DataGridViewColumn ();
			Assert.AreEqual (string.Empty, dvc.Name, "#A1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#A2");
			dvc.Name = "A";
			Assert.AreEqual ("A", dvc.Name, "#B");
			Assert.AreEqual ("A", dvc.HeaderCell.Value, "#B2");
			dvc.Name = "B";
			Assert.AreEqual ("B", dvc.Name, "#C1");
			Assert.AreEqual ("B", dvc.HeaderCell.Value, "#C2");
			dvc.Name = null;
			Assert.AreEqual (string.Empty, dvc.Name, "#D1");
			Assert.AreEqual (string.Empty, dvc.HeaderCell.Value, "#D2");
			dvc.HeaderText = "D";
			dvc.Name = "E";
			Assert.AreEqual ("E", dvc.Name, "#E1");
			Assert.AreEqual ("D", dvc.HeaderCell.Value, "#E2");
		}
	}
}
#endif
