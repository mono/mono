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
using System.Collections;
using System.Text;

namespace System.Windows.Forms {
	public class DataFormats {
		#region DataFormats.Format Subclass
		public class Format {
			#region Local Variables
			private static Format	formats;
			private string		name;
			private int		id;
			private Format		next;
			#endregion Local Variables

			#region Public Constructors
			public Format(string name, int ID) {
				Format	f;

				this.name = name;
				this.id = ID;

				lock (typeof(DataFormats.Format)) {
					if (formats == null) {
						formats = this;
					} else {
						// Insert into the list of known/defined formats
						f = formats;

						while (f.next != null) {
							f = f.next;
						}
						f.next = this;
					}
				}
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

			internal Format Next {
				get {
					return this.next;
				}
			}
			#endregion	// Public Instance Properties

			#region Private Methods
			internal static Format Add(string name) {
				Format f;

				f = Find(name);
				if (f == null) {
					IntPtr cliphandle;

					cliphandle = XplatUI.ClipboardOpen();
					f = new Format(name, XplatUI.ClipboardGetID(cliphandle, name));
					XplatUI.ClipboardClose(cliphandle);
				}
				return f;
			}

			internal static Format Add(int id) {
				Format f;

				f = Find(id);
				if (f == null) {
					f = new Format("Format"+id.ToString(), id);
				}
				return f;
			}

			internal static Format Find(int id) {
				Format f;

				f = formats;
				while ((f != null) && (f.Id != id)) {
					f = f.next;
				}
				return f;
			}

			internal static Format Find(string name) {
				Format f;

				f = formats;
				while ((f != null) && (!f.Name.Equals(name))) {
					f = f.next;
				}
				return f;
			}

			internal static Format List {
				get {
					return formats;
				}
			}
			#endregion	// Private Methods

		}
		#endregion	// DataFormats.Format Subclass

		#region Local Variables
		private static bool	initialized = false;
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

			return Format.Find(ID);
		}

		public static Format GetFormat(string format) {
			if (!initialized) {
				Initialize();
			}

			return Format.Add(format);
		}
		#endregion	// Public Static Methods

		#region Private Methods
		private static void Initialize() {
			lock (typeof(DataFormats.Format)) {
				if (!initialized) {
					IntPtr	cliphandle;

					cliphandle = XplatUI.ClipboardOpen();

					new Format(Text, XplatUI.ClipboardGetID(cliphandle, Text));
					new Format(Bitmap, XplatUI.ClipboardGetID(cliphandle, Bitmap));
					new Format(MetafilePict, XplatUI.ClipboardGetID(cliphandle, MetafilePict));
					new Format(SymbolicLink, XplatUI.ClipboardGetID(cliphandle, SymbolicLink));
					new Format(Dif, XplatUI.ClipboardGetID(cliphandle, Dif)) ;
					new Format(Tiff, XplatUI.ClipboardGetID(cliphandle, Tiff));
					new Format(OemText, XplatUI.ClipboardGetID(cliphandle, OemText));
					new Format(Dib, XplatUI.ClipboardGetID(cliphandle, Dib));
					new Format(Palette, XplatUI.ClipboardGetID(cliphandle, Palette));
					new Format(PenData, XplatUI.ClipboardGetID(cliphandle, PenData));
					new Format(Riff, XplatUI.ClipboardGetID(cliphandle, Riff));
					new Format(WaveAudio, XplatUI.ClipboardGetID(cliphandle, WaveAudio));
					new Format(UnicodeText, XplatUI.ClipboardGetID(cliphandle, UnicodeText));
					new Format(EnhancedMetafile, XplatUI.ClipboardGetID(cliphandle, EnhancedMetafile));
					new Format(FileDrop, XplatUI.ClipboardGetID(cliphandle, FileDrop));
					new Format(Locale, XplatUI.ClipboardGetID(cliphandle, Locale));
					new Format(CommaSeparatedValue, XplatUI.ClipboardGetID(cliphandle, CommaSeparatedValue));
					new Format(Html, XplatUI.ClipboardGetID(cliphandle, Html));
					new Format(Rtf, XplatUI.ClipboardGetID(cliphandle, Rtf));
					new Format(Serializable, XplatUI.ClipboardGetID(cliphandle, Serializable));
					new Format(StringFormat, XplatUI.ClipboardGetID(cliphandle, StringFormat));

					XplatUI.ClipboardClose(cliphandle);
					
				}
				initialized = true;
			}
		}
		#endregion	// Private Methods
	}
}
