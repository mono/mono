//
// Test Region class testing unit
//
// Author:
//   Jordi Mas, jordi@ximian.com
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
	
		public static void DumpRegion (Region rgn)
		{
			Matrix matrix = new Matrix ();		
			RectangleF [] rects = rgn.GetRegionScans (matrix);
	
			for (int i = 0; i < rects.Length; i++)
				Console.WriteLine ( rects[i]);
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
				
			/* First*/
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
			
			/* Second */
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
			
			
			/* Third */
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
			
			Console.WriteLine ("dc1" + rgn1.GetBounds(dc));

					
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
	}
}


