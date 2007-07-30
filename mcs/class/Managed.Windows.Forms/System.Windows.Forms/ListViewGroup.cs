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

#if NET_2_0

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
	public sealed class ListViewGroup : ISerializable
	{
		private string header = string.Empty;
		private string name = null;
		private HorizontalAlignment header_alignment = HorizontalAlignment.Left;
		private ListView list_view_owner = null;
		private ListView.ListViewItemCollection items = null;
		private object tag = null;
		private Rectangle bounds = Rectangle.Empty;
		internal int starting_row;	// At which row the group starts

		#region ListViewGroup constructors

		public ListViewGroup()
		{
			header = "ListViewGroup";
			header_alignment = HorizontalAlignment.Left;
		}

		public ListViewGroup(string header)
		{
			this.header = header;
			header_alignment = HorizontalAlignment.Left;
		}

		public ListViewGroup(string key, string headerText)
		{
			header = headerText;
			name = key;
			header_alignment = HorizontalAlignment.Left;
		}

		public ListViewGroup(string header, HorizontalAlignment headerAlignment)
		{
			this.header = header;
			header_alignment = headerAlignment;
		}

		private ListViewGroup(SerializationInfo info, StreamingContext context)
		{
			header = info.GetString("Header");
			name = info.GetString("Name");
			header_alignment = (HorizontalAlignment)info.GetInt32("HeaderAlignment");
			tag = info.GetValue("Tag", typeof(object));

			int count = info.GetInt32("ListViewItemCount");
			if (count > 0) {
				if(items == null)
				items = new ListView.ListViewItemCollection(list_view_owner);

				for (int i = 0; i < count; i++)
				{
					items.Add((ListViewItem)info.GetValue(string.Format("ListViewItem_{0}", i), typeof(ListViewItem)));
				}
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
				if (items == null)
					items = new ListView.ListViewItemCollection(list_view_owner);

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
				items.Owner = value;
			}
		}

		internal Rectangle Bounds {
			get {
				Rectangle retval = bounds;
				retval.X -= list_view_owner.h_marker;
				retval.Y -= list_view_owner.v_marker;
				return retval;
			}
			set { 
				list_view_owner.item_control.Invalidate (Bounds);
				bounds = value; 
				list_view_owner.item_control.Invalidate (Bounds);
			}
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
}
#endif
