// FieldAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:12 UTC
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

using System.Runtime.InteropServices;

namespace System.Reflection {


	/// <summary>
	/// </summary>
#if NET_2_0
	[ComVisible (true)]
	[Serializable]
#endif
	[Flags]
	public enum FieldAttributes {

		/// <summary>
		/// </summary>
		FieldAccessMask = 7,

		/// <summary>
		/// </summary>
		PrivateScope = 0x0,

		/// <summary>
		/// </summary>
		Private = 0x1,

		/// <summary>
		/// </summary>
		FamANDAssem = 0x2,

		/// <summary>
		/// </summary>
		Assembly = 0x3,

		/// <summary>
		/// </summary>
		Family = 0x4,

		/// <summary>
		/// </summary>
		FamORAssem = 0x5,

		/// <summary>
		/// </summary>
		Public = 0x6,

		/// <summary>
		/// </summary>
		Static = 0x10,

		/// <summary>
		/// </summary>
		InitOnly = 0x20,

		/// <summary>
		/// </summary>
		Literal = 0x40,

		/// <summary>
		/// </summary>
		NotSerialized = 0x80,

		/// <summary>
		/// </summary>
		HasFieldRVA = 0x100,

		/// <summary>
		/// </summary>
		SpecialName = 0x200,

		/// <summary>
		/// </summary>
		RTSpecialName = 0x400,

		/// <summary>
		/// </summary>
		HasFieldMarshal = 0x1000,	

		/// <summary>
		/// </summary>
		PinvokeImpl = 0x2000,

		/// <summary>
		/// </summary>
		// HasSecurity = 0x4000,

		/// <summary>
		/// </summary>
		HasDefault = 0x8000,

		/// <summary>
		/// </summary>
		ReservedMask = HasDefault | HasFieldMarshal | RTSpecialName | HasFieldRVA,

	} // FieldAttributes

} // System.Reflection
