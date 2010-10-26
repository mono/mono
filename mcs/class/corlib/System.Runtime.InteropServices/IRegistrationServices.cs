//
// System.Runtime.InteropServices.IRegistrationServices.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
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
using System.Reflection;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif
namespace System.Runtime.InteropServices {

#if NET_2_0 && !DISABLE_SECURITY
	[ComVisible(true)]
#endif	
	[Guid("CCBD682C-73A5-4568-B8B0-C7007E11ABA2")]
	public interface IRegistrationServices {
		Guid GetManagedCategoryGuid ();
#if NET_2_0 && !DISABLE_SECURITY		
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
#endif		
		string GetProgIdForType (Type type);

#if NET_2_0	&& !DISABLE_SECURITY	
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
#endif		
		Type[] GetRegistrableTypesInAssembly (Assembly assembly);

#if NET_2_0	&& !DISABLE_SECURITY	
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
#endif		
		bool RegisterAssembly (Assembly assembly, AssemblyRegistrationFlags flags);

#if NET_2_0	&& !DISABLE_SECURITY	
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
#endif				
		void RegisterTypeForComClients (Type type, ref Guid g);
		
		bool TypeRepresentsComType (Type type);
		
#if NET_2_0	&& !DISABLE_SECURITY	
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
#endif		
		bool TypeRequiresRegistration (Type type);

#if NET_2_0	&& !DISABLE_SECURITY	
		[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
#endif		
		bool UnregisterAssembly (Assembly assembly);
	}
}
