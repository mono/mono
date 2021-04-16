// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		[DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_OpenDir", SetLastError = true)]
		internal static extern IntPtr OpenDir_native(string path);

		[DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_ReadDirR", SetLastError = false)]
		internal static extern unsafe int ReadDirR_native(IntPtr dir, byte* buffer, int bufferSize, out DirectoryEntry outputEntry);

		[DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_CloseDir", SetLastError = true)]
		internal static extern int CloseDir_native(IntPtr dir);

		internal static IntPtr OpenDir (string path) {
			IntPtr result;
			do {
				result = OpenDir_native (path);
			} while (result == IntPtr.Zero && Marshal.GetLastWin32Error () == (int) Interop.Error.EINTR);
			return result;
		}

		internal static int CloseDir (IntPtr dir) {
			int result;
			do {
				result = CloseDir_native (dir);
			} while (result < 0 && Marshal.GetLastWin32Error () == (int) Interop.Error.EINTR);
			return result;
		}

		internal static unsafe int ReadDirR (IntPtr dir, byte* buffer, int bufferSize, out DirectoryEntry outputEntry) {
			int result;
			do {
				result = ReadDirR_native (dir, buffer, bufferSize, out outputEntry);
			} while (result == (int) Interop.Error.EINTR);
			return result;
		}
	}
}
