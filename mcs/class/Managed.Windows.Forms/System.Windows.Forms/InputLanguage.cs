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
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: InputLanguage.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System.Globalization;

namespace System.Windows.Forms {
	public sealed class InputLanguage {
		internal static InputLanguageCollection	all;
		private IntPtr			handle;
		private CultureInfo		culture;
		private string			layout_name;
		private static InputLanguage	current_input;
		private static InputLanguage	default_input;

		#region Private Constructor
		[MonoTODO("Pull Microsofts InputLanguages and enter them here")]
		internal InputLanguage() {
			if (all == null) {
				all = new InputLanguageCollection();

				all.Add(new InputLanguage(IntPtr.Zero, new CultureInfo(""), "US"));
			}
			if (default_input == null) {
				default_input=InputLanguage.FromCulture(CultureInfo.CurrentUICulture);
			}

			if (current_input == null) {
				current_input=InputLanguage.FromCulture(CultureInfo.CurrentUICulture);
			}
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
				return current_input;
			}

			set {
				if (all.Contains(value)) {
					current_input=value;
				}
			}
		}

		public static InputLanguage DefaultInputLanguage {
			get {
				return default_input;
			}
		}

		public static InputLanguageCollection InstalledInputLanguages {
			get {
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
			foreach (InputLanguage c in all) {
				if (culture.EnglishName==c.culture.EnglishName) {
					return new InputLanguage(c.handle, c.culture, c.layout_name);
				}
			}

			return new InputLanguage(all[0].handle, all[0].culture, all[0].layout_name);
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
