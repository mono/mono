//
// Region class testing unit
//
// Author:
//   Jordi Mas, jordi@ximian.com
//

//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;

namespace MonoTests.System.Drawing
{

	[TestFixture]	
	public class TestRegion : Assertion 
	{
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
		
		/* For debugging */	
		public static void DumpRegion (Region rgn)
		{
			Matrix matrix = new Matrix ();		
			RectangleF [] rects = rgn.GetRegionScans (matrix);
	
			for (int i = 0; i < rects.Length; i++)
				Console.WriteLine ( rects[i]);
		}
		
		[Test]
		public void TestBounds() 
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);        					
			Rectangle rect1, rect2;		
			Region rgn1, rgn2;					
			RectangleF bounds;				
			
			rect1 = new Rectangle (500, 30, 60, 80);		
			rect2 = new Rectangle (520, 40, 60, 80);		
			rgn1 = new Region(rect1);
			rgn2 = new Region(rect2);						
			rgn1.Union(rgn2);				
			
			bounds = rgn1.GetBounds (dc);			
			
			AssertEquals (500, bounds.X);	
			AssertEquals (30, bounds.Y);	
			AssertEquals (80, bounds.Width);	
			AssertEquals (90, bounds.Height);			
		}
		
		[Test]
		public void TestCloneAndEquals() 
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);        					
			Rectangle rect1, rect2;		
			Region rgn1, rgn2;								
			RectangleF [] rects;
			RectangleF [] rects2;
			Matrix matrix = new Matrix ();		
			
			rect1 = new Rectangle (500, 30, 60, 80);		
			rect2 = new Rectangle (520, 40, 60, 80);		
			rgn1 = new Region (rect1);			
			rgn1.Union (rect2);			
			rgn2 = rgn1.Clone ();
			
			rects = rgn1.GetRegionScans (matrix);			
			rects2 = rgn2.GetRegionScans (matrix);						
			
			AssertEquals (rects.Length, rects2.Length);		
			
			for (int i = 0; i < rects.Length; i++) {
				
				AssertEquals (rects[i].X, rects[i].X);	
				AssertEquals (rects[i].Y, rects[i].Y);	
				AssertEquals (rects[i].Width, rects[i].Width);	
				AssertEquals (rects[i].Height, rects[i].Height);	
			}					
			
			AssertEquals (true, rgn1.Equals (rgn2, dc));		
		}
		
		 /*Tests infinite, empty, etc*/
		[Test]
		public void TestInfiniteAndEmpty() 
		{	
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);        					
			Rectangle rect1, rect2;		
			Region rgn1, rgn2;								
			RectangleF [] rects;
			RectangleF [] rects2;
			Matrix matrix = new Matrix ();		
			
			rect1 = new Rectangle (500, 30, 60, 80);		
			rect2 = new Rectangle (520, 40, 60, 80);		
			rgn1 = new Region (rect1);			
			rgn1.Union (rect2);			
			
			AssertEquals (false, rgn1.IsEmpty (dc));	
			AssertEquals (false, rgn1.IsInfinite (dc));
				
			rgn1.MakeEmpty();
			AssertEquals (true, rgn1.IsEmpty (dc));				
			
			rgn1 = new Region (rect1);			
			rgn1.Union (rect2);					
			rgn1.MakeInfinite ();
			rects = rgn1.GetRegionScans (matrix);						
			
			AssertEquals (1, rects.Length);				
			AssertEquals (-4194304, rects[0].X);	
			AssertEquals (-4194304, rects[0].Y);	
			AssertEquals (8388608, rects[0].Width);	
			AssertEquals (8388608, rects[0].Height);	
			AssertEquals (true, rgn1.IsInfinite (dc));			
		}
		
		
		[Test]
		public void TestUnion() 
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);        		
			Matrix matrix = new Matrix ();		
			Rectangle rect1, rect2, rect3, rect4;		
			Region rgn1, rgn2, rgn3, rgn4;					
			RectangleF [] rects;				
			
			rect1 = new Rectangle (500, 30, 60, 80);		
			rect2 = new Rectangle (520, 40, 60, 80);		
			rgn1 = new Region(rect1);
			rgn2 = new Region(rect2);						
			rgn1.Union(rgn2);			
			rects = rgn1.GetRegionScans (matrix);			
			
			AssertEquals (3, rects.Length);				
			AssertEquals (500, rects[0].X);	
			AssertEquals (30, rects[0].Y);	
			AssertEquals (60, rects[0].Width);	
			AssertEquals (10, rects[0].Height);	
			
			AssertEquals (500, rects[1].X);	
			AssertEquals (40, rects[1].Y);	
			AssertEquals (80, rects[1].Width);	
			AssertEquals (70, rects[1].Height);	
			
			AssertEquals (520, rects[2].X);	
			AssertEquals (110, rects[2].Y);	
			AssertEquals (60, rects[2].Width);	
			AssertEquals (10, rects[2].Height);				
			
			rect1 = new Rectangle (20, 180, 40, 50);		
			rect2 = new Rectangle (50, 190, 40, 50);		
			rect3 = new Rectangle (70, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
				
			rgn1.Union (rgn2);
			rgn1.Union (rgn3);						
			rects = rgn1.GetRegionScans (matrix);			
			AssertEquals (5, rects.Length);	
			
			AssertEquals (20, rects[0].X);	
			AssertEquals (180, rects[0].Y);	
			AssertEquals (40, rects[0].Width);	
			AssertEquals (10, rects[0].Height);	
			
			AssertEquals (20, rects[1].X);	
			AssertEquals (190, rects[1].Y);	
			AssertEquals (70, rects[1].Width);	
			AssertEquals (20, rects[1].Height);	
			
			AssertEquals (20, rects[2].X);	
			AssertEquals (210, rects[2].Y);	
			AssertEquals (80, rects[2].Width);	
			AssertEquals (20, rects[2].Height);	
			
			AssertEquals (50, rects[3].X);	
			AssertEquals (230, rects[3].Y);	
			AssertEquals (50, rects[3].Width);	
			AssertEquals (10, rects[3].Height);	
			
			AssertEquals (70, rects[4].X);	
			AssertEquals (240, rects[4].Y);	
			AssertEquals (30, rects[4].Width);	
			AssertEquals (20, rects[4].Height);				
			
			rect1 = new Rectangle (20, 330, 40, 50);		
			rect2 = new Rectangle (50, 340, 40, 50);		
			rect3 = new Rectangle (70, 360, 30, 50);		
			rect4 = new Rectangle (80, 400, 30, 10);				
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);				
			
			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			rgn1.Union (rgn4);					
		
			rects = rgn1.GetRegionScans (matrix);	
			
			AssertEquals (6, rects.Length);	
			
			AssertEquals (20, rects[0].X);	
			AssertEquals (330, rects[0].Y);	
			AssertEquals (40, rects[0].Width);	
			AssertEquals (10, rects[0].Height);	
			
			AssertEquals (20, rects[1].X);	
			AssertEquals (340, rects[1].Y);	
			AssertEquals (70, rects[1].Width);	
			AssertEquals (20, rects[1].Height);	
			
			AssertEquals (20, rects[2].X);	
			AssertEquals (360, rects[2].Y);	
			AssertEquals (80, rects[2].Width);	
			AssertEquals (20, rects[2].Height);	
			
			AssertEquals (50, rects[3].X);	
			AssertEquals (380, rects[3].Y);	
			AssertEquals (50, rects[3].Width);	
			AssertEquals (10, rects[3].Height);	
			
			AssertEquals (70, rects[4].X);	
			AssertEquals (390, rects[4].Y);	
			AssertEquals (30, rects[4].Width);	
			AssertEquals (10, rects[4].Height);			
			
			AssertEquals (70, rects[5].X);	
			AssertEquals (400, rects[5].Y);	
			AssertEquals (40, rects[5].Width);	
			AssertEquals (10, rects[5].Height);						
		}	
		
		[Test]
		public void TestComplement()
		{			
			Bitmap bmp = new Bitmap (600, 800);			
			Graphics dc = Graphics.FromImage (bmp);        		
			Matrix matrix = new Matrix ();		
			Rectangle rect1, rect2, rect3;		
			Region rgn1, rgn2, rgn3;					
			RectangleF [] rects;
			
			rect1 = new Rectangle (20, 30, 60, 80);		
			rect2 = new Rectangle (50, 40, 60, 80);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);						
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			rgn1.Complement (rgn2);
			dc.FillRegion (Brushes.Blue, rgn1);					
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));		
			
			rects = rgn1.GetRegionScans (matrix);
			
			AssertEquals (2, rects.Length);				
			
			AssertEquals (80, rects[0].X);	
			AssertEquals (40, rects[0].Y);	
			AssertEquals (30, rects[0].Width);	
			AssertEquals (70, rects[0].Height);	
			
			AssertEquals (50, rects[1].X);	
			AssertEquals (110, rects[1].Y);	
			AssertEquals (60, rects[1].Width);
			AssertEquals (10, rects[1].Height);							
						
			rect1 = new Rectangle (20, 180, 40, 50);		
			rect2 = new Rectangle (50, 190, 40, 50);		
			rect3 = new Rectangle (70, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			
			rects = rgn1.GetRegionScans (matrix);			
			rgn1.Complement (rgn2);
			rgn1.Complement (rgn3);					
			
			rects = rgn1.GetRegionScans (matrix);	
			
			AssertEquals (2, rects.Length);						
			
			AssertEquals (90, rects[0].X);	
			AssertEquals (210, rects[0].Y);	
			AssertEquals (10, rects[0].Width);	
			AssertEquals (30, rects[0].Height);	
			
			AssertEquals (70, rects[1].X);	
			AssertEquals (240, rects[1].Y);	
			AssertEquals (30, rects[1].Width);
			AssertEquals (20, rects[1].Height);							
			
		}
		
		[Test]
		public void TestExclude()
		{			
			Bitmap bmp = new Bitmap (600, 800);			
			Graphics dc = Graphics.FromImage (bmp);        		
			Matrix matrix = new Matrix ();		
			Rectangle rect1, rect2;		
			Region rgn1, rgn2;					
			RectangleF [] rects;
			
			rect1 = new Rectangle (130, 30, 60, 80);		
			rect2 = new Rectangle (170, 40, 60, 80);		
			rgn1 = new Region (rect1);			
			rgn1.Exclude (rect2);
			rects = rgn1.GetRegionScans (matrix);
			
			AssertEquals (2, rects.Length);							
			
			AssertEquals (130, rects[0].X);	
			AssertEquals (30, rects[0].Y);	
			AssertEquals (60, rects[0].Width);	
			AssertEquals (10, rects[0].Height);	
			
			AssertEquals (130, rects[1].X);	
			AssertEquals (40, rects[1].Y);	
			AssertEquals (40, rects[1].Width);
			AssertEquals (70, rects[1].Height);													
		}
		
		[Test]
		public void TestIntersect()
		{			
			Bitmap bmp = new Bitmap (600, 800);			
			Graphics dc = Graphics.FromImage (bmp);        		
			Matrix matrix = new Matrix ();		
			RectangleF [] rects;
			RectangleF rect3, rect4;
			Region rgn3, rgn4;
			
			Rectangle rect1 = new Rectangle (260, 30, 60, 80);		
			Rectangle rect2 = new Rectangle (290, 40, 60, 80);		
			Region rgn1 = new Region (rect1);
			Region rgn2 = new Region (rect2);			
			rgn1.Intersect (rgn2);			
						
			rects = rgn1.GetRegionScans (matrix);
			AssertEquals (1, rects.Length);							
			
			AssertEquals (290, rects[0].X);	
			AssertEquals (40, rects[0].Y);	
			AssertEquals (30, rects[0].Width);	
			AssertEquals (70, rects[0].Height);	
			
			rect1 = new Rectangle (20, 330, 40, 50);		
			rect2 = new Rectangle (50, 340, 40, 50);		
			rect3 = new Rectangle (70, 360, 30, 50);		
			rect4 = new Rectangle (80, 400, 30, 10);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);				
			
			rgn1.Intersect (rgn2);
			rgn1.Intersect (rgn3);
			rgn1.Intersect (rgn4);
			rects = rgn1.GetRegionScans (matrix);			
			
			AssertEquals (0, rects.Length);							
		}
		
		[Test]
		public void TestXor()
		{			
			Bitmap bmp = new Bitmap (600, 800);			
			Graphics dc = Graphics.FromImage (bmp);        		
			Matrix matrix = new Matrix ();		
			RectangleF [] rects;
			
			Rectangle rect1 = new Rectangle (380, 30, 60, 80);		
			Rectangle rect2 = new Rectangle (410, 40, 60, 80);		
			Region rgn1 = new Region (rect1);
			Region rgn2 = new Region (rect2);			
			rgn1.Xor (rgn2);			
			
			
			rects = rgn1.GetRegionScans (matrix);
			
			AssertEquals (4, rects.Length);							
			
			AssertEquals (380, rects[0].X);	
			AssertEquals (30, rects[0].Y);	
			AssertEquals (60, rects[0].Width);	
			AssertEquals (10, rects[0].Height);		
			
			AssertEquals (380, rects[1].X);	
			AssertEquals (40, rects[1].Y);	
			AssertEquals (30, rects[1].Width);	
			AssertEquals (70, rects[1].Height);		
			
			AssertEquals (440, rects[2].X);	
			AssertEquals (40, rects[2].Y);	
			AssertEquals (30, rects[2].Width);	
			AssertEquals (70, rects[2].Height);		
			
			AssertEquals (410, rects[3].X);	
			AssertEquals (110, rects[3].Y);	
			AssertEquals (60, rects[3].Width);	
			AssertEquals (10, rects[3].Height);		
		}

		
		[Test]
		public void TestIsVisible() 
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);        					
			Rectangle rect1, rect2;		
			Region rgn1, rgn2;								
			Matrix matrix = new Matrix ();		
			
			rect1 = new Rectangle (500, 30, 60, 80);		
			rect2 = new Rectangle (520, 40, 60, 80);		
			
			rgn1 = new Region (new RectangleF (0, 0, 10,10));						
			AssertEquals (false, rgn1.IsVisible (0,0,0,1));		
			
			rgn1 = new Region (rect1);						
			AssertEquals (false, rgn1.IsVisible (500,29));		
			AssertEquals (true, rgn1.IsVisible (500,30));		
			AssertEquals (true, rgn1.IsVisible (rect1));		
			AssertEquals (true, rgn1.IsVisible (rect2));		
			AssertEquals (false, rgn1.IsVisible (new Rectangle (50,50,2,5)));					
			
			Rectangle r = new Rectangle (1,1, 2,1);
			rgn2 = new Region (r);
			AssertEquals (true, rgn2.IsVisible (r));
			AssertEquals (true, rgn2.IsVisible (new Rectangle (1,1, 2,2)));
			AssertEquals (true, rgn2.IsVisible (new Rectangle (1,1, 10,10)));
			AssertEquals (true, rgn2.IsVisible (new Rectangle (1,1, 1,1)));			
			AssertEquals (false, rgn2.IsVisible (new Rectangle (2,2, 1,1)));			
			AssertEquals (false, rgn2.IsVisible (new Rectangle (0,0, 1,1)));
			AssertEquals (false, rgn2.IsVisible (new Rectangle (3,3, 1,1)));
			
			AssertEquals (false, rgn2.IsVisible (0,0));
			AssertEquals (false, rgn2.IsVisible (1,0));
			AssertEquals (false, rgn2.IsVisible (2,0));
			AssertEquals (false, rgn2.IsVisible (3,0));
			AssertEquals (false, rgn2.IsVisible (0,1));
			AssertEquals (true, rgn2.IsVisible (1,1));
			AssertEquals (true, rgn2.IsVisible (2,1));
			AssertEquals (false, rgn2.IsVisible (3,1));
			AssertEquals (false, rgn2.IsVisible (0,2));
			AssertEquals (false, rgn2.IsVisible (1,2));
			AssertEquals (false, rgn2.IsVisible (2,2));
			AssertEquals (false, rgn2.IsVisible (3,2));
			
			
			
		}
		
		[Test]
		public void TestTranslate() 
		{
			Region rgn1 = new Region (new RectangleF (10, 10, 120,120));									
			rgn1.Translate (30,20);
			Matrix matrix = new Matrix ();		
			
			RectangleF [] rects = rgn1.GetRegionScans (matrix);
			
			AssertEquals (1, rects.Length);							
			
			AssertEquals (40, rects[0].X);	
			AssertEquals (30, rects[0].Y);	
			AssertEquals (120, rects[0].Width);	
			AssertEquals (120, rects[0].Height);					
		}

	}
}


