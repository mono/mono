// UnmanagedType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
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


namespace System.Runtime.InteropServices {


	/// <summary>
	/// </summary>
	public enum UnmanagedType {

		/// <summary>
		/// </summary>
		Bool = 2,

		/// <summary>
		/// </summary>
		I1 = 3,

		/// <summary>
		/// </summary>
		U1 = 4,

		/// <summary>
		/// </summary>
		I2 = 5,

		/// <summary>
		/// </summary>
		U2 = 6,

		/// <summary>
		/// </summary>
		I4 = 7,

		/// <summary>
		/// </summary>
		U4 = 8,

		/// <summary>
		/// </summary>
		I8 = 9,

		/// <summary>
		/// </summary>
		U8 = 10,

		/// <summary>
		/// </summary>
		R4 = 11,

		/// <summary>
		/// </summary>
		R8 = 12,

		Currency = 15,

		/// <summary>
		/// </summary>
		BStr = 19,

		/// <summary>
		/// </summary>
		LPStr = 20,

		/// <summary>
		/// </summary>
		LPWStr = 21,

		/// <summary>
		/// </summary>
		LPTStr = 22,

		/// <summary>
		/// </summary>
		ByValTStr = 23,

		/// <summary>
		/// </summary>
		IUnknown = 25,

		/// <summary>
		/// </summary>
		IDispatch = 26,

		/// <summary>
		/// </summary>
		Struct = 27,

		/// <summary>
		/// </summary>
		Interface = 28,

		/// <summary>
		/// </summary>
		SafeArray = 29,

		/// <summary>
		/// </summary>
		ByValArray = 30,

		/// <summary>
		/// </summary>
		SysInt = 31,

		/// <summary>
		/// </summary>
		SysUInt = 32,

		/// <summary>
		/// </summary>
		VBByRefStr = 34,

		/// <summary>
		/// </summary>
		AnsiBStr = 35,

		/// <summary>
		/// </summary>
		TBStr = 36,

		/// <summary>
		/// </summary>
		VariantBool = 37,

		/// <summary>
		/// </summary>
		FunctionPtr = 38,

		/// <summary>
		/// </summary>
		// LPVoid = 39,

		/// <summary>
		/// </summary>
		AsAny = 40,

		/// <summary>
		/// </summary>
		//RPrecise = 41,

		/// <summary>
		/// </summary>
		LPArray = 42,

		/// <summary>
		/// </summary>
		LPStruct = 43,

		/// <summary>
		/// </summary>
		CustomMarshaler = 44,

		/// <summary>
		/// </summary>
		Error = 45,

#if BOOTSTRAP_WITH_OLDLIB
		/// <summary>
		/// </summary>
		mono_bootstrap_NativeTypeMax = 80,
#endif
	} // UnmanagedType

} // System.Runtime.InteropServices
