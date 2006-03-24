//
// Sample to Print diferent font types and sizes
//

using System;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;

public class PrintingTextFile
{

	static private void PrintPageEvent (object sender, PrintPageEventArgs e)
	{
		float left = e.MarginBounds.Left;
		float top = e.MarginBounds.Top;

		Font font = new Font ("Arial", 10);
		e.Graphics.DrawString("This a sample with font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Red), left, top);

		font = new Font ("Verdana", 16);
		e.Graphics.DrawString ("This a sample with font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Blue), left, top + 50);

		font = new Font ("Verdana", 22);
		e.Graphics.DrawString ("This a sample with font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Black), left, top + 150);

		font  = new Font (FontFamily.GenericMonospace, 14);
		e.Graphics.DrawString ("This a sample with font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Black), left, top + 250);

		font  = new Font ("Arial", 48);
		e.Graphics.DrawString ("Font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Red), left, top + 300);

		font  = new Font ("Times New Roman", 32);
		e.Graphics.DrawString ("Another sample font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Black), left, top + 500);

		font  = new Font (FontFamily.GenericSansSerif, 8);
		e.Graphics.DrawString ("Another sample font " + font.Name + " size:" + font.Size,
			font, new SolidBrush (Color.Blue), left, top + 900);

		e.HasMorePages = false;
	}


        public static void Main (string[] args)
        {
		PrintDocument p = new PrintDocument ();
		p.PrintPage += new PrintPageEventHandler (PrintPageEvent);
                p.Print ();
        }
}


