// OpCodeType.cs
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
	///  Describes the types of MSIL instructions.
	/// </summary>
	[ComVisible (true)]
	[Serializable]
	public enum OpCodeType {

		/// <summary>
		///  "Ignorable" instruction.
		///  Such instruction are used to supply
		///  additional information to particular
		///  MSIL processor.
		/// </summary>
		[Obsolete ("This API has been deprecated.")]
		Annotation = 0,

		/// <summary>
		///  Denotes "shorthand" instruction.
		///  Such instructions take less space
		///  than their full-size equivalents
		///  (ex. ldarg.0 vs. ldarg 0).
		/// </summary>
		Macro = 1,

		/// <summary>
		///  Denotes instruction reserved for internal use.
		/// </summary>
		Nternal = 2,

		/// <summary>
		///  Denotes instruction to deal with objects.
		///  (ex. ldobj).
		/// </summary>
		Objmodel = 3,

		/// <summary>
		/// </summary>
		Prefix = 4,

		/// <summary>
		/// </summary>
		Primitive = 5
	}

}
#endif
