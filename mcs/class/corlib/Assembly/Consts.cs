//
// Consts.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2004 Novell, Inc.
//
// NOTE:
//	Ensure that every constant is defined for every version symbol!
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

// This class contains constants that are dependent on the defined symbols
// Use it to shorten and make code more maintainable in situations like:
//
//#if (NET_1_0)
//	[Designer ("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
//#endif
//#if (NET_1_1)
//    	[Designer ("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
//#endif
//
// by changing them into:
//
// [Designer ("System.Diagnostics.Design.ProcessDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
//

using System.Runtime.InteropServices;

internal sealed class Consts
{
	
	private Consts ()
	{
	}

#if (NET_1_0)

	public const string AssemblyI18N = "I18N, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
	public const string AssemblyMono_CSharp_Debugger = "Mono.CSharp.Debugger, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";

#elif (NET_2_0)

	public const string AssemblyI18N = "I18N, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
	public const string AssemblyMono_CSharp_Debugger = "Mono.CSharp.Debugger, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
	
#else
	// NET_1_1 is seen as default if somebody 'forgets' to specify any of the symbols
	// to ensure we are not breaking the build in this case

	public const string AssemblyI18N = "I18N, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";
	public const string AssemblyMono_CSharp_Debugger = "Mono.CSharp.Debugger, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756";

#endif

#if BOOTSTRAP_WITH_OLDLIB
	public const UnmanagedType UnmanagedType_80 = UnmanagedType.mono_bootstrap_NativeTypeMax;
#else
	public const UnmanagedType UnmanagedType_80 = (UnmanagedType) 80;
#endif
}
