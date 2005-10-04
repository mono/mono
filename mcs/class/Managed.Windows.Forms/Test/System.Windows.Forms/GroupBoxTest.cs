//
// GroupBoxTest.cs: Test cases for GroupBox.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class GroupBoxTest
	{
		[Test]
		public void GroupBoxPropertyTest ()
		{
			Form myform = new Form ();
			GroupBox mygrpbox = new GroupBox ();
			RadioButton myradiobutton1 = new RadioButton ();
			RadioButton myradiobutton2 = new RadioButton ();
			mygrpbox.Controls.Add (myradiobutton1);
			mygrpbox.Controls.Add (myradiobutton2);
			myform.Show ();
			Assert.AreEqual (FlatStyle.Standard, mygrpbox.FlatStyle, "#1");
			mygrpbox.FlatStyle = FlatStyle.Popup;
			Assert.AreEqual (FlatStyle.Popup, mygrpbox.FlatStyle, "#2");
			mygrpbox.FlatStyle = FlatStyle.Flat;
			Assert.AreEqual (FlatStyle.Flat, mygrpbox.FlatStyle, "#3");
			mygrpbox.FlatStyle = FlatStyle.System;
			Assert.AreEqual (FlatStyle.System, mygrpbox.FlatStyle, "#4");
		}
	}
}
