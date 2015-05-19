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
using System.ComponentModel;

namespace System.Windows.Forms
{
	public abstract class GridItem
	{
		#region	Fields
		private bool expanded;
		private object tag;
		#endregion Fields

		#region	Constructors
		protected GridItem() 
		{
			expanded = false;
		}
		#endregion	// Constructors

		#region Public Instance Properties
		public virtual bool Expandable
		{
			get {
				return GridItems.Count > 1;
			}
		}

		public virtual bool Expanded
		{
			get {
				return expanded;
			}

			set {
				expanded = value;
			}
		}

		public abstract GridItemCollection GridItems
		{
			get;
		}

		public abstract GridItemType GridItemType
		{
			get;
		}

		public abstract string Label
		{
			get;
		}


		public abstract GridItem Parent
		{
			get;
		}


		public abstract PropertyDescriptor PropertyDescriptor
		{
			get;
		}

		[Localizable (false)]
		[Bindable (true)]
		[DefaultValue (null)]
		[TypeConverter (typeof (StringConverter))]
		public Object Tag {
			get { return this.tag; }
			set { this.tag = value; }
		}

		public abstract object Value
		{
			get;
		}
		#endregion

		#region Public Instance Methods
		public abstract bool Select ();
		#endregion	// Public Instance Methods
	}
}
