//
// Authors:
//   Tim Jenks (tim.jenks@realtimeworlds.com)
//
// (C) 2008 Realtime Worlds Ltd
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mono.Unix.Native {

	public struct RealTimeSignum
#if NET_2_0
		: IEquatable <RealTimeSignum>
#endif
	{
		private int rt_offset;
		private static readonly int MaxOffset = UnixSignal.GetSIGRTMAX () - UnixSignal.GetSIGRTMIN () - 1;
		public static readonly RealTimeSignum MinValue = new RealTimeSignum (0);
		public static readonly RealTimeSignum MaxValue = new RealTimeSignum (MaxOffset);

		public RealTimeSignum (int offset)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("Offset cannot be negative");
			if (offset > MaxOffset)
				throw new ArgumentOutOfRangeException ("Offset greater than maximum supported SIGRT");
			rt_offset = offset;
 		}

		public int Offset {
			get { return rt_offset; }
		}

		public override int GetHashCode ()
		{
			return rt_offset.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if ((obj == null) || (obj.GetType () != GetType ()))
				return false;
			return Equals ((RealTimeSignum)obj);
		}

		public bool Equals (RealTimeSignum value)
		{
			return Offset == value.Offset;
		}

		public static bool operator== (RealTimeSignum lhs, RealTimeSignum rhs)
		{
			return lhs.Equals (rhs);
		}

		public static bool operator!= (RealTimeSignum lhs, RealTimeSignum rhs)
		{
			return !lhs.Equals (rhs);
		}
	}
}
