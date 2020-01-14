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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// NOT COMPLETE

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	public sealed class InputLanguage {
		private static InputLanguageCollection	all;
		private IntPtr			handle;
		private CultureInfo		culture;
		private string			layout_name;
		private static InputLanguage	current_input;
		private static InputLanguage	default_input;


		#region user32.dll functions
		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern IntPtr GetKeyboardLayout(UInt32 idThread);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern Int32 GetKeyboardLayoutList(Int32 nBuff, IntPtr[] lpList);
		#endregion	// user32.dll functions

		#region Private Constructor
		[MonoInternalNote ("Pull Microsofts InputLanguages and enter them here")]
		internal InputLanguage()
		{
		}

		internal InputLanguage(IntPtr handle, CultureInfo culture, string layout_name) : this() {
			this.handle=handle;
			this.culture=culture;
			this.layout_name=layout_name;
		}
		#endregion	// Private Constructor

		#region Public Static Properties
		public static InputLanguage CurrentInputLanguage {
			get {
				if (current_input == null)
					current_input = InputLanguage.FromCulture (CultureInfo.CurrentUICulture);
				return current_input;
			}

			set {
				if (InstalledInputLanguages.Contains (value))
					current_input = value;
			}
		}

		public static InputLanguage DefaultInputLanguage {
			get {
				if (default_input == null)
					default_input = InputLanguage.FromCulture (CultureInfo.CurrentUICulture);

				return default_input;
			}
		}

		public static InputLanguageCollection InstalledInputLanguages {
			get {
				if (all == null) {
					List<InputLanguage> inputLanguageList = new List<InputLanguage>();
					if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
						var keyboardLayoutCount = GetKeyboardLayoutList (0, null);
						var keyboardLayoutIds = new IntPtr[keyboardLayoutCount];
						GetKeyboardLayoutList (keyboardLayoutCount, keyboardLayoutIds);

						foreach (var keyboardLayoutId in keyboardLayoutIds) {
							var languageId = (Int32)keyboardLayoutId & 0xFFFF;
							CultureInfo cultureInfo = CultureInfo.GetCultureInfo (languageId);
							inputLanguageList.Add (new InputLanguage (IntPtr.Zero, cultureInfo, cultureInfo.Name));
						}
					}
					if (inputLanguageList.Count == 0)
						inputLanguageList.Add (new InputLanguage (IntPtr.Zero, new CultureInfo(string.Empty), "US"));
					all = new InputLanguageCollection (inputLanguageList.ToArray());
				}
				return all;
			}
		}
		#endregion	// Public Static Properties

		#region Public Instance Properties
		public CultureInfo Culture {
			get {
				return this.culture;
			}
		}

		public IntPtr Handle {
			get {
				return this.handle;
			}
		}

		public string LayoutName {
			get {
				return this.layout_name;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static InputLanguage FromCulture(System.Globalization.CultureInfo culture) {
			foreach (InputLanguage c in InstalledInputLanguages) {
				if (culture.EnglishName==c.culture.EnglishName) {
					return new InputLanguage(c.handle, c.culture, c.layout_name);
				}
			}

			return new InputLanguage (InstalledInputLanguages[0].handle, InstalledInputLanguages[0].culture, InstalledInputLanguages[0].layout_name);
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public override bool Equals(object value) {
			if (value is InputLanguage) {
				if ((((InputLanguage)value).culture==this.culture) && (((InputLanguage)value).handle==this.handle) && (((InputLanguage)value).layout_name==this.layout_name)) {
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		#endregion	// Public Instance Methods
	}
}
