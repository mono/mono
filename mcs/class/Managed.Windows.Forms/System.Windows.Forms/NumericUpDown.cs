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
#if NET_2_0
	[DefaultBindingProperty ("Value")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public class NumericUpDown : UpDownBase, ISupportInitialize {
		#region Local Variables
		private int	suppress_validation;
		private int	decimal_places;
		private bool	hexadecimal;
		private decimal	increment;
		private decimal	maximum;
		private decimal	minimum;
		private bool	thousands_separator;
		private decimal	dvalue;
		

#if NET_2_0
		NumericUpDownAccelerationCollection accelerations;
		private long buttonPressedTicks;
		private bool isSpinning;
#endif
		#endregion	// Local Variables

		#region Public Constructors
		public NumericUpDown() {
			suppress_validation = 0;
			decimal_places = 0;
			hexadecimal = false;
			increment = 1M;
			maximum = 100M;
			minimum = 0.0M;
			thousands_separator = false;

			Text = "0";
		}
		#endregion	// Public Constructors

		#region Private Methods
		void wide_number_multiply_by_10(int[] number) {
			long carry = 0;

			for (int i=0; i < number.Length; i++) {
				long multiplication = unchecked(carry + 10 * (long)(uint)number[i]);

				carry = multiplication >> 32;

				number[i] = unchecked((int)multiplication);
			}
		}

		void wide_number_multiply_by_16(int[] number) {
			int carry = 0;

			for (int i=0; i < number.Length; i++) {
				int multiplication = unchecked(carry | (number[i] << 4));

				carry = (number[i] >> 28) & 0x0F;

				number[i] = multiplication;
			}
		}

		void wide_number_divide_by_16(int[] number) {
			int carry = 0;

			for (int i=number.Length - 1; i >= 0; i--) {
				int division = unchecked(carry | ((number[i] >> 4) & 0x0FFFFFFF));

				carry = (number[i] << 28);

				number[i] = division;
			}
		}

		bool wide_number_less_than(int[] left, int[] right) {
			unchecked {
				for (int i=left.Length - 1; i >= 0; i--) {
					uint leftvalue = (uint)left[i];
					uint rightvalue = (uint)right[i];

					if (leftvalue > rightvalue)
						return false;
					if (leftvalue < rightvalue)
						return true;
				}
			}

			// equal
			return false;
		}

		void wide_number_subtract(int[] subtrahend, int[] minuend) {
			long carry = 0;

			unchecked {
				for (int i=0; i < subtrahend.Length; i++) {
					long subtrahendvalue = (uint)subtrahend[i];
					long minuendvalue = (uint)minuend[i];

					long result = subtrahendvalue - minuendvalue + carry;

					if (result < 0) {
						carry = -1;
						result -= int.MinValue;
						result -= int.MinValue;
					}
					else
						carry = 0;

					subtrahend[i] = unchecked((int)result);
				}
			}
		}
		#endregion	// Private Methods

		#region Public Instance Properties

#if NET_2_0		
		public NumericUpDownAccelerationCollection Accelerations {
			get {
				if (accelerations == null)
					accelerations = new NumericUpDownAccelerationCollection ();
				return accelerations;
			}
		}
#endif

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
			}
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
					dvalue = ParseEditText (Text);
				return dvalue;
			}

			set {
				if (suppress_validation <= 0) {
					if ((value < minimum) || (value > maximum)) {
						throw new ArgumentException("NumericUpDown.Value must be within the specified Minimum and Maximum values", "value");
					}
				}
				if (value != dvalue) {
					dvalue = value;
					OnValueChanged(EventArgs.Empty);
					Text = UpdateEditText (dvalue);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void BeginInit() {
			suppress_validation++;
		}

		public void EndInit() {
			suppress_validation--;
			if (suppress_validation == 0)
				UpdateEditText ();
		}

		public override string ToString() {
			return string.Format("{0}, Minimum = {1}, Maximum = {2}", base.ToString(), minimum, maximum);
		}

		public override void DownButton() {
			decimal val = dvalue;
			if (UserEdit) {
				val = ParseEditText (Text);
			}

			Value = Math.Max(minimum, unchecked(val - increment));
		}

		public override void UpButton() {
			decimal val = dvalue;
			if (UserEdit)
				val = ParseEditText (Text);

			Value = Math.Min(maximum, unchecked(val + increment));
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
			Value = ParseEditText (Text);
			UserEdit = false;
		}

		private decimal ParseEditText (string text) {
			UserEdit = false;
			decimal ret = dvalue;

			try {
				string user_edit_text = text;

				if (!hexadecimal) {
					ret = decimal.Parse(user_edit_text, CultureInfo.CurrentCulture);
				} else {
#if !NET_2_0
					ret = Convert.ToDecimal (Convert.ToInt32 (user_edit_text, 16));
#else
					ret = Convert.ToDecimal (Convert.ToInt32 (user_edit_text, 10));
#endif
				}

				if (ret < minimum) {
					ret = minimum;
				}

				if (ret > maximum) {
					ret = maximum;
				}
			}
			catch {}
			return ret;
		}

		protected override void UpdateEditText() {
			Text = UpdateEditText (ParseEditText (Text));
		}

		private string UpdateEditText (decimal val) {
			if (suppress_validation > 0)
				return Text;

			decimal ret = val;
			string text = Text;

			if (UserEdit)
				ret = ParseEditText (text); // parse user input

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

				ChangingText = true;
				text = ret.ToString(format_string, CultureInfo.CurrentCulture);
			}
			else {
				// Decimal.ToString doesn't know the "X" formatter, and
				// converting it to an int is narrowing, so do it
				// manually...

				int[] bits = decimal.GetBits(ret);

				bool negative = (bits[3] < 0);

				int scale = (bits[3] >> 16) & 0x1F;

				bits[3] = 0;

				int[] radix = new int[4];

				radix[0] = 1;

				for (int i=0; i < scale; i++)
					wide_number_multiply_by_10(radix);

				int num_chars = 0;

				while (!wide_number_less_than(bits, radix)) {
					num_chars++;
					wide_number_multiply_by_16(radix);
				}

				if (num_chars == 0) {
//					ChangingText = true;
					text = "0";
					return text;
				}

				StringBuilder chars = new StringBuilder ();

				if (negative)
					chars.Append('-');

				for (int i=0; i < num_chars; i++) {
					int digit = 0;

					wide_number_divide_by_16(radix);

					while (!wide_number_less_than(bits, radix)) { // greater than or equals
						digit++;
						wide_number_subtract(bits, radix);
					}

					if (digit < 10) {
						chars.Append((char)('0' + digit));
					} else {
						chars.Append((char)('A' + digit - 10));
					}
				}

//				ChangingText = true;
				text = chars.ToString();
			}
			return text;
		}

		protected override void ValidateEditText() {
			Value = ParseEditText (Text);
		}

#if NET_2_0
		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus(e);
			if (this.UserEdit)
				this.UpdateEditText();
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			isSpinning = false;
			base.OnKeyUp (e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			buttonPressedTicks = DateTime.Now.Ticks;
			isSpinning = true;
			base.OnKeyDown (e);
		}
#endif
		#endregion	// Protected Instance Methods

		#region Events
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
