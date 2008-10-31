//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//		Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using System.Threading;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class CheckBoxTest : TestHelper
	{
		[Test]
		public void CheckBoxPropertyTest () 
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			CheckBox mychkbox = new CheckBox(); 
			myform.Controls.Add (mychkbox);
			Assert.AreEqual (Appearance.Normal, mychkbox.Appearance, "#1");
			mychkbox.Appearance = Appearance.Button;
			Assert.AreEqual (Appearance.Button, mychkbox.Appearance, "#2");
			Assert.AreEqual (true, mychkbox.AutoCheck, "#3");
			mychkbox.AutoCheck = false;
			Assert.AreEqual (false, mychkbox.AutoCheck, "#4");
			Assert.AreEqual (false, mychkbox.Checked, "#5");
			Assert.AreEqual (CheckState.Unchecked, mychkbox.CheckState, "#6");
			Assert.AreEqual (ContentAlignment.MiddleLeft, mychkbox.CheckAlign, "#7");
			Assert.AreEqual (ContentAlignment.MiddleLeft, mychkbox.TextAlign, "#8");
			Assert.AreEqual (false, mychkbox.ThreeState, "#9");
			myform.Dispose();
		}
	}
}
