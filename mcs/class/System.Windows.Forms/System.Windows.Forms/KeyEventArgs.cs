//
// System.Windows.Forms.KeyEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class KeyEventArgs : EventArgs {
		private Keys keydata;
		//
		//  --- Constructor
		//
		public KeyEventArgs ( Keys keyData)
		{
			keydata = keyData;
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public virtual bool Alt {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Control {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Handled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Keys KeyCode {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Keys KeyData {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int KeyValue {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Keys Modifiers {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Shift {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public virtual bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static bool Equals(object o1, object o2)
		{
			throw new NotImplementedException ();
		}
	 }
}
