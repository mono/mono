//
// System.Drawing.Drawing2D.TestHatchBrush.cs 
//
// Author:
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Drawing2D
{
	[TestFixture]	
	public class HatchBrushTest : Assertion
	{
		Graphics gr;
		Bitmap bmp;
		Font font;
		Color bgColor;  // background color
		Color fgColor;  // foreground color
		int currentTop; // the location for next drawing operation
		int spacing;    // space between two consecutive drawing operations
		int fontSize;   // text size
		int textStart;  // text starting location
		int lineStart;  // line starting location
		int length;     // length of the line
		int penWidth;   // width of the Pen used to draw lines

		[SetUp]
		public void GetReady () { }
		
		[TearDown]
		public void Clear () { }

		public HatchBrushTest ()
		{
			fontSize = 16;
			textStart = 10;
			lineStart = 200;
			length = 400;
			penWidth = 50;
			currentTop = 0;
			spacing = 50;

			bgColor = Color.Yellow;
			fgColor = Color.Red;
		}
			
		[Test]
		public void TestProperties () 
		{
			HatchBrush hbr = new HatchBrush(HatchStyle.SolidDiamond, fgColor);

			AssertEquals ("Props#1", hbr.HatchStyle, HatchStyle.SolidDiamond);
			AssertEquals ("Props#2", hbr.ForegroundColor.ToArgb(), fgColor.ToArgb());
			AssertEquals ("Props#3", hbr.BackgroundColor.ToArgb(), Color.Black.ToArgb());

			hbr = new HatchBrush(HatchStyle.Cross, fgColor, bgColor);

			AssertEquals ("Props#4", hbr.HatchStyle, HatchStyle.Cross);
			AssertEquals ("Props#5", hbr.ForegroundColor.ToArgb(), fgColor.ToArgb());
			AssertEquals ("Props#6", hbr.BackgroundColor.ToArgb(), bgColor.ToArgb());
		}
		
		[Test]
		public void TestClone ()
		{
			HatchBrush hbr = new HatchBrush(HatchStyle.Cross, fgColor, bgColor);

			HatchBrush clone = (HatchBrush) hbr.Clone ();

			AssertEquals ("Clone#1", hbr.HatchStyle, clone.HatchStyle);
			AssertEquals ("Clone#2", hbr.ForegroundColor, clone.ForegroundColor);
			AssertEquals ("Clone#3", hbr.BackgroundColor, clone.BackgroundColor);
		}

		[Test]
		public void TestDrawing ()
		{
			// create a bitmap with big enough dimensions 
			// to accomodate all the tests
			bmp = new Bitmap (700, 4000); // width, height
			gr = Graphics.FromImage (bmp);
			font = new Font (new FontFamily ("Arial"), fontSize);

                        // make the background white
                        gr.Clear (Color.White);

			// draw figures using hatch brush constructed
			// using different constructors
			Constructors ();

			// draw figures using different hatchstyles
			HatchStyles ();

			// save the drawing
			string file = getDir () + "TestHatchBrush.png";
			bmp.Save (file, ImageFormat.Png);
		}

		private void Constructors ()
		{
			int top = currentTop;
			SolidBrush br = new SolidBrush (Color.Black);

	 		top += spacing;

	 		gr.DrawString ("Test Constructors", font, br, textStart, top);

	 		// #1
			top += spacing;
			gr.DrawString ("Test #1 Horizontal, BackgroundColor=Black, ForegroundColor=White", font, br, textStart, top);

			top += spacing;
			Pen pen = new Pen (new HatchBrush (HatchStyle.Horizontal, Color.White), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #2
 			top += spacing;
			gr.DrawString ("Test #2 Vertical, BackgroundColor=Blue, ForegroundColor=Red", font, br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.Vertical, Color.Red, Color.Blue), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

			currentTop = top;
		}

		private void HatchStyles ()
		{
			int top = currentTop;
			HatchBrush hbr;
			Pen pen;
			SolidBrush br = new SolidBrush (Color.Black);

	 		top += spacing;

	 		gr.DrawString ("Test HatchStyles", font, br, textStart, top);

	 		// #1
			top += spacing;
			gr.DrawString ("Test #1 Horizontal", font, br, textStart, top);

			top += spacing;
			hbr = new HatchBrush (HatchStyle.Horizontal, fgColor, bgColor);
			pen = new Pen (hbr, penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #2
 			top += spacing;
			gr.DrawString ("Test #2 Min", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.Min, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #3
 			top += spacing;
			gr.DrawString ("Test #3 DarkHorizontal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DarkHorizontal, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #4
 			top += spacing;
			gr.DrawString ("Test #4 LightHorizontal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.LightHorizontal, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #5
 			top += spacing;
			gr.DrawString ("Test #5 NarrowHorizontal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.NarrowHorizontal, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart,top, lineStart + length,top);

 			// #6
 			top += spacing;
			gr.DrawString ("Test #6 Vertical", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.Vertical, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #7
 			top += spacing;
			gr.DrawString ("Test #7 DarkVertical", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DarkVertical, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #8
 			top += spacing;
			gr.DrawString ("Test #8 LightVertical", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.LightVertical, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #9
 			top += spacing;
			gr.DrawString ("Test #9 NarrowVertical", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.NarrowVertical, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #10
			top += spacing;
			gr.DrawString ("Test #10 Cross", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.Cross, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #11
			top += spacing;
			gr.DrawString ("Test #11 LargeGrid", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.LargeGrid, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #12
 			top += spacing;
			gr.DrawString ("Test #12 SmallGrid", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.SmallGrid, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

			// #13
 			top += spacing;
			gr.DrawString ("Test #13 DottedGrid", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DottedGrid, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart,top, lineStart + length,top);

	 		// #14
			top += spacing;
			gr.DrawString ("Test #14 DiagonalCross", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DiagonalCross, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #15
			top += spacing;
			gr.DrawString ("Test #15 BackwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.BackwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #16
			top += spacing;
			gr.DrawString ("Test #16 ForwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.ForwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #17
			top += spacing;
			gr.DrawString ("Test #17 LightDownwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.LightDownwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #18
			top += spacing;
			gr.DrawString ("Test #18 DarkDownwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DarkDownwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #19
			top += spacing;
			gr.DrawString ("Test #19 WideDownwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.WideDownwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #20
			top += spacing;
			gr.DrawString ("Test #20 LightUpwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.LightUpwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #21
			top += spacing;
			gr.DrawString ("Test #21 DarkUpwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DarkUpwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #22
			top += spacing;
			gr.DrawString ("Test #22 WideUpwardDiagonal", font, br, textStart, top);

			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.WideUpwardDiagonal, fgColor, bgColor);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #23
 			top += spacing;
			gr.DrawString ("Test #23 DashedHorizontal", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DashedHorizontal, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #24
 			top += spacing;
			gr.DrawString ("Test #24 DashedVertical", font, br, textStart, top);
			
			top += spacing;
			hbr = new HatchBrush (HatchStyle.DashedVertical, fgColor, bgColor);
	 		gr.FillRectangle (hbr, lineStart, top, length, penWidth);

 			// #25
 			top += spacing;
			gr.DrawString ("Test #25 DashedDownwardDiagonal", font, br, textStart, top);
			
			top += spacing;
			hbr = new HatchBrush (HatchStyle.DashedDownwardDiagonal, fgColor, bgColor);
	 		gr.FillRectangle (hbr, lineStart, top, length, penWidth);

 			// #26
 			top += spacing;
			gr.DrawString ("Test #26 DashedUpwardDiagonal", font, br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DashedUpwardDiagonal, fgColor, bgColor), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #27
 			top += spacing;
			gr.DrawString ("Test #27 HorizontalBrick", font, br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.HorizontalBrick, fgColor, bgColor), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #28
 			top += spacing;
			gr.DrawString ("Test #28 DiagonalBrick", font, br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DiagonalBrick, fgColor, bgColor), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #29
 			top += spacing;
			gr.DrawString ("Test #29 LargeCheckerBoard", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.LargeCheckerBoard, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #30
 			top += spacing;
			gr.DrawString ("Test #30 SmallCheckerBoard", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.SmallCheckerBoard, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #31
 			top += spacing;
			gr.DrawString ("Test #31 OutlinedDiamond", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.OutlinedDiamond, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #32
 			top += spacing;
			gr.DrawString ("Test #32 SolidDiamond", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.SolidDiamond, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #33
 			top += spacing;
			gr.DrawString ("Test #33 DottedDiamond", font, br, textStart, top);
			
			top += spacing;
			pen.Brush = new HatchBrush (HatchStyle.DottedDiamond, fgColor, bgColor);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

			currentTop = top;
		}

		/* Get the right directory depending on the runtime */
		private string getDir ()
		{
			string dir;

			if (Environment.GetEnvironmentVariable ("MSNet") == null)
				dir = "mono/";
			else
				dir = "MSNet/";

			return dir;
		}
	}
}
