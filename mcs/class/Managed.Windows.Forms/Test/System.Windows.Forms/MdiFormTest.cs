//
// MdiFormTest.cs: Test cases for MDI Forms.
//
// Author:
//   Rolf Bjarne Kvinge (RKvinge@novell.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;


namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class MdiFormTest
	{
		[Test]
		public void Text ()
		{
			Form main = null, child1 = null, child2 = null;

			main = new Form ();
			main.IsMdiContainer = true;
			main.Name = "main";
			main.Text = main.Name;
			main.Show();
			
			Assert.AreEqual ("main", main.Text, "#1");
			
			child1 = new Form ();
			child1.Name = "child1";
			child1.MdiParent = main;
			child1.Text = child1.Name;
			child1.WindowState = FormWindowState.Maximized;
			child1.Show ();
			
			Assert.AreEqual ("main - [child1]", main.Text, "#2");
			
			child2 = new Form ();
			child2.Name = "child2";
			child2.Text = child2.Text;
			child2.WindowState = FormWindowState.Maximized;
			child2.Show();
			
			Assert.AreEqual ("main - [child1]", main.Text, "#3");
			
			child1.Activate();
			Assert.AreEqual ("main - [child1]", main.Text, "#4");
			
			child1.WindowState = FormWindowState.Minimized;
			Assert.AreEqual ("main", main.Text, "#5");
			
			child2.Activate ();
			Assert.AreEqual ("main", main.Text, "#6");
			
			child2.WindowState = FormWindowState.Maximized;
			Assert.AreEqual ("main", main.Text, "#7");

			child2.Dispose ();
			child1.Dispose ();
			main.Dispose ();
		}
				
		[Test]
		[Category("NotWorking")]
		public void ActiveMdiChild ()
		{
			Form main, child1, child2;
			
			main = new Form ();
			main.IsMdiContainer = true;
			main.Name = "main";

			child1 = new Form ();
			child1.Name = "child1";
			child1.MdiParent = main;
			child1.WindowState = FormWindowState.Maximized;
			child1.Show ();
			
			child2 = new Form ();
			child2.Name = "child2";
			child2.MdiParent = main;
			child2.Show();
			
			Assert.AreSame (null, main.ActiveMdiChild, "#1");

			main.Show ();
			if (child1 == main.ActiveMdiChild)
				Assert.Fail ("#2");
			Assert.AreSame (child2, main.ActiveMdiChild, "#3");

			main.Visible = false;
			Assert.AreSame (child2, main.ActiveMdiChild, "#4");

			child2.Dispose ();
			child1.Dispose ();
			main.Dispose ();
			main.Close();
		}

		[Test]
		public void MdiChild_WindowState1 ()
		{
			Form main = null, child1 = null, child2 = null;
			try {
				
				main = new Form ();
				main.IsMdiContainer = true;
				main.Name = "main";

				child1 = new Form ();
				child1.Name = "child1";
				child1.MdiParent = main;
				child1.WindowState = FormWindowState.Maximized;
				child1.Show ();
				
				child2 = new Form ();
				child2.Name = "child2";
				child2.MdiParent = main;
				child2.Show();
				
				Assert.AreEqual (FormWindowState.Maximized, child1.WindowState, "#1");
				Assert.AreEqual (FormWindowState.Normal, child2.WindowState, "#2");
				main.Show ();
				Assert.AreEqual (FormWindowState.Normal, child1.WindowState, "#3");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#4");
			} finally {
				child2.Dispose ();
				child1.Dispose ();
				main.Dispose ();
				main.Close();
			}
		}
		
		[Test]
		public void MdiChild_WindowState2 ()
		{
			Form main = null, child1 = null, child2 = null;
			try{
				
				main = new Form ();
				main.Name = "main";
				main.IsMdiContainer = true;
				main.Show ();
				
				child1 = new Form ();
				child1.Name = "child1";
				child1.MdiParent = main;
				child1.WindowState = FormWindowState.Maximized;
				child1.Show ();
				
				child2 = new Form ();
				child2.Name = "child2";
				child2.MdiParent = main;
				child2.Show();

				
				Assert.AreEqual (FormWindowState.Normal, child1.WindowState, "#1");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#2");
			
			} finally {
				child2.Dispose ();
				child1.Dispose ();
				main.Dispose ();
				main.Close();
			}
		}
		
		[Test]
		public void MdiChild_WindowState3 ()
		{
			Form main = null, child1 = null, child2 = null;
			try {				
				main = new Form ();
				main.IsMdiContainer = true;
				main.Show ();
				
				child1 = new Form ();
				child1.MdiParent = main;
				child1.Show ();
				
				child2 = new Form ();
				child2.MdiParent = main;
				child2.WindowState = FormWindowState.Maximized;
				child2.Show();
				
				Assert.AreEqual (FormWindowState.Normal, child1.WindowState, "#1");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#2");
				
			} finally {
				child2.Dispose ();
				child1.Dispose ();
				main.Dispose ();
				main.Close();
			}
		}

		[Test]
		public void MdiChild_WindowState4 ()
		{
			Form main = null, child1 = null, child2 = null;
			try {				
				main = new Form ();
				main.IsMdiContainer = true;
				main.Show ();
				
				child1 = new Form ();
				child1.MdiParent = main;
				child1.WindowState = FormWindowState.Maximized;
				child1.Show ();
				
				child2 = new Form ();
				child2.MdiParent = main;
				child2.WindowState = FormWindowState.Maximized;

				Assert.AreEqual (FormWindowState.Maximized, child1.WindowState, "#1");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#2");

				child2.Show();
				
				Assert.AreEqual (FormWindowState.Normal, child1.WindowState, "#3");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#4");
				
				child2.WindowState = FormWindowState.Normal;

				Assert.AreEqual (FormWindowState.Normal, child1.WindowState, "#5");
				Assert.AreEqual (FormWindowState.Normal, child2.WindowState, "#6");
			} finally {
				child2.Dispose ();
				child1.Dispose ();
				main.Dispose ();
				main.Close();
			}
		}


		[Test]
		public void MdiChild_WindowState5 ()
		{
			Form main = null, child1 = null, child2 = null;
			try {				
				main = new Form ();
				main.Name = "main";
				main.IsMdiContainer = true;
				main.Show ();
				
				child1 = new Form ();
				child1.Name = "child1";
				child1.MdiParent = main;
				child1.WindowState = FormWindowState.Maximized;
				child1.Show ();
				
				child2 = new Form ();
				child2.Name = "child2";
				child2.MdiParent = main;
				child2.WindowState = FormWindowState.Maximized;
				
				Assert.AreEqual (FormWindowState.Maximized, child1.WindowState, "#1");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#2");

				child2.Show();

				Assert.AreEqual (FormWindowState.Normal, child1.WindowState, "#3");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#4");

				child1.Activate ();

				Assert.AreEqual (FormWindowState.Maximized, child1.WindowState, "#5");
				Assert.AreEqual (FormWindowState.Normal, child2.WindowState, "#6");
			} finally {
				child2.Dispose ();
				child1.Dispose ();
				main.Dispose ();
				main.Close();
			}
		}


		[Test]
		public void MdiChild_WindowState6 ()
		{
			Form main = null, child1 = null, child2 = null;
			try {
				
				main = new Form ();
				main.Name = "main";
				main.IsMdiContainer = true;
				main.Show ();
				
				child1 = new Form ();
				child1.Name = "child1";
				child1.MdiParent = main;
				child1.WindowState = FormWindowState.Minimized;
				child1.Show ();
				
				child2 = new Form ();
				child2.Name = "child2";
				child2.MdiParent = main;
				child2.WindowState = FormWindowState.Maximized;

				Assert.AreEqual (FormWindowState.Minimized, child1.WindowState, "#1");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#2");

				child2.Show();
				
				Assert.AreEqual (FormWindowState.Minimized, child1.WindowState, "#3");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#4");
				
				child1.Activate ();

				Assert.AreEqual (FormWindowState.Maximized, child1.WindowState, "#5");
				Assert.AreEqual (FormWindowState.Normal, child2.WindowState, "#6");
				
				child2.Activate ();

				Assert.AreEqual (FormWindowState.Minimized, child1.WindowState, "#7");
				Assert.AreEqual (FormWindowState.Maximized, child2.WindowState, "#8");
			} finally {
				child2.Dispose ();
				child1.Dispose ();
				main.Dispose ();
				main.Close();
			}
		}
	}
}
