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
// Copyright (c) 2004, 2005 Novell, Inc.
//
// Authors:
//	Miguel de Icaza (miguel@novell.com).
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
//

using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms {
	[DefaultEvent ("ValueChanged")]
	[DefaultProperty ("Value")]
	public class NumericUpDown : UpDownBase, ISupportInitialize {
		Decimal updown_value;
		Decimal min = 0m;
		Decimal max = 100m;
		Decimal increment = 1m;
		string format;
		bool thousand;
		bool on_init;
		int decimal_places;
		bool hexadecimal;
		
		public NumericUpDown ()
		{
			UpdateFormat ();
			UpdateEditText ();
		}
		
		#region Events
		public new event EventHandler TextChanged;
		public event EventHandler ValueChanged;
		#endregion Events
		
#region ISupportInitialize methods

		//
		// These are used for batch updates of properties:
		// checking of values should be disabled during this
		// time, assuming that the caller will set us up
		// correctly.
		//
		// See: http://www.vkarlsen.no/articles/02_isupportinitialize/ISupportInitialize.pdf
		// for the strategy.
		//
		public void BeginInit ()
		{
			on_init = true;
		}

		public void EndInit ()
		{
			on_init = false;
			if (updown_value < min)
				updown_value = min;
			if (updown_value > max)
				updown_value = max;
			
			UpdateEditText ();
		}
#endregion

		public override void DownButton ()
		{
			if (UserEdit)
				ParseEditText ();

			if (updown_value-increment >= min){
				Value -= increment;
				UpdateEditText ();
			}
		}

		public override void UpButton ()
		{
			if (UserEdit)
				ParseEditText ();

			if (updown_value + increment <= max){
				Value += increment;
				UpdateEditText ();
			}
		}

		void UpdateFormat ()
		{
			if (hexadecimal)
				format = "X";
			else if (thousand)
				format = "N" + decimal_places.ToString ();
			else
				format = "F" + decimal_places.ToString ();
		}

		public override void UpdateEditText ()
		{
			if (on_init)
				return;

			if (UserEdit)
				ParseEditText ();

			ChangingText = true;
			if (hexadecimal) { // gotta convert to something, decimal throws with "X"
				long val = (long) updown_value;
				Text = val.ToString (format);
			} else {
				Text = updown_value.ToString (format);
			}
		}

		public void ParseEditText ()
		{
			decimal res;
			try {
				if (hexadecimal) {
					res = Int64.Parse (Text, NumberStyles.HexNumber);
				} else {
					res = decimal.Parse (Text);
				}
			} catch {
				res = updown_value;
			}
			
			if (res < min)
				res = min;
			else if (res > max)
				res = max;

			Value = res;
			UserEdit = false;
		}

		protected override void ValidateEditText ()
		{
			ParseEditText ();
			UpdateEditText ();
		}

		
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			//TODO
			return base.CreateAccessibilityInstance ();
		}

		[Bindable(true)]
		public decimal Value {
			get {
				return updown_value;
			}

			set {
				if (updown_value ==  value) {
					return;
				}
				
				if (on_init){
					updown_value = value;
					return;
				}
				
				if (value < min || value > max)
					throw new ArgumentOutOfRangeException (
						String.Format ("Value {0} not within boundaries [{1}, {2}]", value, min, max));
				
				updown_value = value;
				OnValueChanged (EventArgs.Empty);
				UpdateEditText ();
			}
		}

		public decimal Increment {
			get {
				return increment;
			}

			set {
				increment = value;
			}
		}
		
		[RefreshProperties(RefreshProperties.All)]
		public decimal Maximum {
			get {
				return max;
			}

			set {
				max = value;
				if (value > max)
					value = max;
				if (min > max)
					min = max;
				UpdateEditText ();
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public decimal Minimum {
			get {
				return min;
			}

			set {
				min = value;
				if (value < min)
					value = min;
				if (min > max)
					max = min;
				UpdateEditText ();
			}
		}

		[Bindable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable (false)]
		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		[DefaultValue (false)]
		[Localizable (true)]
		public bool ThousandsSeparator {
			get {
				return thousand;
			}

			set {
				if (value == thousand)
					return;

				thousand = value;
				UpdateFormat ();
				UpdateEditText ();
			}
		}

		[DefaultValue (0)]
		public int DecimalPlaces {
			get {
				return decimal_places;
			}

			set {
				if (value == decimal_places)
					return;

				decimal_places = value;
				UpdateFormat ();
				UpdateEditText ();
			}
		}

		[DefaultValue (false)]
		public bool Hexadecimal {
			get { return hexadecimal; }
			set {
				if (value == hexadecimal)
					return;

				hexadecimal = value;
				UpdateFormat ();
				UpdateEditText ();
			}
		}
		
#region Overrides for Control hooks
		static bool CompareCharToString (char c, string str)
		{
			if (str.Length == 1)
				return (c == str [0]);

			return (new string (c, 1) == str);
		}

		protected override void OnTextBoxKeyPress (object source, KeyPressEventArgs e)
		{
			base.OnTextBoxKeyPress (source, e);

			bool handled = true;
			char key = e.KeyChar;
			bool is_digit = Char.IsDigit (key);
			if (is_digit || hexadecimal) {
				handled = !(is_digit || (key >= 'a' && key <= 'f') || (key >= 'A' && key <= 'F'));
			} else {
				NumberFormatInfo ninfo = CultureInfo.CurrentCulture.NumberFormat;
				if (CompareCharToString (key, ninfo.NegativeSign)) {
					handled = false;
				} else if (CompareCharToString (key, ninfo.NumberDecimalSeparator)) {
					handled = false;
				} else if (CompareCharToString (key, ninfo.NumberGroupSeparator)) {
					handled = false;
				}
			}

			e.Handled = handled;
		}
#endregion
		protected virtual void OnValueChanged (EventArgs e)
		{
			if (ValueChanged != null)
				ValueChanged (this, e);
		}

		public override string ToString ()
		{
			return String.Format ("{0} Min={0} Max={1}", base.ToString (), min, max);
		}
	}
}
