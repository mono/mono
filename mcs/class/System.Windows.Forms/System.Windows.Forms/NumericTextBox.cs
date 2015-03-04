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
// Copyright (c) 2005-2006 Novell, Inc.
//
// Authors:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;

namespace System.Windows.Forms
{
	class NumericTextBox : TextBox
	{
		double val;
		double min;

		// Last valid value for the numeric textbox
		public double Value {
			get {
				return val;
			}
			set {
				if (value == val)
					return;
				if (value < min)
					value = min;

				val = value;
				OnValueChanged (EventArgs.Empty);
			}
		}

		public double Min {
			get {
				return min;
			}
			set {
				min = value;
			}
		}

		protected override void OnLostFocus (EventArgs args)
		{
			// Update to the last valid value
			string val = Value.ToString ();
			if (Text != val)
				Text = val;

			base.OnLostFocus (args);
		}

		protected override void OnTextChanged (EventArgs args)
		{
			// Try to set the value to the new text.
			// Otherwise keep the old one.
			try {
				string text = Text.Length == 0 ? "0" : Text;
				double new_value = Double.Parse (text);
				Value = new_value;

			} catch (FormatException) {
			} catch (OverflowException) {
			}

			base.OnTextChanged (args);
		}

		protected override void OnKeyPress (KeyPressEventArgs args)
		{
			string acceptable = "\b.01234567890";
			if (acceptable.IndexOf (args.KeyChar) < 0)
				args.Handled = true;

			base.OnKeyPress (args);
		}

		protected virtual void OnValueChanged (EventArgs args)
		{
			EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
			if (eh != null)
				eh (this, args);
		}

		static object ValueChangedEvent = new object ();
		public event EventHandler ValueChanged {
			add { Events.AddHandler (ValueChangedEvent, value); }
			remove { Events.RemoveHandler (ValueChangedEvent, value); }
		}
	}
}

