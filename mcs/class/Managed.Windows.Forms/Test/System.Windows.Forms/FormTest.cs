//
// FormTest.cs: Test cases for Form.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class FormTest
	{
		[Test]
		public void ShowDialogCloseTest ()
		{
			using (TimeBombedForm f = new TimeBombedForm ()) {
				EventLogger log = new EventLogger (f);
				f.timer.Interval = 1000;
				f.VisibleChanged += new EventHandler (Form_VisibleChanged1);
				f.ShowDialog ();
				
				Assert.AreEqual ("VisibleChanged", f.Reason, "#00");
				Assert.AreEqual (1, log.CountEvents ("Closing"), "#01");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosing"), "#02");
#endif
				Assert.AreEqual (1, log.CountEvents ("HandleDestroyed"), "#03");

				Assert.AreEqual (0, log.CountEvents ("Closed"), "#04");
#if NET_2_0
				Assert.AreEqual (0, log.CountEvents ("FormClosed"), "#05");
#endif
				Assert.AreEqual (0, log.CountEvents ("Disposed"), "#06");
			}

			using (TimeBombedForm f = new TimeBombedForm ()) {
				EventLogger log = new EventLogger (f);
				f.ShowDialog ();

				Assert.AreEqual ("Bombed", f.Reason, "#A0");
				Assert.AreEqual (1, log.CountEvents ("Closing"), "#A1");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosing"), "#A2");
#endif
				Assert.AreEqual (1, log.CountEvents ("HandleDestroyed"), "#A3");

				Assert.AreEqual (1, log.CountEvents ("Closed"), "#A4");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosed"), "#A5");
#endif
				Assert.AreEqual (0, log.CountEvents ("Disposed"), "#A6");
			}


			using (TimeBombedForm f = new TimeBombedForm ()) {
				EventLogger log = new EventLogger (f);
				f.VisibleChanged += new EventHandler (Form_VisibleChanged2);
				f.ShowDialog ();

				Assert.AreEqual ("VisibleChanged", f.Reason, "#B0");
				Assert.AreEqual (1, log.CountEvents ("Closing"), "#B1");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosing"), "#B2");
#endif
				Assert.AreEqual (1, log.CountEvents ("HandleDestroyed"), "#B3");

				Assert.AreEqual (1, log.CountEvents ("Closed"), "#B4");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosed"), "#B5");
#endif
				Assert.AreEqual (0, log.CountEvents ("Disposed"), "#B6");
			}


			using (TimeBombedForm f = new TimeBombedForm ()) {
				EventLogger log = new EventLogger (f);
				f.DialogResult = DialogResult.None;
				f.ShowDialog ();

				Assert.AreEqual ("Bombed", f.Reason, "#C0");
				Assert.AreEqual (1, log.CountEvents ("Closing"), "#C1");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosing"), "#C2");
#endif
				Assert.AreEqual (1, log.CountEvents ("HandleDestroyed"), "#C3");

				Assert.AreEqual (1, log.CountEvents ("Closed"), "#C4");
#if NET_2_0
				Assert.AreEqual (1, log.CountEvents ("FormClosed"), "#C5");
#endif
				Assert.AreEqual (0, log.CountEvents ("Disposed"), "#C6");
				
				Assert.AreEqual (DialogResult.Cancel, f.DialogResult, "#C7");
			}
		}

		void Form_VisibleChanged1 (object sender, EventArgs e)
		{
			TimeBombedForm f = (TimeBombedForm) sender;
			f.Reason = "VisibleChanged";
			f.Visible = false;
		}

		void Form_VisibleChanged2 (object sender, EventArgs e)
		{
			TimeBombedForm f = (TimeBombedForm) sender;
			f.Reason = "VisibleChanged";
			f.Visible = false;
			f.DialogResult = DialogResult.OK;
			Assert.IsFalse (f.Visible);
		}

		[Test]
		public void DialogOwnerTest ()
		{
			using (Form first = new Form ()) {
				using (TimeBombedForm second = new TimeBombedForm ()) {
					first.Show ();
					second.Load += new EventHandler (second_Load);
					second.ShowDialog ();
				}
			}
		}

		void second_Load (object sender, EventArgs e)
		{
			Form second = (Form) sender;
			Assert.IsNull (second.Owner, "#1");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void FormStartupPositionChangeTest ()
		{
			using (Form frm = new Form ())
			{
				frm.ShowInTaskbar = false;
				frm.StartPosition = FormStartPosition.Manual;
				frm.Location = new Point (0, 0);
				frm.Show ();

				// On X there seem to be pending messages in the queue aren't processed
				// before Show returns, so process them. Otherwise the Location returns
				// something like (5,23)
				Application.DoEvents ();
				
				Assert.AreEqual ("{X=0,Y=0}", frm.Location.ToString (), "#01");

				frm.StartPosition = FormStartPosition.CenterParent;
				Assert.AreEqual ("{X=0,Y=0}", frm.Location.ToString (), "#02");

				frm.StartPosition = FormStartPosition.CenterScreen;
				Assert.AreEqual ("{X=0,Y=0}", frm.Location.ToString (), "#03");

				frm.StartPosition = FormStartPosition.Manual;
				Assert.AreEqual ("{X=0,Y=0}", frm.Location.ToString (), "#04");

				frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
				Assert.AreEqual ("{X=0,Y=0}", frm.Location.ToString (), "#05");

				frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
				Assert.AreEqual ("{X=0,Y=0}", frm.Location.ToString (), "#06");
			}
		}
		
		[Test]
		public void FormStartupPositionTest ()
		{
			CreateParams cp;
			
			using (Form frm = new Form ())
			{
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$01");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#01");

				frm.StartPosition = FormStartPosition.CenterParent;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$01");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#02");

				frm.StartPosition = FormStartPosition.CenterScreen;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$01");
				Assert.AreEqual (new Point (Screen.PrimaryScreen.WorkingArea.Width / 2 - frm.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - frm.Height / 2).ToString (), new Point (cp.X, cp.Y).ToString (), "#03");

				frm.StartPosition = FormStartPosition.Manual;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$01");
				Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#04");

				frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$01");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#05");

				frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$01");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#06");
				
			}


			using (Form frm = new Form ()) {
				frm.Location = new Point (23, 45);

				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$A1");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A1");

				frm.StartPosition = FormStartPosition.CenterParent;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$A2");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A2");

				frm.StartPosition = FormStartPosition.CenterScreen;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$A3");
				Assert.AreEqual (new Point (Screen.PrimaryScreen.WorkingArea.Width / 2 - frm.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - frm.Height / 2).ToString (), new Point (cp.X, cp.Y).ToString (), "#A3");

				frm.StartPosition = FormStartPosition.Manual;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$A4");
				Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A4");

				frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$A5");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A5");

				frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
				cp = TestHelper.GetCreateParams (frm);
				Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$A6");
				Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A6");
				

			}
		}
		
		[Test]
		public void MdiFormStartupPositionTest ()
		{
			CreateParams cp;
			using (Form Main = new Form ()) {
				Main.IsMdiContainer = true;
				Main.ShowInTaskbar = false;
				Main.Show ();
				
				using (Form frm = new Form ()) {
					frm.MdiParent = Main;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#01");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#02");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#03");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#04");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#05");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#06");
					frm.Show ();
				}


				using (Form frm = new Form ()) {
					frm.MdiParent = Main;
					frm.Location = new Point (23, 45);

					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$A1");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A1");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$A2");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A2");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$A3");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#A3");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$A4");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A4");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$A5");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A5");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$A6");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#A6");

					frm.Show ();
				}



				using (Form frm = new Form ()) {
					frm.MdiParent = Main;
					frm.Location = new Point (34, 56);

					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$B1");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#B1");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$B2");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#B2");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$B3");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#B3");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$B4");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B4");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$B5");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#B5");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$B6");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#B6");

					frm.Show ();
				}

				Main.Size = new Size (600, 600);
				using (Form frm = new Form ()) {
					frm.MdiParent = Main;
					frm.Location = new Point (34, 56);

					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$C1");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#C1");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$C2");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#C2");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$C3");
					Assert.AreEqual (new Point (Main.Controls [0].ClientSize.Width / 2 - frm.Width / 2, Main.Controls [0].ClientSize.Height / 2 - frm.Height / 2).ToString (), new Point (cp.X, cp.Y).ToString (), "#C3");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$C4");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C4");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$C5");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#C5");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$C6");
					Assert.AreEqual (new Point (int.MinValue, int.MinValue).ToString (), new Point (cp.X, cp.Y).ToString (), "#C6");

					frm.Show ();
				}
			}
		}

		[Test]
		public void ParentedFormStartupPositionTest ()
		{
			CreateParams cp;
			using (Form Main = new Form ()) {
				Main.ShowInTaskbar = false;
				Main.Show ();

				using (Form frm = new Form ()) {
					frm.TopLevel = false;
					Main.Controls.Add (frm);
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$01");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#01");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$02");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#02");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$03");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#03");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$04");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#04");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$05");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#05");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$06");
					Assert.AreEqual (new Point (0, 0).ToString (), new Point (cp.X, cp.Y).ToString (), "#06");
					frm.Show ();
				}


				using (Form frm = new Form ()) {
					frm.TopLevel = false;
					Main.Controls.Add (frm);
					frm.Location = new Point (23, 45);

					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$A1");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A1");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$A2");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A2");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$A3");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A3");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$A4");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A4");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$A5");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A5");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$A6");
					Assert.AreEqual (new Point (23, 45).ToString (), new Point (cp.X, cp.Y).ToString (), "#A6");

					frm.Show ();
				}



				using (Form frm = new Form ()) {
					frm.TopLevel = false;
					Main.Controls.Add (frm);
					frm.Location = new Point (34, 56);

					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$B1");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B1");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$B2");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B2");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$B3");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B3");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$B4");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B4");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$B5");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B5");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$B6");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#B6");

					frm.Show ();
				}


				Main.Size = new Size (600, 600);
				using (Form frm = new Form ()) {
					frm.TopLevel = false;
					Main.Controls.Add (frm);
					frm.Location = new Point (34, 56);

					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$C1");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C1");

					frm.StartPosition = FormStartPosition.CenterParent;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterParent, frm.StartPosition, "$C2");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C2");

					frm.StartPosition = FormStartPosition.CenterScreen;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.CenterScreen, frm.StartPosition, "$C3");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C3");

					frm.StartPosition = FormStartPosition.Manual;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.Manual, frm.StartPosition, "$C4");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C4");

					frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, frm.StartPosition, "$C5");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C5");

					frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
					cp = TestHelper.GetCreateParams (frm);
					Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, frm.StartPosition, "$C6");
					Assert.AreEqual (new Point (34, 56).ToString (), new Point (cp.X, cp.Y).ToString (), "#C6");

					frm.Show ();
				}
			}
		}
		
		[Test] // bug #80791
		public void ClientSizeTest ()
		{
			Form form = new Form ();
			Assert.IsFalse (form.ClientSize == form.Size);
		}

		[Test] // bug #80574
		[Category ("NotWorking")]
		public void FormBorderStyleTest ()
		{
			Form form = new Form ();
			Rectangle boundsBeforeBorderStyleChange = form.Bounds;
			Rectangle clientRectangleBeforeBorderStyleChange = form.ClientRectangle;
			form.FormBorderStyle = FormBorderStyle.None;
			Assert.AreEqual (form.Bounds, boundsBeforeBorderStyleChange, "#A1");
			Assert.AreEqual (form.ClientRectangle, clientRectangleBeforeBorderStyleChange, "#A2");

			form.Visible = true;
			form.FormBorderStyle = FormBorderStyle.Sizable;
			boundsBeforeBorderStyleChange = form.Bounds;
			clientRectangleBeforeBorderStyleChange = form.ClientRectangle;
			form.FormBorderStyle = FormBorderStyle.None;
			Assert.IsFalse (form.Bounds == boundsBeforeBorderStyleChange, "#B1");
			Assert.AreEqual (form.ClientRectangle, clientRectangleBeforeBorderStyleChange, "#B2");

			form.Visible = false;
			form.FormBorderStyle = FormBorderStyle.Sizable;
			boundsBeforeBorderStyleChange = form.Bounds;
			clientRectangleBeforeBorderStyleChange = form.ClientRectangle;
			form.FormBorderStyle = FormBorderStyle.None;
			Assert.IsFalse (form.Bounds == boundsBeforeBorderStyleChange, "#C1");
			Assert.AreEqual (form.ClientRectangle, clientRectangleBeforeBorderStyleChange, "#C2");
		}

		[Test]
		[Category ("NotWorking")]
		public void MaximizedParentedFormTest ()
		{
			using (Form Main = new Form ()) {
				Form Child = new Form ();
				Child.TopLevel = false;
				Main.Controls.Add (Child);
				Main.ShowInTaskbar = false;
				Main.Show ();
				
				Child.WindowState = FormWindowState.Maximized;
				Child.Visible = true;
				// The exact negative value depends on the border with, but it should always be < 0.
				Assert.IsTrue (Child.Location.X < 0 && Child.Location.Y < 0, "#A1");
			}
		}
		[Test]
		[Category ("NotWorking")]
		public void ParentedFormEventTest ()
		{

			using (Form Main = new Form ()) {
				Form Child = new Form ();
				Child.TopLevel = false;
				Child.Visible = true;
				Main.ShowInTaskbar = false;
				Main.Show ();

				EventLogger log = new EventLogger (Child);
				Assert.AreEqual (true, Child.Visible, "#A0");
				Main.Controls.Add (Child);
				Assert.AreEqual (true, Child.Visible, "#B0");
				Assert.AreEqual ("ParentChanged;BindingContextChanged;Layout;VisibleChanged;BindingContextChanged;BindingContextChanged", log.EventsJoined (), "#B1");
			}
		}
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void FormCreateParamsStyleTest ()
		{
			Form frm;
			
			using (frm = new Form ()) {
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles) TestHelper.GetCreateParams (frm).Style), "#01-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles) TestHelper.GetCreateParams (frm).ExStyle), "#01-ExStyle");
			}

			using (frm = new Form ()) {
				frm.AllowDrop = !frm.AllowDrop;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#02-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#02-ExStyle");
			}

			using (frm = new Form ()) {
				frm.AllowTransparency = !frm.AllowTransparency;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#03-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_LAYERED, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#03-ExStyle");
			}

			using (frm = new Form ()) {
				frm.Opacity = 0.50;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#04-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_LAYERED, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#04-ExStyle");
			}

			using (frm = new Form ()) {
				frm.TransparencyKey = Color.Cyan;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#05-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_LAYERED, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#05-ExStyle");
			}
			
			using (frm = new Form ()) {
				frm.CausesValidation = !frm.CausesValidation;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#06-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#06-ExStyle");
			}

			using (frm = new Form ()) {
				frm.ControlBox = !frm.ControlBox;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_GROUP | WindowStyles.WS_THICKFRAME | WindowStyles.WS_BORDER | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#07-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#07-ExStyle");
			}

			using (frm = new Form ()) {
				frm.Enabled = true;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#08-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#08-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.Fixed3D;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_GROUP | WindowStyles.WS_SYSMENU | WindowStyles.WS_CAPTION | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#10-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CLIENTEDGE | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#10-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.FixedDialog;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_GROUP | WindowStyles.WS_SYSMENU | WindowStyles.WS_CAPTION | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#11-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_DLGMODALFRAME | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#11-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.FixedSingle;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_GROUP | WindowStyles.WS_SYSMENU | WindowStyles.WS_CAPTION | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#12-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#12-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_GROUP | WindowStyles.WS_SYSMENU | WindowStyles.WS_CAPTION | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#13-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#13-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.None;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#14-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#14-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.Sizable;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#15-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#15-ExStyle");
			}

			using (frm = new Form ()) {
				frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#16-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#16-ExStyle");
			}

			using (frm = new Form ()) {
				frm.HelpButton = !frm.HelpButton;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#17-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#17-ExStyle");
			}

			using (frm = new Form ()) {
				frm.Icon = null;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#18-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#18-ExStyle");
			}

			using (frm = new Form ()) {
				frm.Icon = SystemIcons.Asterisk;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#19-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#19-ExStyle");
			}

			using (frm = new Form ()) {
				frm.IsMdiContainer = !frm.IsMdiContainer;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#20-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#20-ExStyle");
			}

			using (frm = new Form ()) {
				frm.MaximizeBox = !frm.MaximizeBox;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_GROUP | WindowStyles.WS_THICKFRAME | WindowStyles.WS_SYSMENU | WindowStyles.WS_CAPTION | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#21-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#21-ExStyle");
			}

			using (frm = new Form ()) {
				frm.MinimizeBox = !frm.MinimizeBox;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_THICKFRAME | WindowStyles.WS_SYSMENU | WindowStyles.WS_CAPTION | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#22-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#22-ExStyle");
			}
