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
				return inputLanguage;
			}
		}

		#endregion
	}
}
