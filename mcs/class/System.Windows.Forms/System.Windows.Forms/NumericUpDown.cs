//
// System.Windows.Forms.NumericUpDown.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class NumericUpDown : UpDownBase, ISupportInitialize {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public NumericUpDown()
		{
			
		}

		public override void DownButton(){
			throw new NotImplementedException ();
		}
		//
		//  --- Public Properties
		//

		[MonoTODO]
		public int DecimalPlaces  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool Hexadecimal  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public decimal Maximum  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public decimal Minimum  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string Text  {
			//FIXME: just to get it to run
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		[MonoTODO]
		public bool ThousandsSeparator  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public decimal Value  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void UpButton()
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnValueChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void ParseEditText() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void UpdateEditText() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ValidateEditText() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		void ISupportInitialize.BeginInit(){
			//FIXME:
		}

		void ISupportInitialize.EndInit(){
			//FIXME:
		}

	}
}
