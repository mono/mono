//
// 
// Simple font handeling
//
// Author:
//   Jordi Mas i Hernandez <jordi@ximian.com>
// 
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
			float height = 400.0F;
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
			
			Font f7  = new Font("Verdana", 19, FontStyle.Regular, GraphicsUnit.World);
			Console.WriteLine("Font:" + f7.Name + " size:" + f7.Size + "Points: " + f7.SizeInPoints);					
			
			
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

            str = "This an " +  f1.Name + " test string size: "+ f4.Height;
			gr.DrawString (str, f4, br, 10, 150);

            str = "This a " +  f5.Name + " test string size: "+ f5.Height;
			gr.DrawString( str, f5, colorRed, 10, 250);

            str = "This a " +  f6.Name + " test string size: "+ f6.Height;
			gr.DrawString( str, f6, br, new Rectangle(10,300,0,0));
			
           	str = "This a " +  f7.Name + " test string size: "+ f7.Height;
			gr.DrawString( str, f7, br, 10,350);
			
			bmp.Save("fontdrawing.bmp", ImageFormat.Bmp);			
		}
	}
}
