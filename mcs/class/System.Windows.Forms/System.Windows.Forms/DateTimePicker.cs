//
// System.Windows.Forms.DateTimePicker
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) Ximian, Inc., 2002
//
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
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
		private DateTime val;

		//
		//  --- Constructors/Destructors
		//

		[MonoTODO]
		public DateTimePicker() : base()
		{
			// defaults :)
			calendarFont = Control.DefaultFont;
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
			val = DateTime.Now;
			Size = DefaultSize;
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
			//FIXME:
			return base.ToString();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			//FIXME:
			return base.CreateAccessibilityInstance();;
		}

		[MonoTODO]
		protected override void CreateHandle()
		{
			initCommonControlsLibrary();
			base.CreateHandle();
		}

		[MonoTODO]
		protected override void DestroyHandle()
		{
			//FIXME:
			base.DestroyHandle();
		}

		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}

		protected virtual void OnCloseUp(EventArgs e) {
			if ( CloseUp != null ) 
				CloseUp( this, e );
		}

		protected virtual void OnDropDown(EventArgs e)	{
			if ( DropDown != null )
				DropDown( this, e );
		}

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
			//FIXME:
			//if (FontChanged != null) {
			//	FontChanged(this, e);
			//}
		}

		protected virtual void OnFormatChanged(EventArgs e) {
			if (FormatChanged != null)
				FormatChanged(this, e);
		}

		[MonoTODO]
		protected override void OnSystemColorsChanged(EventArgs e)
		{
			//FIXME: update default colors
			//if (SystemColorsChanged != null) {
			//	SystemColorsChanged(this, e);
			//}
			base.OnSystemColorsChanged( e );
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			setControlRange( (int)( DateTimePickerFlags.GDTR_MIN | DateTimePickerFlags.GDTR_MAX ) );
			setControlValue( );
			setCalendarColors( );
			setCustomFormat( );
			setCalendarFont( );
		}

		protected virtual void OnValueChanged(EventArgs e) {
			if (ValueChanged != null) 
				ValueChanged(this, e);
		}

		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			//FIXME:
			base.SetBoundsCore(x, y, width, height, specified);
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			//FIXME:
			base.WndProc(ref m);
		}

		
		//  --- Public Events
		
		public event EventHandler CloseUp;
		public event EventHandler DropDown;
		public event EventHandler FormatChanged;
		public event EventHandler ValueChanged;

		
		//  --- Public Properties
		
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color BackColor {
			get {	return base.BackColor;	}
			set {	base.BackColor = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Image BackgroundImage 	{
			get {	return base.BackgroundImage; }
			set {	base.BackgroundImage = value;}
		}

		public Font CalendarFont {
			get {	return calendarFont; }
			set {
				calendarFont = value;
				setCalendarFont( );
			}
		}

		public Color CalendarForeColor {
			get {	return calendarForeColor; }
			set {
				if ( calendarForeColor != value ) {
					calendarForeColor = value;
					setCalendarColor( (int) MonthCalColors.MCSC_TEXT, value );
				}
			}
		}

		public Color CalendarMonthBackground {
			get {	return calendarMonthBackground; }
			set {
				if ( calendarMonthBackground != value ) {
					calendarMonthBackground = value;
					setCalendarColor( (int) MonthCalColors.MCSC_MONTHBK, value );
				}
			}
		}

		public Color CalendarTitleBackColor {
			get {	return calendarTitleBackColor; }
			set {
				if ( calendarTitleBackColor != value ) {
					calendarTitleBackColor = value;
					setCalendarColor( (int) MonthCalColors.MCSC_TITLEBK, value );
				}
			}
		}

		public Color CalendarTitleForeColor {
			get {	return calendarTitleForeColor; }
			set {
				if ( calendarTitleForeColor != value ) 	{
					calendarTitleForeColor = value;
					setCalendarColor( (int) MonthCalColors.MCSC_TITLETEXT, value );
				}
			}
		}

		public Color CalendarTrailingForeColor {
			get {	return calendarTrailingForeColor; }
			set {
				if ( calendarTrailingForeColor != value ) {
					calendarTrailingForeColor = value;
					setCalendarColor( (int) MonthCalColors.MCSC_TRAILINGTEXT, value );
				}
			}
		}

		public bool Checked {
			get {	
				if ( ShowCheckBox )
					getControlValue ( false ); // don't actually update the Value property
				return CHecked;
			}
			set {	
				CHecked = value;
				if ( ShowCheckBox )
					setCheckState ( );
			}
		}

		public string CustomFormat {
			get {	return customFormat; }
			set {
				customFormat = value;
				setCustomFormat ( );
			}
		}

		public LeftRightAlignment DropDownAlign 
		{
			get {	return dropDownAlign; }
			set {	
				if ( !Enum.IsDefined ( typeof(LeftRightAlignment), value ) )
					throw new InvalidEnumArgumentException( "DropDownAlign",
						(int)value,
						typeof(LeftRightAlignment));

				if ( dropDownAlign != value ) {
					dropDownAlign = value;
					if ( IsHandleCreated )
						RecreateHandle();
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor {
			get {	return base.ForeColor;	}
			set {	base.ForeColor = value;	}
		}

		public DateTimePickerFormat Format {
			get {	return format;	}
			set {
				if ( !Enum.IsDefined ( typeof(DateTimePickerFormat), value ) )
					throw new InvalidEnumArgumentException( "Format",
						(int)value,
						typeof(DateTimePickerFormat));

				if ( format != value ) {
					int StyleToRemove = formatStyle ( format );
					format = value;
					if ( IsHandleCreated )
						Win32.UpdateWindowStyle ( Handle, StyleToRemove, formatStyle ( format ) );
					OnFormatChanged( EventArgs.Empty );
				}
			}
		}

		public DateTime MaxDate {
			get {	return maxDate; }
			set {
				if ( value == maxDate )
					return;
				
				if ( value < MinDate )
					throw new ArgumentException (
						string.Format ("'{0}' is not a valid value for 'MaxDate'. 'MaxDate'  must be greater than or equal to MinDate", value ) );

				if ( value > MaxDateTime )
					throw new ArgumentException (
						string.Format ("DateTimePicker does not support dates after {0}.", MaxDateTime ) );

				maxDate = value;
				setControlRange	( (int)DateTimePickerFlags.GDTR_MAX );
			}
		}

		public DateTime MinDate {
			get {	return minDate;	}
			set {
				if ( value == minDate )
					return;

				if ( value >= MaxDate )
					throw new ArgumentException (
					string.Format ("'{0}' is not a valid value for 'MinDate'. 'MinDate' must be less than MaxDate.", value ) );

				if ( value < MinDateTime )
					throw new ArgumentException (
					string.Format ("DateTimePicker does not support dates before {0}.", MinDateTime ) );

				minDate = value;
				setControlRange	( (int)DateTimePickerFlags.GDTR_MIN );
			}
		}

		[MonoTODO]
		public int PreferredHeight {
			get{
				return 300;
			}
		}

		public bool ShowCheckBox {
			get {	return showCheckBox; }
			set {
				if ( showCheckBox != value ) {
					showCheckBox = value;
					if ( IsHandleCreated )
						RecreateHandle();
				}
			}
		}

		public bool ShowUpDown {
			get {	return showUpDown; }
			set {	
				if ( showUpDown != value ) {
					showUpDown = value;
					if ( IsHandleCreated )
						RecreateHandle();
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get {	return base.Text; }
			set {	base.Text = value;}
		}

		[MonoTODO]
		public DateTime Value {
			get {
				getControlValue( true );
				return val;
			}
			set {
				if ( val != value ) {
					val = value; // do we need to check that the value is in the range ?
					setControlValue( );
					OnValueChanged ( EventArgs.Empty );
				}
			}
		}

		
		//  --- Protected Properties
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				if ( Parent != null ) {
					CreateParams createParams = new CreateParams ();

					createParams.Caption = Text;
					createParams.ClassName = "SysDateTimePick32";
					createParams.X = Left;
					createParams.Y = Top;
					createParams.Width = Width;
					createParams.Height = Height;
					createParams.ClassStyle = 0;
					createParams.ExStyle = 0;
					createParams.Param = 0;
					createParams.Parent = Parent.Handle;
					createParams.Style = (int) (
						WindowStyles.WS_CHILDWINDOW | 
						WindowStyles.WS_VISIBLE |
						WindowStyles.WS_CLIPCHILDREN|
						WindowStyles.WS_CLIPSIBLINGS);
					
					if ( ShowUpDown )
						createParams.Style |= (int) DateTimePickerControlStyles.DTS_UPDOWN;

					if ( ShowCheckBox )
						createParams.Style |= (int) DateTimePickerControlStyles.DTS_SHOWNONE;

					if ( DropDownAlign == LeftRightAlignment.Right )
						createParams.Style |= (int) DateTimePickerControlStyles.DTS_RIGHTALIGN;

					createParams.Style |= formatStyle ( Format );

					return createParams;
				}
				return null;
			}		
		}

		protected override Size DefaultSize {
			get{	return new System.Drawing.Size(200,20);	}
		}

		private void initCommonControlsLibrary	( ) {
			if ( !RecreatingHandle ) {
				INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
				initEx.dwICC = CommonControlInitFlags.ICC_DATE_CLASSES;
				Win32.InitCommonControlsEx(initEx);
			}
		}

		private SYSTIME toSysTime ( DateTime val ) {
			SYSTIME systime = new SYSTIME() ;
			systime.wDay = (ushort)val.Day;
			systime.wHour = (ushort)val.Hour;
			systime.wMilliseconds = (ushort)val.Millisecond;
			systime.wMinute = (ushort)val.Minute;
			systime.wMonth = (ushort)val.Month;
			systime.wSecond = (ushort)val.Second;
			systime.wYear = (ushort)val.Year;
			return systime;
		}

		private DateTime toDateTime ( ref SYSTIME val ) {
			return new DateTime(	val.wYear, val.wMonth, val.wDay,
						val.wHour, val.wMinute, val.wSecond,
						val.wMilliseconds );
		}

		private void setControlValue ( ) {
			if ( IsHandleCreated ) 	{
				SYSTIME systime = toSysTime ( Value ) ;

				IntPtr ptr = Marshal.AllocCoTaskMem ( Marshal.SizeOf ( systime ) );
				Marshal.StructureToPtr( systime, ptr, false );
				Win32.SendMessage ( Handle, (int)DateTimePickerMessages.DTM_SETSYSTEMTIME,
							(int)DateTimePickerFlags.GDT_VALID, ptr );
				Marshal.FreeCoTaskMem( ptr );
			}
		}

		private void setCheckState ( ) {
			if ( Checked )
				setControlValue ();
			else {
				if ( IsHandleCreated ) 	{
					Win32.SendMessage ( Handle, (int)DateTimePickerMessages.DTM_SETSYSTEMTIME,
						(int)DateTimePickerFlags.GDT_NONE, 0 );
				}
			}
		}

		private void getControlValue ( bool updateProp ) {
			if ( IsHandleCreated ) 	{
				SYSTIME systime = new SYSTIME();
				IntPtr ptr = Marshal.AllocCoTaskMem ( Marshal.SizeOf ( systime ) );
				Marshal.StructureToPtr( systime, ptr, false );
				int res = Win32.SendMessage ( Handle, (int)DateTimePickerMessages.DTM_GETSYSTEMTIME,
							      0	, ptr ).ToInt32();
				if ( res == (int)DateTimePickerFlags.GDT_VALID ) {
					systime = Marshal.PtrToStructure ( ptr, systime.GetType ( ) ) as SYSTIME;
					DateTime newValue = toDateTime ( ref systime );

					CHecked = !( newValue == val || newValue == DateTime.Now );

					if ( updateProp )
						val = newValue;
				}
				else
					CHecked = false;
				Marshal.FreeCoTaskMem( ptr );
			}
		}

		private void setControlRange ( int rangeFlag ) {
			if ( IsHandleCreated ) {
				SYSTIME[] range = { toSysTime ( MinDate ), toSysTime ( MaxDate ) };
				IntPtr buffer = Marshal.AllocCoTaskMem( Marshal.SizeOf( range[0] ) * 2 );
				IntPtr current = buffer;
				Marshal.StructureToPtr ( range[0], current, false );
				current = (IntPtr)( current.ToInt32() + Marshal.SizeOf( range[0] ) );
				Marshal.StructureToPtr ( range[1], current, false );
				Win32.SendMessage( Handle, (int)DateTimePickerMessages.DTM_SETRANGE, rangeFlag, buffer.ToInt32() );
				Marshal.FreeCoTaskMem( buffer );
			}
		}

		private void setCalendarColor ( int ColorFlag, Color clr ) {
			if ( IsHandleCreated )
				Win32.SendMessage ( Handle, (int)DateTimePickerMessages.DTM_SETMCCOLOR, ColorFlag, Win32.RGB(clr) );
		}

		private void setCalendarColors ( ) {
			if ( calendarForeColor != ForeColor )
				setCalendarColor( (int) MonthCalColors.MCSC_TEXT, calendarForeColor );
			if ( calendarMonthBackground != DefaultMonthBackColor )
				setCalendarColor( (int) MonthCalColors.MCSC_MONTHBK, calendarMonthBackground );
			if ( calendarTitleBackColor != DefaultTitleBackColor )
				setCalendarColor( (int) MonthCalColors.MCSC_TITLEBK, calendarTitleBackColor );
			if ( calendarTitleForeColor != DefaultTitleForeColor )
				setCalendarColor( (int) MonthCalColors.MCSC_TITLETEXT, calendarTitleForeColor );
			if ( calendarTrailingForeColor != DefaultTrailingForeColor )
				setCalendarColor( (int) MonthCalColors.MCSC_TRAILINGTEXT, calendarTrailingForeColor );
		}

		private int formatStyle ( DateTimePickerFormat format ) {
			int style = 0;

			switch ( format ) {
			case DateTimePickerFormat.Long:
				style = (int)DateTimePickerControlStyles.DTS_LONGDATEFORMAT;
			break;
			case DateTimePickerFormat.Short:
				style = (int)DateTimePickerControlStyles.DTS_SHORTDATEFORMAT;
			break;
			case DateTimePickerFormat.Time:
				style = (int)DateTimePickerControlStyles.DTS_TIMEFORMAT;
			break;
			}
			return style;
		}

		private void setCustomFormat ( ) {
			if ( Format == DateTimePickerFormat.Custom && IsHandleCreated ) 
				Win32.SendMessage ( Handle, (int)DateTimePickerMessages.DTM_SETFORMATA, 0, CustomFormat );
		}
		
		private void setCalendarFont ( ) {
			// This code will not work because Font.Equals is not implemented
			/*
			if ( IsHandleCreated && !CalendarFont.Equals( Control.DefaultFont ) ) 
				Win32.SendMessage ( Handle, (int)DateTimePickerMessages.DTM_SETMCFONT,
							CalendarFont.ToHfont().ToInt32(), 0 );
			*/
		}
	}
}
