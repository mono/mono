//
// System.Windows.Forms.InputLanguageChangingEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class InputLanguageChangingEventArgs : EventArgs {
		private CultureInfo culture;
		private bool systemcharset;
		private InputLanguage inputlanguage;
		//
		//  --- Constructor
		//
		[MonoTODO] //what about input lang?
		public InputLanguageChangingEventArgs(CultureInfo culture, bool sysCharSet) {
			this.culture = culture;
			this.systemcharset =sysCharSet;
		}

		[MonoTODO] //what about culture?
		public InputLanguageChangingEventArgs(InputLanguage inputlanguage, bool sysCharSet) {
			this.culture = culture;
			this.inputlanguage = inputlanguage;
		}

		//
		//  --- Public Properties
		//
		public CultureInfo Culture {
			get {
				return culture;
			}
		}
		public InputLanguage InputLanguage {
			get {
				return inputlanguage;
			}
		}
		public bool SysCharSet {
			get {
				return systemcharset;
			}
		}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
