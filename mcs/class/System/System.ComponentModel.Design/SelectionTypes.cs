//
// System.ComponentModel.Design.SelectionTypes.cs
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2006 Ivan N. Zlatev

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

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[Flags]
	[ComVisible(true)]
	public enum SelectionTypes
	{
		Auto = 1,
		[Obsolete ("This value has been deprecated. Use SelectionTypes.Auto instead.")]
		Normal = 1,
		Replace = 2,
		[Obsolete ("This value has been deprecated. It is no longer supported.")]
		MouseDown = 4,
		[Obsolete ("This value has been deprecated. It is no longer supported.")]
		MouseUp = 8,
		[Obsolete ("This value has been deprecated. Use SelectionTypes.Primary instead.")]
		Click = 16,
		Primary = 16,
		[Obsolete ("This value has been deprecated. It is no longer supported.")]
		Valid = 31,
		Toggle = 32,
		Add = 64,
		Remove = 128
	}
}
