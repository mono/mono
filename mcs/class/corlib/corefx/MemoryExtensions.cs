// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System
{
	partial class MemoryExtensions
	{
		// copied from corefx - MemoryExtensions.Fast.cs
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool EqualsOrdinal(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
		{
			if (span.Length != value.Length)
				return false;
			if (value.Length == 0)  // span.Length == value.Length == 0
				return true;
			return span.SequenceEqual(value); //TODO: Optimize - https://github.com/dotnet/corefx/issues/27487
		}
	}
}
