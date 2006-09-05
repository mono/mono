//
// 
// Advanced text drawing and formatting sample
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
			
			switch (format.Trimming) {
			case StringTrimming.Character:
				str += "trimm: Char - ";
				break;
			case StringTrimming.EllipsisCharacter:
				str += "trimm: EllipsisChar - ";
				break;
			case StringTrimming.EllipsisPath:
				str += "trimm: EllipsisPath - ";
				break;
			case StringTrimming.EllipsisWord:
				str += "trimm: EllipsisWord - ";
				break;
			case StringTrimming.None:
				str += "trimm: None - ";
				break;				
			case StringTrimming.Word:
				str += "trimm: Word - ";
				break;								
			default:
				break;				
			}			
			
			switch (format.FormatFlags) {
			case StringFormatFlags.NoWrap:
				str+="fmt: NoWrap";
				break;
			case StringFormatFlags.DirectionVertical:
				str+="fmt: DirVer ";
				break;
			case StringFormatFlags.DirectionRightToLeft:
				str+="fmt: rtl ";
				break;
			
			default:
				break;				
			}
			
			return str;	
		}
		
		static Rectangle calcRect(Rectangle rect)
		{
			return new Rectangle (rect.X, rect.Y+rect.Height+10, rect.Width,200);						
		}
		
		public static void Main( ) 
		{						
			float width = 750.0F;
			float height = 1000.0F;
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

			SolidBrush solid  =  new SolidBrush(Color.Blue);

			gr.DrawString("Samples of text with different fonts and formatting", 
				new Font("Verdana",16), new SolidBrush(Color.White), new Rectangle (5,5,600,100), strfmttxt);											

		
			gr.FillEllipse(solid, rect1);

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
			sz =  gr.MeasureString (str, f5, new SizeF (rect7.Width, rect7.Height), strfmt5, out chars, out lines);                             			                                			
			Console.WriteLine("MeasureString str7 [" + str + "] " + sz + ";chars:" + chars + " lines:" + lines);			
			
			/* 3rd row */			
			
			Font f8  = new Font("Verdana", 10);			
			Font f9  = new Font("Verdana", 6);		
			Font f10  = new Font("Verdana", 12);		
			Font f11  = new Font("Verdana", 14);		
			
			Rectangle rect8 = new Rectangle (10, 550,100,100);			
			Rectangle rect9 = new Rectangle (150,550, 100,100);			
			Rectangle rect10 = new Rectangle (300,550, 100,100);			
			Rectangle rect11 = new Rectangle (420,550, 100,100);			
			Rectangle rect12 = new Rectangle (530,550, 200,100);			
			Rectangle rect13 = new Rectangle (530,600, 200,100);			
			Rectangle rect14 = new Rectangle (530,650, 200,100);			
			
			gr.DrawRectangle( new Pen(Color.Yellow), rect8);			
			gr.DrawRectangle( new Pen(Color.Yellow), rect9);			
			gr.DrawRectangle( new Pen(Color.Yellow), rect10);			
			
			StringFormat strfmt8 = new StringFormat();						
			strfmt8.Alignment = StringAlignment.Center;
			strfmt8.LineAlignment = StringAlignment.Near;
			strfmt8.Trimming = StringTrimming.EllipsisCharacter;
			strfmt8.HotkeyPrefix = HotkeyPrefix.Show;			
			
			StringFormat strfmt9 = new StringFormat();						
			strfmt9.Alignment = StringAlignment.Center;
			strfmt9.LineAlignment = StringAlignment.Center;
			strfmt9.Trimming = StringTrimming.EllipsisCharacter;
			strfmt9.HotkeyPrefix = HotkeyPrefix.None;			
			
			StringFormat strfmt10 = new StringFormat();						
			strfmt10.Alignment = StringAlignment.Center;
			strfmt10.LineAlignment = StringAlignment.Near;
			strfmt10.Trimming = StringTrimming.Word;
			strfmt10.HotkeyPrefix = HotkeyPrefix.Show;					
			
			StringFormat strfmt11 = new StringFormat();						
			strfmt11.Alignment = StringAlignment.Center;
			strfmt11.LineAlignment = StringAlignment.Near;
			strfmt11.Trimming = StringTrimming.Word;
			strfmt11.HotkeyPrefix = HotkeyPrefix.Show;					
			strfmt11.FormatFlags = StringFormatFlags.DirectionRightToLeft;
			
			StringFormat strfmt12 = new StringFormat();						
			float[] tabs = {10, 20, 30};
			strfmt12.Alignment = StringAlignment.Center;
			strfmt12.LineAlignment = StringAlignment.Near;
			strfmt12.Trimming = StringTrimming.Word;
			strfmt12.HotkeyPrefix = HotkeyPrefix.Show;								
			strfmt12.SetTabStops(20, tabs);
			
			StringFormat strfmt13 = new StringFormat();						
			float[] tabs2 = {5, 50, 60};
			strfmt13.Alignment = StringAlignment.Center;
			strfmt13.LineAlignment = StringAlignment.Near;
			strfmt13.Trimming = StringTrimming.Word;
			strfmt13.HotkeyPrefix = HotkeyPrefix.Show;											
			strfmt13.SetTabStops(0, tabs2);
			
			StringFormat strfmt14 = new StringFormat();						
			strfmt14.Alignment = StringAlignment.Center;
			strfmt14.LineAlignment = StringAlignment.Near;
			strfmt14.Trimming = StringTrimming.Word;
			strfmt14.HotkeyPrefix = HotkeyPrefix.Show;								
			strfmt14.FormatFlags = StringFormatFlags.DirectionRightToLeft;
			
			str = "Vull alçar la veu,per cantar als homes que han nascut dempeus (" + f8.Name + ")";
			gr.DrawString(str, f8, new SolidBrush(Color.White), rect8, strfmt8);
			gr.DrawString(flagProcessing(strfmt8), fonttxt, brushtxt, calcRect(rect8), strfmttxt);			
			sz =  gr.MeasureString (str, f8, new SizeF (rect8.Width, rect8.Height), strfmt8, out chars, out lines);                             			                                			
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect8.X, rect8.Y, (int)sz.Width, (int)sz.Height));			
			
			str = "I no tinc l'ànima morta i  em sento bollir la sang (" + f9.Name + ")";
			gr.DrawString(str, f9, new SolidBrush(Color.White), rect9, strfmt9);
			gr.DrawString(flagProcessing(strfmt9), fonttxt, brushtxt, calcRect(rect9), strfmttxt);			
			sz =  gr.MeasureString (str, f9, new SizeF (rect9.Width, rect9.Height), strfmt9, out chars, out lines);                             			                                			
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect9.X, rect9.Y, (int)sz.Width, (int)sz.Height));			
			
			str = "I no tinc l'ànima morta i  em sento bollir la sang (" + f10.Name + ")";
			gr.DrawString(str, f10, new SolidBrush(Color.White), rect10, strfmt10);
			gr.DrawString(flagProcessing(strfmt10), fonttxt, brushtxt, calcRect(rect10), strfmttxt);			
			sz =  gr.MeasureString (str, f10, new SizeF (rect10.Width, rect10.Height), strfmt10, out chars, out lines);                             			                                			
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect10.X, rect10.Y, (int)sz.Width, (int)sz.Height));			
			
			str = "I no tinc l'ànima morta i  em sento bollir la sang (" + f11.Name + ")";
			gr.DrawString(str, f11, new SolidBrush(Color.White), rect11, strfmt11);
			gr.DrawString(flagProcessing(strfmt11), fonttxt, brushtxt, calcRect(rect11), strfmttxt);			
			sz =  gr.MeasureString (str, f11, new SizeF (rect11.Width, rect11.Height), strfmt11, out chars, out lines);                             			                                			
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect11.X, rect11.Y, (int)sz.Width, (int)sz.Height));			
			
			str = "Tab1\tTab2\tTab3";
			gr.DrawString(str, f8, new SolidBrush(Color.White), rect12, strfmt12);
			sz =  gr.MeasureString (str, f8, new SizeF (rect12.Width, rect12.Height), strfmt12, out chars, out lines);                             			                                						
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect12.X, rect12.Y, (int)sz.Width, (int)sz.Height));			
			
			str = "Nom\tCognom\tAdreça";
			gr.DrawString(str, f8, new SolidBrush(Color.White), rect13, strfmt13);
			sz =  gr.MeasureString (str, f8, new SizeF (rect13.Width, rect13.Height), strfmt13, out chars, out lines);                             			                                						
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect13.X, rect13.Y, (int)sz.Width, (int)sz.Height));			
			
			str = "Nom Cognom Adreça";
			gr.DrawString(str, f8, new SolidBrush(Color.White), rect14, strfmt14);
			sz =  gr.MeasureString (str, f8, new SizeF (rect14.Width, rect13.Height), strfmt14, out chars, out lines);                             			                                						
			gr.DrawRectangle(new Pen(Color.Red), new Rectangle (rect14.X, rect14.Y, (int)sz.Width, (int)sz.Height));			
			
			bmp.Save("fontDrawingAdv.bmp", ImageFormat.Bmp);
			
		}
	}
}
