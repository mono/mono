//
// PictureBoxTest.cs: Test cases for PictureBox.
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
using System.Threading;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PictureBoxTest
	{
		[Test]
		public void PictureBoxPropertyTest ()
		{
			Form myForm = new Form ();
			PictureBox myPicBox = new PictureBox ();
			myForm.Controls.Add (myPicBox);
			
			// B 
			Assert.AreEqual (BorderStyle.None, myPicBox.BorderStyle, "#B1");
			myPicBox.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (BorderStyle.Fixed3D, myPicBox.BorderStyle, "#B2");

			// I 
			Assert.AreEqual (null, myPicBox.Image, "#I1");
			Image myImage =	Image.FromFile("M.gif");
			myPicBox.Image = myImage;
			Assert.AreEqual (60, myPicBox.Image.Height, "#I2");
			Assert.AreEqual (150, myPicBox.Image.Width, "#I3");
			
			// P 
			Assert.AreEqual (PictureBoxSizeMode.Normal, myPicBox.SizeMode, "#P1");
			myPicBox.SizeMode = PictureBoxSizeMode.AutoSize;
			Assert.AreEqual (PictureBoxSizeMode.AutoSize, myPicBox.SizeMode, "#P2");
		}
			
		
		[Test, Ignore ("This seems to fail.")]
		public void ToStringMethodTest () 
		{
			PictureBox myPicBox = new PictureBox ();
			Assert.AreEqual ("System.Windows.Forms.PictureBox, SizeMode: Normal", myPicBox.ToString (), "#T1");
		}
		
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
			}
		}
	}
}
