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
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Peter Dennis Bartok, pbartok@novell.com
//

using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace System.Windows.Forms
{
	internal enum UIIcon {
		PlacesRecentDocuments,
		PlacesDesktop,
		PlacesPersonal,
		PlacesMyComputer,
		PlacesMyNetwork,
		MessageBoxError,
		MessageBoxQuestion,
		MessageBoxWarning,
		MessageBoxInfo,
		
		NormalFolder
	}
	
	internal struct CPColor {
		internal Color Dark;
		internal Color DarkDark;
		internal Color Light;
		internal Color LightLight;
		
		internal static CPColor Empty;
	}
	
	// Implements a pool of system resources
	internal class SystemResPool
	{
		private Hashtable pens = new Hashtable ();
		private Hashtable dashpens = new Hashtable ();
		private Hashtable sizedpens = new Hashtable ();
		private Hashtable solidbrushes = new Hashtable ();
		private Hashtable hatchbrushes = new Hashtable ();
		private Hashtable uiImages = new Hashtable();
		private Hashtable cpcolors = new Hashtable ();
		
		public SystemResPool () {}
		
		public Pen GetPen (Color color)
		{
			int hash = color.ToArgb ();

			lock (pens) {
				Pen res = pens [hash] as Pen;
				if (res != null)
					return res;
			
				Pen pen = new Pen (color);
				pens.Add (hash, pen);
				return pen;
			}
		}
		
		public Pen GetDashPen (Color color, DashStyle dashStyle)
		{
			string hash = color.ToString() + dashStyle;

			lock (dashpens) {
				Pen res = dashpens [hash] as Pen;
				if (res != null)
					return res;
			
				Pen pen = new Pen (color);
				pen.DashStyle = dashStyle;
				dashpens [hash] = pen;
				return pen;
			}
		}
		
		public Pen GetSizedPen (Color color, int size)
		{
			string hash = color.ToString () + size;
			
			lock (sizedpens) {
				Pen res = sizedpens [hash] as Pen;
				if (res != null)
					return res;
			
				Pen pen = new Pen (color, size);
				sizedpens [hash] = pen;
				return pen;
			}
		}
		
		public SolidBrush GetSolidBrush (Color color)
		{
			int hash = color.ToArgb ();

			lock (solidbrushes) {
				SolidBrush res = solidbrushes [hash] as SolidBrush;
				if (res != null)
					return res;
			
				SolidBrush brush = new SolidBrush (color);
				solidbrushes.Add (hash, brush);
				return brush;
			}
		}		
		
		public HatchBrush GetHatchBrush (HatchStyle hatchStyle, Color foreColor, Color backColor)
		{
			string hash = ((int)hatchStyle).ToString () + foreColor.ToString () + backColor.ToString ();

			lock (hatchbrushes) {
				HatchBrush brush = (HatchBrush) hatchbrushes[hash];
				if (brush == null) {
					brush = new HatchBrush (hatchStyle, foreColor, backColor);
					hatchbrushes.Add (hash, brush);
				}
				return brush;
			}
		}
		
		public void AddUIImage (Image image, string name, int size)
		{
			string hash = name + size.ToString();

			lock (uiImages) {
				if (uiImages.Contains (hash))
					return;
				uiImages.Add (hash, image);
			}
		}
		
		public Image GetUIImage(string name, int size)
		{
			string hash = name + size.ToString();
			
			Image image = uiImages [hash] as Image;
			
			return image;
		}
		
		public CPColor GetCPColor (Color color)
		{
			lock (cpcolors) {
				object tmp = cpcolors [color];
			
				if (tmp == null) {
					CPColor cpcolor = new CPColor ();
					cpcolor.Dark = ControlPaint.Dark (color);
					cpcolor.DarkDark = ControlPaint.DarkDark (color);
					cpcolor.Light = ControlPaint.Light (color);
					cpcolor.LightLight = ControlPaint.LightLight (color);
				
					cpcolors.Add (color, cpcolor);

					return cpcolor;
				}
			
				return (CPColor)tmp;
			}
		}
	}

	internal abstract class Theme
	{
		protected Array syscolors;
		Font default_font;
		protected Color defaultWindowBackColor;
		protected Color defaultWindowForeColor;
		internal SystemResPool ResPool = new SystemResPool ();
		private MethodInfo update;

		protected Theme ()
		{
		}

		private void SetSystemColors (KnownColor kc, Color value)
		{
			if (update == null) {
				Type known_colors = Type.GetType ("System.Drawing.KnownColors, " + Consts.AssemblySystem_Drawing);
				if (known_colors != null)
					update = known_colors.GetMethod ("Update", BindingFlags.Static | BindingFlags.Public);
			}
			if (update != null)
				update.Invoke (null, new object [2] { (int)kc, value.ToArgb () });
		}

		/* OS Feature support */
		public abstract Version Version {
			get;
		}

		/* Default properties */
		public virtual Color ColorScrollBar {
			get { return SystemColors.ScrollBar; }
			set { SetSystemColors (KnownColor.ScrollBar, value); }
		}

		public virtual Color ColorDesktop {
			get { return SystemColors.Desktop;}
			set { SetSystemColors (KnownColor.Desktop, value); }
		}

		public virtual Color ColorActiveCaption {
			get { return SystemColors.ActiveCaption;}
			set { SetSystemColors (KnownColor.ActiveCaption, value); }
		}

		public virtual Color ColorInactiveCaption {
			get { return SystemColors.InactiveCaption;}
			set { SetSystemColors (KnownColor.InactiveCaption, value); }
		}

		public virtual Color ColorMenu {
			get { return SystemColors.Menu;}
			set { SetSystemColors (KnownColor.Menu, value); }
		}

		public virtual Color ColorWindow {
			get { return SystemColors.Window;}
			set { SetSystemColors (KnownColor.Window, value); }
		}

		public virtual Color ColorWindowFrame {
			get { return SystemColors.WindowFrame;}
			set { SetSystemColors (KnownColor.WindowFrame, value); }
		}

		public virtual Color ColorMenuText {
			get { return SystemColors.MenuText;}
			set { SetSystemColors (KnownColor.MenuText, value); }
		}

		public virtual Color ColorWindowText {
			get { return SystemColors.WindowText;}
			set { SetSystemColors (KnownColor.WindowText, value); }
		}

		public virtual Color ColorActiveCaptionText {
			get { return SystemColors.ActiveCaptionText;}
			set { SetSystemColors (KnownColor.ActiveCaptionText, value); }
		}

		public virtual Color ColorActiveBorder {
			get { return SystemColors.ActiveBorder;}
			set { SetSystemColors (KnownColor.ActiveBorder, value); }
		}

		public virtual Color ColorInactiveBorder{
			get { return SystemColors.InactiveBorder;}
			set { SetSystemColors (KnownColor.InactiveBorder, value); }
		}

		public virtual Color ColorAppWorkspace {
			get { return SystemColors.AppWorkspace;}
			set { SetSystemColors (KnownColor.AppWorkspace, value); }
		}

		public virtual Color ColorHighlight {
			get { return SystemColors.Highlight;}
			set { SetSystemColors (KnownColor.Highlight, value); }
		}

		public virtual Color ColorHighlightText {
			get { return SystemColors.HighlightText;}
			set { SetSystemColors (KnownColor.HighlightText, value); }
		}

		public virtual Color ColorControl {
			get { return SystemColors.Control;}
			set { SetSystemColors (KnownColor.Control, value); }
		}

		public virtual Color ColorControlDark {
			get { return SystemColors.ControlDark;}
			set { SetSystemColors (KnownColor.ControlDark, value); }
		}

		public virtual Color ColorGrayText {
			get { return SystemColors.GrayText;}
			set { SetSystemColors (KnownColor.GrayText, value); }
		}

		public virtual Color ColorControlText {
			get { return SystemColors.ControlText;}
			set { SetSystemColors (KnownColor.ControlText, value); }
		}

		public virtual Color ColorInactiveCaptionText {
			get { return SystemColors.InactiveCaptionText;}
			set { SetSystemColors (KnownColor.InactiveCaptionText, value); }
		}

		public virtual Color ColorControlLight {
			get { return SystemColors.ControlLight;}
			set { SetSystemColors (KnownColor.ControlLight, value); }
		}

		public virtual Color ColorControlDarkDark {
			get { return SystemColors.ControlDarkDark;}
			set { SetSystemColors (KnownColor.ControlDarkDark, value); }
		}

		public virtual Color ColorControlLightLight {
			get { return SystemColors.ControlLightLight;}
			set { SetSystemColors (KnownColor.ControlLightLight, value); }
		}

		public virtual Color ColorInfoText {
			get { return SystemColors.InfoText;}
			set { SetSystemColors (KnownColor.InfoText, value); }
		}

		public virtual Color ColorInfo {
			get { return SystemColors.Info;}
			set { SetSystemColors (KnownColor.Info, value); }
		}

		public virtual Color ColorHotTrack {
			get { return SystemColors.HotTrack;}
			set { SetSystemColors (KnownColor.HotTrack, value);}
		}

		public virtual Color DefaultControlBackColor {
			get { return ColorControl; }
			set { ColorControl = value; }
		}

		public virtual Color DefaultControlForeColor {
			get { return ColorControlText; }
			set { ColorControlText = value; }
		}

		public virtual Font DefaultFont {
			get { return default_font ?? (default_font = SystemFonts.DefaultFont); }
		}

		public virtual Color DefaultWindowBackColor {
			get { return defaultWindowBackColor; }
		}

		public virtual Color DefaultWindowForeColor {
			get { return defaultWindowForeColor; }
		}

		public virtual Color GetColor (XplatUIWin32.GetSysColorIndex idx)
		{
			return (Color) syscolors.GetValue ((int)idx);
		}

		public virtual void SetColor (XplatUIWin32.GetSysColorIndex idx, Color color)
		{
			syscolors.SetValue (color, (int) idx);
		}

		// Theme/UI specific defaults
		public virtual ArrangeDirection ArrangeDirection  {
			get {
				return ArrangeDirection.Down;
			}
		}

		public virtual ArrangeStartingPosition ArrangeStartingPosition {
			get {
				return ArrangeStartingPosition.BottomLeft;
			}
		}

		public virtual int BorderMultiplierFactor { get { return 1; } }
		
		public virtual Size BorderSizableSize {
			get {
				return new Size (3, 3);
			}
		}

		public virtual Size Border3DSize {
			get {
				return XplatUI.Border3DSize;
			}
		}

		public virtual Size BorderStaticSize {
			get {
				return new Size(1, 1);
			}
		}

		public virtual Size BorderSize {
			get {
				return XplatUI.BorderSize;
			}
		}

		public virtual Size CaptionButtonSize {
			get {
				return XplatUI.CaptionButtonSize;
			}
		}

		public virtual int CaptionHeight {
			get {
				return XplatUI.CaptionHeight;
			}
		}

		public virtual Size DoubleClickSize {
			get {
				return new Size(4, 4);
			}
		}

		public virtual int DoubleClickTime {
			get {
				return XplatUI.DoubleClickTime;
			}
		}

		public virtual Size FixedFrameBorderSize {
			get {
				return XplatUI.FixedFrameBorderSize;
			}
		}

		public virtual Size FrameBorderSize {
			get {
				return XplatUI.FrameBorderSize;
			}
		}

		public virtual int HorizontalFocusThickness { get { return 1; } }
		
		public virtual int HorizontalScrollBarArrowWidth {
			get {
				return 16;
			}
		}

		public virtual int HorizontalScrollBarHeight {
			get {
				return 16;
			}
		}

		public virtual int HorizontalScrollBarThumbWidth {
			get {
				return 16;
			}
		}

		public virtual Size IconSpacingSize {
			get {
				return new Size(75, 75);
			}
		}

		public virtual bool MenuAccessKeysUnderlined {
			get {
				return XplatUI.MenuAccessKeysUnderlined;
			}
		}
		
		public virtual Size MenuBarButtonSize {
			get { return XplatUI.MenuBarButtonSize; }
		}
		
		public virtual Size MenuButtonSize {
			get {
				return XplatUI.MenuButtonSize;
			}
		}

		public virtual Size MenuCheckSize {
			get {
				return new Size(13, 13);
			}
		}

		public virtual Font MenuFont {
			get {
				return default_font ?? (default_font = SystemFonts.DefaultFont);
			}
		}

		public virtual int MenuHeight {
			get {
				return XplatUI.MenuHeight;
			}
		}

		public virtual int MouseWheelScrollLines {
			get {
				return 3;
			}
		}

		public virtual bool RightAlignedMenus {
			get {
				return false;
			}
		}

		public virtual Size ToolWindowCaptionButtonSize {
			get {
				return XplatUI.ToolWindowCaptionButtonSize;
			}
		}

		public virtual int ToolWindowCaptionHeight {
			get {
				return XplatUI.ToolWindowCaptionHeight;
			}
		}

		public virtual int VerticalFocusThickness { get { return 1; } }

		public virtual int VerticalScrollBarArrowHeight {
			get {
				return 16;
			}
		}

		public virtual int VerticalScrollBarThumbHeight {
			get {
				return 16;
			}
		}

		public virtual int VerticalScrollBarWidth {
			get {
				return 16;
			}
		}

		public abstract Font WindowBorderFont {
			get;
		}

		public int Clamp (int value, int lower, int upper)
		{
			if (value < lower) return lower;
			else if (value > upper) return upper;
			else return value;
		}

		[MonoInternalNote ("Figure out where to point for My Network Places")]
		public virtual string Places(UIIcon index) {
			switch (index) {
				case UIIcon.PlacesRecentDocuments: {
					// Default = "Recent Documents"
					return Environment.GetFolderPath(Environment.SpecialFolder.Recent);
				}

				case UIIcon.PlacesDesktop: {
					// Default = "Desktop"
					return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				}

				case UIIcon.PlacesPersonal: {
					// Default = "My Documents"
					return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				}

				case UIIcon.PlacesMyComputer: {
					// Default = "My Computer"
					return Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
				}

				case UIIcon.PlacesMyNetwork: {
					// Default = "My Network Places"
					return "/tmp";
				}

				default: {
					throw new ArgumentOutOfRangeException("index", index, "Unsupported place");
				}
			}
		}

		//
		// This routine fetches images embedded as assembly resources (not
		// resgen resources).  It optionally scales the image to fit the
		// specified size x dimension (it adjusts y automatically to fit that).
		//
		private Image GetSizedResourceImage(string name, int width)
		{
			Image image = ResPool.GetUIImage (name, width);
			if (image != null)
				return image;
			
			string	fullname;

			if (width > 0) {
				// Try name_width
				fullname = String.Format("{1}_{0}", name, width);
				image = ResourceImageLoader.Get (fullname);
				if (image != null){
					ResPool.AddUIImage (image, name, width);
					return image;
				}
			}

			// Just try name
			image = ResourceImageLoader.Get (name);
			if (image == null)
				return null;
			
			ResPool.AddUIImage (image, name, 0);
			if (image.Width != width && width != 0){
				Console.Error.WriteLine ("warning: requesting icon that not been tuned {0}_{1} {2}", width, name, image.Width);
				int height = (image.Height * width)/image.Width;
				Bitmap b = new Bitmap (width, height);
				using (Graphics g = Graphics.FromImage (b))
					g.DrawImage (image, 0, 0, width, height);
				ResPool.AddUIImage (b, name, width);

				return b;
			}
			return image;
		}
		
		public virtual Image Images(UIIcon index) {
			return Images(index, 0);
		}
			
		public virtual Image Images(UIIcon index, int size) {
			switch (index) {
				case UIIcon.PlacesRecentDocuments:
					return GetSizedResourceImage ("document-open.png", size);
				case UIIcon.PlacesDesktop:
					return GetSizedResourceImage ("user-desktop.png", size);
				case UIIcon.PlacesPersonal:
					return GetSizedResourceImage ("user-home.png", size);
				case UIIcon.PlacesMyComputer:
					return GetSizedResourceImage ("computer.png", size);
				case UIIcon.PlacesMyNetwork:
					return GetSizedResourceImage ("folder-remote.png", size);

				// Icons for message boxes
				case UIIcon.MessageBoxError:
					return GetSizedResourceImage ("dialog-error.png", size);
				case UIIcon.MessageBoxInfo:
					return GetSizedResourceImage ("dialog-information.png", size);
				case UIIcon.MessageBoxQuestion:
					return GetSizedResourceImage ("dialog-question.png", size);
				case UIIcon.MessageBoxWarning:
					return GetSizedResourceImage ("dialog-warning.png", size);
				
				// misc Icons
				case UIIcon.NormalFolder:
					return GetSizedResourceImage ("folder.png", size);

				default: {
					throw new ArgumentException("Invalid Icon type requested", "index");
				}
			}
		}

		public virtual Image Images(string mimetype, string extension, int size) {
			return null;
		}

		#region Principal Theme Methods
		// To let the theme now that a change of defaults (colors, etc) was detected and force a re-read (and possible recreation of cached resources)
		public abstract void ResetDefaults();

		// If the theme writes directly to a window instead of a device context
		public abstract bool DoubleBufferingSupported {get;}
		#endregion	// Principal Theme Methods

		#region	OwnerDraw Support
		public abstract void DrawOwnerDrawBackground (DrawItemEventArgs e);
		public abstract void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e);
		#endregion	// OwnerDraw Support

		#region Button
		public abstract Size CalculateButtonAutoSize (Button button);
		public abstract void CalculateButtonTextAndImageLayout (ButtonBase b, out Rectangle textRectangle, out Rectangle imageRectangle);
		public abstract void DrawButton (Graphics g, Button b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle);
		public abstract void DrawFlatButton (Graphics g, ButtonBase b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle);
		public abstract void DrawPopupButton (Graphics g, Button b, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle);
		#endregion	// Button

		#region ButtonBase
		// Drawing
		public abstract void DrawButtonBase(Graphics dc, Rectangle clip_area, ButtonBase button);

		// Sizing
		public abstract Size ButtonBaseDefaultSize{get;}
		#endregion	// ButtonBase

		#region CheckBox
		public abstract Size CalculateCheckBoxAutoSize (CheckBox checkBox);
		public abstract void CalculateCheckBoxTextAndImageLayout (ButtonBase b, Point offset, out Rectangle glyphArea, out Rectangle textRectangle, out Rectangle imageRectangle);
		public abstract void DrawCheckBox (Graphics g, CheckBox cb, Rectangle glyphArea, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle);
		public abstract void DrawCheckBox (Graphics dc, Rectangle clip_area, CheckBox checkbox);

		#endregion	// CheckBox
		
		#region CheckedListBox
		// Drawing
		public abstract void DrawCheckedListBoxItem (CheckedListBox ctrl, DrawItemEventArgs e);
		#endregion // CheckedListBox
		
		#region ComboBox
		// Drawing
		public abstract void DrawComboBoxItem (ComboBox ctrl, DrawItemEventArgs e);
		public abstract void DrawFlatStyleComboButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void ComboBoxDrawNormalDropDownButton (ComboBox comboBox, Graphics g, Rectangle clippingArea, Rectangle area, ButtonState state);
		public abstract bool ComboBoxNormalDropDownButtonHasTransparentBackground (ComboBox comboBox, ButtonState state);
		public abstract bool ComboBoxDropDownButtonHasHotElementStyle (ComboBox comboBox);
		public abstract void ComboBoxDrawBackground (ComboBox comboBox, Graphics g, Rectangle clippingArea, FlatStyle style);
		public abstract bool CombBoxBackgroundHasHotElementStyle (ComboBox comboBox);
		#endregion	// ComboBox

		#region Control
		public abstract Font GetLinkFont (Control control);
		#endregion	// Control
		
		#region Datagrid
		public abstract int DataGridPreferredColumnWidth { get; }
		public abstract int DataGridMinimumColumnCheckBoxHeight { get; }
		public abstract int DataGridMinimumColumnCheckBoxWidth { get; }
		
		// Default colours
		public abstract Color DataGridAlternatingBackColor { get; }
		public abstract Color DataGridBackColor { get; }
		public abstract Color DataGridBackgroundColor { get; }
		public abstract Color DataGridCaptionBackColor { get; }
		public abstract Color DataGridCaptionForeColor { get; }
		public abstract Color DataGridGridLineColor { get; }
		public abstract Color DataGridHeaderBackColor { get; }
		public abstract Color DataGridHeaderForeColor { get; }
		public abstract Color DataGridLinkColor { get; }
		public abstract Color DataGridLinkHoverColor { get; }
		public abstract Color DataGridParentRowsBackColor { get; }
		public abstract Color DataGridParentRowsForeColor { get; }
		public abstract Color DataGridSelectionBackColor { get; }
		public abstract Color DataGridSelectionForeColor { get; }
		// Paint		
		public abstract void DataGridPaint (PaintEventArgs pe, DataGrid grid);
		public abstract void DataGridPaintCaption (Graphics g, Rectangle clip, DataGrid grid);
		public abstract void DataGridPaintColumnHeaders (Graphics g, Rectangle clip, DataGrid grid);
		public abstract void DataGridPaintColumnHeader (Graphics g, Rectangle bounds, DataGrid grid, int col);
		public abstract void DataGridPaintRowContents (Graphics g, int row, Rectangle row_rect, bool is_newrow, Rectangle clip, DataGrid grid);
		public abstract void DataGridPaintRowHeader (Graphics g, Rectangle bounds, int row, DataGrid grid);
		public abstract void DataGridPaintRowHeaderArrow (Graphics g, Rectangle bounds, DataGrid grid);
		public abstract void DataGridPaintRowHeaderStar (Graphics g, Rectangle bounds, DataGrid grid);
		public abstract void DataGridPaintParentRows (Graphics g, Rectangle bounds, DataGrid grid);
		public abstract void DataGridPaintParentRow (Graphics g, Rectangle bounds, DataGridDataSource row, DataGrid grid);
		public abstract void DataGridPaintRows (Graphics g, Rectangle cells, Rectangle clip, DataGrid grid);
		public abstract void DataGridPaintRow (Graphics g, int row, Rectangle row_rect, bool is_newrow, Rectangle clip, DataGrid grid);
		public abstract void DataGridPaintRelationRow (Graphics g, int row, Rectangle row_rect, bool is_newrow, Rectangle clip, DataGrid grid);
		
		#endregion // Datagrid

		#region DataGridView
		#region DataGridViewHeaderCell
		#region DataGridViewRowHeaderCell
		public abstract bool DataGridViewRowHeaderCellDrawBackground (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds);
		public abstract bool DataGridViewRowHeaderCellDrawSelectionBackground (DataGridViewRowHeaderCell cell);
		public abstract bool DataGridViewRowHeaderCellDrawBorder (DataGridViewRowHeaderCell cell, Graphics g, Rectangle bounds);
		#endregion
		#region DataGridViewColumnHeaderCell
		public abstract bool DataGridViewColumnHeaderCellDrawBackground (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds);
		public abstract bool DataGridViewColumnHeaderCellDrawBorder (DataGridViewColumnHeaderCell cell, Graphics g, Rectangle bounds);
		#endregion
		public abstract bool DataGridViewHeaderCellHasPressedStyle (DataGridView dataGridView);
		public abstract bool DataGridViewHeaderCellHasHotStyle (DataGridView dataGridView);
		#endregion
		#endregion

		#region DateTimePicker
		public abstract void DrawDateTimePicker(Graphics dc, Rectangle clip_rectangle, DateTimePicker dtp);
		public abstract bool DateTimePickerBorderHasHotElementStyle { get; }
		public abstract Rectangle DateTimePickerGetDropDownButtonArea (DateTimePicker dateTimePicker);
		public abstract Rectangle DateTimePickerGetDateArea (DateTimePicker dateTimePicker);
		public abstract bool DateTimePickerDropDownButtonHasHotElementStyle { get; }
		#endregion 	// DateTimePicker

		#region GroupBox
		// Drawing
		public abstract void DrawGroupBox (Graphics dc,  Rectangle clip_area, GroupBox box);

		// Sizing
		public abstract Size GroupBoxDefaultSize{get;}
		#endregion	// GroupBox

		#region HScrollBar
		public abstract Size HScrollBarDefaultSize{get;}	// Default size of the scrollbar
		#endregion	// HScrollBar

		#region ListBox
		// Drawing
		public abstract void DrawListBoxItem (ListBox ctrl, DrawItemEventArgs e);
		#endregion	// ListBox
		
		#region ListView
		// Drawing
		public abstract void DrawListViewItems (Graphics dc, Rectangle clip_rectangle, ListView control);
		public abstract void DrawListViewHeader (Graphics dc, Rectangle clip_rectangle, ListView control);
		public abstract void DrawListViewHeaderDragDetails (Graphics dc, ListView control, ColumnHeader drag_column, int target_x);
		public abstract bool ListViewHasHotHeaderStyle { get; }

		// Sizing
		public abstract int ListViewGetHeaderHeight (ListView listView, Font font);
		public abstract Size ListViewCheckBoxSize { get; }
		public abstract int ListViewColumnHeaderHeight { get; }
		public abstract int ListViewDefaultColumnWidth { get; }
		public abstract int ListViewVerticalSpacing { get; }
		public abstract int ListViewEmptyColumnWidth { get; }
		public abstract int ListViewHorizontalSpacing { get; }
		public abstract Size ListViewDefaultSize { get; }
		public abstract int ListViewGroupHeight { get; }
		public abstract int ListViewItemPaddingWidth { get; }
		public abstract int ListViewTileWidthFactor { get; }
		public abstract int ListViewTileHeightFactor { get; }
		#endregion	// ListView
		
		#region Menus
		public abstract void CalcItemSize (Graphics dc, MenuItem item, int y, int x, bool menuBar);
		public abstract void CalcPopupMenuSize (Graphics dc, Menu menu);
		public abstract int CalcMenuBarSize (Graphics dc, Menu menu, int width);
		public abstract void DrawMenuBar (Graphics dc, Menu menu, Rectangle rect);
		public abstract void DrawMenuItem (MenuItem item, DrawItemEventArgs e);
		public abstract void DrawPopupMenu (Graphics dc, Menu menu, Rectangle cliparea, Rectangle rect);
		#endregion 	// Menus

		#region MonthCalendar
		public abstract void DrawMonthCalendar(Graphics dc, Rectangle clip_rectangle, MonthCalendar month_calendar);
		#endregion 	// MonthCalendar

		#region Panel
		// Sizing
		public abstract Size PanelDefaultSize{get;}
		#endregion	// Panel

		#region PictureBox
		// Drawing
		public abstract void DrawPictureBox (Graphics dc, Rectangle clip, PictureBox pb);

		// Sizing
		public abstract Size PictureBoxDefaultSize{get;}
		#endregion	// PictureBox

		#region PrintPreviewControl
		public abstract int PrintPreviewControlPadding{get;}
		public abstract Size PrintPreviewControlGetPageSize (PrintPreviewControl preview);
		public abstract void PrintPreviewControlPaint (PaintEventArgs pe, PrintPreviewControl preview, Size page_image_size);
		#endregion      // PrintPreviewControl

		#region ProgressBar
		// Drawing
		public abstract void DrawProgressBar (Graphics dc, Rectangle clip_rectangle, ProgressBar progress_bar);

		// Sizing
		public abstract Size ProgressBarDefaultSize{get;}
		#endregion	// ProgressBar

		#region RadioButton
		// Drawing
		public abstract Size CalculateRadioButtonAutoSize (RadioButton rb);
		public abstract void CalculateRadioButtonTextAndImageLayout (ButtonBase b, Point offset, out Rectangle glyphArea, out Rectangle textRectangle, out Rectangle imageRectangle);
		public abstract void DrawRadioButton (Graphics g, RadioButton rb, Rectangle glyphArea, Rectangle textBounds, Rectangle imageBounds, Rectangle clipRectangle);
		public abstract void DrawRadioButton (Graphics dc, Rectangle clip_rectangle, RadioButton radio_button);

		// Sizing
		public abstract Size RadioButtonDefaultSize{get;}
		#endregion	// RadioButton

		#region ScrollBar
		// Drawing
		//public abstract void DrawScrollBar (Graphics dc, Rectangle area, ScrollBar bar, ref Rectangle thumb_pos, ref Rectangle first_arrow_area, ref Rectangle second_arrow_area, ButtonState first_arrow, ButtonState second_arrow, ref int scrollbutton_width, ref int scrollbutton_height, bool vert);
		public abstract void DrawScrollBar (Graphics dc, Rectangle clip_rectangle, ScrollBar bar);

		// Sizing
		public abstract int ScrollBarButtonSize {get;}		// Size of the scroll button

		public abstract bool ScrollBarHasHotElementStyles { get; }

		public abstract bool ScrollBarHasPressedThumbStyle { get; }

		public abstract bool ScrollBarHasHoverArrowButtonStyle { get; }
		#endregion	// ScrollBar

		#region StatusBar
		// Drawing
		public abstract void DrawStatusBar (Graphics dc, Rectangle clip_rectangle, StatusBar sb);

		// Sizing
		public abstract int StatusBarSizeGripWidth {get;}		// Size of Resize area
		public abstract int StatusBarHorzGapWidth {get;}	// Gap between panels
		public abstract Size StatusBarDefaultSize{get;}
		#endregion	// StatusBar

		#region TabControl
		public abstract Size TabControlDefaultItemSize {get; }
		public abstract Point TabControlDefaultPadding {get; }
		public abstract int TabControlMinimumTabWidth {get; }
		public abstract Rectangle TabControlSelectedDelta { get; }
		public abstract int TabControlSelectedSpacing { get; }
		public abstract int TabPanelOffsetX { get; }
		public abstract int TabPanelOffsetY { get; }
		public abstract int TabControlColSpacing { get; }
		public abstract Point TabControlImagePadding { get; }
		public abstract int TabControlScrollerWidth { get; }

		public abstract Rectangle TabControlGetDisplayRectangle (TabControl tab);
		public abstract Rectangle TabControlGetPanelRect (TabControl tab);
		public abstract Size TabControlGetSpacing (TabControl tab);
		public abstract void DrawTabControl (Graphics dc, Rectangle area, TabControl tab);
		#endregion

		#region TextBoxBase
		public abstract void TextBoxBaseFillBackground (TextBoxBase textBoxBase, Graphics g, Rectangle clippingArea);
		public abstract bool TextBoxBaseHandleWmNcPaint (TextBoxBase textBoxBase, ref Message m);
		public abstract bool TextBoxBaseShouldPaintBackground (TextBoxBase textBoxBase);
		#endregion

		#region	ToolBar
		// Drawing
		public abstract void DrawToolBar (Graphics dc, Rectangle clip_rectangle, ToolBar control);

		// Sizing
		public abstract int ToolBarGripWidth {get;}		 // Grip width for the ToolBar
		public abstract int ToolBarImageGripWidth {get;}	 // Grip width for the Image on the ToolBarButton
		public abstract int ToolBarSeparatorWidth {get;}	 // width of the separator
		public abstract int ToolBarDropDownWidth { get; }	 // width of the dropdown arrow rect
		public abstract int ToolBarDropDownArrowWidth { get; }	 // width for the dropdown arrow on the ToolBarButton
		public abstract int ToolBarDropDownArrowHeight { get; }	 // height for the dropdown arrow on the ToolBarButton
		public abstract Size ToolBarDefaultSize{get;}

		public abstract bool ToolBarHasHotElementStyles (ToolBar toolBar);
		public abstract bool ToolBarHasHotCheckedElementStyles { get; }
		#endregion	// ToolBar

		#region ToolTip
		public abstract void DrawToolTip(Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control);
		public abstract Size ToolTipSize(ToolTip.ToolTipWindow tt, string text);
		public abstract bool ToolTipTransparentBackground { get; }
		#endregion	// ToolTip
		
		#region BalloonWindow
		public abstract void ShowBalloonWindow (IntPtr handle, int timeout, string title, string text, ToolTipIcon icon);
		public abstract void HideBalloonWindow (IntPtr handle);
		public abstract void DrawBalloonWindow (Graphics dc, Rectangle clip, NotifyIcon.BalloonWindow control);
		public abstract Rectangle BalloonWindowRect (NotifyIcon.BalloonWindow control);
		#endregion	// BalloonWindow

		#region TrackBar
		// Drawing
		public abstract void DrawTrackBar (Graphics dc, Rectangle clip_rectangle, TrackBar tb);

		// Sizing
		public abstract Size TrackBarDefaultSize{get; }		// Default size for the TrackBar control
		
		public abstract int TrackBarValueFromMousePosition (int x, int y, TrackBar tb);

		public abstract bool TrackBarHasHotThumbStyle { get; }
		#endregion	// TrackBar

		#region UpDownBase
		public abstract void UpDownBaseDrawButton (Graphics g, Rectangle bounds, bool top, VisualStyles.PushButtonState state);
		public abstract bool UpDownBaseHasHotButtonStyle { get; }
		#endregion

		#region VScrollBar
		public abstract Size VScrollBarDefaultSize{get;}	// Default size of the scrollbar
		#endregion	// VScrollBar

		#region TreeView
		public abstract Size TreeViewDefaultSize { get; }
		public abstract void TreeViewDrawNodePlusMinus (TreeView treeView, TreeNode node, Graphics dc, int x, int middle);
		#endregion

		#region Managed window
		public abstract void DrawManagedWindowDecorations (Graphics dc, Rectangle clip, InternalWindowManager wm);
		public abstract int ManagedWindowTitleBarHeight (InternalWindowManager wm);
		public abstract int ManagedWindowBorderWidth (InternalWindowManager wm);
		public abstract int ManagedWindowIconWidth (InternalWindowManager wm);
		public abstract Size ManagedWindowButtonSize (InternalWindowManager wm);
		public abstract void ManagedWindowSetButtonLocations (InternalWindowManager wm);
		public abstract Rectangle ManagedWindowGetTitleBarIconArea (InternalWindowManager wm);
		public abstract Size ManagedWindowGetMenuButtonSize (InternalWindowManager wm);
		public abstract bool ManagedWindowTitleButtonHasHotElementStyle (TitleButton button, Form form);
		public abstract void ManagedWindowDrawMenuButton (Graphics dc, TitleButton button, Rectangle clip, InternalWindowManager wm);
		public abstract void ManagedWindowOnSizeInitializedOrChanged (Form form);
		public const int ManagedWindowSpacingAfterLastTitleButton = 2;
		#endregion

		#region	ControlPaint Methods
		public abstract void CPDrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle);

		public abstract void CPDrawBorder (Graphics graphics, RectangleF bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle);

		public abstract void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides);
		public abstract void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color);
		public abstract void CPDrawButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state);
		public abstract void CPDrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawContainerGrabHandle (Graphics graphics, Rectangle bounds);
		public abstract void CPDrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor);
		public abstract void CPDrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled);
		public abstract void CPDrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor);
		public abstract void CPDrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background);
		public abstract void CPDrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary);
		public abstract void CPDrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph, Color color, Color backColor);
		public abstract void CPDrawMixedCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style);
		public abstract void CPDrawReversibleLine (Point start, Point end, Color backColor);
		public abstract void CPDrawScrollButton (Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state);
		public abstract void CPDrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor);
		public abstract void CPDrawSizeGrip (Graphics graphics, Color backColor, Rectangle bounds);
		public abstract void CPDrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
			StringFormat format);
		public abstract void CPDrawStringDisabled (IDeviceContext dc, string s, Font font, Color color, Rectangle layoutRectangle, TextFormatFlags format);
		public abstract void CPDrawVisualStyleBorder (Graphics graphics, Rectangle bounds);
		public abstract void CPDrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style);
		#endregion	// ControlPaint Methods
	}
}
