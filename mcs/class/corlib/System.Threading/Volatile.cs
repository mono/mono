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

#if NET_4_5

using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	public static class Volatile
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static bool Read (ref bool location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static byte Read (ref byte location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static sbyte Read (ref sbyte location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static short Read (ref short location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static ushort Read (ref ushort location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static int Read (ref int location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static uint Read (ref uint location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static long Read (ref long location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static ulong Read (ref ulong location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static IntPtr Read (ref IntPtr location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static UIntPtr Read (ref UIntPtr location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static double Read (ref double location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static float Read (ref float location);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static T Read<T> (ref T location) where T : class;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref bool location, bool value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref byte location, byte value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static void Write (ref sbyte location, sbyte value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref short location, short value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static void Write (ref ushort location, ushort value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref int location, int value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static void Write (ref uint location, uint value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref long location, long value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static void Write (ref ulong location, ulong value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref IntPtr location, IntPtr value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant (false)]
		public extern static void Write (ref UIntPtr location, UIntPtr value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref double location, double value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public extern static void Write (ref float location, float value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Write<T>(ref T location, T value) where T : class;
	}
}

#endif
