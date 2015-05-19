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
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[ClassInterface(ClassInterfaceType.None)]
	public class DataObject : IDataObject, System.Runtime.InteropServices.ComTypes.IDataObject
	 {
		#region DataObject.Entry Class
		private class Entry {
			#region Local Variables
			private string	type;
			private object	data;
			private bool	autoconvert;
			internal Entry	next;
			#endregion	// Local Variables

			#region Constructors
			internal Entry(string type, object data, bool autoconvert) {
				this.type = type;
				this.data = data;
				this.autoconvert = autoconvert;
			}
			#endregion	// Constructors

			#region Properties
			public object Data {
				get {
					return data;
				}

				set {
					data = value;
				}
			}
			public bool AutoConvert {
				get { 
					return autoconvert;
				}
				set {
					autoconvert = value;
				}
			}
			#endregion	// Properties

			#region Methods
			public static int Count(Entry entries) {
				int	result;

				result = 0;

				while (entries != null) {
					result++;
					entries = entries.next;
				}

				return result;
			}

			public static Entry Find (Entry entries, string type) {
				return Find (entries, type, false);
			}

			public static Entry Find(Entry entries, string type, bool only_convertible) {
				while (entries != null) {
					bool available = true;
					if (only_convertible && !entries.autoconvert)
						available = false;
					if (available && String.Compare (entries.type, type, true) == 0) {
						return entries;
					}
					entries = entries.next;
				}

				return null;
			}

			public static Entry FindConvertible(Entry entries, string type) {
				Entry e;

				e = Find(entries, type);
				if (e != null) {
					return e;
				}

				// map to *any* other text format if needed
				if (type == DataFormats.StringFormat || type == DataFormats.Text || type == DataFormats.UnicodeText) {
					e = entries;
					while (e != null) {
						if (e.type == DataFormats.StringFormat || e.type == DataFormats.Text || e.type == DataFormats.UnicodeText)
							return e;

						e = e.next;
					}
				}

				return null;
			}

			public static string[] Entries(Entry entries, bool convertible) {
				Entry		e;
				ArrayList	list;
				string[]	result;

				// Initially store into something that we can grow easily
				list = new ArrayList(Entry.Count(entries));
				e = entries;

				if (convertible) {
					// Add the convertibles
					Entry text_entry = Entry.Find (entries, DataFormats.Text);
					Entry utext_entry = Entry.Find (entries, DataFormats.UnicodeText);
					Entry string_entry = Entry.Find (entries, DataFormats.StringFormat);
					bool text_convertible = text_entry != null && text_entry.AutoConvert;
					bool utext_convertible = utext_entry != null && utext_entry.AutoConvert;
					bool string_convertible = string_entry != null && string_entry.AutoConvert;

					if (text_convertible || utext_convertible || string_convertible) {
						list.Add (DataFormats.StringFormat);
						list.Add (DataFormats.UnicodeText);
						list.Add (DataFormats.Text);
					}
				}

				while (e != null) {
					if (!list.Contains (e.type))
						list.Add (e.type);
					e = e.next;
				}

				// Copy the results into a string array
				result = new string[list.Count];
				for (int i = 0; i < list.Count; i++) {
					result[i] = (string)list[i];
				}

				return result;
			}
			#endregion	// Methods
		}
		#endregion	// DataObject.Entry class

		#region Local Variables
		private Entry	entries;
		#endregion	// Local Variables

		#region Public Constructors
		public DataObject() {
			entries = null;
		}

		public DataObject(object data) {
			SetData(data);
		}

		public DataObject(string format, object data) {
			SetData(format, data);
		}
		#endregion	// Public Constructors

		#region Public Instance Methods
		public virtual bool ContainsAudio ()
		{
			return GetDataPresent (DataFormats.WaveAudio, true);
		}
		
		public virtual bool ContainsFileDropList ()
		{
			return GetDataPresent (DataFormats.FileDrop, true);
		}
		
		public virtual bool ContainsImage ()
		{
			return GetDataPresent (DataFormats.Bitmap, true);
		}
		
		public virtual bool ContainsText ()
		{
			return GetDataPresent (DataFormats.UnicodeText, true);
		}
		
		public virtual bool ContainsText (TextDataFormat format)
		{
			if (!Enum.IsDefined (typeof (TextDataFormat), format))
				throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for TextDataFormat", format));

			return GetDataPresent (TextFormatToDataFormat (format), true);
		}
		
		public virtual Stream GetAudioStream ()
		{
			return (Stream)GetData (DataFormats.WaveAudio, true);
		}

		public virtual object GetData(string format) {
			return GetData(format, true);
		}

		public virtual object GetData(string format, bool autoConvert) {
			Entry e;
			if (autoConvert) {
				e = Entry.FindConvertible(entries, format);
			} else {
				e = Entry.Find(entries, format);
			}
			if (e == null)
				return null;
			return e.Data;
		}

		public virtual object GetData(Type format) {
			return GetData(format.FullName, true);
		}

		public virtual bool GetDataPresent(string format) {
			return GetDataPresent(format, true);
		}

		public virtual bool GetDataPresent(string format, bool autoConvert) {
			if (autoConvert) {
				return Entry.FindConvertible(entries, format) != null;
			} else {
				return Entry.Find(entries, format) != null;
			}
		}

		public virtual bool GetDataPresent(Type format) {
			return GetDataPresent(format.FullName, true);
		}

		public virtual StringCollection GetFileDropList ()
		{
			return (StringCollection)GetData (DataFormats.FileDrop, true);
		}
		public virtual string[] GetFormats() {
			return GetFormats(true);
		}

		public virtual string[] GetFormats(bool autoConvert) {
			return Entry.Entries(entries, autoConvert);
		}

		public virtual Image GetImage ()
		{
			return (Image)GetData (DataFormats.Bitmap, true);
		}

		public virtual string GetText ()
		{
			return (string)GetData (DataFormats.UnicodeText, true);
		}

		public virtual string GetText (TextDataFormat format)
		{
			if (!Enum.IsDefined (typeof (TextDataFormat), format))
				throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for TextDataFormat", format));

			return (string)GetData (TextFormatToDataFormat (format), false);
		}

		public virtual void SetAudio (byte[] audioBytes)
		{
			if (audioBytes == null)
				throw new ArgumentNullException ("audioBytes");

			MemoryStream ms = new MemoryStream (audioBytes);

			SetAudio (ms);
		}

		public virtual void SetAudio (Stream audioStream)
		{
			if (audioStream == null)
				throw new ArgumentNullException ("audioStream");

			SetData (DataFormats.WaveAudio, audioStream);
		}

		public virtual void SetData(object data) {
			SetData(data.GetType(), data); 
		}

		public virtual void SetData(string format, bool autoConvert, object data) {
			Entry	entry;
			Entry	e;

			entry = Entry.Find(entries, format);

			if (entry == null) {
				entry = new DataObject.Entry(format, data, autoConvert);
			} else {
				entry.Data = data;
				return;
			}

			lock (this) {
				if (entries == null) {
					entries = entry;
				} else {
					// Insert into the list of known/defined formats
					e = entries;

					while (e.next != null) {
						e = e.next;
					}
					e.next = entry;
				}
			}
		}

		public virtual void SetData(string format, object data) {
			SetData(format, true, data);
		}

		public virtual void SetData(Type format, object data) {
			SetData(EnsureFormat(format), true, data);
		}
		
		[MonoInternalNote ("Needs additional checks for valid paths, see MSDN")]
		public virtual void SetFileDropList (StringCollection filePaths)
		{
			if (filePaths == null)
				throw new ArgumentNullException ("filePaths");

			SetData (DataFormats.FileDrop, filePaths);
		}

		public virtual void SetImage (Image image)
		{
			if (image == null)
				throw new ArgumentNullException ("image");

			SetData (DataFormats.Bitmap, image);
		}

		public virtual void SetText (string textData)
		{
			if (string.IsNullOrEmpty (textData))
				throw new ArgumentNullException ("text");

			SetData (DataFormats.UnicodeText, textData);
		}

		public virtual void SetText (string textData, TextDataFormat format)
		{
			if (string.IsNullOrEmpty (textData))
				throw new ArgumentNullException ("text");
			if (!Enum.IsDefined (typeof (TextDataFormat), format))
				throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for TextDataFormat", format));

			switch (format) {
				case TextDataFormat.Text:
					SetData (DataFormats.Text, textData);
					break;
				case TextDataFormat.UnicodeText:
					SetData (DataFormats.UnicodeText, textData);
					break;
				case TextDataFormat.Rtf:
					SetData (DataFormats.Rtf, textData);
					break;
				case TextDataFormat.Html:
					SetData (DataFormats.Html, textData);
					break;
				case TextDataFormat.CommaSeparatedValue:
					SetData (DataFormats.CommaSeparatedValue, textData);
					break;
			}
		}
		#endregion	// Public Instance Methods

		#region Private Methods
		internal string EnsureFormat(string name) {
			DataFormats.Format f;

			f = DataFormats.Format.Find(name);
			if (f == null) {
				// Register the format
				f = DataFormats.Format.Add(name);
			}

			return f.Name;
		}

		internal string EnsureFormat(Type type) {
			return EnsureFormat(type.FullName);
		}

		private string TextFormatToDataFormat (TextDataFormat format)
		{
			switch (format) {
				case TextDataFormat.Text:
				default:
					return DataFormats.Text;
				case TextDataFormat.UnicodeText:
					return DataFormats.UnicodeText;
				case TextDataFormat.Rtf:
					return DataFormats.Rtf;
				case TextDataFormat.Html:
					return DataFormats.Html;
				case TextDataFormat.CommaSeparatedValue:
					return DataFormats.CommaSeparatedValue;
			}
		}
		#endregion	// Private Methods

		#region IDataObject Members
		int System.Runtime.InteropServices.ComTypes.IDataObject.DAdvise (ref System.Runtime.InteropServices.ComTypes.FORMATETC pFormatetc, System.Runtime.InteropServices.ComTypes.ADVF advf, System.Runtime.InteropServices.ComTypes.IAdviseSink adviseSink, out int connection)
		{
			throw new NotImplementedException ();
		}

		void System.Runtime.InteropServices.ComTypes.IDataObject.DUnadvise (int connection)
		{
			throw new NotImplementedException ();
		}

		int System.Runtime.InteropServices.ComTypes.IDataObject.EnumDAdvise (out System.Runtime.InteropServices.ComTypes.IEnumSTATDATA enumAdvise)
		{
			throw new NotImplementedException ();
		}

		System.Runtime.InteropServices.ComTypes.IEnumFORMATETC System.Runtime.InteropServices.ComTypes.IDataObject.EnumFormatEtc (System.Runtime.InteropServices.ComTypes.DATADIR direction)
		{
			throw new NotImplementedException ();
		}

		int System.Runtime.InteropServices.ComTypes.IDataObject.GetCanonicalFormatEtc (ref System.Runtime.InteropServices.ComTypes.FORMATETC formatIn, out System.Runtime.InteropServices.ComTypes.FORMATETC formatOut)
		{
			throw new NotImplementedException ();
		}

		void System.Runtime.InteropServices.ComTypes.IDataObject.GetData (ref System.Runtime.InteropServices.ComTypes.FORMATETC format, out System.Runtime.InteropServices.ComTypes.STGMEDIUM medium)
		{
			throw new NotImplementedException ();
		}

		void System.Runtime.InteropServices.ComTypes.IDataObject.GetDataHere (ref System.Runtime.InteropServices.ComTypes.FORMATETC format, ref System.Runtime.InteropServices.ComTypes.STGMEDIUM medium)
		{
			throw new NotImplementedException ();
		}

		int System.Runtime.InteropServices.ComTypes.IDataObject.QueryGetData (ref System.Runtime.InteropServices.ComTypes.FORMATETC format)
		{
			throw new NotImplementedException ();
		}

		void System.Runtime.InteropServices.ComTypes.IDataObject.SetData (ref System.Runtime.InteropServices.ComTypes.FORMATETC formatIn, ref System.Runtime.InteropServices.ComTypes.STGMEDIUM medium, bool release)
		{
			throw new NotImplementedException ();
		}
		#endregion
	 }
}
