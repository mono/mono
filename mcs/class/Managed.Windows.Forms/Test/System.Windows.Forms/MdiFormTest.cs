//
// MdiFormTest.cs: Test cases for MDI Forms.
//
// Author:
//   Rolf Bjarne Kvinge (RKvinge@novell.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
using System.Threading;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class MdiFormTest : TestHelper
	{
		private Form main;
		private Form child1;
		private Form child2;
		private Form child3;

		[TearDown]
		protected override void TearDown ()
		{
			if (main != null)
				main.Dispose ();
			if (child1 != null)
				child1.Dispose ();
			if (child2 != null)
				child2.Dispose ();
			if (child3 != null)
				child3.Dispose ();
			main = null;
			child1 = null;
			child2 = null;
			child3 = null;
			base.TearDown ();
		}
	
		// No attribute here since this is supposed to be called from 
		// each test directly, not by nunit.
		void SetUp (bool only_create, bool only_text)
		{
			SetUp (only_create, only_text, false);
		}	
				
		// No attribute here since this is supposed to be called from 
		// each test directly, not by nunit.
		public void SetUp (bool only_create, bool only_text, bool set_parent)
		{
			TearDown ();
			
			main = new Form ();
			child1 = new Form ();
			child2 = new Form ();
			child3 = new Form ();

			if (only_create)
				return;

			main.Text = main.Name = "main";
			main.ShowInTaskbar = false;
			child1.Text = child1.Name = "child1";
			child2.Text = child2.Name = "child2";
			child3.Text = child3.Name = "child3";

			if (only_text)
				return;

			main.IsMdiContainer = true;
			
			if (set_parent) {
				child1.MdiParent = main;
				child2.MdiParent = main;
				child3.MdiParent = main;
			}
		}

		[Test]
		public void DeactivatedEventTest ()
		{
			using (Form f = new Form ()) {
				Form c1 = new Form ();
				Form c2 = new Form ();
				EventLogger log1 = new EventLogger (c1);
				EventLogger log2 = new EventLogger (c2);

				f.IsMdiContainer = true;
				f.ShowInTaskbar = false;
				c2.MdiParent = c1.MdiParent = f;

				c1.Show ();
				c2.Show ();
				Assert.AreEqual (0, log1.CountEvents ("Deactivate"), "#A1");
				Assert.AreEqual (0, log2.CountEvents ("Deactivate"), "#A2");

				f.Show ();
				Assert.AreEqual (1, log1.CountEvents ("Deactivate"), "#B1");
				Assert.AreEqual (0, log2.CountEvents ("Deactivate"), "#B2");

				c1.Show ();
				c2.Show ();
				Assert.AreEqual (1, log1.CountEvents ("Deactivate"), "#C1");
				Assert.AreEqual (0, log2.CountEvents ("Deactivate"), "#C2");

				c1.Activate ();
				Assert.AreEqual (1, log1.CountEvents ("Deactivate"), "#D1");
				Assert.AreEqual (1, log2.CountEvents ("Deactivate"), "#D2");

				c1.Activate ();
				Assert.AreEqual (1, log1.CountEvents ("Deactivate"), "#E1");
				Assert.AreEqual (1, log2.CountEvents ("Deactivate"), "#E2");

				c2.Activate ();
				Assert.AreEqual (2, log1.CountEvents ("Deactivate"), "#F1");
				Assert.AreEqual (1, log2.CountEvents ("Deactivate"), "#F2");

				c1.Activate ();
				Assert.AreEqual (2, log1.CountEvents ("Deactivate"), "#G1");
				Assert.AreEqual (2, log2.CountEvents ("Deactivate"), "#G2");
			}
		}
		
		[Test]
		public void ActivatedEventTest ()
		{
			using (Form f = new Form ()) {
				Form c1 = new Form ();
				Form c2 = new Form ();
				EventLogger log1 = new EventLogger (c1);
				EventLogger log2 = new EventLogger (c2);

				f.IsMdiContainer = true;
				f.ShowInTaskbar = false;
				c2.MdiParent = c1.MdiParent = f;

				c1.Show ();
				c2.Show ();
				Assert.AreEqual (0, log1.CountEvents ("Activated"), "#A1");
				Assert.AreEqual (0, log2.CountEvents ("Activated"), "#A2");
				
				f.Show ();
				Assert.AreEqual (1, log1.CountEvents ("Activated"), "#B1");
				Assert.AreEqual (1, log2.CountEvents ("Activated"), "#B2");
				
				c1.Show ();
				c2.Show ();
				Assert.AreEqual (1, log1.CountEvents ("Activated"), "#C1");
				Assert.AreEqual (1, log2.CountEvents ("Activated"), "#C2");

				c1.Activate ();
				Assert.AreEqual (2, log1.CountEvents ("Activated"), "#D1");
				Assert.AreEqual (1, log2.CountEvents ("Activated"), "#D2");

				c1.Activate ();
				Assert.AreEqual (2, log1.CountEvents ("Activated"), "#E1");
				Assert.AreEqual (1, log2.CountEvents ("Activated"), "#E2");

				c2.Activate ();
				Assert.AreEqual (2, log1.CountEvents ("Activated"), "#F1");
				Assert.AreEqual (2, log2.CountEvents ("Activated"), "#F2");

				c1.Activate ();
				Assert.AreEqual (3, log1.CountEvents ("Activated"), "#G1");
				Assert.AreEqual (2, log2.CountEvents ("Activated"), "#G2");
			}
		}
		
		[Test]
		public void ActiveControlTest ()
		{
			SetUp (false, false, true);
			
			main.Show ();
			
			Assert.IsNull (main.ActiveControl, "#01");			
			child1.Visible = true;
			Assert.AreSame (child1, main.ActiveControl, "#02");
			child2.Visible = true;
			Assert.AreSame (child2, main.ActiveControl, "#03");
			child3.Visible = true;
			Assert.AreSame (child3, main.ActiveControl, "#04");
			TearDown ();
		}
		
		[Test]
		public void SelectNextControlTest ()
		{
			SetUp (false, false, true);

			main.Show ();
			
			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;
			
			main.SelectNextControl (main.ActiveControl, true, false, true, true);
			Assert.AreSame (child1, main.ActiveControl, "#01");
			main.SelectNextControl (main.ActiveControl, true, false, true, true);
			Assert.AreSame (child2, main.ActiveControl, "#02");
			main.SelectNextControl (main.ActiveControl, true, false, true, true);
			Assert.AreSame (child3, main.ActiveControl, "#03");
						
			TearDown ();
		}

		[Test]
		public void SelectPreviousControlTest ()
		{
			SetUp (false, false, true);

			main.Show ();
			
			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.SelectNextControl (main.ActiveControl, false, false, true, true);
			Assert.AreSame (child2, main.ActiveControl, "#01");
			main.SelectNextControl (main.ActiveControl, false, false, true, true);
			Assert.AreSame (child1, main.ActiveControl, "#02");
			main.SelectNextControl (main.ActiveControl, false, false, true, true);
			Assert.AreSame (child3, main.ActiveControl, "#03");

			TearDown ();
		}
		
		[TestFixture]
		public class CloseTest
		{
			class ChildForm : Form
			{
				public ChildForm ()
				{
					Closed += new EventHandler (ChildForm_Closed);
					Closing += new CancelEventHandler (ChildForm_Closing);
#if NET_2_0					
					FormClosed += new FormClosedEventHandler (ChildForm_FormClosed);
					FormClosing += new FormClosingEventHandler (ChildForm_FormClosing);
#endif
				}
#if NET_2_0
				void ChildForm_FormClosing (object sender, FormClosingEventArgs e)
				{
					Assert.IsNotNull (MdiParent, "ChildForm_FormClosing");
				}

				void ChildForm_FormClosed (object sender, FormClosedEventArgs e)
				{
					Assert.IsNotNull (MdiParent, "ChildForm_FormClosed");
				}
#endif
				void ChildForm_Closing (object sender, CancelEventArgs e)
				{
					Assert.IsNotNull (MdiParent, "ChildForm_Closing");
				}

				void ChildForm_Closed (object sender, EventArgs e)
				{
					Assert.IsNotNull (MdiParent, "ChildForm_Closed");
				}
			
				protected override void OnClosed (EventArgs e)
				{
					Assert.IsNotNull (MdiParent, "OnClosed 1");
					base.OnClosed (e);
					Assert.IsNotNull (MdiParent, "OnClosed 2");
				}

				protected override void OnClosing (CancelEventArgs e)
				{
					Assert.IsNotNull (MdiParent, "OnClosing 1");
					base.OnClosing (e);
					Assert.IsNotNull (MdiParent, "OnClosing 2");
				}
				
			}
			[Test]
			public void Test () {
				using (Form main = new Form ()) {
					main.IsMdiContainer = true;
					main.ShowInTaskbar = false;
					main.Visible = true;

					ChildForm child = new ChildForm ();
					EventLogger log = new EventLogger (child);
					child.MdiParent = main;
					child.Visible = true;
					child.Close ();
					
					Assert.AreEqual (1, log.CountEvents ("Closed"), "#01");
					Assert.AreEqual (1, log.CountEvents ("Closing"), "#02");
					Assert.IsNull (child.MdiParent, "#03");
					Assert.AreEqual (0, main.MdiChildren.Length, "#04");		
					Assert.AreEqual (false, child.IsMdiChild, "#05");		
				}
			}
		}
		[Test]
		[Category ("NotWorking")]
		public void StartLocationTest ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();

			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual (true, "{X=0,Y=0}" != child2.Location.ToString (), "#2");

			TearDown ();
		}
		
		[Test]
		public void StartLocationTest2 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;
			child2.StartPosition = FormStartPosition.Manual;
			child2.Location = new Point(23, 34);

			main.Show ();

			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=23,Y=34}",child2.Location.ToString (), "#2");

			TearDown ();
		}
		
		[Test]
		public void NewFormShouldAlsoBeMaximized ()
		{
			Form parent = new Form ();
			
			Form child1 = new Form ();
			Form child2 = new Form ();
			
			parent.ShowInTaskbar = false;
			parent.IsMdiContainer = true;
			parent.ClientSize = new Size (300, 300);
			
			parent.Show ();
			
			child1.MdiParent = parent;
			child1.Show ();
			
			child1.WindowState = FormWindowState.Maximized;
			
			child2.MdiParent = parent;
			child2.ClientSize = new Size (100, 100);
			child2.Show ();

			Assert.IsTrue (child2.Width > 200, "Width should be maximized");
			Assert.IsTrue (child2.Height > 200, "Height should be maximized");
			
			parent.Dispose ();
		}

