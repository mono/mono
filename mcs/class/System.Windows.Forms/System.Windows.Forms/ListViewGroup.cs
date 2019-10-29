//
//  ListViewGroup.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//	Daniel Nauck		(dna(at)mono-project(dot)de)

using System;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	[SerializableAttribute]
	[ToolboxItem(false)]
	[DesignTimeVisible(false)]
	[DefaultProperty("Header")]
	[TypeConverter (typeof (ListViewGroupConverter))]
	public sealed class ListViewGroup : ISerializable
	{
		internal string header = string.Empty;
		private string name = null;
		private HorizontalAlignment header_alignment = HorizontalAlignment.Left;
		private ListView list_view_owner = null;
		private ListView.ListViewItemCollection items = null;
		private object tag = null;
		private Rectangle header_bounds = Rectangle.Empty;
		internal int starting_row;	// At which row the group starts
		internal int starting_item; 	// The first display item in group
		internal int rows;
		internal int current_item;	// Current item when doing layout
		internal Point items_area_location;
		bool is_default_group;
		int item_count; // Used by default group to store item count

		#region ListViewGroup constructors

		public ListViewGroup () : this ("ListViewGroup", HorizontalAlignment.Left)
		{
		}

		public ListViewGroup (string header) : this (header, HorizontalAlignment.Left)
		{
		}

		public ListViewGroup (string key, string headerText) : this (headerText, HorizontalAlignment.Left)
		{
			name = key;
		}

		public ListViewGroup (string header, HorizontalAlignment headerAlignment)
		{
			this.header = header;
			header_alignment = headerAlignment;
			items = new ListView.ListViewItemCollection (list_view_owner, this);
		}

		private ListViewGroup(SerializationInfo info, StreamingContext context)
		{
			header = info.GetString("Header");
			name = info.GetString("Name");
			header_alignment = (HorizontalAlignment)info.GetInt32("HeaderAlignment");
			tag = info.GetValue("Tag", typeof(object));

			int count = 0;
			try {
				count = info.GetInt32("ItemsCount");
			} catch (SerializationException e) {
				// Mono backwards compat
				try {
 					count = info.GetInt32("ListViewItemCount");
				} catch (SerializationException e2) {}
			}

			if (items == null) {
				items = new ListView.ListViewItemCollection(list_view_owner);
			}

			for (int i = 0; i < count; i++)
			{
				items.Add((ListViewItem)info.GetValue(string.Format("ListViewItem_{0}", i), typeof(ListViewItem)));
			}
		}

		#endregion

		#region ListViewGroup properties

		public string Header {
			get { return header; }
			set {
				if (!header.Equals(value)) {
					header = value;

					if (list_view_owner != null)
						list_view_owner.Redraw(true);
				}
			}
        	}

		[DefaultValue (HorizontalAlignment.Left)]
		public HorizontalAlignment HeaderAlignment {
			get { return header_alignment; }
			set {
				if (!header_alignment.Equals(value)) {
					if (value != HorizontalAlignment.Left && value != HorizontalAlignment.Right &&
						value != HorizontalAlignment.Center)
						throw new InvalidEnumArgumentException("HeaderAlignment", (int)value, typeof(HorizontalAlignment));

					header_alignment = value;

					if (list_view_owner != null)
						list_view_owner.Redraw(true);
				}
			}
		}

		[Browsable(false)]
		public ListView.ListViewItemCollection Items {
			get {
				return items;
			}
		}

		[DesignerSerializationVisibility(0)]
		[Browsable(false)]
		public ListView ListView {
			get { return list_view_owner; }
		}

		internal ListView ListViewOwner {
			get { return list_view_owner; }
			set { 
				list_view_owner = value; 
				if (!is_default_group)
					items.Owner = value;
			}
		}

		internal Rectangle HeaderBounds {
			get {
				Rectangle retval = header_bounds;
				retval.X -= list_view_owner.h_marker;
				retval.Y -= list_view_owner.v_marker;
				return retval;
			}
			set { 
				if (list_view_owner != null)
					list_view_owner.item_control.Invalidate (HeaderBounds);

				header_bounds = value; 

				if (list_view_owner != null)
					list_view_owner.item_control.Invalidate (HeaderBounds);

			}
		}

		internal bool IsDefault {
			get {
				return is_default_group;
			}
			set {
				is_default_group = value;
			}
		}

		internal int ItemCount {
			get {
				return is_default_group ? item_count : items.Count;
			}
			set {
				if (!is_default_group)
					throw new InvalidOperationException ("ItemCount cannot be set for non-default groups.");

				item_count = value;
			}
		}

		internal int GetActualItemCount ()
		{
			if (is_default_group)
				return item_count;

			int count = 0;
			for (int i = 0; i < items.Count; i++)
				if (items [i].ListView != null) // Ignore.
					count++;

			return count;
		}

		[Browsable(true)]
		[DefaultValue("")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		[Localizable(false)]
		[Bindable(true)]
		public Object Tag {
			get { return tag; }
			set { tag = value; }
		}

		#endregion

		public override string ToString()
		{
			return header;
		}

		#region ISerializable Members

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Header", header);
			info.AddValue("Name", name);
			info.AddValue("HeaderAlignment", header_alignment);
			info.AddValue("Tag", tag);

			info.AddValue("ListViewItemCount", items.Count);

			int i = 0;
			foreach (ListViewItem item in items)
			{
				info.AddValue(string.Format("ListViewItem_{0}", i), item);
				i++;
			}
		}

		#endregion
	}

	internal class ListViewGroupConverter : TypeConverter
	{
		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		// Weird
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return new StandardValuesCollection (new object [] {});
		}
	}
}
