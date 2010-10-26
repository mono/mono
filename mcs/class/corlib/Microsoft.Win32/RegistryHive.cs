//
// Microsoft.Win32.RegistryHive.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)

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

#if !NET_2_1 || UNITY

using System;

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace Microsoft.Win32
{

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public enum RegistryHive
	{
		
		ClassesRoot = -2147483648,
		CurrentConfig = -2147483643,
		CurrentUser = -2147483647,
		DynData = -2147483642,
		LocalMachine = -2147483646,
		PerformanceData = -2147483644,
		Users = -2147483645
	}

}

#endif // NET_2_1

