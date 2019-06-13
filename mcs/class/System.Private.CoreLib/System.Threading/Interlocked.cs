// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Threading
{
	public static class Interlocked
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int CompareExchange (ref int location1, int value, int comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object CompareExchange (ref object location1, object value, object comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static float CompareExchange (ref float location1, float value, float comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Decrement (ref int location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Decrement (ref long location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Increment (ref int location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Increment (ref long location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Exchange (ref int location1, int value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object Exchange (ref object location1, object value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static float Exchange (ref float location1, float value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long CompareExchange (ref long location1, long value, long comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr CompareExchange (ref IntPtr location1, IntPtr value, IntPtr comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static double CompareExchange (ref double location1, double value, double comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static T CompareExchange<T> (ref T location1, T value, T comparand) where T : class;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Exchange (ref long location1, long value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr Exchange (ref IntPtr location1, IntPtr value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static double Exchange (ref double location1, double value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static T Exchange<T> (ref T location1, T value) where T : class;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Read (ref long location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Add (ref int location1, int value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Add (ref long location1, long value);

		public static void MemoryBarrier () => Thread.MemoryBarrier ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void MemoryBarrierProcessWide ();
	}
}
