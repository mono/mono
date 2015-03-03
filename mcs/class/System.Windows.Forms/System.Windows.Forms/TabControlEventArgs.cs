//
// TabControlEventArgs.cs
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

namespace System.Windows.Forms
{
	public class TabControlEventArgs : EventArgs
	{
		private TabControlAction action;
		private TabPage tab_page;
		private int tab_page_index;

		#region Public Constructors
		public TabControlEventArgs (TabPage tabPage, int tabPageIndex, TabControlAction action) : base ()
		{
			this.tab_page = tabPage;
			this.tab_page_index = tabPageIndex;
			this.action = action;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public TabControlAction Action
		{
			get { return this.action; }
		}

		public TabPage TabPage
		{
			get { return this.tab_page; }
		}

		public int TabPageIndex
		{
			get { return this.tab_page_index; }
		}
		#endregion	// Public Instance Properties
	}
}
