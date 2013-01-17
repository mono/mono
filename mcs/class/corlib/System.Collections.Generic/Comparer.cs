//
// Comparer
//
// Authors:
//	Ben Maurer (bmaurer@ximian.com)
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Collections.Generic {
	[Serializable]
	public abstract class Comparer<T> : IComparer<T>, IComparer
	{
		static readonly Comparer <T> _default = typeof (IComparable<T>).IsAssignableFrom (typeof (T)) ?
			(Comparer<T>) Activator.CreateInstance (typeof (GenericComparer <>).MakeGenericType (typeof (T))) :
			new DefaultComparer ();
		
		public abstract int Compare (T x, T y);
	
		public static Comparer<T> Default {
			get {
				return _default;
			}
		}

#if NET_4_5
		public static Comparer<T> Create (Comparison<T> comparison)
		{
			if (comparison == null)
				throw new ArgumentNullException ("comparison");

			return new ComparisonComparer<T> (comparison);
		}
#endif

		int IComparer.Compare (object x, object y)
		{
			
			if (x == null)
				return y == null ? 0 : -1;
			if (y == null)
				return 1;
			
			if (x is T && y is T)
				return Compare ((T) x, (T) y);
			
			throw new ArgumentException ();
		}
	
		[Serializable]
		sealed class DefaultComparer : Comparer<T>
		{
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
		}
	}
	
	[Serializable]
	sealed class GenericComparer<T> : Comparer<T> where T : IComparable<T>
	{
		public override int Compare (T x, T y)
		{
			// `null' is less than any other ref type
			if (x == null)
				return y == null ? 0 : -1;
			if (y == null)
				return 1;
			
			return x.CompareTo (y);
		}
	}
#if NET_4_5
	[Serializable]
	sealed class ComparisonComparer<T> : Comparer<T>
	{
		readonly Comparison<T> comparison;

		public ComparisonComparer (Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public override int Compare (T x, T y)
		{
			return comparison (x, y);
		}
	}
#endif
}
