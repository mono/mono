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
			float width = 400.0F;
			float height = 400.0F;
            string str = "";
			
			Font f1 = new Font("Arial",12);			
			Font f2 = new Font("Verdana", 12, FontStyle.Bold);	
			Font f3 = new Font("Courier New", 12, FontStyle.Italic);
   			Font f4 = new Font("Arial",12);
			
			Console.WriteLine("Name: {1}", f1.Name, f1.Height);
			
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

                        str = "This an " +  f1.Name + " test string size: "+ f1.Height;
			gr.DrawString (str, f1, br, 10, 10);

            str = "This a " +  f2.Name + " bold test string size: "+ f2.Height;
			gr.DrawString( str, f2, colorRed, 10, 50);

            str = "This a " +  f3.Name + " italic test string size: "+ f3.Height;
			gr.DrawString( str, f3, br, 10, 100);
			
			bmp.Save("fontdrawing.bmp", ImageFormat.Bmp);			
		}
	}
}
