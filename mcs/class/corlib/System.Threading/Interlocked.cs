//
// System.Threading.Interlocked.cs
//
// Author:
//	  Patrik Torstensson (patrik.torstensson@labs2.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	public sealed class Interlocked 
	{
		private Interlocked () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int CompareExchange(ref int location1, int value, int comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object CompareExchange(ref object location1, object value, object comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static float CompareExchange(ref float location1, float value, float comparand);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Decrement(ref int location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Decrement(ref long location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Increment(ref int location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long Increment(ref long location);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int Exchange(ref int location1, int value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object Exchange(ref object location1, object value);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static float Exchange(ref float location1, float value);
	}
}

