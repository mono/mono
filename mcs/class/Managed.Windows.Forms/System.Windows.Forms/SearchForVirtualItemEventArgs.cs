//
// SearchForVirtualItemEventArgs.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;

namespace System.Windows.Forms
{
	public class SearchForVirtualItemEventArgs : EventArgs
	{
		private SearchDirectionHint direction;
		private bool include_sub_items_in_search;
		private int index;
		private bool is_prefix_search;
		private bool is_text_search;
		private int start_index;
		private Point starting_point;
		private string text;

		#region Public Constructors
		public SearchForVirtualItemEventArgs (bool isTextSearch, bool isPrefixSearch,
			bool includeSubItemsInSearch, string text, Point startingPoint, 
			SearchDirectionHint direction, int startIndex) : base ()
		{
			this.is_text_search = isTextSearch;
			this.is_prefix_search = isPrefixSearch;
			this.include_sub_items_in_search = includeSubItemsInSearch;
			this.text = text;
			this.starting_point = startingPoint;
			this.direction = direction;
			this.start_index = startIndex;
			this.index = -1;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public SearchDirectionHint Direction {
			get { return this.direction; }
		}

		public bool IncludeSubItemsInSearch {
			get { return this.include_sub_items_in_search; }
		}
		
		public int Index {
			get { return this.index; }
			set { this.index = value; }
		}
		
		public bool IsPrefixSearch {
			get { return this.is_prefix_search; }
		}
		
		public bool IsTextSearch {
			get { return this.is_text_search; }
		}
		
		public int StartIndex {
			get { return this.start_index; }
		}
		
		public Point StartingPoint {
			get { return this.starting_point; }
		}
		
		public string Text {
			get { return this.text; }
		}
			
		#endregion	// Public Instance Properties
	}
}
