//
// System.Windows.Forms.AccessibleStates.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
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

namespace System.Windows.Forms
{
	[Flags]
	public enum AccessibleStates
	{
		AlertLow = 67108864,
		AlertMedium = 134217728,
		AlertHigh = 268435456,
		Animated = 16384,
		Busy = 2048,
		Checked = 16,
		Collapsed = 1024,
		Default = 256,
		Expanded = 512,
		ExtSelectable = 33554432,
		Floating = 4096,
		Focusable = 1048576,
		Focused = 4,
		HotTracked = 128,
		Mixed = 32,
		Indeterminate =32,
		Invisible = 32768,
		Linked = 4194304,
		Marqueed = 8192,
		Moveable = 262144,
		MultiSelectable = 16777216,
		None = 0,
		Pressed = 8,
		Protected = 536870912,
		ReadOnly = 64,
		Offscreen = 65536,
		Selectable = 2097152,
		Selected = 2,
		SelfVoicing = 524288,
		Sizeable = 131072,
		Traversed =8388608,
		Unavailable = 1,
		Valid = 1073741823
	}
}
