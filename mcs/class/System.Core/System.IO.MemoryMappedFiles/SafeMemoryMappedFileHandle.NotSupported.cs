// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Microsoft.Win32.SafeHandles
{
	public sealed partial class SafeMemoryMappedFileHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SafeMemoryMappedFileHandle(
			FileStream fileStream, bool ownsFileStream, HandleInheritability inheritability,
			MemoryMappedFileAccess access, MemoryMappedFileOptions options,
			long capacity)
			: base(ownsHandle: true)
		{
			throw new PlatformNotSupportedException();
		}

		protected override unsafe bool ReleaseHandle()
		{
			throw new PlatformNotSupportedException();
		}

		public override bool IsInvalid
		{
			get { throw new PlatformNotSupportedException(); }
		}
	}
}
