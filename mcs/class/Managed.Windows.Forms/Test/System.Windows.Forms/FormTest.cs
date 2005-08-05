//
// FormTest.cs: Test cases for Form.
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
	public class FormTest
	{
		[Test]
		public void FormPropertyTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			Assert.AreEqual (null, myform.AcceptButton, "#1");
			//Assert.AreEqual (null, myform.ActiveMdiChild, "#2"); 
			//System.NotImplementedException for ActiveMdiChild. Feature not implemented.
			Assert.AreEqual (false, myform.AutoScale, "#3");
			Assert.AreEqual (13, myform.AutoScaleBaseSize.Height, "#4");
			Assert.AreEqual (5, myform.AutoScaleBaseSize.Width, "#5");
			Assert.AreEqual (null, myform.CancelButton, "#6");
			Assert.AreEqual (273, myform.ClientSize.Height, "#7");
			Assert.AreEqual (292, myform.ClientSize.Width, "#8");
			Assert.AreEqual (true, myform.ControlBox, "#9");
			Assert.IsTrue (myform.DesktopBounds.X > 0, "#10a");
			Assert.IsTrue (myform.DesktopBounds.Y > 0, "#10b");
			Assert.AreEqual (300, myform.DesktopBounds.Height, "#10c");
			Assert.AreEqual (300, myform.DesktopBounds.Width, "#10d");
			Assert.IsTrue (myform.DesktopLocation.X > 0, "#11a");
			Assert.IsTrue (myform.DesktopLocation.Y > 0, "#11b");
			Assert.AreEqual (DialogResult.None, myform.DialogResult, "#12");
			Assert.AreEqual (FormBorderStyle.Sizable, myform.FormBorderStyle, "#13");
			Assert.AreEqual (false, myform.HelpButton, "#14");
			Assert.AreEqual ("System.Drawing.Icon", myform.Icon.GetType ().ToString (), "#15");
			Assert.AreEqual (false, myform.IsMdiChild, "#16");
			Assert.AreEqual (false, myform.IsMdiContainer, "#17");
			Assert.AreEqual (false, myform.KeyPreview, "#18");
			Assert.AreEqual (true, myform.MaximizeBox, "#19");
			Assert.AreEqual (0, myform.MaximumSize.Height, "#20a");
			Assert.AreEqual (0, myform.MaximumSize.Width, "#20b");
			Assert.AreEqual (0, myform.MdiChildren.Length, "#21a");
			Assert.AreEqual (1, myform.MdiChildren.Rank, "#21b");
			Assert.AreEqual (false, myform.MdiChildren.IsSynchronized, "#21c");
			Assert.AreEqual (null, myform.MdiParent, "#22");
			Assert.AreEqual (null, myform.Menu, "#23");
			//Assert.AreEqual (null, myform.MergedMenu, "#24");
			//System.NotImplementedException for MergedMenu. Feature not implemented.
			Assert.AreEqual (true, myform.MinimizeBox, "#25");
			Assert.AreEqual (0, myform.MinimumSize.Height, "#26a");
			Assert.AreEqual (0, myform.MinimumSize.Width, "#26b");
			Assert.AreEqual (true, myform.MinimumSize.IsEmpty, "#26c");
			Assert.AreEqual (false, myform.Modal, "#27");
			//Assert.AreEqual (1, myform.Opacity, "#28");
			//System.NotImplementedException for Opacity. Feature not implemented.
			Assert.AreEqual (0, myform.OwnedForms.Length, "#29a");
			Assert.AreEqual (1, myform.OwnedForms.Rank, "#29b");
			Assert.AreEqual (null, myform.Owner, "#30");
			Assert.AreEqual (true, myform.ShowInTaskbar, "#31");
			Assert.AreEqual (300, myform.Size.Height, "#32a");
			Assert.AreEqual (300, myform.Size.Width, "#32b");
			Assert.AreEqual (SizeGripStyle.Auto, myform.SizeGripStyle, "#33");
			Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, myform.StartPosition, "#34");
			Assert.AreEqual (true, myform.TopLevel, "#35");
			Assert.AreEqual (false, myform.TopMost, "#36");
			Assert.AreEqual (Color.Empty, myform.TransparencyKey, "#37");
			Assert.AreEqual (FormWindowState.Normal, myform.WindowState, "#38");
		}

		[Test]
		public void ActivateTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.Activate ();
			Assert.AreEqual (true, myform.Focus (), "#40");
		}		

		[Test]
		public void AddOwnedFormTest ()
		{
			Form parent = new Form ();
			parent.Text = "NewParent";
			Form ownedForm = new Form ();
			ownedForm.Text = "Owned Form";
			parent.AddOwnedForm (ownedForm);
			ownedForm.Show ();
			Assert.AreEqual ("NewParent", ownedForm.Owner.Text, "#41");
		}

		[Test]
		public void CloseTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.Close ();
			Assert.IsTrue (myform.Size.Height > 0, "#42");
		}

		[Test]
		[Ignore ("System.NotImplementedException. LayoutMdi not implemented")]	
		public void LayoutMdiTest ()
		{
			Form parent = new Form ();
			Form child = new Form ();
			parent.IsMdiContainer = true;
			child.IsMdiContainer = false;
			child.MdiParent = parent;
			parent.LayoutMdi (MdiLayout.TileHorizontal);
			child.Visible = true;
			parent.Show ();
		}

		[Test]
		public void RemoveOwnedFormTest ()
		{
			Form myform = new Form ();
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.RemoveOwnedForm (myform);
			myform.Show ();
			Assert.AreEqual (null, myform.Owner, "#44");
		}

		[Test]
		public void SetDesktopBoundsTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.SetDesktopBounds (10, 10, 200 , 200);
			Assert.AreEqual (200, myform.DesktopBounds.Height, "#45");
		}

		[Test]
		public void SetDesktopLocationTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.SetDesktopLocation (10, 10);
			Assert.AreEqual (10, myform.DesktopLocation.X, "#46");
		}

		[Test, Ignore ("Needs Manual Intervention")]
		public void ShowDialogTest ()
		{
			Form myform = new Form ();
			myform.Visible = false;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.ShowDialog ();
			Assert.AreEqual (DialogResult.Cancel, myform.DialogResult, "#47");
			myform.Dispose ();
		}
	}
}
