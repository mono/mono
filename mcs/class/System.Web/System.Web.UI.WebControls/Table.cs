//
// System.Web.UI.WebControls.Table.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("Rows")]
	[Designer ("System.Web.UI.Design.WebControls.TableDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren (true, "Rows")]
	[SupportsEventValidation]
	public class Table : WebControl, IPostBackEventHandler
	{
		TableRowCollection rows;
		bool generateTableSections;

		public Table ()
			: base (HtmlTextWriterTag.Table)
		{
		}

		internal bool GenerateTableSections {
			get { return generateTableSections; }
			set { generateTableSections = value; }
		}
		
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get {
				if (!ControlStyleCreated)
					return String.Empty; // default value
				return TableStyle.BackImageUrl;
			}
			set { TableStyle.BackImageUrl = value; }
		}

		// note: it seems that Caption and CaptionAlign appeared in 1.1 SP1

		[DefaultValue ("")]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public virtual string Caption {
			get {
				object o = ViewState ["Caption"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Caption");
				else
					ViewState ["Caption"] = value;
			}
		}

		[DefaultValue (TableCaptionAlign.NotSet)]
		[WebCategory ("Accessibility")]
		public virtual TableCaptionAlign CaptionAlign {
			get {
				object o = ViewState ["CaptionAlign"];
				return (o == null) ? TableCaptionAlign.NotSet : (TableCaptionAlign) o;
			}
			set {
				if ((value < TableCaptionAlign.NotSet) || (value > TableCaptionAlign.Right)) {
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid TableCaptionAlign value."));
				}
				ViewState ["CaptionAlign"] = value;
			}
		}

		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int CellPadding {
			get {
				if (!ControlStyleCreated)
					return -1; // default value
				return TableStyle.CellPadding;
			}
			set { TableStyle.CellPadding = value; }
		}

		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int CellSpacing {
			get {
				if (!ControlStyleCreated)
					return -1; // default value
				return TableStyle.CellSpacing;
			}
			set { TableStyle.CellSpacing = value; }
		}

		[DefaultValue (GridLines.None)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual GridLines GridLines {
			get {
				if (!ControlStyleCreated)
					return GridLines.None; // default value
				return TableStyle.GridLines;
			}
			set { TableStyle.GridLines = value; }
		}

		[DefaultValue (HorizontalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (!ControlStyleCreated)
					return HorizontalAlign.NotSet; // default value
				return TableStyle.HorizontalAlign;
			}
			set { TableStyle.HorizontalAlign = value; }
		}

		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("")]
		public virtual TableRowCollection Rows {
			get {
				if (rows == null)
					rows = new TableRowCollection (this);
				return rows;
			}
		}

		private TableStyle TableStyle {
			get { return (ControlStyle as TableStyle); }
		}
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new RowControlCollection (this);
		}

		protected override Style CreateControlStyle ()
		{
			return new TableStyle (ViewState);
		}

		protected internal
		override void RenderContents (HtmlTextWriter writer)
		{
			TableRowSection currentTableSection = TableRowSection.TableHeader;
			TableRowSection rowSection;
			bool sectionStarted = false;
			
			if (Rows.Count > 0) {
				foreach (TableRow row in Rows) {
					if (generateTableSections) {
						rowSection = row.TableSection;
						if (rowSection < currentTableSection)
							throw new HttpException ("The table " + ID + " must contain row sections in order of header, body, then footer.");

						if (currentTableSection != rowSection) {
							if (sectionStarted) {
								writer.RenderEndTag ();
								sectionStarted = false;
							}
							
							currentTableSection = rowSection;
						}
						
						if (!sectionStarted) {
							switch (rowSection) {
								case TableRowSection.TableHeader:
									writer.RenderBeginTag (HtmlTextWriterTag.Thead);
									break;

								case TableRowSection.TableBody:
									writer.RenderBeginTag (HtmlTextWriterTag.Tbody);
									break;

								case TableRowSection.TableFooter:
									writer.RenderBeginTag (HtmlTextWriterTag.Tfoot);
									break;
							}
							sectionStarted = true;
						}
					}
					if (row != null)
						row.RenderControl (writer);
				}

				if (sectionStarted)
					writer.RenderEndTag ();
			}
		}


		// new in Fx 1.1 SP1 (to support Caption and CaptionAlign)

		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			base.RenderBeginTag (writer);

			string s = Caption;
			if (s.Length > 0) {
				TableCaptionAlign tca = CaptionAlign;
				if (tca != TableCaptionAlign.NotSet)
					writer.AddAttribute (HtmlTextWriterAttribute.Align, tca.ToString ());
				
				writer.RenderBeginTag (HtmlTextWriterTag.Caption);
				writer.Write (s);
				writer.RenderEndTag ();
			}
// #if !NET_4_0
// 			else if (HasControls ()) {
// 				writer.Indent++;
// 			}
// #endif
		}

		void IPostBackEventHandler.RaisePostBackEvent (string argument)
		{
			RaisePostBackEvent (argument);
		}

		protected virtual void RaisePostBackEvent (string argument)
		{
			ValidateEvent (UniqueID, argument);
		}

		// inner class
		protected class RowControlCollection : ControlCollection {

			internal RowControlCollection (Table owner)
				: base (owner)
			{
			}


			public override void Add (Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is TableRow))
					throw new ArgumentException ("child", Locale.GetText ("Must be an TableRow instance."));

				base.Add (child);
			}

			public override void AddAt (int index, Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is TableRow))
					throw new ArgumentException ("child", Locale.GetText ("Must be an TableRow instance."));

				base.AddAt (index, child);
			}
		}
	}
}
