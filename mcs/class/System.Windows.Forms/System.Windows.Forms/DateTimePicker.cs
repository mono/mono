//
// System.Windows.Forms.DateTimePicker
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template. Nothing is implemented yet.
	//	Represents a Windows date-time picker control.
	// </summary>
	public class DateTimePicker : Control {

		//
		//  --- Public Fields
		//
		public static readonly DateTime MaxDateTime;
		public static readonly DateTime MinDateTime;

		//
		//  --- Protected Fields
		//
		protected static readonly Color DefaultMonthBackColor;
		protected static readonly Color DefaultTitleBackColor;
		protected static readonly Color DefaultTitleForeColor;
		protected static readonly Color DefaultTrailingForeColor;

		//
		//  --- Private Fields
		//
		private Font calendarFont;
		private Color calendarForeColor;
		private Color calendarMonthBackground;
		private Color calendarTitleBackColor;
		private Color calendarTitleForeColor;
		private Color calendarTrailingForeColor;
		private bool CHecked;
		private string customFormat;
		private LeftRightAlignment dropDownAlign;
		private DateTimePickerFormat format;
		private DateTime maxDate;
		private DateTime minDate;
		private int preferredHeight;
		private bool showCheckBox;
		private bool showUpDown;
		private DateTime value;

		//
		//  --- Constructors/Destructors
		//

		[MonoTODO]
		public DateTimePicker() : base()
		{
			// defaults :)
			calendarForeColor = ForeColor;
			calendarMonthBackground = DefaultMonthBackColor;
			calendarTitleBackColor = DefaultTitleBackColor;
			calendarTitleForeColor = DefaultTitleForeColor;
			calendarTrailingForeColor = DefaultTrailingForeColor;
			CHecked = true;
			customFormat = null;
			dropDownAlign = LeftRightAlignment.Left;
			format = DateTimePickerFormat.Long;
			maxDate = MaxDateTime;
			minDate = MinDateTime;
			showCheckBox = false;
			showUpDown = false;
		}

		[MonoTODO]
		static DateTimePicker()
		{
			MaxDateTime = new DateTime(9998,12,31,23,59,59);
			MinDateTime = new DateTime(1753,1,1,0,0,0);
			// As usual, the MS docs aren't very helpful...
			// I'm guessing these are all the right colors... not sure though
			// I'll check on Windows when I'm in a more masochistic mood ;)
			DefaultMonthBackColor = System.Drawing.SystemColors.Window;
			DefaultTitleBackColor = System.Drawing.SystemColors.ActiveCaption;
			DefaultTitleForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			DefaultTrailingForeColor = System.Drawing.SystemColors.WindowText;
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void CreateHandle()
		{
			//FIXME: Just to get it running
			base.CreateHandle();
		}

		[MonoTODO]
		protected override void DestroyHandle()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnCloseUp(EventArgs e)
		{
			if (CloseUp != null) {

				CloseUp(this, e);
			}
		}

		[MonoTODO]
		protected virtual void OnDropDown(EventArgs e)
		{
			if (DropDown != null) {

				DropDown(this, e);
			}
		}

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
			//if (FontChanged != null) {
			//	FontChanged(this, e);
			//}
		}

		[MonoTODO]
		protected virtual void OnFormatChanged(EventArgs e)
		{
			if (FormatChanged != null) {

				FormatChanged(this, e);
			}
		}

		[MonoTODO]
		protected override void OnSystemColorsChanged(EventArgs e)
		{
			//if (SystemColorsChanged != null) {
			//	SystemColorsChanged(this, e);
			//}
		}

		[MonoTODO]
		protected virtual void OnValueChanged(EventArgs e)
		{
			if (ValueChanged != null) {

				ValueChanged(this, e);
			}
		}

		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}

		
		//  --- Public Events
		
		public event EventHandler CloseUp;
		public event EventHandler DropDown;
		public event EventHandler FormatChanged;
		//public new event PaintEventHandler Paint; // This event is internal to the .NET framework.
		public event EventHandler ValueChanged;

		
		//  --- Public Properties
		
		[MonoTODO]
		public override Color BackColor {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Image BackgroundImage {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Font CalendarFont {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color CalendarForeColor {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color CalendarMonthBackground {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color CalendarTitleBackColor {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color CalendarTitleForeColor {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color CalendarTrailingForeColor {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public bool Checked {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string CustomFormat {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public LeftRightAlignment DropDownAlign {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Color ForeColor {
			//FIXME: Just to get it to run
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		[MonoTODO]
		public DateTimePickerFormat Format {

			get {
				throw new NotImplementedException (); 
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DateTime MaxDate {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DateTime MinDate {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int PreferredHeight {
			get{
				return 300;
			}
		}

		[MonoTODO]
		public bool ShowCheckBox {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool ShowUpDown {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string Text {
			//FIXME: just to get it to run
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		[MonoTODO]
		public DateTime Value {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException (); 
			}
		}

		
		//  --- Protected Properties
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "DATETIMEPICKER";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}		
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get{
				return new System.Drawing.Size(300,200);
			}
		}
	}
}
