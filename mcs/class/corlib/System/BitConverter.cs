// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
	public static partial class BitConverter
	{
		[Intrinsic]
		public static readonly bool IsLittleEndian;

		static BitConverter ()
		{
			unsafe {
				ushort i = 0x1234;
				byte *b = (byte*)&i;
				IsLittleEndian = (*b == 0x34);
			}
		}
	}
}
