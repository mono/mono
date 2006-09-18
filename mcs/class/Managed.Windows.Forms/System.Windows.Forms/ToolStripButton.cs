//
// ToolStripButton.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class ToolStripButton : ToolStripItem
	{
		private bool auto_tool_tip;
		private CheckState checked_state;
		private bool check_on_click;

		#region Public Constructors
		public ToolStripButton ()
			: this (String.Empty, null, null, String.Empty)
		{

		}

		public ToolStripButton (Image image)
			: this (String.Empty, image, null, String.Empty)
		{

		}

		public ToolStripButton (string text)
			: this (text, null, null, String.Empty)
		{

		}

		public ToolStripButton (string text, Image image)
			: this (text, image, null, String.Empty)
		{

		}

		public ToolStripButton (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, String.Empty)
		{

		}

		public ToolStripButton (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			this.is_selected = false;
			this.checked_state = CheckState.Unchecked;
			this.auto_tool_tip = true;
			this.check_on_click = false;
		}
		#endregion

		#region Public Events
		public event EventHandler CheckedChanged;
		public event EventHandler CheckStateChanged;
		#endregion
	}
}
#endif