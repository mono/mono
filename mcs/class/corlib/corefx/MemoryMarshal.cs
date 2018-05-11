// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Internal.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
	partial class MemoryMarshal
	{
        /// <summary>
        /// Returns a reference to the 0th element of the Span. If the Span is empty, returns a reference to fake non-null pointer. Such a reference can be used
        /// for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(Span<T> span) =>
            ref (span.Length != 0) ? ref span.DangerousGetPinnableReference() : ref Unsafe.AsRef<T>((void*)1);

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlySpan. If the ReadOnlySpan is empty, returns a reference to fake non-null pointer. Such a reference
        /// can be used for pinning but must never be dereferenced. This is useful for interop with methods that do not accept null pointers for zero-sized buffers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref T GetNonNullPinnableReference<T>(ReadOnlySpan<T> span) => 
            ref (span.Length != 0) ? ref span.DangerousGetPinnableReference() : ref Unsafe.AsRef<T>((void*)1);
	}
}