//
// hering.cs 
// Creates image for Hering illusion.
// Converted to C# from Xr demo application.
//
// Author:
//   Alexandre Pigolkine(pigolkine@gmx.de)
// 
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
 
namespace xrtest {
 public class xrt {
  public static void Main( String[] arr) {
   float width = 400.0F;
   float height = 800.0F;
   
   Bitmap bmp = new Bitmap((int)width, (int)height);
   Graphics gr = Graphics.FromImage(bmp);
   SolidBrush br = new SolidBrush(Color.White);
   gr.FillRectangle(br, 0.0F, 0.0F, width, height);
   int LINES = 32;
   float MAX_THETA  = (.80F * 90.0F);
   float THETA  = (2 * MAX_THETA / (LINES-1));
   
   Pen blackPen = new Pen(Color.Black, 2.0F);
   GraphicsState state	 = gr.Save();
 
   gr.TranslateTransform(width/2.0F, height/2.0F);
   gr.RotateTransform(MAX_THETA);
   for( int i = 0; i < LINES; i++) {
    gr.DrawLine( blackPen, -2.0F * width, 0.0F, 2.0F * width, 0.0F);
    gr.RotateTransform(-THETA);
   }
   gr.Restore(state);
   
   Pen redPen = new Pen(Color.Red, 6F);
   gr.DrawLine( redPen, width / 4F, 0F, width / 4F, height);
   gr.DrawLine( redPen, 3F * width / 4F, 0F, 3F * width / 4F, height);
   
   bmp.Save("Hering.bmp", ImageFormat.Bmp);
   Console.WriteLine("output file Hering.bmp");
  }
 }
}
