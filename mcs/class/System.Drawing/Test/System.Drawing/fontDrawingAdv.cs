//
// 
// Advanced text drawing and formatting sample
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
		
		static string flagProcessing(StringFormat format)
		{
			string str = "";
			
			switch (format.Alignment) {
			case StringAlignment.Far:
				str = "halign: Far - ";
				break;
			case StringAlignment.Near:
				str = "halign: Near - ";
				break;
			case StringAlignment.Center:
				str = "halign: Center - ";
				break;
			default:
				break;				
			}
			
			switch (format.LineAlignment) {
			case StringAlignment.Far:
				str += "valign: Far - ";
				break;
			case StringAlignment.Near:
				str += "valign: Near - ";
				break;
			case StringAlignment.Center:
				str += "valign: Center - ";
				break;
			default:
				break;				
			}
			
			str+="fmt: ";
			
			if (format.FormatFlags==StringFormatFlags.NoWrap) str+="NoWarp ";
			if (format.FormatFlags==StringFormatFlags.DirectionVertical) str+="DirVer ";
			
			return str;	
		}
		
		static Rectangle calcRect(Rectangle rect)
		{
			return new Rectangle (rect.X, rect.Y+rect.Height+10, rect.Width,200);						
		}
		
		public static void Main( ) 
		{						
			float width = 700.0F;
			float height = 600.0F;
			string str;
			int chars = 0;
			int lines = 0;
			SizeF sz;
            
			Font f1 = new Font("Arial",12);				
			Font f2  = new Font("Verdana", 8);			
			Font f3  = new Font("Courier New", 14);
			Font f4  = new Font(FontFamily.GenericSansSerif, 14);
			Font f5  = new Font(FontFamily.GenericMonospace, 14);
			Font f6  = new Font(FontFamily.GenericSerif, 16);
			Font fonttxt= new Font("Verdana", 8);
			SolidBrush brushtxt = new SolidBrush(Color.Pink);
					
			StringFormat strfmt1 = new StringFormat();
			StringFormat strfmt2 = new StringFormat();
			StringFormat strfmt3 = new StringFormat();
			StringFormat strfmt4 = new StringFormat();
			StringFormat strfmt5 = new StringFormat();
			StringFormat strfmt6 = new StringFormat();			
			StringFormat strfmttxt = new StringFormat();			
			
			Bitmap bmp = new Bitmap((int)width, (int)height);
			Graphics gr = Graphics.FromImage(bmp);
			SolidBrush br = new SolidBrush(Color.White);
   			SolidBrush colorRed = new SolidBrush(Color.Red);
   			
			strfmttxt.Alignment = StringAlignment.Near;
			strfmttxt.LineAlignment = StringAlignment.Near;
			strfmttxt.Trimming = StringTrimming.Character;
			
			strfmt1.Alignment = StringAlignment.Center;
			strfmt1.LineAlignment = StringAlignment.Near;
			strfmt1.Trimming = StringTrimming.Character;
			strfmt1.HotkeyPrefix = HotkeyPrefix.Show;
			
			strfmt2.Alignment = StringAlignment.Far;
			strfmt2.LineAlignment = StringAlignment.Center;
			strfmt2.Trimming = StringTrimming.Character;
			
			strfmt3.Alignment = StringAlignment.Far;
			strfmt3.LineAlignment = StringAlignment.Near;
			strfmt3.Trimming = StringTrimming.None;
			
			strfmt4.Alignment = StringAlignment.Far;
			strfmt4.LineAlignment = StringAlignment.Far;
			strfmt4.Trimming = StringTrimming.EllipsisCharacter;
			
			strfmt5.Alignment = StringAlignment.Far;
			strfmt5.LineAlignment = StringAlignment.Near;
			strfmt5.Trimming = StringTrimming.None;
			strfmt5.FormatFlags = StringFormatFlags.DirectionVertical;
			
			strfmt6.Alignment = StringAlignment.Far;
			strfmt6.LineAlignment = StringAlignment.Far;
			strfmt6.Trimming = StringTrimming.EllipsisCharacter;
			strfmt6.FormatFlags = StringFormatFlags.NoWrap;
			
			
			Rectangle rect1 = new Rectangle (10,50,100,150);
			Rectangle rect2 = new Rectangle (10,300,150,150);
			
			Rectangle rect3 = new Rectangle (200,50,175,175);
			Rectangle rect4 = new Rectangle (200,300,150,150);
			
			Rectangle rect5 = new Rectangle (400,50,175,175);
			Rectangle rect6 = new Rectangle (400,300,150,150);			
			Rectangle rect7 = new Rectangle (550,300, 140,120);			
			
			gr.DrawRectangle( new Pen(Color.Yellow), rect3);			
			gr.DrawRectangle( new Pen(Color.Blue), rect4);			
			
			gr.DrawRectangle( new Pen(Color.Yellow), rect5);			
			gr.DrawRectangle( new Pen(Color.Blue), rect6);				
			
			gr.DrawString("Samples of text with different fonts and formatting", 
				new Font("Verdana",16), new SolidBrush(Color.White), new Rectangle (5,5,600,100), strfmttxt);											
			
			gr.FillEllipse(new SolidBrush(Color.Blue), rect1);
			gr.DrawRectangle( new Pen(Color.Green), rect2);			
			gr.DrawRectangle( new Pen(Color.Green), rect7);			
			
			str = "Ara que tinc &vint anys, ara que encara tinc força,que no tinc l'ànima morta, i em sento bullir la sang. (" + f1.Name + ")";			
			gr.DrawString( str,	f1, new SolidBrush(Color.White), rect1, strfmt1);						
			gr.DrawString(flagProcessing(strfmt1), fonttxt, brushtxt, calcRect(rect1), strfmttxt);						                                    
            sz =  gr.MeasureString (str, f1, new SizeF (rect1.Width, rect1.Height), strfmt1, out chars, out lines);                             			                                
			Console.WriteLine("MeasureString str1 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);
			
			str = "Ara que em sento capaç de cantar si un altre canta. Avui que encara tinc veu i encara puc creure en déus (" + f2.Name + ")";
			gr.DrawString(str, f2, new SolidBrush(Color.Red),rect2, strfmt2);														
			gr.DrawString(flagProcessing(strfmt2), fonttxt, brushtxt, calcRect(rect2), strfmttxt);						
			sz =  gr.MeasureString (str, f2, new SizeF (rect2.Width, rect2.Height), strfmt2, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str2 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);
			
			str = "Vull cantar a les pedres, la terra, l'aigua, al blat i al camí, que vaig trepitjant. (" + f3.Name + ")";
			gr.DrawString(str,f3, new SolidBrush(Color.White), rect3, strfmt3);				
			gr.DrawString(flagProcessing(strfmt3), fonttxt, brushtxt, calcRect(rect3), strfmttxt);			
			sz =  gr.MeasureString (str, f3, new SizeF (rect3.Width, rect3.Height), strfmt3, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str3 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);
			
			str = "A la nit, al cel i a aquet mar tan nostre i al vent que al matí ve a besar-me el rostre (" + f4.Name + ")";				
			gr.DrawString(str, f4, new SolidBrush(Color.Red),rect4, strfmt4);
			gr.DrawString(flagProcessing(strfmt4), fonttxt, brushtxt, calcRect(rect4), strfmttxt);			
			sz =  gr.MeasureString (str, f4, new SizeF (rect4.Width, rect4.Height), strfmt4, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str4 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);			
			
			str = "Vull cantar a les pedres, la terra, l'aigua, al blat i al camí, que vaig trepitjant. (" + f5.Name + ")";
			gr.DrawString(str, f5, new SolidBrush(Color.White), rect5, strfmt5);
			gr.DrawString(flagProcessing(strfmt5), fonttxt, brushtxt, calcRect(rect5), strfmttxt);			
			sz =  gr.MeasureString (str, f5, new SizeF (rect5.Width, rect5.Height), strfmt5, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str4 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);			
				
			str = "Ve a besar-me el rostre (" + f6.Name + ")";
			gr.DrawString(str, 	f6, new SolidBrush(Color.Red),rect6, strfmt6);
			gr.DrawString(flagProcessing(strfmt6), fonttxt, brushtxt, calcRect(rect6), strfmttxt);						
			sz =  gr.MeasureString (str, f6, new SizeF (rect6.Width, rect6.Height), strfmt6, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str6 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);				
			
			str = "Vull plorar amb aquells que es troben tots sols, sense cap amor van passant pel món.. (" + f5.Name + ")";
			gr.DrawString(str, f5, new SolidBrush(Color.White), rect7, strfmt4);
			gr.DrawString(flagProcessing(strfmt4), fonttxt, brushtxt, calcRect(rect7), strfmttxt);			
			sz =  gr.MeasureString (str, f4, new SizeF (rect7.Width, rect7.Height), strfmt5, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str7 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);			
			
			bmp.Save("fontDrawingAdv.bmp", ImageFormat.Bmp);
			
		}
	}
}
