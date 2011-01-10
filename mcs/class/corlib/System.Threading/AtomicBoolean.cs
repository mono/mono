// AtomicBoolean.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

using System;

#if INSIDE_MONO_PARALLEL
using System.Threading;

namespace Mono.Threading
#else
namespace System.Threading
#endif
{
#if INSIDE_MONO_PARALLEL
	public
#endif
	struct AtomicBooleanValue
	{
		int flag;
		const int UnSet = 0;
		const int Set = 1;

		public bool CompareAndExchange (bool expected, bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			int expectedTemp = expected ? Set : UnSet;

			return Interlocked.CompareExchange (ref flag, newTemp, expectedTemp) == expectedTemp;
		}

		public static AtomicBooleanValue FromValue (bool value)
		{
			AtomicBooleanValue temp = new AtomicBooleanValue ();
			temp.Value = value;

			return temp;
		}

		public bool TrySet ()
		{
			return !Exchange (true);
		}

		public bool TryRelaxedSet ()
		{
			return flag == UnSet && !Exchange (true);
		}

		public bool Exchange (bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			return Interlocked.Exchange (ref flag, newTemp) == Set;
		}

		public bool Value {
			get {
				return flag == Set;
			}
			set {
				Exchange (value);
			}
		}

		public bool Equals (AtomicBooleanValue rhs)
		{
			return this.flag == rhs.flag;
		}

		public override bool Equals (object rhs)
		{
			return rhs is AtomicBooleanValue ? Equals ((AtomicBooleanValue)rhs) : false;
		}

		public override int GetHashCode ()
		{
			return flag.GetHashCode ();
		}

		public static explicit operator bool (AtomicBooleanValue rhs)
		{
			return rhs.Value;
		}

		public static implicit operator AtomicBooleanValue (bool rhs)
		{
			return AtomicBooleanValue.FromValue (rhs);
		}
	}

#if INSIDE_MONO_PARALLEL
	public
#endif
	class AtomicBoolean
	{
		int flag;
		const int UnSet = 0;
		const int Set = 1;

		public bool CompareAndExchange (bool expected, bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			int expectedTemp = expected ? Set : UnSet;

			return Interlocked.CompareExchange (ref flag, newTemp, expectedTemp) == expectedTemp;
		}

		public static AtomicBoolean FromValue (bool value)
		{
			AtomicBoolean temp = new AtomicBoolean ();
			temp.Value = value;

			return temp;
		}

		public bool TrySet ()
		{
			return !Exchange (true);
		}

		public bool TryRelaxedSet ()
		{
			return flag == UnSet && !Exchange (true);
		}

		public bool Exchange (bool newVal)
		{
			int newTemp = newVal ? Set : UnSet;
			return Interlocked.Exchange (ref flag, newTemp) == Set;
		}

		public bool Value {
			get {
				return flag == Set;
			}
			set {
				Exchange (value);
			}
		}

		public bool Equals (AtomicBoolean rhs)
		{
			return this.flag == rhs.flag;
		}

		public override bool Equals (object rhs)
		{
			return rhs is AtomicBoolean ? Equals ((AtomicBoolean)rhs) : false;
		}

		public override int GetHashCode ()
		{
			return flag.GetHashCode ();
		}

		public static explicit operator bool (AtomicBoolean rhs)
		{
			return rhs.Value;
		}

		public static implicit operator AtomicBoolean (bool rhs)
		{
			return AtomicBoolean.FromValue (rhs);
		}
	}
}