#if NET_2_0
			using (frm = new Form ()) {
				frm.ShowIcon = !frm.ShowIcon;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#23-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_DLGMODALFRAME | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#23-ExStyle");
			}
#endif		
			using (frm = new Form ()) {
				frm.ShowInTaskbar = !frm.ShowInTaskbar;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#24-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#24-ExStyle");
			}


			using (frm = new Form ()) {
				frm.TabStop = !frm.TabStop;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#25-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#25-ExStyle");
			}

			using (frm = new Form ()) {
				frm.TopLevel = !frm.TopLevel;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CHILD, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#26-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#26-ExStyle");
			}

			using (frm = new Form ()) {
				frm.Visible = !frm.Visible;
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TILEDWINDOW | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#27-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#27-ExStyle");
			}

			using (frm = new Form ()) {
				frm.ControlBox = false;
				frm.Text = "";
				Assert.AreEqual (WindowStyles.WS_TILED | WindowStyles.WS_TABSTOP | WindowStyles.WS_GROUP | WindowStyles.WS_THICKFRAME | WindowStyles.WS_BORDER | WindowStyles.WS_CLIPCHILDREN, ((WindowStyles)TestHelper.GetCreateParams (frm).Style), "#28-Style");
				Assert.AreEqual (WindowExStyles.WS_EX_LEFT | WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_CONTROLPARENT | WindowExStyles.WS_EX_APPWINDOW, ((WindowExStyles)TestHelper.GetCreateParams (frm).ExStyle), "#28-ExStyle");
			}
		}
		
		[Test]
		public void FormParentedTest ()
		{
			using (Form frm = new Form ()) {
				using (Form frm2 = new Form ()) {
					frm2.TopLevel = false;
					frm.ShowInTaskbar = false;
					frm2.ShowInTaskbar = false;
					frm2.Visible = true;
					frm.Visible = true;
					
					EventLogger log = new EventLogger (frm);
					EventLogger log2 = new EventLogger (frm2);
					
					frm.Controls.Add (frm2);

					Assert.IsTrue (log2.EventRaised ("ParentChanged"), "#C1");
					Assert.IsTrue (log.EventRaised ("ControlAdded"), "#P1");
					Assert.AreSame (frm, frm2.Parent, "#02");
				}
			}
		}
		
		[Test]
		public void FormPropertyTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			Assert.IsNull (myform.AcceptButton, "#1");
			Assert.IsNull (myform.ActiveMdiChild, "#2"); 
			Assert.IsFalse (myform.AutoScale, "#3");
			Assert.IsNull (myform.CancelButton, "#6");
			Assert.IsTrue (myform.ControlBox, "#9");
			Assert.IsTrue (myform.DesktopBounds.X > 0, "#10a");
			Assert.IsTrue (myform.DesktopBounds.Y > 0, "#10b");
			Assert.AreEqual (300, myform.DesktopBounds.Height, "#10c");
			Assert.AreEqual (300, myform.DesktopBounds.Width, "#10d");
			Assert.IsTrue (myform.DesktopLocation.X > 0, "#11a");
			Assert.IsTrue (myform.DesktopLocation.Y > 0, "#11b");
			Assert.AreEqual (DialogResult.None, myform.DialogResult, "#12");
			Assert.AreEqual (FormBorderStyle.Sizable, myform.FormBorderStyle, "#13");
			Assert.IsFalse (myform.HelpButton, "#14");
			Assert.AreEqual ("System.Drawing.Icon", myform.Icon.GetType ().ToString (), "#15");
			Assert.IsFalse (myform.IsMdiChild, "#16");
			Assert.IsFalse (myform.IsMdiContainer, "#17");
			Assert.IsFalse (myform.KeyPreview, "#18");
			Assert.IsTrue (myform.MaximizeBox, "#19");
			Assert.AreEqual (0, myform.MaximumSize.Height, "#20a");
			Assert.AreEqual (0, myform.MaximumSize.Width, "#20b");
			Assert.AreEqual (0, myform.MdiChildren.Length, "#21a");
			Assert.AreEqual (1, myform.MdiChildren.Rank, "#21b");
			Assert.IsFalse (myform.MdiChildren.IsSynchronized, "#21c");
			Assert.IsNull (myform.MdiParent, "#22");
			Assert.IsNull (myform.Menu, "#23");
			Assert.IsNull (myform.MergedMenu, "#24");
			Assert.IsTrue (myform.MinimizeBox, "#25");
			Assert.AreEqual (0, myform.MinimumSize.Height, "#26a");
			Assert.AreEqual (0, myform.MinimumSize.Width, "#26b");
			Assert.IsTrue (myform.MinimumSize.IsEmpty, "#26c");
			Assert.IsFalse (myform.Modal, "#27");
			Assert.AreEqual (1, myform.Opacity, "#28");
			Assert.AreEqual (0, myform.OwnedForms.Length, "#29a");
			Assert.AreEqual (1, myform.OwnedForms.Rank, "#29b");
			Assert.IsNull (myform.Owner, "#30");
			Assert.IsTrue (myform.ShowInTaskbar, "#31");
			Assert.AreEqual (300, myform.Size.Height, "#32a");
			Assert.AreEqual (300, myform.Size.Width, "#32b");
			Assert.AreEqual (SizeGripStyle.Auto, myform.SizeGripStyle, "#33");
			Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, myform.StartPosition, "#34");
			Assert.IsTrue (myform.TopLevel, "#35");
			Assert.IsFalse (myform.TopMost, "#36");
			Assert.AreEqual (Color.Empty, myform.TransparencyKey, "#37");
			Assert.AreEqual (FormWindowState.Normal, myform.WindowState, "#38");
			Assert.AreEqual (ImeMode.NoControl, myform.ImeMode, "#39");
			myform.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ActivateTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.Activate ();
			Assert.AreEqual (true, myform.Focus (), "#40");
			myform.Dispose ();
		}

		[Test]
		public void AddOwnedFormTest ()
		{
			Form parent = new Form ();
			parent.ShowInTaskbar = false;
			parent.Text = "NewParent";
			Form ownedForm = new Form ();
			ownedForm.ShowInTaskbar = false;
			ownedForm.Text = "Owned Form";
			parent.AddOwnedForm (ownedForm);
			ownedForm.Show ();
			Assert.AreEqual ("NewParent", ownedForm.Owner.Text, "#41");
			ownedForm.Dispose ();
			parent.Dispose ();
		}

		[Test] // bug #80020
		public void IsHandleCreated ()
		{
			Form main = new Form ();
			main.Name = "main";
			main.IsMdiContainer = true;
			main.ShowInTaskbar = false;
			Assert.IsFalse (main.IsHandleCreated, "#1");

			Form child = new Form ();
			child.MdiParent = main;
			child.WindowState = FormWindowState.Maximized;
			Assert.IsFalse (main.IsHandleCreated, "#2");

			child.Show ();
			Assert.IsFalse (child.IsHandleCreated, "#3");
			Assert.IsFalse (main.IsHandleCreated, "#4");

			main.Show ();
			Assert.IsTrue (child.IsHandleCreated, "#5");
			Assert.IsTrue (main.IsHandleCreated, "#6");

			child.Dispose ();
			main.Dispose ();
		}

		[Test]
		public void RemoveOwnedFormTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.RemoveOwnedForm (myform);
			myform.Show ();
			Assert.AreEqual (null, myform.Owner, "#44");
			myform.Dispose ();
		}

		[Test]
		public void SetDesktopBoundsTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.SetDesktopBounds (10, 10, 200 , 200);
			Assert.AreEqual (200, myform.DesktopBounds.Height, "#45");
			myform.Dispose ();
		}

		[Test]
		public void SetDesktopLocationTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.SetDesktopLocation (10, 10);
			Assert.AreEqual (10, myform.DesktopLocation.X, "#46");
			myform.Dispose ();
		}

		[Test]
		public void SetDialogResultOutOfRange ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			try {
				myform.DialogResult = (DialogResult) (-1);
				Assert.Fail ("#48");
			} catch (InvalidEnumArgumentException) {
			}

			try {
				myform.DialogResult = (DialogResult) ((int) DialogResult.No + 1);
				Assert.Fail ("#49");
			} catch (InvalidEnumArgumentException) {
			}
			myform.Dispose ();
		}

		void myform_set_dialogresult (object sender, EventArgs e)
		{
			Form f = (Form)sender;

			f.DialogResult = DialogResult.OK;
		}

		void myform_close (object sender, EventArgs e)
		{
			Form f = (Form)sender;

			f.Close();
		}

		[Test]
		public void SetDialogResult ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;

			myform.DialogResult = DialogResult.Cancel;

			Assert.IsTrue (myform.Visible, "A1");
			Assert.IsFalse (myform.IsDisposed, "A2");

			myform.Close ();

			Assert.IsFalse (myform.Visible, "A3");
			Assert.IsTrue (myform.IsDisposed, "A4");

			DialogResult result;

			myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.VisibleChanged += new EventHandler (myform_set_dialogresult);
			result = myform.ShowDialog ();

			Assert.AreEqual (result, DialogResult.OK, "A5");
			Assert.IsFalse (myform.Visible, "A6");
			Assert.IsFalse (myform.IsDisposed, "A7");
			myform.Dispose ();
			
			myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.VisibleChanged += new EventHandler (myform_close);
			result = myform.ShowDialog ();

			Assert.AreEqual (result, DialogResult.Cancel, "A8");
			Assert.IsFalse (myform.Visible, "A9");
			Assert.IsFalse (myform.IsDisposed, "A10");
			
			myform.Dispose ();
		}

		[Test]
		public void ShowDialog_Child ()
		{
			Form main = new Form ();
			main.IsMdiContainer = true;
			Form child = new Form ();
			child.MdiParent = main;
			try {
				child.ShowDialog ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Forms that are not top level forms cannot be displayed as a
				// modal dialog. Remove the form from any parent form before 
				// calling ShowDialog.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			Assert.IsNull (child.Owner, "#5");
			child.Dispose ();
			main.Dispose ();
		}

		[Test]
		public void ShowDialog_Disabled ()
		{
			Form form = new Form ();
			form.Enabled = false;
			try {
				form.ShowDialog ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Forms that are not enabled cannot be displayed as a modal
				// dialog. Set the form's enabled property to true before
				// calling ShowDialog.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}
			Assert.IsNull (form.Owner, "#A5");
			form.Dispose ();

			Form main = new Form ();
			form = new Form ();
			form.Owner = main;
			form.Enabled = false;
			try {
				form.ShowDialog ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException) {
			}
			Assert.IsNotNull (form.Owner, "#B2");
			Assert.AreSame (main, form.Owner, "#B3");
			form.Dispose ();
			main.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ShowDialog_Owner_Circular ()
		{
			Form main = new Form ();
			Form child = new Form ();
			child.Owner = main;
			try {
				main.ShowDialog (child);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// A circular control reference has been made. A control cannot
				// be owned or parented to itself
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
			Assert.IsNull (main.Owner, "#6");
			main.Dispose ();
			child.Dispose ();
		}

		[Test] // bug #80773
		public void ShowDialog_Owner_Self ()
		{
			Form form = new Form ();
			try {
				form.ShowDialog (form);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Forms cannot own themselves or their owners
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("owner", ex.ParamName, "#A6");
			}
			Assert.IsNull (form.Owner, "#A7");
			form.Dispose ();

			Form main = new Form ();
			form = new Form ();
			form.Owner = main;
			try {
				form.ShowDialog (form);
				Assert.Fail ("#B1");
			} catch (ArgumentException) {
			}
			Assert.IsNotNull (form.Owner);
			Assert.AreSame (main, form.Owner, "#B2");
			form.Dispose ();
			main.Dispose ();
		}

		[Test]
		public void ShowDialog_Visible ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Visible = true;
			try {
				form.ShowDialog ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Forms that are already visible cannot be displayed as a modal
				// dialog. Set the form's visible property to false before 
				// calling ShowDialog.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}
			Assert.IsNull (form.Owner, "#A5");
			form.Dispose ();

			Form main = new Form ();
			form = new Form ();
			form.Owner = main;
			form.Visible = true;
			try {
				form.ShowDialog ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException) {
			}
			Assert.IsNotNull (form.Owner, "#B2");
			Assert.AreSame (main, form.Owner, "#B3");
			form.Dispose ();
			main.Dispose ();
		}

		[Test] // bug #80604
		public void VisibleOnLoad ()
		{
			MockForm form = new MockForm ();
			form.CloseOnLoad = true;
			Application.Run (form);
			Assert.IsTrue (form.VisibleOnLoad, "#1");
			form.Dispose ();

			form = new MockForm ();
			form.ShowInTaskbar = false;
			form.Show ();
			Assert.IsTrue (form.VisibleOnLoad, "#2");
			form.Dispose ();
		}

		[Test] // bug #80052
		[Category ("NotWorking")]
		public void Location ()
		{
			// 
			// CenterParent
			// 

			Form formA = new Form ();
			formA.ShowInTaskbar = false;
			formA.StartPosition = FormStartPosition.CenterParent;
			formA.Location = new Point (151, 251);
			formA.Show ();

			Assert.AreEqual (FormStartPosition.CenterParent, formA.StartPosition, "#A1");
			Assert.IsFalse (formA.Location.X == 151, "#A2");
			Assert.IsFalse (formA.Location.Y == 251, "#A3");

			formA.Location = new Point (311, 221);

			Assert.AreEqual (FormStartPosition.CenterParent, formA.StartPosition, "#A4");
			Assert.AreEqual (311, formA.Location.X, "#A5");
			Assert.AreEqual (221, formA.Location.Y, "#A6");

			formA.Dispose ();

			// 
			// CenterScreen
			// 

			Form formB = new Form ();
			formB.ShowInTaskbar = false;
			formB.StartPosition = FormStartPosition.CenterScreen;
			formB.Location = new Point (151, 251);
			formB.Show ();

			Assert.AreEqual (FormStartPosition.CenterScreen, formB.StartPosition, "#B1");
			Assert.IsFalse (formB.Location.X == 151, "#B2");
			Assert.IsFalse (formB.Location.Y == 251, "#B3");

			formB.Location = new Point (311, 221);

			Assert.AreEqual (FormStartPosition.CenterScreen, formB.StartPosition, "#B4");
			Assert.AreEqual (311, formB.Location.X, "#B5");
			Assert.AreEqual (221, formB.Location.Y, "#B6");

			formB.Dispose ();

			// 
			// Manual
			// 

			Form formC = new Form ();
			formC.ShowInTaskbar = false;
			formC.StartPosition = FormStartPosition.Manual;
			formC.Location = new Point (151, 251);
			formC.Show ();

			Assert.AreEqual (FormStartPosition.Manual, formC.StartPosition, "#C1");
			Assert.AreEqual (151, formC.Location.X, "#C2");
			Assert.AreEqual (251, formC.Location.Y, "#C3");

			formC.Location = new Point (311, 221);

			Assert.AreEqual (FormStartPosition.Manual, formC.StartPosition, "#C4");
			Assert.AreEqual (311, formC.Location.X, "#C5");
			Assert.AreEqual (221, formC.Location.Y, "#C6");

			formC.Dispose ();

			// 
			// WindowsDefaultBounds
			// 

			Form formD = new Form ();
			formD.ShowInTaskbar = false;
			formD.StartPosition = FormStartPosition.WindowsDefaultBounds;
			formD.Location = new Point (151, 251);
			formD.Show ();

			Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, formD.StartPosition, "#D1");
			Assert.IsFalse (formD.Location.X == 151, "#D2");
			Assert.IsFalse (formD.Location.Y == 251, "#D3");

			formD.Location = new Point (311, 221);

			Assert.AreEqual (FormStartPosition.WindowsDefaultBounds, formD.StartPosition, "#D4");
			Assert.AreEqual (311, formD.Location.X, "#D5");
			Assert.AreEqual (221, formD.Location.Y, "#D6");

			formD.Dispose ();

			// 
			// WindowsDefaultLocation
			// 

			Form formE = new Form ();
			formE.ShowInTaskbar = false;
			formE.Location = new Point (151, 251);
			formE.Show ();

			Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, formE.StartPosition, "#E1");
			Assert.IsFalse (formE.Location.X == 151, "#E2");
			Assert.IsFalse (formE.Location.Y == 251, "#E3");

			formE.Location = new Point (311, 221);

			Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, formE.StartPosition, "#E4");
			Assert.AreEqual (311, formE.Location.X, "#E5");
			Assert.AreEqual (221, formE.Location.Y, "#E6");

			formE.Dispose ();
		}

		[Test]
		public void DisposeOwnerTest ()
		{
			Form f1 = new Form ();
			Form f2 = new Form ();

			f2.Owner = f1;

			f1.Dispose ();

			Assert.IsNull (f2.Owner, "1");
			Assert.AreEqual (0, f1.OwnedForms.Length, "2");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void AccessDisposedForm ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;

			myform.Show ();
			myform.Close (); // this should result in the form being disposed
			myform.Show (); // and this line should result in the ODE being thrown
		}

		class MyForm : Form
		{
			public void DoDestroyHandle ()
			{
				DestroyHandle();
			}
			public void DoRecreateHandle ()
			{
				RecreateHandle();
			}
		}

		int handle_destroyed_count;
		void handle_destroyed (object sender, EventArgs e)
		{
			handle_destroyed_count++;
		}

		[Test]
		public void DestroyHandleTest ()
		{
			handle_destroyed_count = 0;

			MyForm f1 = new MyForm ();
			f1.HandleDestroyed += new EventHandler (handle_destroyed);
			f1.Show ();
			f1.DoDestroyHandle ();
			Assert.AreEqual (1, handle_destroyed_count, "1");

			MyForm f2 = new MyForm ();
			f2.HandleDestroyed += new EventHandler (handle_destroyed);
			f2.Show ();
			f2.DoRecreateHandle ();
			Assert.AreEqual (2, handle_destroyed_count, "2");
			
			f1.Dispose ();
			f2.Dispose ();
		}

		[Test]
		public void FormClose ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;

			Assert.IsFalse (myform.Visible, "A1");
			Assert.IsFalse (myform.IsDisposed, "A2");

			myform.Close ();

			Assert.IsFalse (myform.Visible, "A3");
			Assert.IsFalse (myform.IsDisposed, "A4");

			myform.Show ();

			Assert.IsTrue (myform.Visible, "A5");
			Assert.IsFalse (myform.IsDisposed, "A6");

			myform.Close ();

			Assert.IsFalse (myform.Visible, "A7");
			Assert.IsTrue (myform.IsDisposed, "A8");
		}

		[Test]
		public void FormClose2 ()
		{
			WMCloseWatcher f = new WMCloseWatcher ();
			f.ShowInTaskbar = false;

			f.close_count = 0;
			Assert.IsFalse (f.Visible, "A1");
			f.Close ();
			Assert.AreEqual (0, f.close_count, "A2");


			f.Show ();
			f.Close ();
			Assert.AreEqual (1, f.close_count, "A3");
		}

		class WMCloseWatcher : Form {
			public int close_count;

			protected override void WndProc (ref Message msg) {
				if (msg.Msg == 0x0010 /* WM_CLOSE */) {
					close_count ++;
				}

				base.WndProc (ref msg);
			}
		}

		class SwallowOnActivated : Form {
			protected override void OnActivated (EventArgs e)
			{
				// do nothing
			}

			protected override void OnCreateControl () {
				base.OnCreateControl ();
			}
		}

		class EnterTest : Button {
			protected override void OnEnter (EventArgs e)
			{
				on_enter = true;
				base.OnEnter (e);
			}

			public bool on_enter;
		}

		[Test]
		public void OnActivateEventHandlingTest1 ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Relies on form.Show() synchronously generating WM_ACTIVATE");
			}

			SwallowOnActivated f = new SwallowOnActivated ();

			f.ShowInTaskbar = false;

			EnterTest c = new EnterTest ();
			f.Controls.Add (c);

			f.Show ();

			Assert.IsTrue (c.on_enter, "1");

			f.Dispose ();
		}
		
