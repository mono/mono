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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

using System.Collections;
using System.ComponentModel;
using System.Text;

namespace System.Windows.Forms {
	public class KeysConverter : TypeConverter, IComparer {
		#region Public Constructors
		public KeysConverter() {
		}
		#endregion	// Public Constructors

		#region Public Instance Methods
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}
			return false;
		}

		public int Compare(object a, object b) {
			if (a is string && b is string) {
				return String.Compare((string) a, (string)b);
			}
			return String.Compare(a.ToString(), b.ToString());
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if (value is string) {
				string[]	keys;
				Keys		key;

				keys = ((string)value).Split(new char[] {'+'});
				key = Keys.None;

				if (keys.Length > 1) {
					for (int i = 0; i < keys.Length - 1; i++) {
						if (keys[i].Equals("Ctrl")) {
							key |= Keys.Control;
						} else {
							key |= (Keys)Enum.Parse(typeof(Keys), keys[i], true);
						}
					}
				}
				key |= (Keys)Enum.Parse(typeof(Keys), keys[keys.Length - 1], true);
				return key;
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				StringBuilder	sb;
				Keys		key;

				sb = new StringBuilder();
				key = (Keys)value;

				// Modifiers first
				if ((key & Keys.Control) != 0) {
					sb.Append("Ctrl+");
				}

				if ((key & Keys.Alt) != 0) {
					sb.Append("Alt+");
				}

				if ((key & Keys.Shift) != 0) {
					sb.Append("Shift+");
				}

				// Keycode last
				sb.Append(Enum.GetName(typeof(Keys), key & Keys.KeyCode));

				return sb.ToString();
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
			string [] stdVal = new string [] {
				"Alt", "Back", "Control", "Delete", "End", "Enter", "F1", "F10", "F11", "F12", "F2",
				"F3", "F4", "F5", "F6", "F7", "F8", "F9", "Home", "Insert", "Next", "Prior", "Shift"};

			return new TypeConverter.StandardValuesCollection (stdVal);
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
			
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
			return true;
		}

		#endregion	// Public Instance Methods
	}
}
