//
// System.CodeDom MemberAttributes Enum implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
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

using System.Runtime.InteropServices;

namespace System.CodeDom {

	[Serializable]
	[ComVisible(true)]
	public enum MemberAttributes {
		Abstract =		0x00000001,
		Final =			0x00000002,
		Static =		0x00000003,
		Override =		0x00000004,
		Const =			0x00000005,
		ScopeMask =		0x0000000F,

		New =			0x00000010,
		VTableMask =		0x000000F0,

		Overloaded =		0x00000100,

		Assembly =		0x00001000, // internal
		FamilyAndAssembly =	0x00002000, // protected AND internal
		Family =		0x00003000, // protected
		FamilyOrAssembly =	0x00004000, // protected internal
		Private =		0x00005000, // private
		Public =		0x00006000, // public
		AccessMask =		0x0000F000
	}
}
