//
// Comparer
//
// Authors:
//	Ben Maurer (bmaurer@ximian.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[CLSCompliant(false)]
	public abstract class Comparer<T> : IComparer<T>, System.Collections.IKeyComparer,
		System.Collections.IComparer, System.Collections.IHashCodeProvider {
	
		public Comparer () {} /* workaround 60438 by not having a protected ctor */
		public abstract int Compare (T x, T y);
		public virtual bool Equals (T x, T y)
		{
			return Compare (x, y) == 0;
		}
		public virtual int GetHashCode (T obj)
		{
			if (obj == null)
				throw new ArgumentNullException ();
			return obj.GetHashCode ();
		}
	
		static DefaultComparer <T> _default;
		
		[MonoTODO ("This is going to make a really slow comparer. We need to speed this up if T : ICompareable<T> create a class with a where clause of T : ICompareable <T>")]
		public static Comparer<T> Default {
			get {
				if (_default != null)
					return _default;
				return _default = new DefaultComparer<T> ();
			}
		}
	
		int System.Collections.IComparer.Compare (object x, object y)
		{
			
			if (x == null)
				return y == null ? 0 : -1;
			if (y == null)
				return 1;
			
			if (x is T && y is T)
				return Compare ((T) x, (T) y);
			
			throw new ArgumentException ();
		}
	
		bool System.Collections.IKeyComparer.Equals (object x, object y)
		{
			if (x == y)
				return true;
			
			if (x == null || y == null)
				return false;
			
			if (x is T && y is T)
				return Equals ((T) x, (T) y);
			
			throw new ArgumentException ();
		}
	
		int System.Collections.IHashCodeProvider.GetHashCode (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ();
			if (obj is T)
				return GetHashCode ((T) obj);
			
			throw new ArgumentException ();
		}
	
		class DefaultComparer<T> : Comparer<T> {
	
			public override int Compare (T x, T y)
			{
				// `null' is less than any other ref type
				if (x == null)
					return y == null ? 0 : -1;
				else if (y == null)
					return 1;
	
				if (x is IComparable<T>)
					return ((IComparable<T>) x).CompareTo (y);
				else if (x is IComparable)
					return ((IComparable) x).CompareTo (y);
				else
					throw new ArgumentException ("does not implement right interface");
			}
	
			public override bool Equals (T x, T y)
			{
				return Object.Equals (x, y);
			}
		}
	}

}
#endif
