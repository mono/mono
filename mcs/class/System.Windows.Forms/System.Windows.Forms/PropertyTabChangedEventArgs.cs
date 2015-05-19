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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

// COMPLETE

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	[ComVisible(true)]
	public class PropertyTabChangedEventArgs : EventArgs
	{
		#region Local Variables
		private PropertyTab old_tab;
		private PropertyTab new_tab;
		#endregion	// Local Variables

		#region Constructor
		public PropertyTabChangedEventArgs ( PropertyTab oldTab , PropertyTab newTab )
		{
			old_tab = oldTab;
			new_tab = newTab;
		}
		#endregion	// Constructor

		#region Public Instance Properties
		public PropertyTab NewTab
		{
			get {
				return new_tab;
			}
		}

		public PropertyTab OldTab
		{
			get {
				return old_tab;
			}
		}
		#endregion	// Public Instance Properties
	}
}
