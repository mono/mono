//
// StructuralComparisons.cs
//
// Authors:
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

namespace System.Collections
{
	public static class StructuralComparisons
	{
		sealed class ComparerImpl : IComparer, IEqualityComparer
		{
			int IComparer.Compare (object x, object y)
			{
				var comparer = x as IStructuralComparable;
				if (comparer != null)
					return comparer.CompareTo (y, this);

				return Comparer.Default.Compare (x, y);
			}

			int IEqualityComparer.GetHashCode (object obj)
			{
				var comparer = obj as IStructuralEquatable;
				if (comparer != null)
					return comparer.GetHashCode (this);

				return Generic.EqualityComparer<object>.Default.GetHashCode (obj);
			}

			bool IEqualityComparer.Equals (object x, object y)
			{
				var comparer = x as IStructuralEquatable;
				if (comparer != null)
					return comparer.Equals (y, this);

				return Generic.EqualityComparer<object>.Default.Equals (x, y);
			}
		}
		
		static readonly ComparerImpl comparer = new ComparerImpl ();
		
		public static IComparer StructuralComparer {
			get {
				return comparer;
			}
		}
		
		public static IEqualityComparer StructuralEqualityComparer {
			get {
				return comparer;
			}
		}
	}
}

#endif
