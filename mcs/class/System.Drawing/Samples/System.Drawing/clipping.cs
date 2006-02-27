//
// Author:
//   Jordi Mas i Hernandez
//
// Sample to test clipping. Requires SWF.
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Windows.Forms;
using System.Drawing;
using System;

namespace MyFormProject
{


	class MainForm : System.Windows.Forms.Form
	{
		class ourLabelTwoAreas : Label
		{
			public ourLabelTwoAreas ()
			{

			}
			protected override void OnPaint (PaintEventArgs pevent)
			{
				Console.WriteLine ("ourLabelTwoAreas pevents {0}, pos {1} - size {2}", pevent.ClipRectangle,
					Location, Size);

				Region reg = new Region (new Rectangle (20, 20, 10, 10));
				reg.Union (new Rectangle (5, 5, 10, 10));
				pevent.Graphics.Clip  = reg;
				pevent.Graphics.FillRectangle (Brushes.Red, pevent.ClipRectangle);
			}
		}

		class ourLabelOverflows : Label
		{
			public ourLabelOverflows ()
			{

			}
			protected override void OnPaint (PaintEventArgs pevent)
			{
				Console.WriteLine ("ourLabelOverflows pevents {0}, pos {1} - size {2}", pevent.ClipRectangle,
					Location, Size);

				pevent.Graphics.FillRectangle (Brushes.Yellow,
					new Rectangle (0,0, 1000, 1000));
			}
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{
			pevent.Graphics.FillRectangle (Brushes.Green, pevent.ClipRectangle);
		}

		static private ourLabelTwoAreas label = new ourLabelTwoAreas ();
		static private ourLabelOverflows label2 = new ourLabelOverflows ();

		public MainForm ()
		{
			label.Location = new Point (20, 20);
			label.Size = new Size (50, 80);
			label.Text = "Hola";
			Controls.Add (label);

			label2.Location = new Point (100, 100);
			label2.Size = new Size (50, 80);
			label2.Text = "Hola";
			Controls.Add (label2);

			ClientSize = new Size (400, 400);
		}

		public static void Main(string[] args)
		{
			Application.Run (new MainForm ());
		}
	}

}

