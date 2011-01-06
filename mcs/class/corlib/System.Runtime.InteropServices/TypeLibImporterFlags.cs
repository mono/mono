//
// System.Runtime.InteropServices.TypeLibImporterFlags.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

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

using System;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	[Serializable,Flags]
	public enum TypeLibImporterFlags
	{
		PrimaryInteropAssembly = 1,
		UnsafeInterfaces = 2,
		SafeArrayAsSystemArray = 4,
		TransformDispRetVals = 8,
		None = 0,
		PreventClassMembers = 16,
		ImportAsAgnostic = 2048,
		ImportAsItanium = 1024,
		ImportAsX64 = 512,
		ImportAsX86 = 256,
		ReflectionOnlyLoading = 4096,
		SerializableValueClasses = 32,
#if NET_4_0
		NoDefineVersionResource = 8192
#endif
	}
}
