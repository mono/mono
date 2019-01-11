//
// PictureBoxTest.cs: Test cases for PictureBox.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PictureBoxTest : TestHelper
	{
		[Test]
		public void PictureBoxPropertyTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			PictureBox myPicBox = new PictureBox ();
			myForm.Controls.Add (myPicBox);
			
			// B 
			Assert.AreEqual (BorderStyle.None, myPicBox.BorderStyle, "#B1");
			myPicBox.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (BorderStyle.Fixed3D, myPicBox.BorderStyle, "#B2");
			
			// P 
			Assert.AreEqual (PictureBoxSizeMode.Normal, myPicBox.SizeMode, "#P1");
			myPicBox.SizeMode = PictureBoxSizeMode.AutoSize;
			Assert.AreEqual (PictureBoxSizeMode.AutoSize, myPicBox.SizeMode, "#P2");

			myForm.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ImageLocation_Async ()
		{
			Form f = new Form ();
			PictureBox pb = new PictureBox ();
			f.Controls.Add (pb);
			f.Show ();

			Assert.IsNull (pb.ImageLocation, "#A");

			pb.ImageLocation = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");
			Application.DoEvents ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#B1");
			Assert.AreSame (pb.InitialImage, pb.Image, "#B2");

			using (Stream s = TestResourceHelper.GetStreamOfResource ("Test/resources/32x32.ico")) {
				pb.Image = Image.FromStream (s);
			}
			Application.DoEvents ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#C1");
			Assert.IsNotNull (pb.Image, "#C2");
			Assert.AreEqual (60, pb.Image.Height, "#C3");
			Assert.AreEqual (150, pb.Image.Width, "#C4");

			pb.ImageLocation = null;
			Application.DoEvents ();

			Assert.IsNull (pb.ImageLocation, "#D1");
			Assert.IsNull (pb.Image, "#D2");

			pb.ImageLocation = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");
			Application.DoEvents ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#E1");
			Assert.IsNull (pb.Image, "#E2");

			pb.Load ();
			Application.DoEvents ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#F1");
			Assert.IsNotNull (pb.Image, "#F2");
			Assert.AreEqual (60, pb.Image.Height, "#F3");
			Assert.AreEqual (150, pb.Image.Width, "#F4");

			pb.ImageLocation = null;
			Application.DoEvents ();

			Assert.IsNull (pb.ImageLocation, "#G1");
			Assert.IsNull (pb.Image, "#G2");

			pb.ImageLocation = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");
			pb.Load ();
			pb.ImageLocation = "XYZ.gif";
			Application.DoEvents ();

			Assert.AreEqual ("XYZ.gif", pb.ImageLocation, "#H1");
			Assert.IsNotNull (pb.Image, "#H2");
			Assert.AreEqual (60, pb.Image.Height, "#H3");
			Assert.AreEqual (150, pb.Image.Width, "#H4");

			pb.ImageLocation = string.Empty;
			Application.DoEvents ();

			Assert.AreEqual (string.Empty, pb.ImageLocation, "#I1");
			Assert.IsNull (pb.Image, "#I2");

			using (Stream s = TestResourceHelper.GetStreamOfResource ("Test/resources/32x32.ico")) {
				pb.Image = Image.FromStream (s);
			}
			Application.DoEvents ();

			Assert.AreEqual (string.Empty, pb.ImageLocation, "#J1");
			Assert.IsNotNull (pb.Image, "#J2");
			Assert.AreEqual (96, pb.Image.Height, "#J3");
			Assert.AreEqual (96, pb.Image.Width, "#J4");

			pb.Load (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
			Application.DoEvents ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#K1");
			Assert.IsNotNull (pb.Image, "#K2");
			Assert.AreEqual (60, pb.Image.Height, "#K3");
			Assert.AreEqual (150, pb.Image.Width, "#K4");

			pb.ImageLocation = null;
			Application.DoEvents ();

			Assert.IsNull (pb.ImageLocation, "#L1");
			Assert.IsNull (pb.Image, "#L2");

			f.Dispose ();
		}

		[Test]
		public void ImageLocation_Sync ()
		{
			Form f = new Form ();
			PictureBox pb = new PictureBox ();
			pb.WaitOnLoad = true;
			f.Controls.Add (pb);
			f.Show ();

			Assert.IsNull (pb.ImageLocation, "#A");

			pb.ImageLocation = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#B1");
			Assert.IsNotNull (pb.Image, "#B2");
			Assert.AreEqual (60, pb.Image.Height, "#B3");
			Assert.AreEqual (150, pb.Image.Width, "#B4");

			using (Stream s = TestResourceHelper.GetStreamOfResource ("Test/resources/32x32.ico")) {
				pb.Image = Image.FromStream (s);
			}

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#C1");
			Assert.IsNotNull (pb.Image, "#C2");
			Assert.AreEqual (96, pb.Image.Height, "#C3");
			Assert.AreEqual (96, pb.Image.Width, "#C4");

			pb.ImageLocation = null;

			Assert.IsNull (pb.ImageLocation, "#D1");
			Assert.IsNotNull (pb.Image, "#D2");
			Assert.AreEqual (96, pb.Image.Height, "#D3");
			Assert.AreEqual (96, pb.Image.Width, "#D4");

			pb.ImageLocation = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#E1");
			Assert.IsNotNull (pb.Image, "#E2");
			Assert.AreEqual (60, pb.Image.Height, "#E3");
			Assert.AreEqual (150, pb.Image.Width, "#E4");

			pb.Load ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#F1");
			Assert.IsNotNull (pb.Image, "#F2");
			Assert.AreEqual (60, pb.Image.Height, "#F3");
			Assert.AreEqual (150, pb.Image.Width, "#F4");

			pb.ImageLocation = null;

			Assert.IsNull (pb.ImageLocation, "#G1");
			Assert.IsNull (pb.Image, "#G2");

			using (Stream s = TestResourceHelper.GetStreamOfResource ("Test/resources/32x32.ico")) {
				pb.Image = Image.FromStream (s);
			}

			Assert.IsNull (pb.ImageLocation, "#H1");
			Assert.IsNotNull (pb.Image, "#H2");
			Assert.AreEqual (96, pb.Image.Height, "#H3");
			Assert.AreEqual (96, pb.Image.Width, "#H4");

			pb.Load (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#I1");
			Assert.IsNotNull (pb.Image, "#I2");
			Assert.AreEqual (60, pb.Image.Height, "#I3");
			Assert.AreEqual (150, pb.Image.Width, "#I4");

			pb.ImageLocation = string.Empty;

			Assert.AreEqual (string.Empty, pb.ImageLocation, "#J1");
			Assert.IsNull (pb.Image, "#J2");

			pb.ImageLocation = TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif");

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"), pb.ImageLocation, "#K1");
			Assert.IsNotNull (pb.Image, "#K2");
			Assert.AreEqual (60, pb.Image.Height, "#K3");
			Assert.AreEqual (150, pb.Image.Width, "#K4");

			try {
				pb.ImageLocation = "XYZ.gif";
				Assert.Fail ("#L1");
			} catch (FileNotFoundException ex) {
				Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#L2");
				Assert.IsNull (ex.InnerException, "#L3");
				Assert.IsNotNull (ex.Message, "#L4");
			}

			Assert.AreEqual ("XYZ.gif", pb.ImageLocation, "#M1");
			Assert.IsNotNull (pb.Image, "#M2");
			Assert.AreEqual (60, pb.Image.Height, "#M3");
			Assert.AreEqual (150, pb.Image.Width, "#M4");

			f.Dispose ();
		}

		[Test]
		public void ImagePropertyTest ()
		{
			PictureBox myPicBox = new PictureBox ();
			// I 
			Assert.IsNull (myPicBox.Image, "#1");
			Image myImage = Image.FromFile (TestResourceHelper.GetFullPathOfResource ("Test/resources/M.gif"));
			myPicBox.Image = myImage;
			Assert.AreSame (myImage, myPicBox.Image, "#2");
			Assert.AreEqual (60, myPicBox.Image.Height, "#3");
			Assert.AreEqual (150, myPicBox.Image.Width, "#4");
			myPicBox.Image = null;
			Assert.IsNull (myPicBox.Image, "#5");
			myPicBox.Image = null;
			Assert.IsNull (myPicBox.Image, "#6");
		}

		[Test] // Load ()
		public void Load_ImageLocation_Empty ()
		{
			PictureBox pb = new PictureBox ();
			pb.ImageLocation = string.Empty;

			try {
				pb.Load ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Load ()
		public void Load_ImageLocation_Null ()
		{
			PictureBox pb = new PictureBox ();
			pb.ImageLocation = null;

			try {
				pb.Load ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Load (String)
		public void Load2_Url_Empty ()
		{
			PictureBox pb = new PictureBox ();

			try {
				pb.Load (string.Empty);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Load (String)
		public void Load2_Url_Null ()
		{
			PictureBox pb = new PictureBox ();

			try {
				pb.Load ((string) null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // LoadAsync ()
		public void LoadAsync1_ImageLocation_Empty ()
		{
			PictureBox pb = new PictureBox ();
			pb.ImageLocation = string.Empty;

			try {
				pb.LoadAsync ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // LoadAsync ()
		public void LoadAsync1_ImageLocation_Null ()
		{
			PictureBox pb = new PictureBox ();
			pb.ImageLocation = null;

			try {
				pb.LoadAsync ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // LoadAsync (String)
		public void LoadASync2_Url_Empty ()
		{
			PictureBox pb = new PictureBox ();

			try {
				pb.LoadAsync (string.Empty);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // LoadAsync (String)
		public void LoadAsync2_Url_Null ()
		{
			PictureBox pb = new PictureBox ();

			try {
				pb.LoadAsync ((string) null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// ImageLocation must be set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void ToStringMethodTest ()
		{
			PictureBox myPicBox = new PictureBox ();
			Assert.AreEqual ("System.Windows.Forms.PictureBox, SizeMode: Normal", myPicBox.ToString (), "#T1");
		}

		[Test]
		public void Defaults ()
		{
			PictureBox pb = new PictureBox ();
			
			Assert.IsNotNull (pb.ErrorImage, "A1");
			Assert.AreEqual (false, pb.WaitOnLoad, "A2");
			
			Assert.AreEqual (false, pb.AutoSize, "A3");
			pb.SizeMode = PictureBoxSizeMode.AutoSize;
			Assert.AreEqual (true, pb.AutoSize, "A4");
			
		}

		[TestFixture]
		public class PictureBoxSizeModeEventClass : TestHelper
		{
			static bool eventhandled = false;
			public static void SizeMode_EventHandler (object sender, EventArgs e)
			{
				eventhandled = true;
			}

			[Test]
			public void PictureBoxEventTest ()
			{
				Form myForm = new Form ();
				myForm.ShowInTaskbar = false;
				PictureBox myPicBox = new PictureBox ();
				myForm.Controls.Add (myPicBox);
				myPicBox.SizeModeChanged += new EventHandler (SizeMode_EventHandler);
				myPicBox.SizeMode = PictureBoxSizeMode.AutoSize;
				Assert.AreEqual (true, eventhandled, "#SM1");
				eventhandled = false;
				myPicBox.SizeMode = PictureBoxSizeMode.CenterImage;
				Assert.AreEqual (true, eventhandled, "#SM2");
				eventhandled = false;
				myPicBox.SizeMode = PictureBoxSizeMode.StretchImage;
				Assert.AreEqual (true, eventhandled, "#SM3");	
				myForm.Dispose ();
			}
		}
	}
}
