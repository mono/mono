//
// System.Windows.Forms.NumericUpDown.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Alexandre Pigolkine (pigolkine@gxm.de)
//
// (C) 2002/3 Ximian, Inc
//

//
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
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class NumericUpDown : UpDownBase, ISupportInitialize {

		private decimal Value_ = 0;
    	private int DecimalPlaces_;
    	private bool Hexadecimal_ = false;
    	private decimal Increment_ = 1;
    	private decimal Maximum_ = 100;
    	private decimal Minimum_ = 0;
		//
		//  --- Constructor
		//
		[MonoTODO]
		public NumericUpDown()
		{
			
		}

		public override void DownButton(){
			if( Value_ > Minimum_) {
				Value = Math.Max(Value_ - Increment_, Minimum_);
			}
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public int DecimalPlaces  {
			get {
				return DecimalPlaces_;
			}
			set {
				//FIXME:
				DecimalPlaces_ = value;
			}
		}

		[MonoTODO]
		public bool Hexadecimal  {
			get {
				return Hexadecimal_;
			}
			set {
				//FIXME:
				Hexadecimal_ = value;
			}
		}

		public decimal Increment {
			get {
				return Increment_;
			} 
			set {
				Increment_ = value;
			}
		}
		
		[MonoTODO]
		public decimal Maximum  {
			get {
				return Maximum_;
			}
			set {
				//FIXME:
				if( Maximum_ != value) {
					Maximum_ = value;
					Minimum = Math.Min(Maximum_,Minimum_);
					Value = Math.Min(Value_,Minimum_);
				}
			}
		}

		[MonoTODO]
		public decimal Minimum  {
			get {
				return Minimum_;
			}
			set {
				//FIXME:
				if( Minimum_ != value) {
					Minimum_ = value;
					Maximum = Math.Max(Maximum_,Minimum_);
					Value = Math.Max(Value_,Minimum_);
				}
			}
		}

		[MonoTODO]
		public override string Text  {
			//FIXME: just to get it to run
			get {
				return Value_.ToString();
			}
			set {
				Value = Decimal.Parse(value);
			}
		}

		[MonoTODO]
		public bool ThousandsSeparator  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public decimal Value  {
			get {
				return Value_;
			}
			set {
				//FIXME:
				if( Value_ != value) {
					Value_ = value;
					UpdateEditText();
				}
			}
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

		[MonoTODO]
		public override void UpButton()
		{
			if( Value_ != Maximum_) {
				Value = Math.Min(Value_ + Increment_, Maximum_);
			}
		}

		//
		//  --- Public Events
		//

		public event EventHandler ValueChanged;

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance() 
		{
			//FIXME:
			return base.CreateAccessibilityInstance();
		}

		[MonoTODO]
		protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
		{
			//FIXME:
			base.OnTextBoxKeyPress(source, e);
		}

		[MonoTODO]
		protected virtual void OnValueChanged(EventArgs e) 
		{
			//FIXME:
		}

		[MonoTODO]
		protected void ParseEditText() 
		{
			//FIXME:
		}

		[MonoTODO]
		protected override void UpdateEditText() 
		{
			//FIXME:
			base.Text = Value_.ToString();
		}

		[MonoTODO]
		protected override void ValidateEditText() 
		{
			//FIXME:
			base.ValidateEditText();
		}

		void ISupportInitialize.BeginInit(){
			//FIXME:
		}

		void ISupportInitialize.EndInit(){
			//FIXME:
		}

	}
}
