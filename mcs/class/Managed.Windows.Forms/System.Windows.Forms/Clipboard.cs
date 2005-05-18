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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	public sealed class Clipboard {
		#region Local Variables
		#endregion	// Local Variables

		#region Constructors
		private Clipboard() {
		}
		#endregion	// Constructors

		#region	Private Methods
		private static bool ConvertToClipboardData(ref int type, object obj, out byte[] data) {
			data = null;
			return false;
		}

		private static bool ConvertFromClipboardData(int type, IntPtr data, out object obj) {
			obj = null;
			if (data == IntPtr.Zero) {
				return false;
			}
			return false;
		}
		#endregion	// Private Methods

		#region Public Static Methods
		[MonoTODO]
		public static IDataObject GetDataObject() {
			DataObject		clipboard;
			IntPtr			clipboard_handle;
			IntPtr			retrieve_handle;
			int[]			native_formats;
			DataFormats.Format	item_format;
			object			managed_clipboard_item;
			XplatUI.ClipboardToObject converter;

			converter = new XplatUI.ClipboardToObject(ConvertFromClipboardData);

			clipboard_handle = XplatUI.ClipboardOpen();
			native_formats = XplatUI.ClipboardAvailableFormats(clipboard_handle);
			if (native_formats == null) {
				return null;	// Clipboard empty
			}

			// Build the IDataObject
			clipboard = new DataObject();
			for (int i = 0; i < native_formats.Length; i++) {
				// We might get a format we don't understand or know
				item_format = DataFormats.GetFormat(native_formats[i]);
				if (item_format != null) {
					managed_clipboard_item = XplatUI.ClipboardRetrieve(clipboard_handle, native_formats[i], converter);

					if (managed_clipboard_item != null) {
						clipboard.SetData(item_format.Name, managed_clipboard_item);
						// We don't handle 'bitmap' since it involves handles, so we'll equate it to dib
						if (item_format.Name == DataFormats.Dib) {
							clipboard.SetData(DataFormats.Bitmap, managed_clipboard_item);
						}
					}
				}
			}

			XplatUI.ClipboardClose(clipboard_handle);

			return clipboard;
		}

		public static void SetDataObject(object data) {
			SetDataObject(data, true);
			
		}

		public static void SetDataObject(object data, bool copy) {
			IntPtr			clipboard_handle;
			XplatUI.ObjectToClipboard converter;
			int			native_format;
			DataFormats.Format	item_format;

			converter = new XplatUI.ObjectToClipboard(ConvertToClipboardData);

			clipboard_handle = XplatUI.ClipboardOpen();

			native_format = -1;

			item_format = DataFormats.Format.Find(data.GetType().FullName);
			if ((item_format != null) && (item_format.Name != DataFormats.StringFormat)) {
				native_format = item_format.Id;
			}

			XplatUI.ClipboardStore(clipboard_handle, data, native_format, converter);
			XplatUI.ClipboardClose(clipboard_handle);
		}
		#endregion	// Public Static Methods
	}
}
