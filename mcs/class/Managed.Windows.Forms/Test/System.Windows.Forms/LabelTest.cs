//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Hisham Mardam Bey (hisham.mardambey@gmail.com)
//
//
                                                

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
        [TestFixture]
	[Ignore ("This test has to be completly reviewed")]
        public class LabelTest2
        {
		
		[Test]
		public void PubPropTest ()
		{
			Label l = new Label ();
			
			// A
			Assert.AreEqual (false, l.AutoSize, "A1");
			l.AutoSize = true;
			Assert.AreEqual (true, l.AutoSize, "A2");
			l.AutoSize = false;
			Assert.AreEqual (false, l.AutoSize, "A3");
			
			// B
			Assert.AreEqual (null, l.BackgroundImage, "B1");
			l.BackgroundImage = Image.FromFile ("a.png");;
			Assert.IsNotNull (l.BackgroundImage, "B2");
			Bitmap bmp = (Bitmap)l.BackgroundImage;
			Assert.IsNotNull (bmp.GetPixel (0, 0), "B3");
									
			Assert.AreEqual (BorderStyle.None, l.BorderStyle, "B4");
			l.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (BorderStyle.FixedSingle, l.BorderStyle, "B5");
			l.BorderStyle = BorderStyle.Fixed3D;
			Assert.AreEqual (BorderStyle.Fixed3D, l.BorderStyle, "B6");
			l.BorderStyle = BorderStyle.None;
			Assert.AreEqual (BorderStyle.None, l.BorderStyle, "B7");
			
			// C
			string name = l.CompanyName;
			if (!name.Equals("Mono Project, Novell, Inc.") && !name.Equals("Microsoft Corporation")) {
				Assert.Fail("CompanyName property does not match any accepted value - C1");
			}
			
			
			// F
			Assert.AreEqual (FlatStyle.Standard, l.FlatStyle, "F1");
			l.FlatStyle = FlatStyle.Flat;
			Assert.AreEqual (FlatStyle.Flat, l.FlatStyle, "F1");
			l.FlatStyle = FlatStyle.Popup;
			Assert.AreEqual (FlatStyle.Popup, l.FlatStyle, "F2");
			l.FlatStyle = FlatStyle.Standard;
			Assert.AreEqual (FlatStyle.Standard, l.FlatStyle, "F3");
			l.FlatStyle = FlatStyle.System;
			Assert.AreEqual (FlatStyle.System, l.FlatStyle, "F4");
			
			// I
			Assert.AreEqual (ContentAlignment.MiddleCenter, l.ImageAlign, "I1");
			l.ImageAlign = ContentAlignment.TopLeft;
			Assert.AreEqual (ContentAlignment.TopLeft, l.ImageAlign, "I2");
			l.ImageAlign = ContentAlignment.TopCenter;
			Assert.AreEqual (ContentAlignment.TopCenter, l.ImageAlign, "I3");
			l.ImageAlign = ContentAlignment.TopRight;
			Assert.AreEqual (ContentAlignment.TopRight, l.ImageAlign, "I4");
			l.ImageAlign = ContentAlignment.MiddleLeft;
			Assert.AreEqual (ContentAlignment.MiddleLeft, l.ImageAlign, "I5");
			l.ImageAlign = ContentAlignment.MiddleCenter;
			Assert.AreEqual (ContentAlignment.MiddleCenter, l.ImageAlign, "I6");
			l.ImageAlign = ContentAlignment.MiddleRight;
			Assert.AreEqual (ContentAlignment.MiddleRight, l.ImageAlign, "I7");
			l.ImageAlign = ContentAlignment.BottomLeft;
			Assert.AreEqual (ContentAlignment.BottomLeft, l.ImageAlign, "I8");
			l.ImageAlign = ContentAlignment.BottomCenter;
			Assert.AreEqual (ContentAlignment.BottomCenter, l.ImageAlign, "I9");
			l.ImageAlign = ContentAlignment.BottomRight;
			Assert.AreEqual (ContentAlignment.BottomRight, l.ImageAlign, "I10");
			Assert.AreEqual (-1, l.ImageIndex, "I11");
			Assert.AreEqual (null, l.ImageList, "I12");
			Assert.AreEqual (null, l.Image, "I13");
			l.Image = Image.FromFile ("a.png");
			Assert.IsNotNull (l.Image, "I14");
			bmp = (Bitmap)l.Image;
			Assert.IsNotNull (bmp.GetPixel (0, 0), "I15");
			
			
			ImageList il = new ImageList ();
			il.ColorDepth = ColorDepth.Depth32Bit;
			il.ImageSize = new Size (15, 15);
			il.Images.Add (Image.FromFile ("a.png"));
			l.ImageList = il;
			l.ImageIndex = 0;
			
			Assert.AreEqual (0, l.ImageIndex, "I16");
			Assert.IsNotNull (l.ImageList, "I17");
			
			// PreferredHeight
			// PregerredWidth
			// RenderTransparent
			
			// T
			// Assert.AreEqual (false, l.TabStop, "T1");
			Assert.AreEqual (ContentAlignment.TopLeft, l.TextAlign, "T2");
			
			// U
			Assert.AreEqual (true, l.UseMnemonic, "U1");
			l.UseMnemonic = false;
			Assert.AreEqual (false, l.UseMnemonic, "U2");
		}
		
		[Test]
		public void PubMethodTest ()
		{
			Label l = new Label ();
			
			l.Text = "My Label";
			
			Assert.AreEqual ("System.Windows.Forms.LabelText: My Label", l.ToString (), "T1");
			  
		}
	}
}
	   
