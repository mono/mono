//
// System.Windows.Forms.HScrollBar.cs
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
// Copyright (C) 2004, Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	jordi@ximian.com
//
//
//
// $Revision: 1.3 $
// $Modtime: $
// $Log: HScrollBar.cs,v $
// Revision 1.3  2004/09/28 18:44:25  pbartok
// - Streamlined Theme interfaces:
//   * Each DrawXXX method for a control now is passed the object for the
//     control to be drawn in order to allow accessing any state the theme
//     might require
//
//   * ControlPaint methods for the theme now have a CP prefix to avoid
//     name clashes with the Draw methods for controls
//
//   * Every control now retrieves it's DefaultSize from the current theme
//
// Revision 1.2  2004/08/23 20:10:03  jordi
// fixes properties and methods
//
// Revision 1.1  2004/07/13 15:33:46  jordi
// vertical and hort. classes commit
//
//

using System.Drawing;

namespace System.Windows.Forms 
{
	
	public class HScrollBar : ScrollBar 
	{
		public HScrollBar() 
		{			
			vert = false; 
		}

		protected override  Size DefaultSize {
			get { return ThemeEngine.Current.HScrollBarDefaultSize;}
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}
	}
}
