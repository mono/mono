// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.SafeHandles
{
	partial class SafePasswordHandle
	{
		internal string Mono_DangerousGetString ()
		{
			return Marshal.PtrToStringAnsi (DangerousGetHandle ());
		}
	}
}
