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

// COMPLETE

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms {
	[DefaultEvent("ValueChanged")]
	[DefaultProperty("Value")]
	public class NumericUpDown : UpDownBase, ISupportInitialize {
		#region Local Variables
		private int	suppress_validation;
		private int	decimal_places;
		private bool	hexadecimal;
		private decimal	increment;
		private decimal	maximum;
		private decimal	minimum;
		private bool	thousands_separator;
		private decimal	value;
		#endregion	// Local Variables

		#region Public Constructors
		public NumericUpDown() {
			suppress_validation = 0;
			decimal_places = 0;
			hexadecimal = false;
			increment = 1M;
			maximum = 100.0M;
			minimum = 0.0M;
			thousands_separator = false;
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
		[DefaultValue(0)]
		public int DecimalPlaces {
			get {
				return decimal_places;
			}

			set {
				decimal_places = value;
				if (!UserEdit) {
					UpdateEditText();
				}
			}
		}

		[DefaultValue(false)]
		public bool Hexadecimal {
			get {
				return hexadecimal;
			}

			set {
				hexadecimal = value;
				if (!UserEdit) {
					UpdateEditText();
				}
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

				if (value > maximum)
					value = maximum;
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

				if (value < minimum)
					value = minimum;
			}
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return base.txtView.Text;
			}

			set {
				base.txtView.Text = value;
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

				if (!UserEdit) {
					UpdateEditText();
				}
			}
		}

		[Bindable(true)]
		public decimal Value {
			get {
				return value;
			}

			set {
				if (suppress_validation <= 0) {
					if ((value < minimum) || (value > maximum)) {
						throw new ArgumentException("NumericUpDown.Value must be within the specified Minimum and Maximum values", "value");
					}
				}

				this.value = value;
				OnValueChanged(EventArgs.Empty);
				UpdateEditText();
			} 
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void BeginInit() {
			suppress_validation++;
		}

		public override void DownButton() {
			if (UserEdit) {
				ParseEditText();
			}

			Value = Math.Max(minimum, unchecked(value - increment));
		}

		public void EndInit() {
			suppress_validation--;
			if (suppress_validation == 0)
				UpdateEditText ();
		}

		public override string ToString() {
			return string.Format("{0}: value {1} in range [{2}, {3}]", base.ToString(), value, minimum, maximum);
		}

		public override void UpButton() {
			if (UserEdit)
				ParseEditText();

			Value = Math.Min(maximum, unchecked(value + increment));
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

			string acceptable = hexadecimal ? "\b-.,0123456789ABCDEF" : "\b-.,0123456789";

			if (acceptable.IndexOf(e.KeyChar) < 0) {
				// prevent the key from reaching the text box
				e.Handled = true;
			}

			base.OnTextBoxKeyPress(source, e);
		}

		protected virtual void OnValueChanged(EventArgs e) {
			if (ValueChanged != null) {
				ValueChanged(this, e);
			}
		}

		protected void ParseEditText() {
			UserEdit = false;

			try {
				string user_edit_text = Text.Replace(",", "").Trim();

				if (!hexadecimal) {
					value = decimal.Parse(user_edit_text);
				} else {
					value = 0M;

					for (int i=0; i < user_edit_text.Length; i++) {
						int hex_digit = Convert.ToInt32(user_edit_text.Substring(i, 1), 16);

						value = unchecked(value * 16M + (decimal)hex_digit);
					}
				}

				if (value < minimum) {
					value = minimum;
				}

				if (value > maximum) {
					value = maximum;
				}

				OnValueChanged(EventArgs.Empty);
			}
			catch {}
		}

		protected override void UpdateEditText() {
			if (suppress_validation > 0)
				return;

			if (UserEdit)
				ParseEditText(); // validate user input

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
				Text = value.ToString(format_string);
			}
			else {
				// Decimal.ToString doesn't know the "X" formatter, and
				// converting it to an int is narrowing, so do it
				// manually...

				int[] bits = decimal.GetBits(value);

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
					ChangingText = true;
					Text = "0";
					return;
				}

				StringBuilder chars = new StringBuilder();

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

				ChangingText = true;
				Text = chars.ToString();
			}
		}

		protected override void ValidateEditText() {
			ParseEditText();
			UpdateEditText();
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler ValueChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler TextChanged;
		#endregion	// Events
	}
}
