// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.SendFile.cs'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error SendFile (SafeHandle out_fd, SafeHandle in_fd, long offset, long count, out long sent)
		{
			return (Error)SendFile_internal (out_fd.DangerousGetHandle (), in_fd.DangerousGetHandle (), offset, count, out sent);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int SendFile_internal (IntPtr out_fd, IntPtr in_fd, long offset, long count, out long sent);
	}
}
