// StackBehaviour.cs
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

#if !FULL_AOT_RUNTIME
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	/// <summary>
	///  Describes how values are pushed onto or popped off a stack.
	/// </summary>
	[ComVisible (true)]
	[Serializable]
	public enum StackBehaviour {

		/// <summary>
		/// </summary>
		Pop0 = 0,

		/// <summary>
		/// </summary>
		Pop1 = 1,

		/// <summary>
		/// </summary>
		Pop1_pop1 = 2,

		/// <summary>
		/// </summary>
		Popi = 3,

		/// <summary>
		/// </summary>
		Popi_pop1 = 4,

		/// <summary>
		/// </summary>
		Popi_popi = 5,

		/// <summary>
		/// </summary>
		Popi_popi8 = 6,

		/// <summary>
		/// </summary>
		Popi_popi_popi = 7,

		/// <summary>
		/// </summary>
		Popi_popr4 = 8,

		/// <summary>
		/// </summary>
		Popi_popr8 = 9,

		/// <summary>
		/// </summary>
		Popref = 0x0A,

		/// <summary>
		/// </summary>
		Popref_pop1 = 0x0B,

		/// <summary>
		/// </summary>
		Popref_popi = 0x0C,

		/// <summary>
		/// </summary>
		Popref_popi_popi = 0x0D,

		/// <summary>
		/// </summary>
		Popref_popi_popi8 = 0x0E,

		/// <summary>
		/// </summary>
		Popref_popi_popr4 = 0x0F,

		/// <summary>
		/// </summary>
		Popref_popi_popr8 = 0x10,

		/// <summary>
		/// </summary>
		Popref_popi_popref = 0x11,

		/// <summary>
		/// </summary>
		Push0 = 0x12,

		/// <summary>
		/// </summary>
		Push1 = 0x13,

		/// <summary>
		/// </summary>
		Push1_push1 = 0x14,

		/// <summary>
		/// </summary>
		Pushi = 0x15,

		/// <summary>
		/// </summary>
		Pushi8 = 0x16,

		/// <summary>
		/// </summary>
		Pushr4 = 0x17,

		/// <summary>
		/// </summary>
		Pushr8 = 0x18,

		/// <summary>
		/// </summary>
		Pushref = 0x19,

		/// <summary>
		/// </summary>
		Varpop = 0x1A,

		/// <summary>
		/// </summary>
		Varpush = 0x1B,
		Popref_popi_pop1 = 0x1C

	}

}
#endif
