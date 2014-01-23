//
// System.Runtime.InteropServices.TypeLibConverter.cs
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

#if !FULL_AOT_RUNTIME
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	[Guid ("f1c3bf79-c3e4-11d3-88e7-00902754c43a")]
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class TypeLibConverter : ITypeLibConverter
	{
		public TypeLibConverter ()
		{
		}

		[MonoTODO ("implement")]
		[return: MarshalAs (UnmanagedType.Interface)]
		public object ConvertAssemblyToTypeLib (Assembly assembly, string strTypeLibName, TypeLibExporterFlags flags, ITypeLibExporterNotifySink notifySink)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public AssemblyBuilder ConvertTypeLibToAssembly ([MarshalAs(UnmanagedType.Interface)] object typeLib, string asmFileName, int flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, bool unsafeInterfaces)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public AssemblyBuilder ConvertTypeLibToAssembly ([MarshalAs(UnmanagedType.Interface)] object typeLib, string asmFileName, TypeLibImporterFlags flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, string asmNamespace, Version asmVersion)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public bool GetPrimaryInteropAssembly (Guid g, int major, int minor, int lcid, out string asmName, out string asmCodeBase)
		{
			throw new NotImplementedException ();
		}	
	}
}
#endif
