//
// font1.cs 
// font/text operations
//
// Author:
//   Alexandre Pigolkine(pigolkine@gmx.de)
// 
//
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace Font1Sample {
	public class Font1 {
		public static void Main( ) {
			float width = 400.0F;
			float height = 800.0F;
		
			FontCollection ifc = new InstalledFontCollection();
			foreach( FontFamily ffm in ifc.Families) {
				Console.WriteLine(ffm.Name);
			}
		
			Font f = new Font(ifc.Families[0],12);
			Console.WriteLine("Height: {0}", f.Height);
			
			Bitmap bmp = new Bitmap((int)width, (int)height);
			Graphics gr = Graphics.FromImage(bmp);
			SolidBrush br = new SolidBrush(Color.White);
			gr.FillRectangle(br, 0.0F, 0.0F, width, height);
			
			br = new SolidBrush(Color.Black);
			gr.DrawString( "The test string", f, br, 10, 10);
			
			bmp.Save("font1.bmp", ImageFormat.Bmp);
			Console.WriteLine("output file font1.bmp");
			
		}
	}
}