#if NET_2_0
		[Test]
		public void FormClosingEvents ()
		{
			// Standard Close
			Form f = new Form ();
			string events = string.Empty;

			f.Closing += new CancelEventHandler (delegate (Object obj, CancelEventArgs e) { events += ("Closing;"); });
			f.FormClosing += new FormClosingEventHandler (delegate (Object obj, FormClosingEventArgs e) { events += string.Format ("FormClosing [Reason:{0} - Cancel:{1}]", e.CloseReason, e.Cancel); });
	
			f.Show ();
			f.Close ();
			
			Assert.AreEqual ("Closing;FormClosing [Reason:UserClosing - Cancel:False]", events, "A1");			
		}

		[Test]
		public void FormClosingEventsCancel ()
		{
			// Shows that setting Cancel in Closing flows through to FormClosing
			Form f = new Form ();
			string events = string.Empty;

			f.Closing += new CancelEventHandler (delegate (Object obj, CancelEventArgs e) { events += ("Closing;"); e.Cancel = true; });
			f.FormClosing += new FormClosingEventHandler (delegate (Object obj, FormClosingEventArgs e) { events += string.Format("FormClosing [Reason:{0} - Cancel:{1}]", e.CloseReason, e.Cancel); e.Cancel = false; });

			f.Show ();
			f.Close ();

			Assert.AreEqual ("Closing;FormClosing [Reason:UserClosing - Cancel:True]", events, "A1");
		}

		[Test]
		public void FormClosedEvents ()
		{
			// Standard Closed
			Form f = new Form ();
			string events = string.Empty;

			f.Closed += new EventHandler (delegate (Object obj, EventArgs e) { events += ("Closed;"); });
			f.FormClosed += new FormClosedEventHandler (delegate (Object obj, FormClosedEventArgs e) { events += string.Format ("FormClosed [Reason:{0}]", e.CloseReason); });

			f.Show ();
			f.Close ();

			Assert.AreEqual ("Closed;FormClosed [Reason:UserClosing]", events, "A1");
		}

		[Test]
		public void ShowWithOwner ()
		{
			Form f = new Form ();
			Button b = new Button ();
			f.Controls.Add (b);

			Form f2 = new Form ();

			f2.Show (f);

			Assert.AreSame (f, f2.Owner, "A1");
			f2.Close ();

			f2 = new Form ();

			f2.Show (b);
			Assert.AreSame (f, f2.Owner, "A2");
			f2.Close ();
			
			Button b2 = new Button ();
			f2 = new Form ();

			f2.Show (b2);
			Assert.AreEqual (null, f2.Owner, "A3");
			f2.Close ();

			f2 = new Form ();
			f2.Show (null);
			Assert.AreEqual (null, f2.Owner, "A4");
			f2.Close ();
			
			f.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ShowWithOwnerIOE ()
		{
			using (Form f = new Form ()) {
				f.Show (f);
			}
		}
		
		[Test]	// Bug #79959, #80574, #80791
		[Category ("NotWorking")]
		public void BehaviorResizeOnBorderStyleChanged ()
		{
			// Marked NotWorking because the ClientSize is dependent on the WM.
			// The values below match XP Luna to make sure our behavior is the same.
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Show ();

			Assert.AreEqual (true, f.IsHandleCreated, "A0");

			Assert.AreEqual (new Size (300, 300), f.Size, "A1");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A2");

			f.FormBorderStyle = FormBorderStyle.Fixed3D;
			Assert.AreEqual (new Size (302, 302), f.Size, "A3");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A4");

			f.FormBorderStyle = FormBorderStyle.FixedDialog;
			Assert.AreEqual (new Size (298, 298), f.Size, "A5");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A6");

			f.FormBorderStyle = FormBorderStyle.FixedSingle;
			Assert.AreEqual (new Size (298, 298), f.Size, "A7");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A8");

			f.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			Assert.AreEqual (new Size (298, 290), f.Size, "A9");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A0");

			f.FormBorderStyle = FormBorderStyle.None;
			Assert.AreEqual (new Size (292, 266), f.Size, "A11");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A12");

			f.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			Assert.AreEqual (new Size (300, 292), f.Size, "A13");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A14");

			f.FormBorderStyle = FormBorderStyle.Sizable;
			Assert.AreEqual (new Size (300, 300), f.Size, "A15");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A16");
			
			f.Close ();
		}

		[Test]  // Bug #80574, #80791
		[Category ("NotWorking")]
		public void BehaviorResizeOnBorderStyleChangedNotVisible ()
		{
			// Marked NotWorking because the ClientSize is dependent on the WM.
			// The values below match XP Luna to make sure our behavior is the same.
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Assert.AreEqual (false, f.IsHandleCreated, "A0");
			
			Assert.AreEqual (new Size (300, 300), f.Size, "A1");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A2");

			f.FormBorderStyle = FormBorderStyle.Fixed3D;
			Assert.AreEqual (new Size (300, 300), f.Size, "A3");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A4");

			f.FormBorderStyle = FormBorderStyle.FixedDialog;
			Assert.AreEqual (new Size (300, 300), f.Size, "A5");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A6");

			f.FormBorderStyle = FormBorderStyle.FixedSingle;
			Assert.AreEqual (new Size (300, 300), f.Size, "A7");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A8");

			f.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			Assert.AreEqual (new Size (300, 300), f.Size, "A9");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A0");

			f.FormBorderStyle = FormBorderStyle.None;
			Assert.AreEqual (new Size (300, 300), f.Size, "A11");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A12");

			f.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			Assert.AreEqual (new Size (300, 300), f.Size, "A13");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A14");

			f.FormBorderStyle = FormBorderStyle.Sizable;
			Assert.AreEqual (new Size (300, 300), f.Size, "A15");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A16");
		}

		[Test]  // Bug #80574, #80791
		[Category ("NotWorking")]
		public void MoreBehaviorResizeOnBorderStyleChangedNotVisible ()
		{
			// Marked NotWorking because the ClientSize is dependent on the WM.
			// The values below match XP Luna to make sure our behavior is the same.
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Show ();
			f.Hide ();

			Assert.AreEqual (true, f.IsHandleCreated, "A0");

			f.FormBorderStyle = FormBorderStyle.Sizable;
			Assert.AreEqual (new Size (300, 300), f.Size, "A1");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A2");
			f.FormBorderStyle = FormBorderStyle.None;
			Assert.AreEqual (new Size (292, 266), f.Size, "A3");
			Assert.AreEqual (new Size (292, 266), f.ClientSize, "A4");
		}
#endif
		
		[Test]  // Bug #81582
		[Category ("NotWorking")]
		public void GotFocusWithoutCallingOnLoadBase ()
		{
			NoOnLoadBaseForm f = new NoOnLoadBaseForm ();
			f.Show ();
			Assert.AreEqual (true, f.got_focus_called, "H1");
			f.Dispose ();
		}

		private class NoOnLoadBaseForm : Form
		{
			public bool got_focus_called = false;

			public NoOnLoadBaseForm ()
			{
				TreeView tv = new TreeView ();
				tv.GotFocus += new EventHandler (tv_GotFocus);
				Controls.Add (tv);
			}

			void tv_GotFocus (object sender, EventArgs e)
			{
				got_focus_called = true;
			}

			protected override void OnLoad (EventArgs e)
			{
			}
		}

		[Test]  // Bug #80773
		public void DontSetOwnerOnShowDialogException ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			try { f.ShowDialog (f); }
			catch {	}
			
			Assert.AreEqual (null, f.Owner, "H1");

			f.Dispose ();
		}

		[Test]
		public void MinimumWindowSize ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Show ();
			
			f.Size = new Size (0, 0);
			Assert.AreEqual (SystemInformation.MinimumWindowSize, f.Size);

			f.Dispose ();
		}

