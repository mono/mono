//
// System.Windows.Forms.InputLanguageChangeEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partial Completed by Dennis Hayes (dennish@raytek.com)
//  Giananadrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.Globalization;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class InputLanguageChangedEventArgs : EventArgs {

		#region Fields
		private CultureInfo culture;
		private byte charSet;
		private InputLanguage inputLanguage;
		#endregion

		//
		//  --- Constructor
		//
		public InputLanguageChangedEventArgs ( CultureInfo culture, byte charSet) {
			this.culture = culture;
			this.charSet = charSet;
			inputLanguage = InputLanguage.FromCulture(culture);
		}

		public InputLanguageChangedEventArgs ( InputLanguage inputLanguage, byte charSet) {
			this.inputLanguage = inputLanguage;
			this.charSet = charSet;
			culture = this.inputLanguage.Culture;
		}

		#region Public Properties

		public byte CharSet 
		{
			get {
				
				return charSet;
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
				return InputLanguage;
			}
		}

		#endregion
	}
}
