//
// Test application for pie graphics functions implementation
//
// Author:
//   Jordi Mas, jordi@ximian.com
//

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

//
public class graphicsUI
{	
	
	public static void Main( ) 
	{

		Bitmap bmp = new Bitmap (300, 300);
		Graphics dc = Graphics.FromImage (bmp);        
		
		SolidBrush blueBrush = new SolidBrush (Color.Blue);
		SolidBrush redBrush = new SolidBrush (Color.Red);
		SolidBrush yellowBrush = new SolidBrush (Color.Yellow);
		SolidBrush whiteBrush = new SolidBrush (Color.White);				
		Pen bluePen = new Pen (Color.Blue);
				
		Rectangle rect1 = new Rectangle (0,0, 75, 75);		
		dc.DrawPie (bluePen, rect1, 10, 60);
		
		Rectangle rect2 = new Rectangle (100,100, 75, 75);		
		dc.DrawPie (bluePen, rect2, 100, 75);
		
		Rectangle rect3 = new Rectangle (100, 0, 75, 75);		
		dc.FillPie (yellowBrush, rect3, 0, 300);
		
		Rectangle rect4 = new Rectangle (0, 100, 75, 75);		
		dc.FillPie (whiteBrush, rect4, 200, 30);

		Rectangle rect5 = new Rectangle (200, 0, 75, 75);		
		dc.FillPie (yellowBrush, rect5, 190, 300);
		
		Rectangle rect6 = new Rectangle (200, 100, 75, 75);		
		dc.FillPie (whiteBrush, rect6, 200, 20);

        	bmp.Save("fillpie.bmp", ImageFormat.Bmp);				
	}	

}


