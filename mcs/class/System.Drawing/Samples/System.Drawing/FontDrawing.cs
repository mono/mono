//
// 
// Simple font handeling
//
// Author:
//   Jordi Mas i Hernandez <jordi@ximian.com>
// 
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace Font1Sample {
	public class FontDrawing {
		
		public static void listFonts()
		{		
			FontCollection ifc = new InstalledFontCollection();
			foreach( FontFamily ffm in ifc.Families) {				
				try
				{
					Font f = new Font(ffm.Name,12);	
					Console.WriteLine("Family Name:" + ffm.Name + "," + f.Name);
				}			
				catch (Exception ex) 	{}
			}
		}
		
		public static void checkFontProperties()
		{
			Font f = new Font("Arial",12);	
			Console.WriteLine("Font:" + f.Name + " size:" + f.Size);
			
			f = new Font("Verdana", 12);	
			Console.WriteLine("Font:" + f.Name + " size:" + f.Size);
			
			f = new Font("Courier New", 12);	
			Console.WriteLine("Font:" + f.Name + " size:" + f.Size);
					
		}
		
		public static void Main( ) 
		{
			Console.WriteLine("Fonts--------------------");			
                 	listFonts();			
		
			Console.WriteLine("Propierties--------------------");						
			checkFontProperties();            
			
			Console.WriteLine("Draw--------------------");
			float width = 800.0F;
			float height = 650.0F;
                        string str = "";
			
			Font f1 = new Font("Arial",12);			
			Font f2 = new Font("Verdana", 12, FontStyle.Bold);	
			Font f3 = new Font("Courier New", 12, FontStyle.Italic);
			
   			Font f4  = new Font(FontFamily.GenericSansSerif, 19, FontStyle.Regular, GraphicsUnit.Millimeter);
			Console.WriteLine("Font:" + f4.Name + " size:" + f4.Size + "Points: " + f4.SizeInPoints);
			
			Font f5  = new Font(FontFamily.GenericSerif, 15, FontStyle.Regular, GraphicsUnit.Point);
			Console.WriteLine("Font:" + f5.Name + " size:" + f5.Size + "Points: " + f5.SizeInPoints);
			
			Font f6  = new Font("Arial", 40, FontStyle.Regular, GraphicsUnit.Pixel);
			Console.WriteLine("Font:" + f6.Name + " size:" + f6.Size + "Points: " + f6.SizeInPoints);
			
			Font f7  = new Font("Courier New", 19, FontStyle.Regular, GraphicsUnit.World);
			Console.WriteLine("Font:" + f7.Name + " size:" + f7.Size + "Points: " + f7.SizeInPoints);							

       			Font f8  = new Font("Courier New", 19, FontStyle.Bold |  FontStyle.Underline, GraphicsUnit.World);
                        Console.WriteLine("Font:" + f8.Name + " size:" + f8.Size + "Points: " + f8.SizeInPoints);

               		Font f9  = new Font("Courier New", 19, FontStyle.Bold |  FontStyle.Underline|  FontStyle.Italic, GraphicsUnit.World);
			Console.WriteLine("Font:" + f9.Name + " size:" + f9.Size + "Points: " + f9.SizeInPoints);

                        Font f10  = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Strikeout, GraphicsUnit.Millimeter);
			Console.WriteLine("Font:" + f10.Name + " size:" + f10.Size + "Points: " + f10.SizeInPoints);
			
			
			Bitmap bmp = new Bitmap((int)width, (int)height);
			Graphics gr = Graphics.FromImage(bmp);
			SolidBrush br = new SolidBrush(Color.White);
   			SolidBrush colorRed = new SolidBrush(Color.Red);

			gr.FillRectangle(br, 0.0F, 0.0F, width, height);
			
			br = new SolidBrush(Color.Black);

                        str = "This an " +  f1.Name + " test string size: "+ f1.Height;                        
			gr.DrawString (str, f1, br, 10, 10);

                        str = "This a " +  f2.Name + " bold test string size: "+ f2.Height;
			gr.DrawString( str, f2, colorRed, 10, 50);

                        str = "This a " +  f3.Name + " italic test string size: "+ f3.Height;
			gr.DrawString( str, f3, br, 10, 100);

                        str = "This an " +  f4.Name + " test string size: "+ f4.Height;
			gr.DrawString (str, f4, br, 10, 150);

                        str = "This a " +  f5.Name + " test string size: "+ f5.Height;
			gr.DrawString( str, f5, colorRed, 10, 250);

                        str = "This a " +  f6.Name + " test string size: "+ f6.Height;
			gr.DrawString( str, f6, br, new Rectangle(10,300,0,0));
			
                        str = "This a " +  f7.Name + " test string size: "+ f7.Height;
			gr.DrawString( str, f7, br, 10,350);

                        str = "This a " +  f8.Name + " test (Underline/Bold) string size: "+ f8.Height;
			gr.DrawString( str, f8, br, 10,400);

                        str = "This a " +  f9.Name + " test (Underline/Bold/Italic) string size: "+ f9.Height;
			gr.DrawString( str, f9, br, 10,450);

                        str = "This a " +  f10.Name + " test (Strikeout) string size: "+ f10.Height;
			gr.DrawString( str, f10, br, 10,500);

			
			bmp.Save("FontDrawing.bmp", ImageFormat.Bmp);			
		}
	}
}
