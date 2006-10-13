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
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms.PropertyGridInternal
{
	internal class GridEntry : GridItem
	{
		#region Local Variables
		private bool expanded = true;
		private GridItemCollection grid_items;
		private GridItem parent;
		private PropertyDescriptor property_descriptor;
		private object[] selected_objects;
		private string label;
		private int top;
		private Rectangle plus_minus_bounds;
		private Rectangle bounds;
		#endregion	// Local Variables

		#region  Contructors
		public GridEntry() : base() {
			plus_minus_bounds = new Rectangle(0,0,0,0);
			bounds = new Rectangle(0,0,0,0);
			top = -1;
			grid_items = new GridItemCollection();
		}

		public GridEntry(object[] objs, PropertyDescriptor prop_desc) : this() {
			selected_objects = objs;
			property_descriptor = prop_desc;
		}
		#endregion	// Constructors

		#region Public Instance Properties
		public override bool Expandable
		{
			get {
				return grid_items.Count > 0;
			}
		}

		public override bool Expanded
		{
			get {
				return expanded;
			}

			set {
				expanded = value;
			}
		}

		public override System.Windows.Forms.GridItemCollection GridItems
		{
			get {
				return grid_items;
			}
		}

		public override System.Windows.Forms.GridItemType GridItemType
		{
			get {
				return GridItemType.Property;
			}
		}

		public override string Label
		{
			get {
				return property_descriptor.Name;
			}
		}

		public override System.Windows.Forms.GridItem Parent
		{
			get {
				return parent;
			}
		}

		public override System.ComponentModel.PropertyDescriptor PropertyDescriptor
		{
			get {
				return property_descriptor;
			}
		}

		public override object Value
		{
			get {
				/* we should probably put this logic
				 * someplace else, maybe when we're
				 * initially populating the
				 * PropertyGrid? */
				if (selected_objects == null || selected_objects.Length == 0)
					return null;

				object v = property_descriptor.GetValue(selected_objects[0]);
				for (int i = 1; i < selected_objects.Length; i ++) {
					if (!Object.Equals (v, property_descriptor.GetValue(selected_objects[i])))
						return null;
				}

				return v;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		[MonoTODO]
		public override bool Select () {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		internal object[] SelectedObjects {
			get {
				return selected_objects;
			}
		}

		internal override int Top {
			get {
				return top;
			}
			set {
				top = value;
			}
		}

		internal override Rectangle PlusMinusBounds {
			get{
				return plus_minus_bounds;
			}
			set{
				plus_minus_bounds = value;
			}
		}

		internal override Rectangle Bounds
		{
			get
			{
				return bounds;
			}
			set
			{
				bounds = value;
			}
		}

		internal void SetParent (GridItem parent)
		{
			this.parent = parent;
		}

	}
}
