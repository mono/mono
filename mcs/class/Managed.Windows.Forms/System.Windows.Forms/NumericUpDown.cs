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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Miguel de Icaza (miguel@novell.com).
//
//

using System;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class NumericUpDown : UpDownBase, ISupportInitialize {
		Decimal updown_value;
		Decimal min = 0m;
		Decimal max = 100m;
		bool thousand = false;
		bool on_init = false;
		
		public NumericUpDown () : base ()
		{
			UpdateEditText ();
		}
		
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

			if (updown_value > min){
				updown_value--;
				UpdateEditText ();
			}
		}

		public override void UpButton ()
		{
			if (UserEdit)
				ParseEditText ();

			if (updown_value < max){
				updown_value++;
				UpdateEditText ();
			}
		}

		public override void UpdateEditText ()
		{
			if (on_init)
				return;
			
			ChangingText = true;
			Text = updown_value.ToString ();
		}

		public void ParseEditText ()
		{
			decimal res;
			
			try {
				res = decimal.Parse (Text);
			} catch {
				res = min;
			}
			
			if (res < min)
				res = min;
			else if (res > max)
				res = max;
			updown_value = res;
		}

		protected override void ValidateEditText ()
		{
			ParseEditText ();
			UpdateEditText ();
		}

		public decimal Value {
			get {
				return updown_value;
			}

			set {
				if (on_init){
					updown_value = value;
					return;
				}
				
				if (value < min || value > max)
					throw new ArgumentOutOfRangeException (
						String.Format ("Value {0} not within boundaries [{1}, {2}]", value, min, max));
				
				updown_value = value;
				Text = updown_value.ToString ();
			}
		}

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

		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		public bool ThousandsSepator {
			get {
				return thousand;
			}

			set {
				thousand = value;
				UpdateEditText ();
			}
		}

#region Overrides for Control hooks

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
			if (UserEdit){
				ParseEditText ();
				UpdateEditText ();
			}
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			// TODO: What to do?
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			// TODO: What to do?
		}

		protected override void OnTextBoxKeyPress (object source, KeyPressEventArgs e)
		{
			Console.WriteLine ("OnTextBoxKeyPress: " + e);

			// TODO: Apparently we must validate digit input here
		}
#endregion
		
		
	}
}
