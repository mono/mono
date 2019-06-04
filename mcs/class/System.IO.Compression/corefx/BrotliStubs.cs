// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.IO.Compression;
using Microsoft.Win32.SafeHandles;
using size_t = System.IntPtr;

internal static partial class Interop
{
    internal static partial class Brotli
    {
        internal static SafeBrotliEncoderHandle BrotliEncoderCreateInstance(IntPtr allocFunc, IntPtr freeFunc, IntPtr opaque) => 
            throw new PlatformNotSupportedException();

        internal static bool BrotliEncoderSetParameter(SafeBrotliEncoderHandle state, BrotliEncoderParameter parameter, UInt32 value) => 
            throw new PlatformNotSupportedException();

        internal static unsafe bool BrotliEncoderCompressStream(
            SafeBrotliEncoderHandle state, BrotliEncoderOperation op, ref size_t availableIn,
            byte** nextIn, ref size_t availableOut, byte** nextOut, out size_t totalOut) => 
            throw new PlatformNotSupportedException();

        internal static bool BrotliEncoderHasMoreOutput(SafeBrotliEncoderHandle state) => 
            throw new PlatformNotSupportedException();

        internal static void BrotliEncoderDestroyInstance(IntPtr state) => 
            throw new PlatformNotSupportedException();

        internal static unsafe bool BrotliEncoderCompress(int quality, int window, int v, size_t availableInput, byte* inBytes, ref size_t availableOutput, byte* outBytes) => 
            throw new PlatformNotSupportedException();

        internal static SafeBrotliDecoderHandle BrotliDecoderCreateInstance(IntPtr allocFunc, IntPtr freeFunc, IntPtr opaque) => 
            throw new PlatformNotSupportedException();

        internal static unsafe int BrotliDecoderDecompressStream(
            SafeBrotliDecoderHandle state, ref size_t availableIn, byte** nextIn,
            ref size_t availableOut, byte** nextOut, out size_t totalOut) => 
            throw new PlatformNotSupportedException();

        internal static unsafe bool BrotliDecoderDecompress(size_t availableInput, byte* inBytes, ref size_t availableOutput, byte* outBytes) => 
            throw new PlatformNotSupportedException();

        internal static void BrotliDecoderDestroyInstance(IntPtr state) => 
            throw new PlatformNotSupportedException();

        internal static bool BrotliDecoderIsFinished(SafeBrotliDecoderHandle state) => 
            throw new PlatformNotSupportedException();
    }
}