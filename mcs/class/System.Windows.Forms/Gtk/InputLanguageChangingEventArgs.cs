//
// System.Windows.Forms.InputLanguageChangingEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.Globalization;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class InputLanguageChangingEventArgs : CancelEventArgs {

		#region Fields
		private CultureInfo culture;
		private bool systemcharset;
		private InputLanguage inputlanguage;
		#endregion

		//
		//  --- Constructor
		//

		public InputLanguageChangingEventArgs(CultureInfo culture, bool sysCharSet) {
			this.culture = culture;
			this.systemcharset =sysCharSet;
		}

		public InputLanguageChangingEventArgs(InputLanguage inputlanguage, bool sysCharSet) {
			this.culture = culture;
			this.inputlanguage = inputlanguage;
		}

		#region Public Properties

		public CultureInfo Culture 
		{
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

		#endregion
	}
}
