// TypeAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:40:22 UTC
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
	[ComVisible (true)]
	[Serializable]
	[Flags]
	public enum TypeAttributes {

		/// <summary>
		/// </summary>
		VisibilityMask = 7,

		/// <summary>
		/// </summary>
		NotPublic = 0,

		/// <summary>
		/// </summary>
		Public = 1,

		/// <summary>
		/// </summary>
		NestedPublic = 2,

		/// <summary>
		/// </summary>
		NestedPrivate = 3,

		/// <summary>
		/// </summary>
		NestedFamily = 4,

		/// <summary>
		/// </summary>
		NestedAssembly = 5,

		/// <summary>
		/// </summary>
		NestedFamANDAssem = 6,

		/// <summary>
		/// </summary>
		NestedFamORAssem = 7,

		/// <summary>
		/// </summary>
		LayoutMask = 24,

		/// <summary>
		/// </summary>
		AutoLayout = 0,

		/// <summary>
		/// </summary>
		SequentialLayout = 8,

		/// <summary>
		/// </summary>
		ExplicitLayout = 16,

		/// <summary>
		/// </summary>
		ClassSemanticsMask = 32,

		/// <summary>
		/// </summary>
		Class = 0,

		/// <summary>
		/// </summary>
		Interface = 32,

		/// <summary>
		/// </summary>
		Abstract = 128,

		/// <summary>
		/// </summary>
		Sealed = 256,

		/// <summary>
		/// </summary>
		SpecialName = 1024,

		/// <summary>
		/// </summary>
		Import = 4096,

		/// <summary>
		/// </summary>
		Serializable = 8192,

#if NET_4_5
		WindowsRuntime = 16384,
#endif

		/// <summary>
		/// </summary>
		StringFormatMask = 196608,

		/// <summary>
		/// </summary>
		AnsiClass = 0,

		/// <summary>
		/// </summary>
		UnicodeClass = 65536,

		/// <summary>
		/// </summary>
		AutoClass = 131072,

		/// <summary>
		/// </summary>
		BeforeFieldInit = 1048576,

		/// <summary>
		/// </summary>
		ReservedMask = 264192,

		/// <summary>
		/// </summary>
		RTSpecialName = 2048,

		/// <summary>
		/// </summary>
		HasSecurity = 262144,

		/// <summary>
		/// </summary>
		CustomFormatClass = 0x30000,

		/// <summary>
		/// </summary>
		CustomFormatMask = 0xc00000
	} // TypeAttributes

} // System.Reflection
