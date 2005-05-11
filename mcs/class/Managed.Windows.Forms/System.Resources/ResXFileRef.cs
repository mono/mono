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

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace System.Resources {
	[Serializable]
	[TypeConverter(typeof(ResXFileRef.Converter))]
	public class ResXFileRef {
		#region Converter Class
		public class Converter : TypeConverter {
			#region Constructors
			public Converter() {
			}
			#endregion	// Constructors

			#region Methods
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
				return sourceType == typeof(string);
			}

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
				return destinationType == typeof(string);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
				string[]	parts;
				byte[]		buffer;

				if ( !(value is String)) {
					return base.ConvertFrom(context, culture, value);
				}

				parts = ((string)value).Split(';');

				using (FileStream file = new FileStream(parts[0], FileMode.Open, FileAccess.Read, FileShare.Read)) {
					buffer = new byte[file.Length];

					file.Read(buffer, 0, (int)file.Length);
				}

				return Activator.CreateInstance(Type.GetType(parts[1]), BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, new object[] { new MemoryStream(buffer) }, culture);
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
				if (destinationType != typeof(String)) {
					return base.ConvertTo (context, culture, value, destinationType);
				}

				return ((ResXFileRef)value).ToString();
			}
			#endregion	// Methods
		}
		#endregion	// Converter Class

		#region Local Variables
		private string filename;
		private string typename;
		#endregion	// Local Variables

		#region Public Constructors
		public ResXFileRef(string fileName, string typeName) {
			this.filename = fileName;
			this.typename = typeName;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override string ToString() {
			return filename + ";" + typename;
		}
		#endregion	// Public Instance Methods
	}
}
