// BindingFlags.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:33:54 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
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

namespace System.Reflection {

	/// <summary>
	/// </summary>
#if NET_2_0
	[ComVisible (true)]
	[Serializable]
#endif
	[Flags]
	public enum BindingFlags {

		Default = 0,

		/// <summary>
		/// </summary>
		IgnoreCase = 0x00000001,

		/// <summary>
		/// </summary>
		DeclaredOnly = 0x00000002,

		/// <summary>
		/// </summary>
		Instance = 0x00000004,

		/// <summary>
		/// </summary>
		Static = 0x00000008,

		/// <summary>
		/// </summary>
		Public = 0x00000010,

		/// <summary>
		/// </summary>
		NonPublic = 0x00000020,

		FlattenHierarchy = 0x00000040,
		
		/// <summary>
		/// </summary>
		InvokeMethod = 0x00000100,

		/// <summary>
		/// </summary>
		CreateInstance = 0x00000200,

		/// <summary>
		/// </summary>
		GetField = 0x00000400,

		/// <summary>
		/// </summary>
		SetField = 0x00000800,

		/// <summary>
		/// </summary>
		GetProperty = 0x00001000,

		/// <summary>
		/// </summary>
		SetProperty = 0x00002000,
	       
		PutDispProperty = 0x00004000,

		PutRefDispProperty = 0x00008000,

		/// <summary>
		/// </summary>
		ExactBinding = 0x00010000,

		/// <summary>
		/// </summary>
		SuppressChangeType = 0x00020000,

		/// <summary>
		/// </summary>
		OptionalParamBinding = 0x00040000,

		IgnoreReturn = 0x01000000
	} // BindingFlags

} // System.Reflection
