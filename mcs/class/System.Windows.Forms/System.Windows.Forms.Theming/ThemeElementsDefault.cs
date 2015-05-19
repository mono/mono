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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//	Andreia Gaita (avidigal@novell.com)

using System;

namespace System.Windows.Forms.Theming
{
	internal class ThemeElementsDefault
	{
		protected Default.TabControlPainter tabControlPainter;
		public virtual Default.TabControlPainter TabControlPainter {
			get {
				if (tabControlPainter == null)
					tabControlPainter = new Default.TabControlPainter ();
				return tabControlPainter;
			}
		}

		protected Default.ButtonPainter buttonPainter;
		public virtual Default.ButtonPainter ButtonPainter {
			get {
				if (buttonPainter == null)
					buttonPainter = new Default.ButtonPainter ();
				return buttonPainter;
			}
		}

		protected Default.LabelPainter labelPainter;
		public virtual Default.LabelPainter LabelPainter {
			get {
				if (labelPainter == null)
					labelPainter = new Default.LabelPainter ();
				return labelPainter;
			}
		}

		protected Default.LinkLabelPainter linklabelPainter;
		public virtual Default.LinkLabelPainter LinkLabelPainter {
			get {
				if (linklabelPainter == null)
					linklabelPainter = new Default.LinkLabelPainter ();
				return linklabelPainter;
			}
		}

		protected Default.ToolStripPainter toolStripPainter;
		public virtual Default.ToolStripPainter ToolStripPainter {
			get {
				if (toolStripPainter == null)
					toolStripPainter = new Default.ToolStripPainter ();
				return toolStripPainter;
			}
		}

		protected Default.CheckBoxPainter checkBoxPainter;
		public virtual Default.CheckBoxPainter CheckBoxPainter {
			get {
				if (checkBoxPainter == null)
					checkBoxPainter = new Default.CheckBoxPainter ();
				return checkBoxPainter;
			}
		}

		protected Default.RadioButtonPainter radioButtonPainter;
		public virtual Default.RadioButtonPainter RadioButtonPainter {
			get {
				if (radioButtonPainter == null)
					radioButtonPainter = new Default.RadioButtonPainter ();
				return radioButtonPainter;
			}
		}
	}
}
