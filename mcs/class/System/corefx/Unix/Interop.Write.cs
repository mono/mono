// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.Write.cs'

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System;

internal static partial class Interop
{
	internal static partial class Sys
	{
		/// <summary>
		/// Writes the specified buffer to the provided open file descriptor
		/// </summary>
		/// <param name="fd">The file descriptor to try and write to</param>
		/// <param name="buffer">The data to attempt to write</param>
		/// <param name="bufferSize">The amount of data to write, in bytes</param>
		/// <returns>
		/// Returns the number of bytes written on success; otherwise, returns -1 and sets errno
		/// </returns>
		internal static unsafe int Write (SafeFileHandle fd, byte* buffer, int bufferSize)
		{
			int bytes_written = -1;
			do {
				bytes_written = Write_internal (fd.DangerousGetHandle (), buffer, bufferSize);
			} while (bytes_written < 0 && Marshal.GetLastWin32Error () == (int) Interop.Error.EINTR);
			return bytes_written;
		}

		internal static unsafe int Write (int fd, byte* buffer, int bufferSize)
		{
			int bytes_written = -1;
			do {
				bytes_written = Write_internal ((IntPtr)fd, buffer, bufferSize);
			} while (bytes_written < 0 && Marshal.GetLastWin32Error () == (int) Interop.Error.EINTR);
			return bytes_written;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int Write_internal (IntPtr fd, byte* buffer, int count);
	}
}
