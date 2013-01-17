// MethodImplAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:42 UTC
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
	public enum MethodImplAttributes {

		/// <summary>
		/// </summary>
		CodeTypeMask = 3,

		/// <summary>
		/// </summary>
		IL = 0,

		/// <summary>
		/// </summary>
		Native = 1,

		/// <summary>
		/// </summary>
		OPTIL = 2,

		/// <summary>
		/// </summary>
		Runtime = 3,

		/// <summary>
		/// </summary>
		ManagedMask = 4,

		/// <summary>
		/// </summary>
		Unmanaged = 4,

		/// <summary>
		/// </summary>
		Managed = 0,

		/// <summary>
		/// </summary>
		ForwardRef = 16,

		/// <summary>
		/// </summary>
		PreserveSig = 128,

		/// <summary>
		/// </summary>
		InternalCall = 4096,

		/// <summary>
		/// </summary>
		Synchronized = 32,

		/// <summary>
		/// </summary>
		NoInlining = 8,

		/// <summary>
		/// </summary>
		NoOptimization = 64,

		/// <summary>
		/// </summary>
		MaxMethodImplVal = 65535,
		
#if NET_4_5
		AggressiveInlining = 256
#endif
	}
}
