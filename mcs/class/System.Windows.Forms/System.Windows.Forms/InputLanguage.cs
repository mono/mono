//
// System.Windows.Forms.InputLanguage.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
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

    public sealed class InputLanguage {

		private InputLanguage(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public CultureInfo Culture {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public static InputLanguage CurrentInputLanguage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public static InputLanguage DefaultInputLanguage {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public IntPtr Handle {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public static InputLanguageCollection InstalledInputLanguages {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string LayoutName {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object value)
		{
			//FIXME:
			return base.Equals(value);
		}

		[MonoTODO]
		public static InputLanguage FromCulture(CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override int GetHashCode()
		{
			//FIXME:
			return base.GetHashCode();
		}
	 }
}
