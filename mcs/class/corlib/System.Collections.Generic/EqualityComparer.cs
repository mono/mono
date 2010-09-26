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

using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[Serializable]
	public abstract class EqualityComparer <T> : IEqualityComparer, IEqualityComparer <T> {
		
		static EqualityComparer ()
		{
			if (typeof (T) == typeof (string)){
				_default = (EqualityComparer<T>) (object) new InternalStringComparer ();
				return;
			}
			if (typeof (IEquatable <T>).IsAssignableFrom (typeof (T)))
				_default = (EqualityComparer <T>) Activator.CreateInstance (typeof (GenericEqualityComparer <>).MakeGenericType (typeof (T)));
			else
				_default = new DefaultComparer ();
		}
		
		
		public abstract int GetHashCode (T obj);
		public abstract bool Equals (T x, T y);
	
		static readonly EqualityComparer <T> _default;
		
		public static EqualityComparer <T> Default {
			get {
				return _default;
			}
		}

		int IEqualityComparer.GetHashCode (object obj)
		{
			return GetHashCode ((T)obj);
		}

		bool IEqualityComparer.Equals (object x, object y)
		{
			if (!(x is T))
				throw new ArgumentException ("Argument is not compatible", "x");
			if (!(y is T))
				throw new ArgumentException ("Argument is not compatible", "y");
			return Equals ((T)x, (T)y);
		}
		
		[Serializable]
		sealed class DefaultComparer : EqualityComparer<T> {
	
			public override int GetHashCode (T obj)
			{
				if (obj == null)
					return 0;
				return obj.GetHashCode ();
			}
	
			public override bool Equals (T x, T y)
			{
				if (x == null)
					return y == null;

				return x.Equals (y);
			}
		}
	}
	
	[Serializable]
	sealed class InternalStringComparer : EqualityComparer<string> {
	
		public override int GetHashCode (string obj)
		{
			if (obj == null)
				return 0;
			return obj.GetHashCode ();
		}
	
		public override bool Equals (string x, string y)
		{
			if (x == null)
				return y == null;

			if ((object) x == (object) y)
				return true;
				
			return x.Equals (y);
		}
	}

	[Serializable]
	sealed class GenericEqualityComparer <T> : EqualityComparer <T> where T : IEquatable <T> {

		public override int GetHashCode (T obj)
		{
			if (obj == null)
				return 0;
			return obj.GetHashCode ();
		}

		public override bool Equals (T x, T y)
		{
			if (x == null)
				return y == null;
			
			return x.Equals (y);
		}
	}
}
