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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

// NOT COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms
{
	public abstract class GridItem
	{
		#region	Constructors
		protected GridItem() {
		}
		#endregion	// Constructors

		#region Public Instance Properties
		[MonoTODO]
		public virtual new bool Expandable
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public virtual new bool Expanded
		{
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public abstract new System.Windows.Forms.GridItemCollection GridItems
		{
			get;
		}

		public abstract new System.Windows.Forms.GridItemType GridItemType
		{
			get;
		}

		public abstract new string Label
		{
			get;
		}


		public abstract new System.Windows.Forms.GridItem Parent
		{
			get;
		}


		public abstract new System.ComponentModel.PropertyDescriptor PropertyDescriptor
		{
			get;
		}

		public abstract new object Value
		{
			get;
		}
		#endregion

		#region Public Instance Methods
		public abstract new bool Select ();
		#endregion	// Public Instance Methods

		internal abstract int Top {
			get;
			set;
		}

		internal abstract Rectangle PlusMinusBounds {
			get;
			set;
		}

	}
}
