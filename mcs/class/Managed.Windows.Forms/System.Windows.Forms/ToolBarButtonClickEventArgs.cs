//
// System.Windows.Forms.ToolBarButtonClickEventArgs.cs
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
// Copyright (C) 2004 Novell, Inc.
//
// Authors:
//	Ravindra (rkumar@novell.com)
//
//
// $Revision: 1.3 $
// $Modtime: $
// $Log: ToolBarButtonClickEventArgs.cs,v $
// Revision 1.3  2004/08/22 00:03:20  ravindra
// Fixed toolbar control signatures.
//
// Revision 1.2  2004/08/21 21:52:54  pbartok
// - Added missing base class
//
// Revision 1.1  2004/08/15 23:13:15  ravindra
// First Implementation of ToolBar control.
//
//

// COMPLETE

using System;

namespace System.Windows.Forms
{
	public class ToolBarButtonClickEventArgs : EventArgs
	{
		#region Local Variables
		private ToolBarButton button;
		#endregion

		#region Public Constructors
		public ToolBarButtonClickEventArgs (ToolBarButton button)
		{
			this.button = button;
		}
		#endregion Public Constructors

		#region Public Instance Properties
		public ToolBarButton Button {
			get { return button; }
			set { button = value; }
		}
		#endregion Instance Properties
	}
}
