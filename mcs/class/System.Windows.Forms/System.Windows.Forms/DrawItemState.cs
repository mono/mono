//
// System.Windows.Forms.DrawItemState.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

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

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies the state of an item that is being drawn.
	/// this enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	public enum DrawItemState : int {

		None = 0,
		Selected = 1,
		Grayed = 2,
		Disabled = 4,
		Checked = 8,
		Focus = 16,
		Default = 32,
		HotLight = 64,
		Inactive = 128,
		NoAccelerator = 256,
		NoFocusRect = 512,
		ComboBoxEdit = 1024,
	}
}
