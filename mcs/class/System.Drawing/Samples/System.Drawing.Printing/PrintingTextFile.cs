//
// Simple text file printing sample
//

using System;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;

public class PrintingTextFile
{
	private static StreamReader stream;

	static private void PrintPageEvent (object sender, PrintPageEventArgs e)
	{		
		float lines_page, y;		
		int count = 0;
		float left = e.MarginBounds.Left;
		float top = e.MarginBounds.Top;
		String line = null;
		Font font = new Font ("Arial", 10);
		float font_height = font.GetHeight (e.Graphics);
		lines_page = e.MarginBounds.Height  / font_height;		

		while (count < lines_page) {			
			line = stream.ReadLine ();
			
			if (line == null)
				break;
				
			y = top + (count * font_height);	
			e.Graphics.DrawString (line, font, Brushes.Black, left, y, new StringFormat());
			
			count++;
		}
		
		if (line != null)
			e.HasMorePages = true;
		else
			e.HasMorePages = false;
	}


        public static void Main (string[] args)
        {
                stream = new StreamReader ("PrintMe.txt");
		PrintDocument p = new PrintDocument ();
		p.PrintPage += new PrintPageEventHandler (PrintPageEvent);
                p.Print ();
		stream.Close();
        }
}


