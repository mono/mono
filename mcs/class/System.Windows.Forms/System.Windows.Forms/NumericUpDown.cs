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
			//FIXME:
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
				//FIXME:
			}
		}

		[MonoTODO]
		public bool Hexadecimal  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public decimal Maximum  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public decimal Minimum  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
				//FIXME:
			}
		}

		[MonoTODO]
		public decimal Value  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
			//FIXME:
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
