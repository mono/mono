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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Brian O'Keefe (zer0keefie@gmail.com)
//

using System;

namespace System.ComponentModel {

	public struct SortDescription
	{
		private string sortPropertyName;
		private ListSortDirection sortDirection;
		private bool isSealed;

		public SortDescription (string propertyName, ListSortDirection direction)
		{
			if(direction != ListSortDirection.Ascending && direction != ListSortDirection.Descending)
				throw new InvalidEnumArgumentException("direction", (int)direction, typeof(ListSortDirection));

			sortPropertyName = propertyName;
			sortDirection = direction;
			isSealed = false;
		}

		public static bool operator!= (SortDescription sd1, SortDescription sd2)
		{
			return !(sd1 == sd2);
		}

		public static bool operator== (SortDescription sd1, SortDescription sd2)
		{
			return sd1.sortDirection == sd2.sortDirection && sd1.sortPropertyName == sd2.sortPropertyName;
		}

		public ListSortDirection Direction {
			get { return sortDirection; }
			set { 
				if(isSealed)
					throw new InvalidOperationException("Cannot change Direction once the SortDescription has been sealed.");
				
				if(value != ListSortDirection.Ascending && value != ListSortDirection.Descending)
					throw new InvalidEnumArgumentException("direction", (int)value, typeof(ListSortDirection));
				
				sortDirection = value; 
			}
		}

		public bool IsSealed {
			get { return isSealed; }
		}

		public string PropertyName {
			get { return sortPropertyName; }
			set {
				if(isSealed)
					throw new InvalidOperationException("Cannot change Direction once the SortDescription has been sealed.");
				
				sortPropertyName = value;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SortDescription))
				return false;
			return ((SortDescription)obj) == this;
		}

		public override int GetHashCode ()
		{
			if(sortPropertyName == null)
				return sortDirection.GetHashCode ();
			return sortPropertyName.GetHashCode () ^ sortDirection.GetHashCode ();
		}

		internal void Seal()
		{
			isSealed = true;
		}
	}
}