#if NET_2_0
		[Test]
		public void AutoSizeGrowOnly ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.AutoSize = true;

			Button b = new Button ();
			b.Size = new Size (200, 200);
			b.Location = new Point (200, 200);
			f.Controls.Add (b);

			f.Show ();

			Assert.AreEqual (new Size (403, 403), f.ClientSize, "A1");
		}

		[Test]
		public void AutoSizeReset ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Button b = new Button ();
			b.Size = new Size (200, 200);
			b.Location = new Point (200, 200);
			f.Controls.Add (b);

			f.Show ();

			Size start_size = f.ClientSize;

			f.AutoSize = true;
			Assert.AreEqual (new Size (403, 403), f.ClientSize, "A1");

			f.AutoSize = false;
			Assert.AreEqual (start_size, f.ClientSize, "A2");
		}

		[Test]
		public void AutoSizeGrowAndShrink ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.AutoSize = true;
			f.AutoSizeMode = AutoSizeMode.GrowAndShrink;

			f.Show ();

			Assert.AreEqual (SystemInformation.MinimumWindowSize, f.Size, "A1");

			Button b = new Button ();
			b.Size = new Size (200, 200);
			b.Location = new Point (0, 0);
			f.Controls.Add (b);

			Assert.AreEqual (new Size (203, 203), f.ClientSize, "A2");
		}
#endif

		private class MockForm : Form
		{
			public bool CloseOnLoad {
				get { return _closeOnLoad; }
				set { _closeOnLoad = value; }
			}

			public bool VisibleOnLoad {
				get { return _visibleOnLoad; }
			}

			protected override void OnLoad(EventArgs e) {
				base.OnLoad(e);
				_visibleOnLoad = Visible;
				if (CloseOnLoad)
					Close ();
			}

			private bool _closeOnLoad;
			private bool _visibleOnLoad;
		}	
	}

	public class TimeBombedForm : Form
	{
		public Timer timer;
		public bool CloseOnPaint;
		public string Reason;
		public TimeBombedForm ()
		{
			timer = new Timer ();
			timer.Interval = 500;
			timer.Tick += new EventHandler (timer_Tick);
			timer.Start ();
		}

		void timer_Tick (object sender, EventArgs e)
		{
			Reason = "Bombed";
			Close ();
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{
			base.OnPaint (pevent);
			if (CloseOnPaint) {
				Reason = "OnPaint";
				Close ();
			}
		}
	}
}
