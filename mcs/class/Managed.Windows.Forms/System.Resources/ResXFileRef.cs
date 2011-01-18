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
//	Gert Driesen	(drieseng@users.sourceforge.net)
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.Resources {
	[Serializable]
	[TypeConverter(typeof(ResXFileRef.Converter))]
#if INSIDE_SYSTEM_WEB
	internal
#else
	public 
#endif
	class ResXFileRef {
#if INSIDE_SYSTEM_WEB
		internal
#else
		public
#endif
		class Converter : TypeConverter {
			public Converter() {
			}

			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
				return sourceType == typeof(string);
			}

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
				return destinationType == typeof(string);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
				byte[]		buffer;

				if ( !(value is String)) {
					return null;
				}

				string [] parts = ResXFileRef.Parse ((string) value);
				if (parts.Length == 1)
					throw new ArgumentException ("value");

				Type type = Type.GetType (parts [1]);
				if (type == typeof(string)) {
					Encoding encoding;
					if (parts.Length > 2) {
						encoding = Encoding.GetEncoding (parts [2]);
					} else {
						encoding = Encoding.Default;
					}

					using (TextReader reader = new StreamReader(parts [0], encoding)) {
						return reader.ReadToEnd();
					}
				}

				using (FileStream file = new FileStream (parts [0], FileMode.Open, FileAccess.Read, FileShare.Read)) {
					buffer = new byte [file.Length];
					file.Read(buffer, 0, (int) file.Length);
				}

				if (type == typeof(System.Byte[]))
					return buffer;

				if (type == typeof (Bitmap) && Path.GetExtension (parts [0]) == ".ico") {
					MemoryStream ms = new MemoryStream (buffer);
					return new Icon (ms).ToBitmap ();
				}

				if (type == typeof (MemoryStream))
					return new MemoryStream (buffer);

				return Activator.CreateInstance(type, BindingFlags.CreateInstance
					| BindingFlags.Public | BindingFlags.Instance, null, 
					new object[] { new MemoryStream (buffer) }, culture);
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
				if (destinationType != typeof(String)) {
					return base.ConvertTo (context, culture, value, destinationType);
				}

				return ((ResXFileRef)value).ToString();
			}
		}

		private string filename;
		private string typename;
		private Encoding textFileEncoding;

		public ResXFileRef (string fileName, string typeName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			this.filename = fileName;
			this.typename = typeName;
		}

		public ResXFileRef (string fileName, string typeName, Encoding textFileEncoding)
			: this (fileName, typeName) 
		{
			this.textFileEncoding = textFileEncoding;
		}

		public string FileName {
			get { return filename; }
		}

		public Encoding TextFileEncoding {
			get { return textFileEncoding; }
		}

		public string TypeName {
			get { return typename; }
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder ();
			if (filename != null) {
				sb.Append (filename);
			}
			sb.Append (';');
			if (typename != null) {
				sb.Append (typename);
			}
			if (textFileEncoding != null) {
				sb.Append (';');
				sb.Append (textFileEncoding.WebName);
			}
			return sb.ToString ();
		}

		internal static string [] Parse (string fileRef)
		{
			// we cannot return ResXFileRef, as that would mean we'd have to
			// instantiate the encoding, and we do not always need this

			if (fileRef == null)
				throw new ArgumentNullException ("fileRef");

			return fileRef.Split (';');
		}
	}
}