#if NET_2_0
		[Test]
		public void RestoreWithMainMenuStrip ()
		{
			Form parent = new Form ();

			Form child1 = new Form ();

			parent.ShowInTaskbar = false;
			parent.IsMdiContainer = true;
			parent.ClientSize = new Size (500, 500);

			MenuStrip ms = new MenuStrip ();
			ms.Items.Add (new ToolStripMenuItem ("main", null, new ToolStripMenuItem ("item1")));
			parent.Controls.Add (ms);
			parent.MainMenuStrip = ms;

			parent.Show ();

			child1.MdiParent = parent;
			child1.Show ();

			Size s = child1.Size;
			
			child1.WindowState = FormWindowState.Maximized;
			child1.WindowState = FormWindowState.Normal;

			Assert.AreEqual (s, child1.Size, "A1");

			parent.Dispose ();
		}
#endif

		/* These tests are all failing on WinXP with XP Theme. 
		 * offset seems to be 22,22 normally, and 22,29 with XP theme.
		 * Find a way to test this reliably.
		 * /
		[Category("NotWorking")]
		[Test]
		public void StartLocationTest1 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest2 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			child1.StartPosition = FormStartPosition.Manual;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");

			TearDown ();
		}


		[Category("NotWorking")]
		[Test]
		public void StartLocationTest3 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			child2.StartPosition = FormStartPosition.Manual;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest4 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			child1.StartPosition = FormStartPosition.Manual;
			child2.StartPosition = FormStartPosition.Manual;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest5 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.Manual;

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest6 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.CenterParent;

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest7 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.CenterScreen;

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest8 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.WindowsDefaultBounds;

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest9 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.WindowsDefaultLocation;

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}
		
		[Category("NotWorking")]
		[Test]
		public void StartLocationTest10 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.Manual;
			child2.Location = new Point (123, 123);

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=123,Y=123}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest11 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.Location = new Point (123, 123);

			Assert.AreEqual ("{X=123,Y=123}", child2.Location.ToString (), "#0");

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			Assert.AreEqual ("{X=123,Y=123}", child2.Location.ToString (), "#0");

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest12 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child1.Visible = true;
			//child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=22,Y=22}", child3.Location.ToString (), "#3");

			child2.Visible = true;
			Assert.AreEqual ("{X=44,Y=44}", child2.Location.ToString (), "#4");

			child1.Visible = false;
			child1.Visible = true;
			Assert.AreEqual ("{X=66,Y=66}", child1.Location.ToString (), "#1");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest13 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child1.Visible = true;
			//child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=22,Y=22}", child3.Location.ToString (), "#3");

			child2.Visible = true;
			Assert.AreEqual ("{X=44,Y=44}", child2.Location.ToString (), "#4");

			child1.Visible = false;
			child1.Visible = true;
			Assert.AreEqual ("{X=66,Y=66}", child1.Location.ToString (), "#5");
			
			child3.Visible = true;
			Assert.AreEqual ("{X=22,Y=22}", child3.Location.ToString (), "#6");

			// MDI Child size does not seem to affect layout.
			// Size 500,500
			child2.Visible = false;
			child2.Size = new Size (500, 500);
			child2.Visible = true;
			Assert.AreEqual ("{X=88,Y=88}", child2.Location.ToString (), "#7");

			child2.Visible = false;
			child2.Visible = true;
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#8");
			
			child2.Visible = false;
			child2.Visible = true;
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#9");

			// Size 5,5
			child2.Visible = false;
			child2.Size = new Size (5, 5);
			child2.Visible = true;
			Assert.AreEqual ("{X=44,Y=44}", child2.Location.ToString (), "#10");

			child2.Visible = false;
			child2.Visible = true;
			Assert.AreEqual ("{X=66,Y=66}", child2.Location.ToString (), "#11");
			
			child2.Visible = false;
			child2.Visible = true;
			Assert.AreEqual ("{X=88,Y=88}", child2.Location.ToString (), "#12");

			child2.Visible = false;
			child2.Visible = true;
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#13");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest14 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child2.StartPosition = FormStartPosition.Manual;
			child2.Location = new Point (44, 44);
			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=44,Y=44}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			child1.Visible = false;
			child1.Visible = true;
			Assert.AreEqual ("{X=66,Y=66}", child1.Location.ToString (), "#4");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest15 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			main.Show ();

			child1.Visible = true;
			child2.Visible = true;
			child2.Location = new Point (22, 44);
			child3.Visible = true;

			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=44}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			child1.Visible = false;
			child1.Visible = true;
			Assert.AreEqual ("{X=66,Y=66}", child1.Location.ToString (), "#4");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest16 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;
			child2.Location = new Point (22, 44);
			child3.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#3");

			child1.Visible = false;
			child1.Visible = true;
			Assert.AreEqual ("{X=66,Y=66}", child1.Location.ToString (), "#4");

			TearDown ();
		}
		
		[Category("NotWorking")]
		[Test]
		public void StartLocationTest17 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=0,Y=0}", child3.Location.ToString (), "#3");

			child2.Visible = false;
			child3.Visible = true;

			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#4");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		public void StartLocationTest18 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=22,Y=22}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=0,Y=0}", child3.Location.ToString (), "#3");

			child2.Visible = false;
			child2.Close ();
			child2.Dispose ();
			child2 = null;

			child3.Visible = true;

			Assert.AreEqual ("{X=44,Y=44}", child3.Location.ToString (), "#4");

			TearDown ();
		}

		[Category("NotWorking")]		
		[Test]
		public void StartLocationTest19 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			child1.StartPosition = FormStartPosition.Manual;
			child2.StartPosition = FormStartPosition.Manual;
			child3.StartPosition = FormStartPosition.Manual;

			main.Show ();
			
			Assert.AreEqual ("{X=0,Y=0}", child1.Location.ToString (), "#1");
			Assert.AreEqual ("{X=0,Y=0}", child2.Location.ToString (), "#2");
			Assert.AreEqual ("{X=0,Y=0}", child3.Location.ToString (), "#3");

			TearDown ();
		}
*/
		[Test]
		public void StartSizeTest1 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;
			child3.MdiParent = main;

			main.Show ();
			
			Assert.AreEqual ("{Width=300, Height=300}", main.Size.ToString (), "#1");
			Assert.AreEqual ("{Width=300, Height=300}", child1.Size.ToString (), "#2");
			Assert.AreEqual ("{Width=300, Height=300}", child2.Size.ToString (), "#3");
			Assert.AreEqual ("{Width=300, Height=300}", child3.Size.ToString (), "#4");

			child1.Visible = true;
			child2.Visible = true;
			child3.Visible = true;

			Assert.AreEqual ("{Width=300, Height=300}", main.Size.ToString (), "#1");
			Assert.AreEqual ("{Width=300, Height=300}", child1.Size.ToString (), "#2");
			Assert.AreEqual ("{Width=300, Height=300}", child2.Size.ToString (), "#3");
			Assert.AreEqual ("{Width=300, Height=300}", child3.Size.ToString (), "#4");

			TearDown ();
		}

		[Test]
		public void IsMdiContainerTest ()
		{
			SetUp (false, true);

			main.Visible = true;
			main.Visible = false;
			
			main.IsMdiContainer = true;
			
			child1.MdiParent = main;

			main.IsMdiContainer = false;

			Assert.AreSame (null, main.ActiveMdiChild, "#1");

			main.Visible = true;
			Assert.AreSame (null, main.ActiveMdiChild, "#2");
			
			Assert.AreSame (null, main.MdiParent, "#3");

			TearDown ();
		}

		[Category("NotWorking")]
		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot add a top level control to a control.")]
		public void AddToControlsTest ()
		{
			SetUp (false, true);
			
			main.Visible = true;
			main.Visible = false;

			main.Controls.Add (child1);

			TearDown ();
		}

		[Test]
		public void Text ()
		{
			Form main = null, child1 = null, child2 = null, child3 = null;

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
			child1.MdiParent = main;
			child2.Text = string.Empty;
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

			child3 = new Form ();
			child3.Name = "child3";
			child3.MdiParent = main;
			child3.Text = child3.Name;
			child3.WindowState = FormWindowState.Maximized;
			child3.Show ();

			Assert.AreEqual ("main - [child3]", main.Text, "#8");
			child3.WindowState = FormWindowState.Normal;
			Assert.AreEqual ("main", main.Text, "#9");

			main.Text = string.Empty;
			child3.WindowState = FormWindowState.Maximized;
			Assert.AreEqual (" - [child3]", main.Text, "#10");

			child3.Text = string.Empty;
			Assert.AreEqual (string.Empty, main.Text, "#11");

			main.Close ();
			
			child3.Dispose ();
			child2.Dispose ();
			child1.Dispose ();
			main.Dispose ();
		}

		[Test]
		public void Text_MdiContainer ()
		{
			Form main = new Form ();
			main.ShowInTaskbar = false;
			main.Text = "main";
			main.IsMdiContainer = true;
			main.Show ();

			Assert.AreEqual ("main", main.Text, "#1");

			Form child = new Form ();
			child.Name = "child";
			child.MdiParent = main;
			child.Text = child.Name;
			child.WindowState = FormWindowState.Maximized;
			child.Show ();

			Assert.AreEqual ("main - [child]", main.Text, "#2");

			main.Dispose ();
		}

		[Test] // bug 80038
		public void Text_ChildClose ()
		{
			Form main = new Form ();
			main.ShowInTaskbar = false;
			main.IsMdiContainer = true;
			main.Text = "main";
			main.Show ();

			Assert.AreEqual ("main", main.Text, "#1");

			Form child = new Form ();
			child.Name = "child";
			child.MdiParent = main;
			child.Text = child.Name;
			child.WindowState = FormWindowState.Maximized;
			child.Show ();

			Assert.AreEqual ("main - [child]", main.Text, "#2");

			child.Close ();
			Assert.AreEqual ("main", main.Text, "#3");

			main.Dispose ();
		}

		[Test]
		public void Text_Maximized ()
		{
			Form main = new Form ();
			main.IsMdiContainer = true;
			main.Name = "main";
			main.Text = main.Name;
			main.Show ();

			Assert.AreEqual ("main", main.Text, "#1");

			Form child1 = new Form ();
			child1.Name = "child1";
			child1.MdiParent = main;
			child1.Text = child1.Name;
			child1.WindowState = FormWindowState.Maximized;
			child1.Show ();

			Assert.AreEqual ("main - [child1]", main.Text, "#2");

			Form child2 = new Form ();
			child2.Name = "child2";
			child2.MdiParent = main;
			child2.Text = child2.Name;
			child2.WindowState = FormWindowState.Maximized;
			child2.Show ();

			Assert.AreEqual ("main - [child2]", main.Text, "#3");

			child1.WindowState = FormWindowState.Maximized;

			Assert.AreEqual ("main - [child1]", main.Text, "#4");

			main.Dispose ();
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TopLevelTest ()
		{
			Form main, child1;

			main = new Form ();
			main.IsMdiContainer = true;
			main.Name = "main";

			child1 = new Form ();
			child1.Name = "child1";
			Assert.AreEqual (true, child1.TopLevel, "#01");
			child1.MdiParent = main;
			Assert.AreEqual (false, child1.TopLevel, "#02");
			
			child1.Dispose ();
			main.Dispose ();
		}
		[Test]
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
			
			Assert.IsNull (main.ActiveMdiChild, "#1");

			main.Show ();
			Assert.AreSame (child2, main.ActiveMdiChild, "#2");

			child1.WindowState = FormWindowState.Maximized;
			Assert.AreSame (child1, main.ActiveMdiChild, "#3");

			child2.WindowState = FormWindowState.Maximized;
			Assert.AreSame (child2, main.ActiveMdiChild, "#4");

			main.Visible = false;
#if NET_2_0
			Assert.IsNull (main.ActiveMdiChild, "#5");
#else
			Assert.AreSame (child2, main.ActiveMdiChild, "#5");
#endif

			child2.Dispose ();
			child1.Dispose ();
			main.Dispose ();
			main.Close();
		}

		[Test]
		public void ActiveMdiChild2 ()
		{
			SetUp (false, false);

			child1.MdiParent = main;
			child2.MdiParent = main;

			main.Show ();
			child1.Show ();
			child2.Show ();
			
			child1.Activate ();
			child1.Visible = false;
			
			Assert.AreSame (child2, main.ActiveMdiChild, "#1");

			TearDown ();
		}

		[Test]
		public void ActiveMdiChild3 ()
		{
			SetUp (false, false);
			
			child1.MdiParent = main;
			child2.MdiParent = main;

			main.Visible = true;
			main.Visible = false;

			Assert.AreSame (null, main.ActiveMdiChild, "#1");
			//child2.Visible = true; This will cause StackOverflowException on MS.
			main.Visible = true;
			Assert.AreSame (null, main.ActiveMdiChild, "#2");

			TearDown ();
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
				child1.Text = "child1";
				child1.MdiParent = main;
				child1.WindowState = FormWindowState.Maximized;
				child1.Show ();
				
				child2 = new Form ();
				child2.Name = "child2";
				child2.Text = "child2";
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
		
		[Test] // bug 80995
		public void ChangeClientSizeTest ()
		{
			SetUp (false, false, false);
			child1.ClientSize = new Size (224, 37);
			child1.MdiParent = main;
			Assert.AreEqual ("{Width=224, Height=37}", child1.ClientSize.ToString (), "#01");
			child1.Show ();
			Assert.AreEqual ("{Width=224, Height=37}", child1.ClientSize.ToString (), "#02");
			main.Show ();
			Assert.AreEqual ("{Width=224, Height=37}", child1.ClientSize.ToString (), "#03");
		}
		
		private string events_fired;
		
		[Test]
		public void MdiClosingClosedEvent ()
		{
			Form f = new Form ();
			f.IsMdiContainer = true;
			
			MdiChild mdi = new MdiChild ();
			mdi.can_close = false;
			mdi.MdiParent = f;

			f.Closing += new CancelEventHandler (f_Closing);
			f.Closed += new EventHandler (f_Closed);
			mdi.Closing += new CancelEventHandler (mdi_Closing);
			mdi.Closed += new EventHandler (mdi_Closed);

#if NET_2_0
			f.FormClosing += new FormClosingEventHandler (f_FormClosing);
			f.FormClosed += new FormClosedEventHandler (f_FormClosed);
			mdi.FormClosing += new FormClosingEventHandler (mdi_FormClosing);
			mdi.FormClosed += new FormClosedEventHandler (mdi_FormClosed);
#endif

			f.Show ();
			mdi.Show ();
			
			events_fired = string.Empty;
			
			f.Close ();
			
#if NET_2_0
			Assert.AreEqual ("Child.Closing: True, Child.FormClosing: True, Parent.Closing: True, Parent.FormClosing: True, ", events_fired, "A1-2.0");
#else
			Assert.AreEqual ("Child.Closing: True, Parent.Closing: True, ", events_fired, "A1-1.1");
#endif

			events_fired = string.Empty;
			mdi.can_close = true;
			f.Close ();
			
#if NET_2_0
			Assert.AreEqual ("Child.Closing: False, Child.FormClosing: False, Parent.Closing: False, Parent.FormClosing: False, Child.Closed, Child.FormClosed, Parent.Closed, Parent.FormClosed, ", events_fired, "A2-2.0");
#else
			Assert.AreEqual ("Child.Closing: False, Parent.Closing: False, Child.Closed, Parent.Closed, ", events_fired, "A2-1.1");
#endif

			f.Dispose ();
		}

#if NET_2_0
		void mdi_FormClosed (object sender, FormClosedEventArgs e)
		{
			events_fired += "Child.FormClosed, ";
		}

		void mdi_FormClosing (object sender, FormClosingEventArgs e)
		{
			events_fired += string.Format ("Child.FormClosing: {0}, ", e.Cancel);
		}

		void f_FormClosed (object sender, FormClosedEventArgs e)
		{
			events_fired += "Parent.FormClosed, ";
		}

		void f_FormClosing (object sender, FormClosingEventArgs e)
		{
			events_fired += string.Format ("Parent.FormClosing: {0}, ", e.Cancel);
		}
#endif

		void mdi_Closed (object sender, EventArgs e)
		{
			events_fired += "Child.Closed, ";
		}

		void mdi_Closing (object sender, CancelEventArgs e)
		{
			events_fired += string.Format ("Child.Closing: {0}, ", e.Cancel);
		}

		void f_Closed (object sender, EventArgs e)
		{
			events_fired += "Parent.Closed, ";
		}

		void f_Closing (object sender, CancelEventArgs e)
		{
			events_fired += string.Format ("Parent.Closing: {0}, ", e.Cancel);
		}
		
		private class MdiChild : Form
		{
			public bool can_close = false;
			
			public MdiChild ()
			{
				Closing += new CancelEventHandler (MdiChild_Closing);
			}

			void MdiChild_Closing (object sender, CancelEventArgs e)
			{
				e.Cancel = !can_close;
			}
		}
	}
}
