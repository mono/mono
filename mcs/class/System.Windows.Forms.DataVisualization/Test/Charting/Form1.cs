//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com) 
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
	public partial class Form1 : Form
	{
		public Form1 ()
		{
			InitializeComponent ();

			Text = Path.GetFileNameWithoutExtension (Application.ExecutablePath);
		}

		private class MyChart : Chart
		{
			protected override void OnPaintBackground (PaintEventArgs pevent)
			{
				base.OnPaintBackground (pevent);
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				base.OnPaint (e);
			}

			//protected override void OnPrePaint (ChartPaintEventArgs e)
			//{
			//        if (e.ChartElement is Series)
			//                e.Position.X = 100;
			//        else
			//                Console.WriteLine (e.ChartElement);

			//        Console.WriteLine ("PRE: " + e.ChartElement.ToString () + " - " + e.Position.ToString ());
			//        base.OnPrePaint (e);
			//}

			//protected override void OnPostPaint (ChartPaintEventArgs e)
			//{
			//        Console.WriteLine ("POST: " + e.ChartElement.ToString ());
			//        base.OnPostPaint (e);
			//}
		}
	}
}
