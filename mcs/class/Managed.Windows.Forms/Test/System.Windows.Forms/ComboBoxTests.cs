//
// ComboBoxTests.cs: Test cases for ComboBox.
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
// Copyright (c) 2006 Matt Hargett
//                      
// Authors:             
//      Matt Hargett  <matt@use.net>
//


using System.Windows.Forms;
using System;
using NUnit.Framework;

[TestFixture]
public class ComboBoxTests
{
	ComboBox comboBox;
	bool textChanged, layoutUpdated;

	[SetUp]
	public void SetUp()
	{
		comboBox = new ComboBox();
		textChanged = false;
		layoutUpdated = false;
		comboBox.TextChanged += new EventHandler(textChangedEventHandler);
		comboBox.Layout += new LayoutEventHandler(layoutEventHandler);
	}

	private void textChangedEventHandler(object sender, EventArgs e)
	{
		textChanged = true;
	}

	private void layoutEventHandler(object sender, LayoutEventArgs e)
	{
		layoutUpdated = true;
	}


	[Test]
	public void InitialPropertyValues()
	{
		
		Assert.AreEqual(String.Empty, comboBox.Text);
		Assert.AreEqual(-1, comboBox.SelectedIndex);
		Assert.IsNull(comboBox.SelectedItem);
		Assert.AreEqual(121, comboBox.Size.Width);
		Assert.AreEqual(20, comboBox.Size.Height);
		Assert.IsFalse(textChanged);
		Assert.IsFalse(layoutUpdated);
	}

	[Test]
	public void SetNegativeOneSelectedIndex()
	{
		comboBox.SelectedIndex = -1;
		Assert.AreEqual(String.Empty, comboBox.Text);
		Assert.IsFalse(textChanged);
	}

	[Test]
	public void SetDifferentText()
	{
		comboBox.Text = "foooooooooooooooooooooooooo";
		Assert.IsTrue(textChanged);
		Assert.IsFalse(layoutUpdated);
	}

	[Test]
	public void SetSameText()
	{
		comboBox.Text = String.Empty;
		Assert.IsFalse(textChanged);
		Assert.IsFalse(layoutUpdated);
	}
}
