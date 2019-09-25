// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.Fcntl.cs'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static class Fcntl
		{
			internal static int DangerousSetIsNonBlocking (IntPtr fd, int isNonBlocking)
			{
				return SetIsNonBlocking_internal (fd, isNonBlocking);
			}

			internal static int SetIsNonBlocking (SafeHandle fd, int isNonBlocking)
			{
				return SetIsNonBlocking_internal (fd.DangerousGetHandle (), isNonBlocking);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern int SetIsNonBlocking_internal (IntPtr fd, int isNonBlocking);
	}
}
