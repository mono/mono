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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jonathan Gilbert	<logic@deltaq.org>
//
// Integration into MWF:
//	Peter Bartok		<pbartok@novell.com>
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms {
	[DefaultEvent("ValueChanged")]
	[DefaultProperty("Value")]
	[DefaultBindingProperty ("Value")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class NumericUpDown : UpDownBase, ISupportInitialize {
		#region Local Variables
		private bool	suppress_validation;
		private int	decimal_places;
		private bool	hexadecimal;
		private decimal	increment;
		private decimal	maximum;
		private decimal	minimum;
		private bool	thousands_separator;
		private decimal	dvalue;
		

		NumericUpDownAccelerationCollection accelerations;
//		private long buttonPressedTicks;
		//private bool isSpinning;
		#endregion	// Local Variables

		#region UIA FrameWork Events
		static object UIAMinimumChangedEvent = new object ();

		internal event EventHandler UIAMinimumChanged {
			add { Events.AddHandler (UIAMinimumChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMinimumChangedEvent, value); }
		}

		internal void OnUIAMinimumChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIAMinimumChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		static object UIAMaximumChangedEvent = new object ();

		internal event EventHandler UIAMaximumChanged {
			add { Events.AddHandler (UIAMaximumChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMaximumChangedEvent, value); }
		}

		internal void OnUIAMaximumChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIAMaximumChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		static object UIASmallChangeChangedEvent = new object ();

		internal event EventHandler UIASmallChangeChanged {
			add { Events.AddHandler (UIASmallChangeChangedEvent, value); }
			remove { Events.RemoveHandler (UIASmallChangeChangedEvent, value); }
		}

		internal void OnUIASmallChangeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIASmallChangeChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Public Constructors
		public NumericUpDown() {
			suppress_validation = false;
			decimal_places = 0;
			hexadecimal = false;
			increment = 1M;
			maximum = 100M;
			minimum = 0.0M;
			thousands_separator = false;

			Text = "0";
		}
		#endregion	// Public Constructors

		#region Public Instance Properties

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public NumericUpDownAccelerationCollection Accelerations {
			get {
				if (accelerations == null)
					accelerations = new NumericUpDownAccelerationCollection ();
				return accelerations;
			}
		}

		[DefaultValue(0)]
		public int DecimalPlaces {
			get {
				return decimal_places;
			}

			set {
				decimal_places = value;
				UpdateEditText();
			}
		}

		[DefaultValue(false)]
		public bool Hexadecimal {
			get {
				return hexadecimal;
			}

			set {
				hexadecimal = value;
				UpdateEditText();
			}
		}

		public decimal Increment {
			get {

				return increment;
			}

			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException("value", value, "NumericUpDown increment cannot be negative");
				}

				increment = value;

				// UIA Framework Event: SmallChange Changed
				OnUIASmallChangeChanged (EventArgs.Empty);
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public decimal Maximum {
			get {
				return maximum;
			}

			set {
				maximum = value;

				if (minimum > maximum)
					minimum = maximum;

				if (dvalue > maximum)
					Value = maximum;

				// UIA Framework Event: Maximum Changed
				OnUIAMaximumChanged (EventArgs.Empty);
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public decimal Minimum {
			get {
				return minimum;
			}

			set {
				minimum = value;

				if (maximum < minimum)
					maximum = minimum;

				if (dvalue < minimum)
					Value = minimum;

				// UIA Framework Event: Minimum Changed
				OnUIAMinimumChanged (EventArgs.Empty);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return Padding.Empty; }
			set { }
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		public bool ThousandsSeparator {
			get {
				return thousands_separator;
			}

			set {
				thousands_separator = value;
				UpdateEditText();
			}
		}

		[Bindable(true)]
		public decimal Value {
			get {
				if (UserEdit)
					ValidateEditText ();
				return dvalue;
			}

			set {
				if (value != dvalue) {
					if (!suppress_validation && ((value < minimum) || (value > maximum))) {
						throw new ArgumentOutOfRangeException ("value", "NumericUpDown.Value must be within the specified Minimum and Maximum values");
					}

					dvalue = value;
					OnValueChanged (EventArgs.Empty);
					UpdateEditText ();
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void BeginInit() {
			suppress_validation = true;
		}

		public void EndInit() {
			suppress_validation = false;
			Value = Check (dvalue);
			UpdateEditText ();
		}

		public override string ToString() {
			return string.Format("{0}, Minimum = {1}, Maximum = {2}", base.ToString(), minimum, maximum);
		}

		public override void DownButton() {
			if (UserEdit) {
				ParseEditText ();
			}

			Value = Math.Max (minimum, unchecked (dvalue - increment));

			// UIA Framework Event: DownButton Click
			OnUIADownButtonClick (EventArgs.Empty);
		}

		public override void UpButton() {
			if (UserEdit) {
				ParseEditText ();
			}

			Value = Math.Min (maximum, unchecked (dvalue + increment));

			// UIA Framework Event: UpButton Click
			OnUIAUpButtonClick (EventArgs.Empty);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			AccessibleObject	acc;

			acc = new AccessibleObject(this);
			acc.role = AccessibleRole.SpinButton;

			return acc;
		}

		protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e) {
			if ((ModifierKeys & ~Keys.Shift) != Keys.None) {
				return;
			}

			NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
			string pressedKey = e.KeyChar.ToString ();

			if ((pressedKey != nfi.NegativeSign) && (pressedKey != nfi.NumberDecimalSeparator) && 
				(pressedKey != nfi.NumberGroupSeparator)) {
				string acceptable = hexadecimal ? "\b0123456789abcdefABCDEF" : "\b0123456789";
				if (acceptable.IndexOf (e.KeyChar) == -1) {
					// FIXME: produce beep to signal that "invalid" key was pressed
					// prevent the key from reaching the text box
					e.Handled = true;
				}
			}

			base.OnTextBoxKeyPress(source, e);
		}

		protected virtual void OnValueChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected void ParseEditText () {
			try {
				if (!hexadecimal) {
					Value = Check (decimal.Parse (Text, CultureInfo.CurrentCulture));
				} else {
#if !NET_2_0
					Value = Check (Convert.ToDecimal (Convert.ToInt32 (Text, 16)));
#else
					Value = Check (Convert.ToDecimal (Convert.ToInt32 (Text, 10)));
#endif
				}
			}
			catch { }
			finally {
				UserEdit = false;
			}
		}

		private decimal Check (decimal val)
		{
			decimal ret = val;
			if (ret < minimum) {
				ret = minimum;
			}

			if (ret > maximum) {
				ret = maximum;
			}

			return ret;
		}

		protected override void UpdateEditText () {
			if (suppress_validation)
				return;

			if (UserEdit)
				ParseEditText ();

			ChangingText = true;
			if (!hexadecimal) {
				// "N" and "F" differ only in that "N" includes commas
				// every 3 digits to the left of the decimal and "F"
				// does not.

				string format_string;

				if (thousands_separator) {
					format_string = "N";
				} else {
					format_string = "F";
				}

				format_string += decimal_places;

				Text = dvalue.ToString (format_string, CultureInfo.CurrentCulture);

			} else {
				// Cast to Int64 to be able to use the "X" formatter.
				// If the value is below Int64.MinValue or above Int64.MaxValue,
				// then an OverflowException will be thrown, as with .NET.
				Text = ((Int64)dvalue).ToString("X", CultureInfo.CurrentCulture);
			}
		}


		protected override void ValidateEditText() {
			ParseEditText ();
			UpdateEditText ();
		}

		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus(e);
			if (UserEdit)
				UpdateEditText();
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
//			isSpinning = false;
			base.OnKeyUp (e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
//			buttonPressedTicks = DateTime.Now.Ticks;
//			isSpinning = true;
			base.OnKeyDown (e);
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		static object ValueChangedEvent = new object ();

		public event EventHandler ValueChanged {
			add { Events.AddHandler (ValueChangedEvent, value); }
			remove { Events.RemoveHandler (ValueChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion	// Events
	}
}
