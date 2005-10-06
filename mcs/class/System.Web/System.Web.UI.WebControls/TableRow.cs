//
// System.Web.UI.WebControls.TableRow.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

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
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[Bindable (false)]
#endif
	[DefaultProperty("Cells")]
	[ParseChildren(true, "Cells")]
	[ToolboxItem (false)]
	public class TableRow: WebControl
	{
		private TableCellCollection cells;

		public TableRow() : base (HtmlTextWriterTag.Tr)
		{
			PreventAutoID ();
		}

		[MergableProperty (false), PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("All cells that exist in a table row.")]
		public virtual TableCellCollection Cells
		{
			get {
				if (cells == null)
					cells = new TableCellCollection (this);
				return cells;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (HorizontalAlign), "NotSet"), WebCategory ("Layout")]
		[WebSysDescription ("The horizontal alignment for all table cells in that row.")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get {
				if (!ControlStyleCreated)
					return HorizontalAlign.NotSet;
				return ((TableItemStyle)ControlStyle).HorizontalAlign;			
			}

			set { ((TableItemStyle)ControlStyle).HorizontalAlign = value; }
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (VerticalAlign), "NotSet"), WebCategory ("Layout")]
		[WebSysDescription ("The verical alignment for all table cells in that row.")]
		public virtual VerticalAlign VerticalAlign
		{
			get {
				if (!ControlStyleCreated)
					return VerticalAlign.NotSet;
				return ((TableItemStyle)ControlStyle).VerticalAlign;			
			}

			set { ((TableItemStyle)ControlStyle).VerticalAlign = value; }
		}

		protected override Style CreateControlStyle ()
		{
			return new TableItemStyle (ViewState);
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new CellControlCollection (this);
		}

		protected class CellControlCollection : ControlCollection
		{
			internal CellControlCollection (Control owner) : base (owner)
			{
			}

			public override void Add (Control child)
			{
				if (!(child is TableCell))
					throw new ArgumentException (HttpRuntime.FormatResourceString (
								     "Cannot_Have_Children_Of_Type",
								     "TableRow",
								     GetType ().Name.ToString ()));
				base.Add (child);
			}

			public override void AddAt(int index, Control child)
			{
				if (!(child is TableCell))
					throw new ArgumentException (HttpRuntime.FormatResourceString (
								     "Cannot_Have_Children_Of_Type",
								     "TableRow",
								     GetType ().Name.ToString ()));
				base.AddAt (index, child);
			}
		}
	}
}

