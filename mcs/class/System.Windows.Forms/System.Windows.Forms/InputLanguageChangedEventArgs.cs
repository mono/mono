//
// System.Windows.Forms.InputLanguageChangeEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partial Completed by Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System.Globalization;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class InputLanguageChangedEventArgs : EventArgs {
		private CultureInfo culture;
		private byte b;

		//
		//  --- Constructor
		//
		[MonoTODO]
		public InputLanguageChangedEventArgs ( CultureInfo culture, byte b) {
			this.culture = culture;
			this.b = b;
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public byte CharSet {
			get {
				throw new NotImplementedException ();
			}
		}
		public CultureInfo Culture {
			get {
				return culture;
			}
		}
		[MonoTODO]
		public InputLanguage InputLanguage {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2) {
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
	}
}
