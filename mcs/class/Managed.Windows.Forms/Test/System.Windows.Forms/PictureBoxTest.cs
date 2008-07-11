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

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PictureBoxTest
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
		
#if NET_2_0
		[Test]
		[Category ("NotWorking")]
		public void ImageLocation ()
		{
			PictureBox pb = new PictureBox ();
			Assert.IsNull (pb.ImageLocation, "#A");

			pb.ImageLocation = "M.gif";

			Assert.AreEqual ("M.gif", pb.ImageLocation, "#B1");
			Assert.IsNull (pb.Image, "#B2");

			using (Stream s = this.GetType ().Assembly.GetManifestResourceStream ("32x32.ico")) {
				pb.Image = Image.FromStream (s);
			}

			Assert.AreEqual ("M.gif", pb.ImageLocation, "#C1");
			Assert.IsNotNull (pb.Image, "#C2");
			Assert.AreEqual (96, pb.Image.Height, "#C3");
			Assert.AreEqual (96, pb.Image.Width, "#C4");

			pb.ImageLocation = null;

			Assert.IsNull (pb.ImageLocation, "#D1");
			Assert.IsNotNull (pb.Image, "#D2");
			Assert.AreEqual (96, pb.Image.Height, "#D3");
			Assert.AreEqual (96, pb.Image.Width, "#D4");

			pb.ImageLocation = "M.gif";

			Assert.AreEqual ("M.gif", pb.ImageLocation, "#E1");
			Assert.IsNotNull (pb.Image, "#E2");
			Assert.AreEqual (96, pb.Image.Height, "#E3");
			Assert.AreEqual (96, pb.Image.Width, "#E4");

			pb.Load ();

			Assert.AreEqual ("M.gif", pb.ImageLocation, "#F1");
			Assert.IsNotNull (pb.Image, "#F2");
			Assert.AreEqual (60, pb.Image.Height, "#F3");
			Assert.AreEqual (150, pb.Image.Width, "#F4");

			pb.ImageLocation = null;

			Assert.IsNull (pb.ImageLocation, "#G1");
			Assert.IsNull (pb.Image, "#G2");

			pb.ImageLocation = "M.gif";
			pb.Load ();
			pb.ImageLocation = "XYZ.gif";

			Assert.AreEqual ("XYZ.gif", pb.ImageLocation, "#H1");
			Assert.IsNotNull (pb.Image, "#H2");
			Assert.AreEqual (60, pb.Image.Height, "#H3");
			Assert.AreEqual (150, pb.Image.Width, "#H4");

			pb.ImageLocation = string.Empty;

			Assert.AreEqual (string.Empty, pb.ImageLocation, "#I1");
			Assert.IsNull (pb.Image, "#I2");

			using (Stream s = this.GetType ().Assembly.GetManifestResourceStream ("32x32.ico")) {
				pb.Image = Image.FromStream (s);
			}

			Assert.AreEqual (string.Empty, pb.ImageLocation, "#J1");
			Assert.IsNotNull (pb.Image, "#J2");
			Assert.AreEqual (96, pb.Image.Height, "#J3");
			Assert.AreEqual (96, pb.Image.Width, "#J4");

			pb.Load ("M.gif");

			Assert.AreEqual ("M.gif", pb.ImageLocation, "#K1");
			Assert.IsNotNull (pb.Image, "#K2");
			Assert.AreEqual (60, pb.Image.Height, "#K3");
			Assert.AreEqual (150, pb.Image.Width, "#K4");

			pb.ImageLocation = null;

			Assert.IsNull (pb.ImageLocation, "#L1");
			Assert.IsNull (pb.Image, "#L2");
		}
#endif

		[Test]
		public void ImagePropertyTest ()
		{
			PictureBox myPicBox = new PictureBox ();
			// I 
			Assert.AreEqual (null, myPicBox.Image, "#I1");
			Image myImage = Image.FromFile ("M.gif");
			myPicBox.Image = myImage;
			Assert.AreEqual (60, myPicBox.Image.Height, "#I2");
			Assert.AreEqual (150, myPicBox.Image.Width, "#I3");
		}
		
		[Test]
		public void ToStringMethodTest ()
		{
			PictureBox myPicBox = new PictureBox ();
			Assert.AreEqual ("System.Windows.Forms.PictureBox, SizeMode: Normal", myPicBox.ToString (), "#T1");
		}

#if NET_2_0
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
#endif

		[TestFixture]
		public class PictureBoxSizeModeEventClass
		{
			static bool eventhandled = false;
			public static void SizeMode_EventHandler (object sender, EventArgs e)
			{
				eventhandled = true;
			}

			[Test]
			public void PictureBoxEvenTest ()
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
