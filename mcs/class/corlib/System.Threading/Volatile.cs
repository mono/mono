//
// Volatile.cs
//
// Authors:
//       Marek Safar (marek.safar@gmail.com)
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	public
	static class Volatile
	{
        #region Boolean
        private struct VolatileBoolean { public volatile bool Value; }

        [Intrinsic]
        public static bool Read(ref bool location) =>
            Unsafe.As<bool, VolatileBoolean>(ref location).Value;

        [Intrinsic]
        public static void Write(ref bool location, bool value) =>
            Unsafe.As<bool, VolatileBoolean>(ref location).Value = value;
        #endregion

        #region Byte
        private struct VolatileByte { public volatile byte Value; }

        [Intrinsic]
        public static byte Read(ref byte location) =>
            Unsafe.As<byte, VolatileByte>(ref location).Value;

        [Intrinsic]
        public static void Write(ref byte location, byte value) =>
            Unsafe.As<byte, VolatileByte>(ref location).Value = value;
        #endregion

        #region Int16
        private struct VolatileInt16 { public volatile short Value; }

        [Intrinsic]
        public static short Read(ref short location) =>
            Unsafe.As<short, VolatileInt16>(ref location).Value;

        [Intrinsic]
        public static void Write(ref short location, short value) =>
            Unsafe.As<short, VolatileInt16>(ref location).Value = value;
        #endregion

        #region Int32
        private struct VolatileInt32 { public volatile int Value; }

        [Intrinsic]
        public static int Read(ref int location) =>
            Unsafe.As<int, VolatileInt32>(ref location).Value;

        [Intrinsic]
        public static void Write(ref int location, int value) =>
            Unsafe.As<int, VolatileInt32>(ref location).Value = value;
        #endregion

        #region IntPtr
        private struct VolatileIntPtr { public volatile IntPtr Value; }

        [Intrinsic]
        public static IntPtr Read(ref IntPtr location) =>
            Unsafe.As<IntPtr, VolatileIntPtr>(ref location).Value;

        [Intrinsic]
        public static void Write(ref IntPtr location, IntPtr value) =>
            Unsafe.As<IntPtr, VolatileIntPtr>(ref location).Value = value;
        #endregion

        #region SByte
        private struct VolatileSByte { public volatile sbyte Value; }

        [CLSCompliant(false)]
        [Intrinsic]
        public static sbyte Read(ref sbyte location) =>
            Unsafe.As<sbyte, VolatileSByte>(ref location).Value;

        [CLSCompliant(false)]
        [Intrinsic]
        public static void Write(ref sbyte location, sbyte value) =>
            Unsafe.As<sbyte, VolatileSByte>(ref location).Value = value;
        #endregion

        #region Single
        private struct VolatileSingle { public volatile float Value; }

        [Intrinsic]
        public static float Read(ref float location) =>
            Unsafe.As<float, VolatileSingle>(ref location).Value;

        [Intrinsic]
        public static void Write(ref float location, float value) =>
            Unsafe.As<float, VolatileSingle>(ref location).Value = value;
        #endregion

        #region UInt16
        private struct VolatileUInt16 { public volatile ushort Value; }

        [CLSCompliant(false)]
        [Intrinsic]
        public static ushort Read(ref ushort location) =>
            Unsafe.As<ushort, VolatileUInt16>(ref location).Value;

        [CLSCompliant(false)]
        [Intrinsic]
        public static void Write(ref ushort location, ushort value) =>
            Unsafe.As<ushort, VolatileUInt16>(ref location).Value = value;
        #endregion

        #region UInt32
        private struct VolatileUInt32 { public volatile uint Value; }

        [CLSCompliant(false)]
        [Intrinsic]
        public static uint Read(ref uint location) =>
            Unsafe.As<uint, VolatileUInt32>(ref location).Value;

        [CLSCompliant(false)]
        [Intrinsic]
        public static void Write(ref uint location, uint value) =>
            Unsafe.As<uint, VolatileUInt32>(ref location).Value = value;
        #endregion

        #region UIntPtr
        private struct VolatileUIntPtr { public volatile UIntPtr Value; }

        [CLSCompliant(false)]
        [Intrinsic]
        public static UIntPtr Read(ref UIntPtr location) =>
            Unsafe.As<UIntPtr, VolatileUIntPtr>(ref location).Value;

        [CLSCompliant(false)]
        [Intrinsic]
        public static void Write(ref UIntPtr location, UIntPtr value) =>
            Unsafe.As<UIntPtr, VolatileUIntPtr>(ref location).Value = value;
        #endregion

        #region T
        private struct VolatileObject { public volatile object Value; }

        [Intrinsic]
        public static T Read<T>(ref T location) where T : class =>
            Unsafe.As<T>(Unsafe.As<T, VolatileObject>(ref location).Value);

        [Intrinsic]
        public static void Write<T>(ref T location, T value) where T : class =>
            Unsafe.As<T, VolatileObject>(ref location).Value = value;
        #endregion

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static long Read (ref long location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static ulong Read (ref ulong location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static double Read (ref double location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref long location, long value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static void Write (ref ulong location, ulong value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref double location, double value);
	}
}
