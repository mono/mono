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
