// OperandType.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Reflection.Emit {

	/// <summary>
	///  Describes the operand types of MSIL instructions.
	/// </summary>
	[ComVisible (true)]
	[Serializable]
	public enum OperandType {

		/// <summary>
		/// </summary>
		InlineBrTarget = 0,

		/// <summary>
		/// </summary>
		InlineField = 1,

		/// <summary>
		/// </summary>
		InlineI = 2,

		/// <summary>
		/// </summary>
		InlineI8 = 3,

		/// <summary>
		/// </summary>
		InlineMethod = 4,

		/// <summary>
		/// </summary>
		InlineNone = 5,

		/// <summary>
		/// </summary>
		[Obsolete ("This API has been deprecated.")]
		InlinePhi = 6,

		/// <summary>
		/// </summary>
		InlineR = 7,

		/// <summary>
		/// </summary>
		InlineSig = 9,

		/// <summary>
		/// </summary>
		InlineString = 0x0A,

		/// <summary>
		/// </summary>
		InlineSwitch = 0x0B,

		/// <summary>
		/// </summary>
		InlineTok = 0x0C,

		/// <summary>
		/// </summary>
		InlineType = 0x0D,

		/// <summary>
		/// </summary>
		InlineVar = 0x0E,

		/// <summary>
		/// </summary>
		ShortInlineBrTarget = 0x0F,

		/// <summary>
		/// </summary>
		ShortInlineI = 0x10,

		/// <summary>
		/// </summary>
		ShortInlineR = 0x11,

		/// <summary>
		/// </summary>
		ShortInlineVar = 0x12
	}

}
