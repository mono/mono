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

// NOT COMPLETE

using System;
using System.Collections;
using System.Text;

namespace System.Windows.Forms {
	public class DataFormats {
		#region DataFormats.Format Subclass
		public class Format {
			#region Local Variables
			private string	name;
			private int	id;
			private Format	next;
			#endregion Local Variables

			#region Public Constructors
			public Format(string name, int ID) {
				this.name = name;
				this.id = ID;
			}

			internal Format(string name, int ID, Format after) : this(name, ID) {
				after.next = this;
			}
			#endregion	// Public Constructors

			#region Public Instance Properties
			public int Id {
				get {
					return this.id;
				}
			}

			public string Name {
				get {
					return this.name;
				}
			}
			#endregion	// Public Instance Properties

			#region Private Methods
			internal static Format Find(Format f, int id) {
				while ((f != null) && (f.Id != id)) {
					f = f.next;
				}
				return f;
			}

			internal static Format Find(Format f, string name) {
				while ((f != null) && (!f.Name.Equals(name))) {
					f = f.next;
				}
				return f;
			}
			#endregion	// Private Methods

		}
		#endregion	// DataFormats.Format Subclass

		#region Local Variables
		private static bool	initialized = false;
		private static Format	formats;
		#endregion	// Local Variables

		#region Constructors
		private DataFormats() {
		}
		#endregion	// Constructors

		#region Public Static Fields
		public static readonly string Bitmap			= "Bitmap";
		public static readonly string CommaSeparatedValue	= "Csv";
		public static readonly string Dib			= "DeviceIndependentBitmap";
		public static readonly string Dif			= "DataInterchangeFormat";
		public static readonly string EnhancedMetafile		= "EnhancedMetafile";
		public static readonly string FileDrop			= "FileDrop";
		public static readonly string Html			= "HTML Format";
		public static readonly string Locale			= "Locale";
		public static readonly string MetafilePict		= "MetaFilePict";
		public static readonly string OemText			= "OEMText";
		public static readonly string Palette			= "Palette";
		public static readonly string PenData			= "PenData";
		public static readonly string Riff			= "RiffAudio";
		public static readonly string Rtf			= "Rich Text Format";
		public static readonly string Serializable		= "WindowsForms10PersistentObject";
		public static readonly string StringFormat		= "System.String";
		public static readonly string SymbolicLink		= "SymbolicLink";
		public static readonly string Text			= "Text";
		public static readonly string Tiff			= "Tiff";
		public static readonly string UnicodeText		= "UnicodeText";
		public static readonly string WaveAudio			= "WaveAudio";
		#endregion	// Public Static Fields

		#region Public Static Methods
		public static Format GetFormat(int ID) {
			if (!initialized) {
				Initialize();
			}

			return Format.Find(formats, ID);
		}

		public static Format GetFormat(string format) {
			if (!initialized) {
				Initialize();
			}

			return Format.Find(formats, format);
		}
		#endregion	// Public Static Methods

		#region Private Methods
		private static void Initialize() {
			lock (typeof(DataFormats.Format)) {
				if (!initialized) {
					Format	f;
					IntPtr	cliphandle;

					cliphandle = XplatUI.ClipboardOpen();
					formats = new DataFormats.Format(Text, 1);
					f = new Format(Bitmap, 2, formats);
					f = new Format(MetafilePict, 3, f);
					f = new Format(SymbolicLink, 4, f);
					f = new Format(Dif, 5, f);
					f = new Format(Tiff, 6, f);
					f = new Format(OemText, 7, f);
					f = new Format(Dib, 8, f);
					f = new Format(Palette, 9, f);
					f = new Format(PenData, 10, f);
					f = new Format(Riff, 11, f);
					f = new Format(WaveAudio, 12, f);
					f = new Format(UnicodeText, 13, f);
					f = new Format(EnhancedMetafile, 14, f);
					f = new Format(FileDrop, 15, f);
					f = new Format(Locale, 16, f);

					f = new Format(CommaSeparatedValue, XplatUI.ClipboardGetID(cliphandle, CommaSeparatedValue), f);
					f = new Format(Html, XplatUI.ClipboardGetID(cliphandle, Html), f);
					f = new Format(Rtf, XplatUI.ClipboardGetID(cliphandle, Rtf), f);
					f = new Format(Serializable, XplatUI.ClipboardGetID(cliphandle, Serializable), f);
					f = new Format(StringFormat, XplatUI.ClipboardGetID(cliphandle, StringFormat), f);

					XplatUI.ClipboardClose(cliphandle);
					
				}
				initialized = true;
			}
		}
		#endregion	// Private Methods
	}
}
