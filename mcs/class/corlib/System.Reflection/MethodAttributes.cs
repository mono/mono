// MethodAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:32 UTC
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
	public enum MethodAttributes {

		/// <summary>
		/// </summary>
		MemberAccessMask = 7,

		/// <summary>
		/// </summary>
		PrivateScope = 0,

		/// <summary>
		/// </summary>
		Private = 1,

		/// <summary>
		/// </summary>
		FamANDAssem = 2,

		/// <summary>
		/// </summary>
		Assembly = 3,

		/// <summary>
		/// </summary>
		Family = 4,

		/// <summary>
		/// </summary>
		FamORAssem = 5,

		/// <summary>
		/// </summary>
		Public = 6,

		/// <summary>
		/// </summary>
		Static = 16,

		/// <summary>
		/// </summary>
		Final = 32,

		/// <summary>
		/// </summary>
		Virtual = 64,

		/// <summary>
		/// </summary>
		HideBySig = 128,

		/// <summary>
		/// </summary>
		VtableLayoutMask = 256,

#if NET_1_1
		/// <summary>
		/// </summary>		
		CheckAccessOnOverride = 512,
#endif

		/// <summary>
		/// </summary>
		ReuseSlot = 0,

		/// <summary>
		/// </summary>
		NewSlot = 256,

		/// <summary>
		/// </summary>
		Abstract = 1024,

		/// <summary>
		/// </summary>
		SpecialName = 2048,

		/// <summary>
		/// </summary>
		PinvokeImpl = 8192,

		/// <summary>
		/// </summary>
		UnmanagedExport = 8,

		/// <summary>
		/// </summary>
		RTSpecialName = 4096,

		/// <summary>
		/// </summary>
		ReservedMask = 53248,

		/// <summary>
		/// </summary>
		HasSecurity = 16384,

		/// <summary>
		/// </summary>
		RequireSecObject = 32768,
	} // MethodAttributes

} // System.Reflection
