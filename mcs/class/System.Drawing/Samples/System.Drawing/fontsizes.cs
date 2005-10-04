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
// (c) 2005 Jordi Mas i Hernandez, <jordi@ximian.com>
//

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace MonoSamples.System.Drawing
{
	public class FontSizes
	{
		public static void PrintFontInfo (Font f)
		{
			Console.WriteLine ("Font: {0} size in pixels: {1}", f, f.Height);
		}
		
		public static void Main ()
		{			
			Console.WriteLine (";----------------------------------------------------");
			PrintFontInfo (new Font ("Arial", 12));
			PrintFontInfo (new Font ("Arial", 14));
			PrintFontInfo (new Font ("Arial", 16));
			PrintFontInfo (new Font ("Arial", 22));
			PrintFontInfo (new Font ("Verdana", 44));
			PrintFontInfo (new Font ("Verdana", 8));
			
		}
	}
}
