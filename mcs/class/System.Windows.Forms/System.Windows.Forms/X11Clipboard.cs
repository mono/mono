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
// Copyright (c) 2009 Novell, Inc.
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Forms {

	internal class ClipboardData {
		ListDictionary source_data;			// Source in its different formats, if any
		string plain_text_source;			// Cached source as plain-text string
		Image image_source;				// Cached source as image

		internal object		Item;			// Object on the clipboard
		internal ArrayList	Formats;		// list of formats available in the clipboard
		internal bool		Retrieving;		// true if we are requesting an item
		internal bool		Enumerating;		// true if we are enumerating through all known types
		internal XplatUI.ObjectToClipboard Converter;

		public ClipboardData ()
		{
			source_data = new ListDictionary ();
		}

		public void ClearSources ()
		{
			source_data.Clear ();
			plain_text_source = null;
			image_source = null;
		}

		public void AddSource (int type, object source)
		{
			// Try to detect plain text, based on the old behaviour of XplatUIX11, which usually assigns
			// -1 as the type when a string is stored in the Clipboard
			if (source is string && (type == DataFormats.GetFormat (DataFormats.Text).Id || type == -1))
				plain_text_source = source as string;
			else if (source is Image)
				image_source = source as Image;

			source_data [type] = source;
		}

		public object GetSource (int type)
		{
			return source_data [type];
		}

		public string GetPlainText ()
		{
			return plain_text_source;
		}

		public string GetRtfText ()
		{
			DataFormats.Format format = DataFormats.GetFormat (DataFormats.Rtf);
			if (format == null)
				return null; // FIXME - is RTF not supported on any system?

			return (string)GetSource (format.Id);
		}

		public Image GetImage ()
		{
			return image_source;
		}

		public bool IsSourceText {
			get {
				return plain_text_source != null;
			}
		}

		public bool IsSourceImage {
			get {
				return image_source != null;
			}
		}
	}
}

