//
// System.Drawing.TestHatchBrush.cs 
//
// Author:
//   Ravindra (rkumar@novell.com)
//
// (C) Novell, Inc. http://www.novell.com
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonoTests.System.Drawing {
	
	public class TestHatchBrush {
		// to be used for creating a new bitmap
		Graphics gr;
		Bitmap bmp;
		int currentTop;
		int spacing;
		int fontSize = 16;
		int textStart = 10;
		int lineStart = 200;
		int length = 400;
		int penWidth = 50;
		
		public TestHatchBrush (int width, int height, int top, int sp)
		{
			currentTop = top;
			spacing = sp;
			bmp = new Bitmap (width,height);
			gr = Graphics.FromImage (bmp);
			// make the background white 
			Brush br = new SolidBrush (Color.White);
			gr.FillRectangle (br, 0.0F, 0.0F, width, height);		
		}
		
		public void TestConstructors ()
		{
			int top = currentTop;	

			//Test different pens
	 		top += spacing;
			Brush br = Brushes.Black;

	 		gr.DrawString ("Test Constructors with different styles", 
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

	 		// #1
			top += spacing;
			gr.DrawString ("Test #1 Horizon",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			Pen pen = new Pen (new HatchBrush (HatchStyle.Horizontal, Color.White), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #2
 			top += spacing;
			gr.DrawString ("Test #2 Min",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.Min, Color.White), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);
		
 			// #3
 			top += spacing;
			gr.DrawString ("Test #3 Dark Horizon",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DarkHorizontal, Color.Green, Color.Blue), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #4
 			top += spacing;
			gr.DrawString ("Test #4 Light Horizon",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.LightHorizontal, Color.White), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #5
 			top += spacing;
			gr.DrawString ("Test #5 NarrowHorizon",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.NarrowHorizontal, Color.Red, Color.Blue), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #6
 			top += spacing;
			gr.DrawString ("Test #6 Vertical",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.Vertical, Color.White), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);
		
 			// #7
 			top += spacing;
			gr.DrawString ("Test #7 Dark vertical",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DarkVertical, Color.Green, Color.Blue), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #8
 			top += spacing;
			gr.DrawString ("Test #8 Light vertical",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.LightVertical, Color.White), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

 			// #9
 			top += spacing;
			gr.DrawString ("Test #9 NarrowVertical",
							new Font(new FontFamily("Arial"),
							fontSize), br, textStart, top);
			
			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.NarrowVertical, Color.Red, Color.Blue), penWidth);
	 		gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #10
			top += spacing;
			gr.DrawString ("Test #10 Cross",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.Cross, Color.Red), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #11
			top += spacing;
			gr.DrawString ("Test #11 LargeGrid",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.LargeGrid, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #12
			top += spacing;
			gr.DrawString ("Test #12 Diagonal Cross",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DiagonalCross, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #13
			top += spacing;
			gr.DrawString ("Test #13 Backward diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.BackwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #14
			top += spacing;
			gr.DrawString ("Test #14 Forward diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.ForwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #15
			top += spacing;
			gr.DrawString ("Test #15 light down diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.LightDownwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #16
			top += spacing;
			gr.DrawString ("Test #16 dark down diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DarkDownwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #17
			top += spacing;
			gr.DrawString ("Test #17 wide down diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.WideDownwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #18
			top += spacing;
			gr.DrawString ("Test #18 light up diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.LightUpwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #19
			top += spacing;
			gr.DrawString ("Test #19 dark up diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.DarkUpwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);

	 		// #20
			top += spacing;
			gr.DrawString ("Test #20 wide up diagonal",
							new Font (new FontFamily ("Arial"),
							fontSize), br, textStart, top);

			top += spacing;
			pen = new Pen (new HatchBrush (HatchStyle.WideUpwardDiagonal, Color.Red, Color.Yellow), penWidth);
			gr.DrawLine (pen, lineStart, top, lineStart + length, top);
 		
			currentTop = top;
		}
		
		public void TestProperties ()
		{
			HatchBrush hbr1 = new HatchBrush(HatchStyle.SolidDiamond, Color.Red);

			Console.WriteLine ("HatchStyle: SolidDiamond: " + hbr1.HatchStyle);
			Console.WriteLine ("Foreground: Red: " + hbr1.ForegroundColor);
			Console.WriteLine ("Background: Black: " + hbr1.BackgroundColor);
				
			HatchBrush hbr2 = new HatchBrush(HatchStyle.Cross, Color.Red, Color.Yellow);

			Console.WriteLine ("HatchStyle: Cross: " + hbr2.HatchStyle);
			Console.WriteLine ("Foreground: Red: " + hbr2.ForegroundColor);
			Console.WriteLine ("Background: Yellow: " + hbr2.BackgroundColor);		
		}
		
		public void Save ()
		{
			// save the bmp
			bmp.Save ("TestHatchBrush.bmp"); 
		}
		
		// Main to test the things
		public static void Main () 
 		{
 			// make sure that the image dimensions are 
			// sufficient to hold all the test results.
			TestHatchBrush thb = new TestHatchBrush (700, 3000, 0, 50); // width,height, top,sp
 			
 			// Test the constructors
			thb.TestConstructors ();
 			
 		 	// Test Properties
 			thb.TestProperties ();
 					
 			// save the image file when done
 			thb.Save ();
		}
 	}
}
