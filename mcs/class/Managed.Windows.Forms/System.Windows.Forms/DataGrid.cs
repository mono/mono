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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

// NOT COMPLETE


using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms
{
	public class DataGrid : Control
	{
		[Flags]
		[Serializable]
		public enum HitTestType
		{
			None		= 0,
			Cell		= 1,
			ColumnHeader	= 2,
			RowHeader	= 4,
			ColumnResize	= 8,
			RowResize	= 16,
			Caption		= 32,
			ParentRows	= 64
		}

		public sealed class HitTestInfo
		{
			public static readonly HitTestInfo Nowhere = null;

			#region	Local Variables
			private int column;
			private int row;
			private HitTestType type;
			#endregion // Local Variables

			#region Private Constructors
			internal HitTestInfo ()
			{
				column = 0;
				row = 0;
				type =  HitTestType.None;
			}
			#endregion


			#region Public Instance Properties
			public int Column {
				get { return column; }
			}
			public int Row {
				get { return row; }
			}
			public DataGrid.HitTestType Type {
				get { return type; }
			}
			#endregion //Public Instance Properties

			public override bool Equals (object o)
			{
				if (!(o is HitTestInfo))
					return false;

				HitTestInfo obj = (HitTestInfo) o;
				return (obj.Column == column && obj.Row == row && obj.Type ==type);
			}

			public override int GetHashCode ()
			{
				return row ^ column;
			}

			public override string ToString ()
			{
				return base.ToString ();
			}

		}

		#region	Local Variables
		private static readonly Color	def_alternating_backcolor = SystemColors.Window;
		private static readonly Color	def_background_color = SystemColors.Window;
		private static readonly Color	def_caption_backcolor = SystemColors.ActiveCaption;
		private static readonly Color	def_caption_forecolor = SystemColors.ActiveCaptionText;
		private static readonly Color	def_gridline_color = SystemColors.Control;
		private static readonly Color	def_header_backcolor = SystemColors.Control;
		private static readonly Font	def_header_font = ThemeEngine.Current.DefaultFont;
		private static readonly Color	def_header_forecolor = SystemColors.ControlText;
		private static readonly Color	def_link_hovercolor = SystemColors.HotTrack;
		private static readonly Color	def_parentrowsback_color = SystemColors.Control;
		private static readonly Color	def_parentrowsfore_color = SystemColors.WindowText;
		private static readonly Color	def_selection_backcolor = SystemColors.ActiveCaption;
		private static readonly Color	def_selection_forecolor = SystemColors.ActiveCaptionText;
		private static readonly Color	def_link_color = SystemColors.HotTrack;

		private bool allow_navigation;
		private bool allow_sorting;
		private Color alternating_backcolor;
		private Color background_color;
		private BorderStyle border_style;
		private Color caption_backcolor;
		private Font caption_font;
		private Color caption_forecolor;
		private string caption_text;
		private bool caption_visible;
		private bool columnheaders_visible;
		private object datasource;
		private string datamember;
		private int firstvisible_column;
		private bool flatmode;
		private Color gridline_color;
		private DataGridLineStyle gridline_style;
		private Color header_backcolor;
		private Color header_forecolor;
		private Font header_font;
		private Color link_color;
		private Color link_hovercolor;
		private CurrencyManager currency_manager;
		private Color parentrowsback_color;
		private Color parentrowsfore_color;
		private bool parentrows_visible;
		private int preferredcolumn_width;
		private int preferredrow_height;
		private bool _readonly;
		private bool rowheaders_visible;
		private Color selection_backcolor;
		private Color selection_forecolor;
		private int rowheaders_width;
		private int visiblecolumn_count;
		private int visiblerow_count;
		private int currentrow_index;
		private GridTableStylesCollection styles_collection;
		private DataGridParentRowsLabelStyle parentrowslabel_style;
		private DataGridCell current_cell;
		private Color forecolor;
		private Color backcolor;
		#endregion // Local Variables

		#region Public Constructors
		public DataGrid ()
		{
			styles_collection = new GridTableStylesCollection (this);
			allow_navigation = true;
			allow_sorting = true;
			alternating_backcolor = def_alternating_backcolor;
			background_color = def_background_color;
			border_style = BorderStyle.Fixed3D;
			caption_backcolor = def_caption_backcolor;
			caption_font = ThemeEngine.Current.DefaultFont;
			caption_forecolor = def_caption_forecolor;
			caption_text = string.Empty;
			caption_visible = true;
			columnheaders_visible = true;
			datasource = null;
			datamember = string.Empty;
			firstvisible_column = 0;
			flatmode = false;
			gridline_color = def_gridline_color;
			gridline_style = DataGridLineStyle.Solid;
			header_backcolor = def_header_backcolor;
			header_forecolor = def_header_forecolor;
			header_font = def_header_font;
			link_color = def_link_color;
			link_hovercolor = def_link_hovercolor;
			currency_manager = null;
			parentrowsback_color = def_parentrowsback_color;
			parentrowsfore_color = def_parentrowsfore_color;
			parentrows_visible = true;
			preferredcolumn_width = 75;
			preferredrow_height = 16;
			_readonly = false ;
			rowheaders_visible = true;
			selection_backcolor = def_selection_backcolor;
			selection_forecolor = def_selection_forecolor;
			rowheaders_width = 35;
			visiblecolumn_count = 0;
			visiblerow_count = 0;
			current_cell = new DataGridCell ();
			currentrow_index = -1;
			forecolor = SystemColors.WindowText;
			parentrowslabel_style = DataGridParentRowsLabelStyle.Both;
			backcolor = SystemColors.Window;

		}
		#endregion	// Public Constructor

		#region Public Instance Properties

		public bool AllowNavigation {
			get {
				return allow_navigation;
			}

			set {
				if (allow_navigation != value) {
					allow_navigation = value;
					OnAllowNavigationChanged (EventArgs.Empty);
				}
			}
		}

		public bool AllowSorting {
			get {
				return allow_sorting;
			}

			set {
				if (allow_sorting != value) {
					allow_sorting = value;
				}
			}
		}

		public Color AlternatingBackColor  {
			get {
				return alternating_backcolor;
			}

			set {
				if (alternating_backcolor != value) {
					alternating_backcolor = value;
					Refresh ();
				}
			}
		}

		public Color BackColor {
			get {
				return backcolor;
			}
			set {
				if (backcolor != value) {
					backcolor = value;
					Refresh ();
				}
			}
		}

		public Color BackgroundColor {
			get {
				return background_color;
			}
			set {
				 if (background_color != value) {
					background_color = value;
					OnBackgroundColorChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}

			set {
				base.BackgroundImage = value;
			}
		}

		public BorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				 if (border_style != value) {
					border_style = value;
					OnBorderStyleChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public Color CaptionBackColor {
			get {
				return caption_backcolor;
			}

			set {
				if (caption_backcolor != value) {
					caption_backcolor = value;

					if (caption_visible) {
						//TODO: Invalidate caption rect
					}
				}
			}
		}


		public Font CaptionFont {
			get {
				return caption_font;
			}

			set {
				if (caption_font!= null && !caption_font.Equals (value)) {
					caption_font = value;

					if (caption_visible) {
						//TODO: Invalidate caption rect
					}
				}
			}
		}

		public Color CaptionForeColor {
			get {
				return caption_forecolor;
			}

			set {
				if (caption_forecolor != value) {
					caption_forecolor = value;

					if (caption_visible) {
						//TODO: Invalidate caption rect
					}
				}
			}
		}

		public string CaptionText {
			get {
				return caption_text;
			}

			set {
				if (caption_text != value) {
					caption_text = value;

					if (caption_visible) {
						//TODO: Invalidate caption rect
					}
				}
			}
		}

		public bool CaptionVisible {
			get {
				return caption_visible;
			}

			set {
				if (caption_visible != value) {
					caption_visible = value;
					OnCaptionVisibleChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public bool ColumnHeadersVisible {
			get {
				return columnheaders_visible;
			}

			set {
				if (columnheaders_visible != value) {
					columnheaders_visible = value;
					Refresh ();
				}
			}
		}

		public DataGridCell CurrentCell {
			get {
				return current_cell;
			}

			set {
				if (!current_cell.Equals (value)) {
					current_cell = value;
					Refresh ();
				}
			}
		}

		public int CurrentRowIndex {
			get {
				return currentrow_index;
			}

			set {
				if (currentrow_index != value) {
					currentrow_index = value;
					Refresh ();
				}
			}
		}

		public override Cursor Cursor {
			get {
				return base.Cursor;
			}
			set {
				base.Cursor = value;
			}
		}

		public string DataMember {
			get { return datamember; }
			set {
				if (SetDataMember (datamember)) {
					SetNewDataSource ();
					Refresh ();
				}
			}
		}

		public object DataSource {
			get {
				return datasource;
			}

			set {
				if (SetDataSource (value)) {
					SetNewDataSource ();
					Refresh ();
				}
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size (130, 80);
			}
		}

		public int FirstVisibleColumn {
			get {
				return firstvisible_column;
			}
		}

		public bool FlatMode {
			get {
				return flatmode;
			}

			set {
				if (flatmode != value) {
					flatmode = value;
					OnFlatModeChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public Color ForeColor {
			get {
				return forecolor;
			}

			set {
				if (forecolor != value) {
					forecolor = value;
					OnForeColorChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public Color GridLineColor {
			get {
				return gridline_color;
			}

			set {
				if (gridline_color != value) {
					gridline_color = value;
					Refresh ();
				}
			}
		}

		public DataGridLineStyle GridLineStyle {
			get {
				return gridline_style;
			}

			set {
				if (gridline_style != value) {
					gridline_style = value;
					Refresh ();
				}
			}
		}

		public Color HeaderBackColor {
			get {
				return header_backcolor;
			}

			set {
				if (header_backcolor != value) {
					header_backcolor = value;
					Refresh ();
				}
			}
		}

		public Font HeaderFont {
			get {
				return header_font;
			}

			set {
				if (header_font != null && !header_font.Equals (value)) {
					header_font = value;
					Refresh ();
				}
			}
		}

		public Color HeaderForeColor {
			get {
				return header_forecolor;
			}

			set {
				if (header_forecolor != value) {
					header_forecolor = value;
					Refresh ();
				}
			}
		}

		protected ScrollBar HorizScrollBar {
			get {
				throw new NotImplementedException ();
			}
		}

		public object this [DataGridCell cell] {
			get  {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public object this [int rowIndex, int columnIndex] {
			get  {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public Color LinkColor {
			get {
				return link_color;
			}
			set {
				if (link_color != value) {
					link_color = value;
					Refresh ();
				}
			}
		}

		[ComVisible(false)]
		public Color LinkHoverColor {
			get {
				return link_hovercolor;
			}

			set {
				if (link_hovercolor != value) {
					link_hovercolor = value;
					Refresh ();
				}
			}
		}

		protected internal CurrencyManager ListManager {
			get {
				return currency_manager;
			}

			set {
				if (currency_manager != value) {
					currency_manager = value;
				}
			}
		}

		public Color ParentRowsBackColor {
			get {
				return parentrowsback_color;
			}

			set {
				if (parentrowsback_color != value) {
					parentrowsback_color = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		public Color ParentRowsForeColor {
			get {
				return parentrowsfore_color;
			}

			set {
				if (parentrowsfore_color != value) {
					parentrowsfore_color = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		public DataGridParentRowsLabelStyle ParentRowsLabelStyle {
			get {
				return parentrowslabel_style;
			}

			set {
				if (parentrowslabel_style != value) {
					parentrowslabel_style = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		public bool ParentRowsVisible {
			get {
				return parentrows_visible;
			}

			set {
				if (parentrows_visible != value) {
					parentrows_visible = value;
					OnParentRowsVisibleChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public int PreferredColumnWidth {
			get {
				return preferredcolumn_width;
			}

			set {
				if (preferredcolumn_width != value) {
					preferredcolumn_width = value;
					Refresh ();
				}
			}
		}

		public int PreferredRowHeight {
			get {
				return preferredrow_height;
			}

			set {
				if (preferredrow_height != value) {
					preferredrow_height = value;
					Refresh ();
				}
			}
		}

		public bool ReadOnly {
			get {
				return _readonly;
			}

			set {
				if (_readonly != value) {
					_readonly = value;
					OnReadOnlyChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public bool RowHeadersVisible {
			get {
				return rowheaders_visible;
			}

			set {
				if (rowheaders_visible != value) {
					rowheaders_visible = value;
					Refresh ();
				}
			}
		}

		public int RowHeaderWidth {
			get {
				return rowheaders_width;
			}

			set {
				if (rowheaders_width != value) {
					rowheaders_width = value;
					Refresh ();
				}
			}
		}

		public Color SelectionBackColor {
			get {
				return selection_backcolor;
			}

			set {
				if (selection_backcolor != value) {
					selection_backcolor = value;
					Refresh ();
				}
			}
		}

		public Color SelectionForeColor  {
			get {
				return selection_forecolor;
			}

			set {
				if (selection_forecolor != value) {
					selection_forecolor = value;
					Refresh ();
				}
			}
		}

		public override ISite Site {
			get {
				return base.Site;
			}
			set {
				base.Site = value;
			}
		}

		public GridTableStylesCollection TableStyles {
			get { return styles_collection; }
		}

		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		protected ScrollBar VertScrollBar {
			get {
				throw new NotImplementedException ();
			}
		}

		public int VisibleColumnCount {
			get {
				return visiblecolumn_count;
			}

		}

		public int VisibleRowCount {
			get {
				return visiblerow_count;
			}
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods

		public virtual bool BeginEdit (DataGridColumnStyle gridColumn, int rowNumber)
		{
			throw new NotImplementedException ();
		}

		public virtual void BeginInit ()
		{

		}


		protected virtual void CancelEditing ()
		{

		}

		public void Collapse (int row)
		{

		}

		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{

		}

		protected internal virtual void ColumnStartedEditing (Rectangle bounds)
		{

		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}

		protected virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop)
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public virtual bool EndEdit (DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
		{
			throw new NotImplementedException ();
		}

		public virtual void EndInit ()
		{

		}

		public void Expand (int row)
		{

		}

		public Rectangle GetCellBounds (DataGridCell dgc)
		{
			throw new NotImplementedException ();
		}

		public Rectangle GetCellBounds (int row, int col)
		{
			throw new NotImplementedException ();
		}

		public Rectangle GetCurrentCellBounds ()
		{
			throw new NotImplementedException ();
		}

		protected virtual string GetOutputTextDelimiter ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void GridHScrolled (object sender, ScrollEventArgs se)
		{

		}

		protected virtual void GridVScrolled (object sender, ScrollEventArgs se)
		{

		}

		public HitTestInfo HitTest (Point position)
		{
			throw new NotImplementedException ();
		}

		public HitTestInfo HitTest (int x, int y)
		{
			throw new NotImplementedException ();
		}

		public bool IsExpanded (int rowNumber)
		{
			throw new NotImplementedException ();
		}

		public bool IsSelected (int row)
		{
			throw new NotImplementedException ();
		}

		public void NavigateBack ()
		{

		}

		public void NavigateTo (int rowNumber, string relationName)
		{

		}

		protected virtual void OnAllowNavigationChanged (EventArgs e)
		{
			if (AllowNavigationChanged != null) {
				AllowNavigationChanged (this, e);
			}
		}

		protected void OnBackButtonClicked (object sender,  EventArgs e)
		{
			if (BackButtonClick != null) {
				BackButtonClick (sender, e);
			}
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected virtual void OnBackgroundColorChanged (EventArgs e)
		{
			if (BackgroundColorChanged != null) {
				BackgroundColorChanged (this, e);
			}
		}

		protected override void OnBindingContextChanged( EventArgs e)
		{
			base.OnBindingContextChanged (e);
		}

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			if (BorderStyleChanged != null) {
				BorderStyleChanged (this, e);
			}
		}

		protected virtual void OnCaptionVisibleChanged (EventArgs e)
		{
			if (CaptionVisibleChanged != null) {
				CaptionVisibleChanged (this, e);
			}
		}

		protected virtual void OnCurrentCellChanged (EventArgs e)
		{
			if (CurrentCellChanged != null) {
				CurrentCellChanged (this, e);
			}
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			if (DataSourceChanged != null) {
				DataSourceChanged (this, e);
			}
		}

		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
		}

		protected virtual void OnFlatModeChanged (EventArgs e)
		{
			if (FlatModeChanged != null) {
				FlatModeChanged (this, e);
			}
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnKeyDown (KeyEventArgs ke)
		{
			base.OnKeyDown (ke);
		}

		protected override void OnKeyPress (KeyPressEventArgs kpe)
		{
			base.OnKeyPress (kpe);
		}

		protected override void OnLayout (LayoutEventArgs levent)
		{
			base.OnLayout (levent);
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);
		}

		protected void OnNavigate (NavigateEventArgs e)
		{
			if (Navigate != null) {
				Navigate (this, e);
			}
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			base.OnPaint (pe);
		}

		protected override void OnPaintBackground (PaintEventArgs ebe)
		{
			base.OnPaintBackground (ebe);
		}

		protected virtual void OnParentRowsVisibleChanged (EventArgs e)
		{
			if (ParentRowsVisibleChanged != null) {
				ParentRowsVisibleChanged (this, e);
			}
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			if (ReadOnlyChanged != null) {
				ReadOnlyChanged (this, e);
			}
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected void OnRowHeaderClick (EventArgs e)
		{
			if (RowHeaderClick != null) {
				RowHeaderClick (this, e);
			}
		}

		protected void OnScroll (EventArgs e)
		{
			if (Scroll != null) {
				Scroll (this, e);
			}
		}

		protected void OnShowParentDetailsButtonClicked (object sender, EventArgs e)
		{
			if (ShowParentDetailsButtonClick != null) {
				ShowParentDetailsButtonClick (sender, e);
			}
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		public void ResetAlternatingBackColor ()
		{
			alternating_backcolor = def_alternating_backcolor;
		}

		public override void ResetBackColor ()
		{
			background_color = def_background_color;
		}

		public override void ResetForeColor ()
		{
			base.ResetForeColor ();
		}

		public void ResetGridLineColor ()
		{
			gridline_color = def_gridline_color;
		}

		public void ResetHeaderBackColor ()
		{
			header_backcolor = def_header_backcolor;
		}

		public void ResetHeaderFont ()
		{
			header_font = def_header_font;
		}

		public void ResetHeaderForeColor ()
		{
			header_forecolor = def_header_forecolor;
		}

		public void ResetLinkColor ()
		{
			link_color = def_link_color;
		}

		public void ResetLinkHoverColor ()
		{
			link_hovercolor = def_link_hovercolor;
		}

		protected void ResetSelection ()
		{

		}

		public void ResetSelectionBackColor ()
		{
			selection_backcolor = def_selection_backcolor;
		}

		public void ResetSelectionForeColor ()
		{
			selection_forecolor = def_selection_forecolor;
		}

		public void Select (int row)
		{

		}

		public void SetDataBinding (object dataSource, string dataMember)
		{
			if (SetDataSource (dataSource) == false  && SetDataMember (dataMember) == false) {
				return;
			}
			
			SetNewDataSource ();
		}

		protected virtual bool ShouldSerializeAlternatingBackColor ()
		{
			return (alternating_backcolor != def_alternating_backcolor);
		}

		protected virtual bool ShouldSerializeBackgroundColor ()
		{
			return (background_color != def_background_color);
		}

		protected virtual bool ShouldSerializeCaptionBackColor ()
		{
			return (caption_backcolor != def_caption_backcolor);
		}

		protected virtual bool ShouldSerializeCaptionForeColor ()
		{
			return (caption_forecolor != def_caption_forecolor);
		}

		protected virtual bool ShouldSerializeGridLineColor ()
		{
			return (gridline_color != def_gridline_color);
		}

		protected virtual bool ShouldSerializeHeaderBackColor ()
		{
			return (header_backcolor != def_header_backcolor);
		}

		protected bool ShouldSerializeHeaderFont ()
		{
			return (header_font != def_header_font);
		}

		protected virtual bool ShouldSerializeHeaderForeColor ()
		{
			return (header_forecolor != def_header_forecolor);
		}

		protected virtual bool ShouldSerializeLinkHoverColor ()
		{
			return (link_hovercolor != def_link_hovercolor);
		}

		protected virtual bool ShouldSerializeParentRowsBackColor ()
		{
			return (parentrowsback_color != def_parentrowsback_color);
		}

		protected virtual bool ShouldSerializeParentRowsForeColor ()
		{
			return (parentrowsback_color != def_parentrowsback_color);
		}

		protected bool ShouldSerializePreferredRowHeight ()
		{
			return (parentrowsfore_color != def_parentrowsfore_color);
		}

		protected bool ShouldSerializeSelectionBackColor ()
		{
			return (selection_backcolor != def_selection_backcolor);
		}

		protected virtual bool ShouldSerializeSelectionForeColor ()
		{
			return (selection_forecolor != def_selection_forecolor);
		}

		public void SubObjectsSiteChange (bool site)
		{

		}

		public void UnSelect (int row)
		{

		}
		#endregion	// Public Instance Methods

		#region Private Instance Methods
		

		private bool SetDataMember (string member)
		{
			if (member == datamember) {
				return false;
			}

			datamember = member;
			return true;
		}

		private bool SetDataSource (object source)
		{
			if (datasource != null && datasource.Equals (source)) {
				return false;
			}

			if (source != null && source as IListSource != null && source as IList != null) {
				throw new Exception ("Wrong complex data binding source");
			}

			datasource = source;
			OnDataSourceChanged (EventArgs.Empty);
			return true;
		}
		
		private void SetNewDataSource ()
		{
			// Create Table Style 
			// Create columns Styles
			// Bind data
			
		}


		#endregion Private Instance Methods


		#region Events
		public event EventHandler AllowNavigationChanged;
		public event EventHandler BackButtonClick;
		public event EventHandler BackgroundColorChanged;
		public new event EventHandler BackgroundImageChanged;
		public event EventHandler BorderStyleChanged;
		public event EventHandler CaptionVisibleChanged;
		public event EventHandler CurrentCellChanged;
		public new event EventHandler CursorChanged;
		public event EventHandler DataSourceChanged;
		public event EventHandler FlatModeChanged;
		public event NavigateEventHandler Navigate;
		public event EventHandler ParentRowsLabelStyleChanged;
		public event EventHandler ParentRowsVisibleChanged;
		public event EventHandler ReadOnlyChanged;
		protected event EventHandler RowHeaderClick;
		public event EventHandler Scroll;
		public event EventHandler ShowParentDetailsButtonClick;
		public new event EventHandler TextChanged;
		#endregion	// Events
	}
}
