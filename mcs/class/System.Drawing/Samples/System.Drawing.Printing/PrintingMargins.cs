// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
//
// Author:
//	Jordi Mas <jordimash@gmail.com>
////
// Draws a simple margin to be able to check if they are correct in different
// paper sizes.
//


using System;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;
using System.Drawing.Imaging;

public class PrintingMargins
{	
	
	static private void QueryPageSettings (object sender, QueryPageSettingsEventArgs e)
	{
		
	}

	static private void PrintPageEvent (object sender, PrintPageEventArgs e)
	{					
		e.Graphics.DrawRectangle (Pens.Red, e.MarginBounds);
		e.Graphics.DrawRectangle (Pens.Green, e.PageBounds);		
		e.HasMorePages = false;
	}


        public static void Main (string[] args)
        {                
		PrintDocument p = new PrintDocument ();
		p.PrintPage += new PrintPageEventHandler (PrintPageEvent);
		p.QueryPageSettings += new  QueryPageSettingsEventHandler (QueryPageSettings);
                p.Print ();
		
        }
}

