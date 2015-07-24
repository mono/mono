//
// PopupEventArgs.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//


using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	public class PopupEventArgs : CancelEventArgs
	{
		private Control associated_control;
		private IWin32Window associated_window;
		private bool is_balloon;
		private Size tool_tip_size;

		#region Public Constructors
		public PopupEventArgs (IWin32Window associatedWindow, Control associatedControl, bool isBalloon, Size size) : base ()
		{
			this.associated_window = associatedWindow;
			this.associated_control = associatedControl;
			this.is_balloon = isBalloon;
			this.tool_tip_size = size;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Control AssociatedControl {
			get { return this.associated_control; }
		}

		public IWin32Window AssociatedWindow {
			get { return this.associated_window; }
		}
		
		public bool IsBalloon {
			get { return this.is_balloon; }
		}
		
		public Size ToolTipSize {
			get { return this.tool_tip_size; }
			set { this.tool_tip_size = value; }
		}
		#endregion	// Public Instance Properties
	}
}
