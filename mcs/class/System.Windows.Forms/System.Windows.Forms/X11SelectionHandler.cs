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
// Copyright (c) 2005 Novell, Inc.
// Copyright (c) 2021 Thomas Kuehne
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//	Thomas Kuehne (thomas@kuehne.cn)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace System.Windows.Forms
{
	sealed class X11SelectionHandler
	{
		[Flags]
		enum Direction
		{
			In = 1,
			Out = 2,
			InOut = 3
		}

		const string IMAGE_FORMAT = "System.Drawing.Image";
		const string FileName_FORMAT = "FileName";
		const string FileNameW_FORMAT = "FileNameW";

		static List<X11SelectionHandler> MIME_HANDLERS;

		static Encoding UTF8;
		static Encoding UTF16BE;
		static Encoding UTF16LE;


		readonly string[] NetNames;
		readonly Encoding CharsetEncoding;
		readonly DataConverter Converter;
		readonly string Name;
		readonly IntPtr Type;
		readonly Direction Dir;

		X11SelectionHandler (string name, DataConverter converter, params string[] netNames)
			: this (name, null, Direction.InOut, converter, netNames)
		{
		}

		X11SelectionHandler (string name, string charset, Direction dir, DataConverter converter, params string[] netNames)
		{
			Name = name;
			Converter = converter;
			NetNames = netNames;
			Dir = dir;

			CharsetEncoding = GetEncoding (charset);

			Type = XplatUIX11.XInternAtom (XplatUIX11.Display, Name, false);

			MIME_HANDLERS.Add (this);
		}

		internal void GetData (ref XEvent xevent, IDataObject data)
		{
			if (Direction.In != (Dir & Direction.In)) {
				// NOP - wrong direction
			} else if (!UnsetTargetPresent (data)) {
				// NOP - nothing to do
			} else {
				try {
					// due to MULTIPLE handling:
					// forwarding xevent to Converter isn't possible
					// without fake XEvents or corrupting xevent's data
					IntPtr display = xevent.SelectionEvent.display;
					IntPtr requestor = xevent.SelectionEvent.requestor;
					IntPtr property = xevent.SelectionEvent.property;
					Converter.ImportFromX11 (this, data, display, requestor, property);
				} catch {
					// 1) selection content likely from another application
					// 2) de-serialization can have a multitude of interresting issues
					// 3) no good place to treat those issues
					//
					// => silently ignore all exceptions
				}
			}
		}

		internal void SetData (ref XEvent xevent, object data)
		{
			try {
				// direction filter is only used for advertising
				// all formats - wether advertised or not - supply data
				// this is important for mime-types with charset information

				// due to MULTIPLE handling:
				// forwarding xevent to Converter isn't possible
				// without fake XEvents or corrupting xevent's data
				IntPtr display = xevent.SelectionRequestEvent.display;
				IntPtr requestor = xevent.SelectionRequestEvent.requestor;
				IntPtr property = xevent.SelectionRequestEvent.property;
				IntPtr target = xevent.SelectionRequestEvent.target;
				IntPtr selection = xevent.SelectionRequestEvent.selection;

				var success = Converter.ExportToX11 (this, data, display,
						requestor, property, target, selection);

				DataConverter.SetDelivered (ref xevent, success);
			} catch {
				// always send data or requester is going to timeout
				SetUnsupported (ref xevent);

				// 1) selections have quite a few non-trival race conditions
				// 2) de-serialization can have a multitude of interresting issues
				// 3) no good place to treat those issues
				//
				// => silently ignore all exceptions
			}
		}

		internal static void SetUnsupported (ref XEvent xevent)
		{
			DataConverter.SetDelivered (ref xevent, false);
		}

		internal static void SetEmpty (ref XEvent xevent)
		{
			var display = xevent.SelectionRequestEvent.display;
			var requestor = xevent.SelectionRequestEvent.requestor;
			var property = xevent.SelectionRequestEvent.property;
			var target = xevent.SelectionRequestEvent.target;

			bool success = DataConverter.SetEmpty (display, requestor, property, target);

			DataConverter.SetDelivered (ref xevent, success);
		}

		internal static List<IntPtr> GetDataPtrs (IntPtr display, IntPtr window,
				IntPtr property, IntPtr propertyType)
		{
			IntPtr bytes_after;
			var res = new List<IntPtr>(64);

			do {
				IntPtr nitems;
				IntPtr actual_type;
				int actual_fmt;
				IntPtr data = IntPtr.Zero;
					XplatUIX11.XGetWindowProperty (display, window, property,
					    IntPtr.Zero, new IntPtr (32), false,
					    propertyType, out actual_type,
					    out actual_fmt, out nitems, out bytes_after,
					    ref data);
				try {
					if (actual_fmt == 0) {
						return null;
					}

					if (propertyType != actual_type && propertyType != (IntPtr)Atom.AnyPropertyType) {
						return null;
					}

					if (actual_fmt != 32) {
						return null;
					}

					for (int i = 0; i < nitems.ToInt32 ( ); i++) {
						res.Add(Marshal.ReadIntPtr(data, i * IntPtr.Size));
					}
				} finally {
					if (data != IntPtr.Zero)
						XplatUIX11.XFree (data);
				}
			} while (bytes_after.ToInt32 ( ) > 0);

			return res;
		}

		static IntPtr GetTargetAtom (IntPtr selection, IntPtr type) {
			// TODO cache atoms?
			string name = string.Format("_MONO+S{0}+T{1}", selection, type);
			return XplatUIX11.XInternAtom (XplatUIX11.Display, name, false);
		}

		internal void ConvertSelection (IntPtr display, IntPtr selection, IntPtr toplevel)
		{
			var target = GetTargetAtom (selection, Type);
			XplatUIX11.XConvertSelection (display, selection, Type, target, toplevel, IntPtr.Zero /* CurrentTime */);
		}

		internal void ConvertSelection (IntPtr display, IntPtr selection, IntPtr toplevel, X11SelectionHandler[] formats)
		{
			var wanted = new IntPtr[formats.Length * 2];
			for(int i = 0; i < formats.Length; i++) {
				wanted[i * 2] = formats[i].Type;
				wanted[i * 2 + 1] = GetTargetAtom (selection, formats[i].Type);
			}

			var property = GetTargetAtom (selection, Type);
			var target = X11Selection.ATOM_PAIR;

			DataConverter.SetAtoms (wanted, display, toplevel, property, target);

			XplatUIX11.XConvertSelection (display, selection, Type, property, toplevel, IntPtr.Zero /* CurrentTime */);
		}

		internal static void FreeNativeSelectionBuffers (IntPtr selection)
		{
			DataConverter.FreeNativeBuffers (selection);
		}


		X11SelectionHandler ForConversion (string charset, string raw_name)
		{
			X11SelectionHandler handler;
			Encoding encoding;

			if (charset == null)
				encoding = CharsetEncoding;
			else
				encoding = GetEncoding (charset);

			if (Name == raw_name){
				if(encoding == CharsetEncoding) {
					return this;
				}
			}

			handler = new X11SelectionHandler (raw_name, charset, Direction.In, Converter, NetNames);

			return handler;
		}

		bool UnsetTargetPresent (IDataObject data)
		{
			foreach (string format in NetNames)
				if (!data.GetDataPresent (format, false))
					return true;

			// have to run those converters as there is no easy way to predict if they
			// may supply new data
			return (Converter is SerializedObjectConverter) || (Converter is MultipleConverter);
		}

		void SetUnsetTargets (IDataObject data, object target)
		{
			foreach (string format in NetNames)
				if (!data.GetDataPresent (format, false))
					data.SetData (format, target);
		}

		internal static X11SelectionHandler Find (IntPtr type_atom)
		{
			string name;
			X11SelectionHandler handler;

			if (type_atom == IntPtr.Zero)
				return null;

			// by position as a new Handler might be added concurrently
			for (int pos = 0; pos < MIME_HANDLERS.Count; pos++) {
				handler = MIME_HANDLERS[pos];
				if (handler.Type == type_atom)
					return handler;
			}

			// see Find (string) for why searching by known atom is not enough
			name = XplatUIX11.XGetAtomName (XplatUIX11.Display, type_atom);

			if (name == null)
				return null;

			handler = Find (name);

			return handler;
		}

		internal static X11SelectionHandler Find (string raw_name, ICollection<IntPtr> type_list = null)
		{
			// these are all legal type names:
			//    UTF8_STRING
			//    text/html
			//    text/plain;charset=utf-16
			//    application/x-openoffice-dif;windows_formatname="DIF"
			//    TeXt/HtMl

			string name;
			string charset;
			string netName;
			ContentType type;
			X11SelectionHandler handler;
			X11SelectionHandler conversion_handler;

			if (string.IsNullOrEmpty (raw_name))
				return null;

			if (raw_name.IndexOf (';') < 1) {
				if (raw_name.IndexOf ('/') < 1) {
					// Windows style name or reserverd X11 style name
					name = null;
					charset = null;
					netName = raw_name;
				} else {
					// plain MIME type without parameters
					name = raw_name;
					charset = null;
					netName = null;
				}
			} else {
				// MIME type with parameters
				try {
					type = new ContentType (raw_name);
					name = type.MediaType;
					charset = type.Parameters["charset"];
					netName = type.Parameters["windows_formatname"];
				} catch {
					// mallformed type
					return null;
				}
			}

			// find exact match - by position as a new Handler might be added concurrently
			for (int pos = 0; pos < MIME_HANDLERS.Count; pos++) {
				handler = MIME_HANDLERS[pos];
				if (raw_name == handler.Name) {
					if (type_list != null) {
						if (!type_list.Contains (handler.Type) && (Direction.Out == (handler.Dir & Direction.Out)))
							type_list.Add (handler.Type);
					} else {
						return handler;
					}
				}
			}

			if (name != null) {
				// find MIME match - by position as a new Handler might be added concurrently
				for (int pos = 0; pos < MIME_HANDLERS.Count; pos++) {
					handler = MIME_HANDLERS[pos];
					if (string.Equals (name, handler.Name, StringComparison.OrdinalIgnoreCase)) {
						conversion_handler = handler.ForConversion (charset, raw_name);
						if (type_list != null) {
							if (!type_list.Contains (conversion_handler.Type) && (Direction.Out == (conversion_handler.Dir & Direction.Out)))
								type_list.Add (conversion_handler.Type);
						} else {
							return conversion_handler;
						}
					}
				}
			}

			if (netName != null) {
				// find netName match
				for (int pos = 0; pos < MIME_HANDLERS.Count; pos++) {
					handler = MIME_HANDLERS[pos];
					foreach (string handlerNetName in handler.NetNames) {
						if (string.Equals (netName, handlerNetName, StringComparison.OrdinalIgnoreCase)) {
							if (type_list != null) {
								conversion_handler = handler.ForConversion (charset, handler.Name);
								if (!type_list.Contains (conversion_handler.Type) && (Direction.Out == (conversion_handler.Dir & Direction.Out)))
									type_list.Add (conversion_handler.Type);
							} else {
								conversion_handler = handler.ForConversion (charset, raw_name);
								return conversion_handler;
							}
						}
					}
				}
			}

			return null;
		}


		static bool IsObjectSerializable (object obj)
		{
			if (obj == null)
				return false;

			// checking for ISerializable is not enough and misses primitve types

			return obj.GetType ( ).IsSerializable;
		}

		internal static IntPtr [] DetermineSupportedTypes (object data, bool clipboard)
		{
			List<IntPtr> res;
			IDataObject data_object;


			res = new List<IntPtr> ();

			// TODO TIMESTAMP
			res.Add (X11Selection.TARGETS);
			res.Add (X11Selection.MULTIPLE);
			if (clipboard)
				res.Add (X11Selection.SAVE_TARGETS);

			if (data == null)
				return res.ToArray ( );

			if (IsObjectSerializable (data)) {
				Find (DataFormats.Serializable, res);
				// don't end processing here
				// objects might also be (IDataObject, Image, ...)
				// and those are required for native applications
			}


			data_object = data as IDataObject;
			if (data_object != null) {
				// don't ignore exceptions from GetFormats
				// they were caused by 'our' application
				foreach (string format in data_object.GetFormats (true))
					Find (format, res);
				// TODO add plain text support for IDataObject content
			}

			if (data is Image)
				Find (IMAGE_FORMAT, res);

			if (data is Uri || data is IEnumerable<Uri>) {
				Find (DataFormats.FileDrop, res);
				Find (DataFormats.StringFormat, res);
			}

			if (data.GetType ( ).IsPrimitive || data is IEnumerable<string> || data is StringCollection)
				Find (DataFormats.StringFormat, res);

			return res.ToArray ( );
		}


		public override string ToString ()
		{
			return string.Concat ("X11SelectionHandler {", Name, "}");
		}

		internal static IDataObject SetDataWithFormats (object val)
		{
			StringBuilder builder;
			List<string> strings;
			IEnumerable<Uri> enumerableUri;
			string[] urls;
			Uri url;
			string str;
			var data = new DataObject();

			if (val == null)
				return data;

			data.SetData (val);

			if (IsObjectSerializable (val))
				data.SetData (DataFormats.Serializable, val);

			if (val is string) {
				data.SetData (DataFormats.UnicodeText, val);
				data.SetData (DataFormats.Text, val);
				// no explicit DataFormats.StringFormat here
				// as that is done above automatically for all types
			}

			if (val is Image)
				data.SetData (DataFormats.Bitmap, val);

			url = val as Uri;
			if (url != null) {
				try {
					if (string.Equals ("file", url.Scheme, StringComparison.OrdinalIgnoreCase)) {
						urls = new string[]{ url.AbsolutePath };
						data.SetData (DataFormats.FileDrop, urls);
						data.SetData (FileNameW_FORMAT, urls);
						data.SetData (FileName_FORMAT, urls);
					}
				} catch {
					// relative URI or other schemas or ...
				}

				try {
					str = val.ToString ( );
					data.SetData (DataFormats.StringFormat, str);
					data.SetData (DataFormats.UnicodeText, str);
					data.SetData (DataFormats.Text, str);
				} catch {
					// nop
				}
			}

			enumerableUri = val as IEnumerable<Uri>;
			if (enumerableUri != null) {
				strings = new List<string> ();
				builder = new StringBuilder ();
				foreach (Uri uri in enumerableUri) {
					try {
						if (string.Equals ("file", uri.Scheme, StringComparison.OrdinalIgnoreCase))
							strings.Add (uri.AbsolutePath);

					} catch {
						// relative URI or other schemas or ...
					}

					try {
						if (0 < builder.Length)
							builder.Append ("\n");

						builder.Append (uri);
					} catch {
						// NOP
					}
				}

				if (0 < builder.Length) {
					str = builder.ToString ( );
					data.SetData (DataFormats.StringFormat, str);
					data.SetData (DataFormats.UnicodeText, str);
					data.SetData (DataFormats.Text, str);
				}

				if (0 < strings.Count) {
					urls = strings.ToArray ( );
					data.SetData (DataFormats.FileDrop, urls);
					data.SetData (FileNameW_FORMAT, urls);
					data.SetData (FileName_FORMAT, urls);
				}
			}

			return data;
		}

		static Encoding GetEncoding (string charset)
		{
			if (charset == null)
				return null;

			string upper = charset.ToUpperInvariant ( );

			// UTF8 and UTF16 aren't official names but do
			// crop up once in a while
			switch (upper) {
				case "UTF8":
				case "UTF-8":
					// always without Byte Order Mark (BOM)
					return UTF8;
				case "UTF-16BE":
					// always without Byte Order Mark (BOM)
					return UTF16BE;
				case "UTF16":
				case "UTF-16":
				case "UTF-16LE":
					// always without Byte Order Mark (BOM)
					return UTF16LE;
				default:
					return Encoding.GetEncoding (charset);
			}
		}

		internal static void Init ()
		{
			if (MIME_HANDLERS != null)
				return;

			MIME_HANDLERS = new List<X11SelectionHandler> ();

			UTF8 = new UTF8Encoding (false);
			UTF16BE = new UnicodeEncoding (true, false);
			UTF16LE = new UnicodeEncoding (false, false);


			// clipboard target list - only relevant for non-dotnet receivers
			new X11SelectionHandler ("TARGETS", new TargetsConverter ());

			// list of requests - special infrastructure handler
			new X11SelectionHandler ("MULTIPLE", new MultipleConverter ());

			// object (de-)serialization
			new X11SelectionHandler ("application/x-mono-serialized-object", new SerializedObjectConverter (), DataFormats.Serializable);

			// plain text formats:
			new X11SelectionHandler ("text/plain;charset=utf-8", "UTF-8", Direction.InOut, new TextConverter (),
				DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text);

			new X11SelectionHandler ("UTF8_STRING", "UTF-8", Direction.InOut, new TextConverter (),
				DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text);

			new X11SelectionHandler ("text/plain", null, Direction.InOut, new TextConverter (UTF8),
				DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text);

			new X11SelectionHandler ("text/unicode", null, Direction.In, new TextConverter (UTF16LE),
				DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text);

			new X11SelectionHandler ("STRING", null, Direction.InOut, new TextConverter (),
				DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text);

			new X11SelectionHandler ("TEXT", null, Direction.InOut, new TextConverter (),
				DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text);


			// value-added text formats:
			new X11SelectionHandler ("text/csv", new TextConverter (),
				DataFormats.CommaSeparatedValue);

			// TODO support "text/html" - DataFormats.Html
			// TODO support "application/xhtml+xml" - DataFormats.Html

			new X11SelectionHandler ("text/rtf", new TextConverter (),
				DataFormats.Rtf);

			new X11SelectionHandler ("FILE_NAME", new UriListConverter (null, "\0", false),
				DataFormats.FileDrop, FileNameW_FORMAT, FileName_FORMAT);

			new X11SelectionHandler ("text/uri-list", new UriListConverter (null, "\r\n", false),
				DataFormats.FileDrop, FileNameW_FORMAT, FileName_FORMAT);


			// image formats:
			new X11SelectionHandler ("image/png", new ImageConverter (ImageFormat.Png),
				IMAGE_FORMAT, DataFormats.Bitmap);

			new X11SelectionHandler ("image/bmp", null, Direction.In, new ImageConverter (ImageFormat.Bmp),
				IMAGE_FORMAT, DataFormats.Bitmap);

			new X11SelectionHandler ("image/jpeg", new ImageConverter (ImageFormat.Jpeg),
				IMAGE_FORMAT, DataFormats.Bitmap);

			new X11SelectionHandler ("image/gif", new ImageConverter (ImageFormat.Gif),
				IMAGE_FORMAT, DataFormats.Bitmap);

			new X11SelectionHandler ("image/tiff", null, Direction.In, new ImageConverter (ImageFormat.Tiff),
				IMAGE_FORMAT, DataFormats.Bitmap);


			// Mozilla:
			new X11SelectionHandler ("text/x-moz-url", new UriListConverter (UTF16LE, "\n", true),
				DataFormats.FileDrop, FileNameW_FORMAT, FileName_FORMAT);

			new X11SelectionHandler ("_NETSCAPE_URL", new UriListConverter (null, "\n", true),
				DataFormats.FileDrop, FileNameW_FORMAT, FileName_FORMAT);
		}

		// tries to detect the encoding of a readable, seekeable Stream
		// detects deterministically: UTF-8, UTF-16LE, UTF-16BE, UTF-32LE, UTF-32BE, GB18030
		// detects heuristically: UTF-8, UTF-16LE, UTF-16BE
		// returns null if unsure
		internal static Encoding DetectEncoding (Stream stream)
		{
			if (stream == null || !stream.CanRead || !stream.CanSeek)
				return null;

			long read;
			// snooping array's length should a multiple of 2 to aid UTF-16 heuristic
			byte[] header = new byte[1024 * 512 * 2];

			long start_position = stream.Position;

			read = stream.Read (header, 0, header.Length);

			stream.Position = start_position;

			return DetectEncodingBOM (header, read) ?? DetectEncodingHeuyristic (header, read, stream.Length);
		}

		static Encoding DetectEncodingBOM (byte[] x, long length)
		{
			if (length < 2)
				return null;

			if (x[0] == 0xFE && x[1] == 0xFF)
				return new UnicodeEncoding(false, true);

			if (x[0] == 0xFF && x[1] == 0xFE)
				return new UnicodeEncoding(true, true);

			if (length < 3)
				return null;

			if (x[0] == 0xEF && x[1] == 0xBB && x[2] == 0xBF)
				return new UTF8Encoding (true);

			if (length < 4)
				return null;

			if (x[0] == 0x00 && x[1] == 0x00 && x[2] == 0xFE && x[3] == 0xFF)
				return new UTF32Encoding(true, true);

			if (x[0] == 0x00 && x[1] == 0x00 && x[2] == 0xFF && x[3] == 0xFE)
				return new UTF32Encoding(false, true);

			if (x[0] == 0x84 && x[1] == 0x31 && x[2] == 0x95 && x[3] == 0x33)
				return GetEncoding ("GB18030");

			return null;
		}

		static Encoding DetectEncodingHeuyristic (byte[] data, long length, long total)
		{
			// detect ASCII
			// report it as UTF-8 if this wasn't all
			// UTF-8 is a super set of ASCII
			long pos;
			for (pos = 0; pos < length; pos++) {
				int c = 0xFF & data[pos];
				if (c == '\n' || c == '\r' || c == '\t' ) {
					// OK
				} else if (c < ' ' || '~' < c) {
					break;
				}
			}
			if (pos == total)
				return new ASCIIEncoding();

			if (pos == length)
				return UTF8;


			// zero detection for UTF16-BE / UTF16-LE
			// - stream lenght has to be even
			// - zero count at odd and even positions are wildely different
			// TODO improve heuristic for non-latin
			if (total % 2 == 0) {
				int[] zeros =  new int[2];
				for (int i = 0; i < length; i++) {
					if (0x00 == data[i])
						zeros[i % 2]++;
				}

				if (zeros[0] * 3 > length && zeros[1] * 3 < zeros[0])
					return UTF16BE;

				if (zeros[1] * 3 > length && zeros[0] * 3 < zeros[1])
					return UTF16LE;
			}

			return null;
		}

		static IList<IntPtr> TypeList (IntPtr source_display, IntPtr source_window, IntPtr type_property, ref XClientMessageEvent dnd)
		{
			if (((int) dnd.ptr2 & 0x1) == 0) {
				var res = new List<IntPtr>(3);

				if (dnd.ptr3 != IntPtr.Zero)
					res.Add (dnd.ptr3);

				if (dnd.ptr4 != IntPtr.Zero)
					res.Add (dnd.ptr4);

				if (dnd.ptr5 != IntPtr.Zero)
					res.Add (dnd.ptr5);

				return res;
			}

			return GetDataPtrs (source_display, source_window, type_property, (IntPtr) Atom.XA_ATOM);
		}

		internal static string[] TypeListConvert (IntPtr source_display, IntPtr source_window, IntPtr type_property, ref XClientMessageEvent dnd){
			var formats = TypeList (source_display, source_window, type_property, ref dnd);

			if (formats == null || formats.Count < 1)
				return new string[0];

			var net_formats = new List<string>();

			foreach (var format in formats){
				var handler = X11SelectionHandler.Find (format);
				if (handler != null) {
					foreach (var net_format in handler.NetNames) {
						if (net_formats.IndexOf (net_format) < 0) {
							net_formats.Add (net_format);
						}
					}
				}
			}

			return net_formats.ToArray();
		}

		internal static X11SelectionHandler[] TypeListHandlers (IntPtr source_display, IntPtr source_window, IntPtr type_property, ref XClientMessageEvent dnd, out X11SelectionHandler multiple){
			var formats = TypeList (source_display, source_window, type_property, ref dnd);
			multiple = null;

			if (formats == null || formats.Count < 1)
				return new X11SelectionHandler[0];

			var net_formats = new List<string>();
			var handlers = new List<X11SelectionHandler>();

			foreach (var format in formats){
				var handler = X11SelectionHandler.Find (format);
				if (handler != null) {
					if (format == X11Selection.MULTIPLE) {
						multiple = handler;
					} else {
						var handler_added = false;
						foreach (var net_format in handler.NetNames) {
							if (net_formats.IndexOf (net_format) < 0) {
								net_formats.Add (net_format);
								if (!handler_added) {
									handlers.Add (handler);
									handler_added = true;
								}
							}
						}
					}
				}
			}

			return handlers.ToArray();
		}

		abstract class DataConverter
		{
			internal abstract void ImportFromX11 (X11SelectionHandler handler, IDataObject data,
					IntPtr display, IntPtr requestor, IntPtr property);

			internal abstract bool ExportToX11 (X11SelectionHandler handler, object data, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection);

			// a dummy buffer filled only with zeros
			static readonly IntPtr DUMMY_PTR;

			// 24 <= max(ptr_size) * 3
			const int CANARY_LENGTH = 24;

			static readonly Dictionary<IntPtr, List<IntPtr>> NATIVE_BUFFERS;

			static DataConverter ()
			{
				DUMMY_PTR = Marshal.AllocHGlobal (CANARY_LENGTH);
				for (int i = 0; i < CANARY_LENGTH; i++)
					Marshal.WriteByte (DUMMY_PTR, i, 0);

				NATIVE_BUFFERS = new Dictionary<IntPtr, List<IntPtr>> ();
			}

			// native buffers may only be released after the dnd / copy has finished
			static IntPtr AllocNativeBuffer (IntPtr selection, int bytes)
			{
				List<IntPtr> list;
				IntPtr ptr;

				if (false == NATIVE_BUFFERS.TryGetValue (selection, out list)) {
					list = new List<IntPtr> ();
					NATIVE_BUFFERS.Add (selection, list);
				}

				ptr = Marshal.AllocHGlobal (bytes);
				lock (list) {
					list.Add (ptr);
				}

				return ptr;
			}

			internal static void FreeNativeBuffers (IntPtr selection)
			{
				List<IntPtr> list;

				if (NATIVE_BUFFERS.TryGetValue (selection, out list)) {
					lock (list) {
						foreach (IntPtr buffer in list)
							Marshal.FreeHGlobal (buffer);

						list.Clear ( );
					}
				}
			}

			static bool SetProperty (IntPtr data, int length, IntPtr display,
					IntPtr window, IntPtr property, IntPtr target)
			{
				if (0 <= length) {
					XplatUIX11.XChangeProperty (display, window, property, target,
							8, PropertyMode.Replace, data, length);

					return true;
				}
				return false;
			}


			protected static MemoryStream GetDataStream (IntPtr display, IntPtr window,
					IntPtr property)
			{
				IntPtr nitems;
				IntPtr bytes_after;

				MemoryStream res = new MemoryStream ();
				do {
					IntPtr actual_type;
					int actual_fmt;
					IntPtr data = IntPtr.Zero;

					XplatUIX11.XGetWindowProperty (display, window, property,
						    IntPtr.Zero, new IntPtr (0xffffff), false,
						    (IntPtr)Atom.AnyPropertyType, out actual_type,
						    out actual_fmt, out nitems, out bytes_after,
						    ref data);

					try {
						if (actual_fmt == 0) {
							break;
						}

						if (actual_fmt == 8) {
							for (int i = 0; i < nitems.ToInt32 ( ); i++)
								res.WriteByte (Marshal.ReadByte (data, i));
						} else {
							throw new NotImplementedException ("actual data format: " + actual_fmt);
						}
					} finally {
						if (data != IntPtr.Zero)
							XplatUIX11.XFree (data);
					}
				} while (bytes_after.ToInt32 ( ) > 0);

				res.Seek (0, SeekOrigin.Begin);
				return res;
			}

			internal static bool SetEmpty (IntPtr display,
					IntPtr window, IntPtr property, IntPtr target)
			{
				return SetProperty (DUMMY_PTR, 0, display, window, property, target);
			}

			internal static void SetDelivered (ref XEvent xevent, bool success)
			{
				XEvent sel = new XEvent ();
				sel.SelectionEvent.type = XEventName.SelectionNotify;
				sel.SelectionEvent.send_event = true;
				sel.SelectionEvent.display = xevent.SelectionRequestEvent.display;
				sel.SelectionEvent.selection = xevent.SelectionRequestEvent.selection;
				sel.SelectionEvent.target = xevent.SelectionRequestEvent.target;
				sel.SelectionEvent.requestor = xevent.SelectionRequestEvent.requestor;
				sel.SelectionEvent.time = xevent.SelectionRequestEvent.time;

				sel.SelectionEvent.property = success ? xevent.SelectionRequestEvent.property : IntPtr.Zero;

				XplatUIX11.XSendEvent (xevent.SelectionRequestEvent.display,
						xevent.SelectionRequestEvent.requestor, false,
						(IntPtr)EventMask.NoEventMask, ref sel);
			}

			internal static bool SetAtoms (IntPtr[] atoms, IntPtr display, IntPtr window,
					IntPtr property, IntPtr target)
			{
				if (atoms != null) {
					XplatUIX11.XChangeProperty (display, window, property, target,
						32, PropertyMode.Replace, atoms, atoms.Length);

					return true;
				}

				return false;
			}

			protected static bool SetBytes (byte[] bytes, IntPtr display,
					IntPtr window, IntPtr property, IntPtr target, IntPtr selection)
			{
				IntPtr buffer;
				int length_canary;

				if (bytes.Length < 1)
					return SetEmpty (display, window, property, target);
				else {
					// strictly speaking no zero-canaries are required
					// but they help with broken receivers that uses
					// strlen instead of the length included in X11 message
					// for example older Mono versions

					length_canary = bytes.Length + CANARY_LENGTH;
					if (length_canary < bytes.Length)
						length_canary = int.MaxValue;

					buffer = AllocNativeBuffer (selection, length_canary);

					// write payload
					Marshal.Copy (bytes, 0, buffer, bytes.Length);

					// write canary zeros at the end
					for (int i = bytes.Length; i < length_canary; i++)
						Marshal.WriteByte (buffer, i, 0);

					return SetProperty (buffer, bytes.Length, display, window, property, target);
				}
			}

			protected static bool SetStream (Stream stream, IntPtr display,
					IntPtr window, IntPtr property, IntPtr target, IntPtr selection)
			{
				IntPtr buffer;
				int pos;
				int length;
				int length_canary;
				int c;

				stream.Seek (0, SeekOrigin.Begin);

				length = (int)stream.Length;

				if (length < 1)
					return SetEmpty (display, window, property, target);
				else {
					// strictly speaking no zero-canaries are required
					// but they help with broken receivers that uses strlen
					// instead of the length included in X11 message
					// for example older Mono versions

					length_canary = length + CANARY_LENGTH;
					if (length_canary < length)
						length_canary = int.MaxValue;

					buffer = AllocNativeBuffer (selection, length_canary);

					// write payload
					for (pos = 0; pos < length; pos++) {
						c = stream.ReadByte ( );
						if (c < 0)
							break;

						Marshal.WriteByte (buffer, pos, (byte)c);
					}

					// write canary zeros at the end
					for (int i = pos; i < length_canary; i++)
						Marshal.WriteByte (buffer, i, 0);

					return SetProperty (buffer, pos, display, window, property, target);
				}
			}

		}

		sealed class SerializedObjectConverter : DataConverter
		{
			static readonly byte[] SERIALIZED_OBJECT_MAGIC;

			static SerializedObjectConverter ()
			{
				Guid guid = new Guid ("FD9EA796-3B13-4370-A679-56106BB288FB");
				SERIALIZED_OBJECT_MAGIC = guid.ToByteArray ( );
			}

			internal override void ImportFromX11 (X11SelectionHandler handler, IDataObject data,
					IntPtr display, IntPtr requestor, IntPtr property)
			{

				MemoryStream stream;
				BinaryFormatter formatter;
				Object obj;
				int pos;
				int val;

				using (stream = GetDataStream (display, requestor, property)) {
					// check the GUID marker - compatibility with Windows
					for (pos = 0; pos < SERIALIZED_OBJECT_MAGIC.Length; pos++) {
						val = stream.ReadByte ( );
						if (val < 0)
							return;
						if ((byte)val != SERIALIZED_OBJECT_MAGIC[pos])
							return;
					}

					formatter = new BinaryFormatter ();
					obj = formatter.Deserialize (stream);

					data.SetData (obj);
					data.SetData (DataFormats.Serializable, obj);
				}
			}

			internal override bool ExportToX11 (X11SelectionHandler handler, object data, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{
				IDataObject data_obj;
				MemoryStream stream;
				BinaryFormatter formatter;

				if (!IsObjectSerializable (data)) {
					data_obj = data as IDataObject;
					data = data_obj.GetData (DataFormats.Serializable, true);
					if (!IsObjectSerializable (data)) {
						return false;
					}
				}

				using (stream = new MemoryStream ()) {
					// write GUID marker - compatibility with Windows
					stream.Write (SERIALIZED_OBJECT_MAGIC, 0, SERIALIZED_OBJECT_MAGIC.Length);

					formatter = new BinaryFormatter ();
					formatter.Serialize (stream, data);

					return SetStream (stream, display, requestor, property, target, selection);
				}
			}
		}

		sealed class ImageConverter : DataConverter
		{
			readonly ImageFormat ImgFormat;

			internal ImageConverter (ImageFormat format)
			{
				ImgFormat = format;
			}

			internal override void ImportFromX11 (X11SelectionHandler handler, IDataObject data,
					IntPtr display, IntPtr requestor, IntPtr property)
			{
				MemoryStream stream;
				Bitmap image;

				// don't dispose / free the stream
				// it has to be alive as long as the image is

				stream = GetDataStream (display, requestor, property);
				if (stream.Length < 1)
					return;

				image = new System.Drawing.Bitmap (stream);

				handler.SetUnsetTargets (data, image);
			}

			internal override bool ExportToX11 (X11SelectionHandler handler, object data, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{
				Image image;
				MemoryStream stream;
				IDataObject data_object;

				image = data as Image;
				if (image == null) {
					data_object = data as IDataObject;
					if (data_object != null) {
						foreach (string netName in handler.NetNames) {
							image = data_object.GetData (netName, false) as Image;
							if (image != null)
								break;
						}
					}
					if (image == null) {
						return false;
					}
				}


				using (stream = new MemoryStream ()) {
					image.Save (stream, ImgFormat);
					return SetStream (stream, display, requestor, property, target, selection);
				}
			}
		}


		class TextConverter : DataConverter
		{
			readonly Encoding DefaultOutEncoding;

			internal TextConverter (Encoding encoding = null)
			{
				DefaultOutEncoding = encoding;
			}

			internal override void ImportFromX11 (X11SelectionHandler handler, IDataObject data,
					IntPtr display, IntPtr requestor, IntPtr property)
			{

				object decoded;
				string text = GetText (display, requestor, property, handler.CharsetEncoding);

				if (text == null)
					return;

				decoded = FromNative (text);

				if (decoded != null)
					handler.SetUnsetTargets (data, decoded);
			}

			internal override bool ExportToX11 (X11SelectionHandler handler, object data, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{
				IDataObject data_object;
				StringBuilder builder;
				string str = data as string;
				object obj = null;
				object obj_good = data;
				string native;
				bool first;
				Uri uri;
				IEnumerable<Uri> enumerableUri;
				IEnumerable enumerable;

				if (str == null) {
					data_object = data as IDataObject;
					if (data_object != null) {
						obj = data_object.GetData (handler.Name, false);
						if (obj != null) {
							obj_good = obj;
							str = obj_good as string;
						}

						if (str == null) {
							foreach (string netName in handler.NetNames) {
								obj = data_object.GetData (netName, false);
								if (obj != null) {
									obj_good = obj;
									str = obj_good as string;
									if (str != null)
										break;
								}
							}
						}
					}
				}


				if (str != null)
					native = ToNative (str);
				else if (obj_good != null) {
					uri = obj_good as Uri;
					if (uri != null)
						native = UriListConverter.Encode (new []{ uri });
					else {
						enumerableUri = obj_good as IEnumerable<Uri>;
						if (enumerableUri != null)
							native = UriListConverter.Encode (enumerableUri);
						else {
							enumerable = obj_good as IEnumerable;
							if (enumerable != null) {
								builder = new StringBuilder ();
								first = true;
								foreach (object o in enumerable) {
									if (first)
										first = false;
									else
										builder.Append ('\n');

									builder.Append (o);
								}
								native = builder.ToString ( );
							} else
								native = obj_good.ToString ( );
						}
					}
				} else
					return false;

				return SetText (handler, native, display, requestor, property, target, selection);

			}

			protected virtual object FromNative (string native)
			{
				return native;
			}

			protected virtual string ToNative (string native)
			{
				return native;
			}

			protected static string GetText (IntPtr display, IntPtr window,
					IntPtr property, Encoding encoding = null)
			{
				MemoryStream stream;
				StreamReader reader;
				string text;

				using (stream = GetDataStream (display, window, property)) {

					if (encoding == null) {
						encoding = X11SelectionHandler.DetectEncoding (stream);

						if (encoding == null) {
							try {
								encoding = Encoding.Default;
							} catch {
								// NOP
							}
							if (encoding == null)
								encoding = UTF8;
						}
					}

					using (reader = new StreamReader (stream, encoding)) {
						text = reader.ReadToEnd ( );

						if (0 < text.Length && 0 == text[text.Length - 1])
							return text.Substring (0, text.Length - 1);

						return text;
					}
				}
			}

			protected bool SetText (X11SelectionHandler handler, string str, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{
				Encoding encoding = null;
				byte[] bytes;

				if (str == null) {
					return false;
				}

				if (handler.CharsetEncoding != null)
					encoding = handler.CharsetEncoding;
				else if (DefaultOutEncoding != null)
					encoding = DefaultOutEncoding;
				else {
					try {
						encoding = Encoding.Default;
					} catch {
						// NOP
					}
					if (encoding == null)
						encoding = UTF8;
				}

				bytes = encoding.GetBytes (str);

				return SetBytes (bytes, display, requestor, property, target, selection);
			}
		}

		sealed class TargetsConverter : DataConverter
		{
			[Obsolete("should never be called - special DataConverter", true)]
			internal override void ImportFromX11 (X11SelectionHandler handler, IDataObject data,
					IntPtr display, IntPtr requestor, IntPtr property)
			{
				// NOP
				throw new NotImplementedException("should never be called - special DataConverter");
			}

			internal override bool ExportToX11 (X11SelectionHandler handler, object data, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{
				// TARGETS is "special"
				target = (IntPtr) Atom.XA_ATOM;

				bool isClipboard = selection == X11Selection.CLIPBOARD;
				IntPtr[] formats = DetermineSupportedTypes (data, isClipboard);
				return SetAtoms (formats, display, requestor, property, target);
			}
		}

		sealed class UriListConverter : TextConverter
		{
			readonly string Seperator;
			readonly bool Netscape;

			internal UriListConverter (Encoding encoding, string seperator, bool netscape)
				: base (encoding)
			{
				Seperator = seperator;
				Netscape = netscape;
			}

			internal override void ImportFromX11 (X11SelectionHandler handler, IDataObject data,
					IntPtr display, IntPtr requestor, IntPtr property)
			{

				string text;
				List<string> uri_list;
				string[] lines;
				string clean;
				string[] all;
				Uri uri;

				text = GetText (display, requestor, property, handler.CharsetEncoding);
				if (text == null)
					return;

				uri_list = new List<string> ();
				lines = text.Split (Seperator);
				foreach (string line in lines) {
					if (Netscape && 0 < uri_list.Count)
						break;

					// # is a comment line (see RFC 2483)
					if (line.Length < 1 || line[0] == '#')
						continue;

					clean = line.Trim ( );

					if (0 < clean.Length) {
						try {
							uri = new Uri (clean);
							if (string.Equals ("file", uri.Scheme, StringComparison.OrdinalIgnoreCase)) {
								uri_list.Add (uri.LocalPath);
								continue;
							}
						} catch {
							// input might already be just the LocalPath part
							uri_list.Add (clean);
						}
					}
				}

				if (uri_list.Count < 1)
					return;

				all = uri_list.ToArray ( );
				handler.SetUnsetTargets (data, all);
			}

			internal override bool ExportToX11 (X11SelectionHandler handler, object data, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{

				object o;
				string str;
				Uri uri;
				string text;

				IEnumerable<string> str_list = null;
				IEnumerable<Uri> uri_list = null;
				IDataObject data_object = data as IDataObject;

				if (data_object != null) {
					foreach (string netName in handler.NetNames) {
						o = data_object.GetData (netName, false);
						str = o as string;
						if (str != null) {
							str_list = new string[]{ str };
							break;
						}

						uri = o as Uri;
						if (uri != null) {
							uri_list = new Uri[]{ uri };
							break;
						}

						uri_list = o as IEnumerable<Uri>;
						if (uri_list != null)
							break;

						str_list = o as IEnumerable<string>;
						if (str_list != null)
							break;
					}
				}

				if (uri_list == null && str_list == null) {
					str = data as string;
					if (str != null)
						str_list = new string[]{ str };
					else {
						uri = data as Uri;
						if (uri != null)
							uri_list = new Uri[]{ uri };
						else {
							uri_list = data as IEnumerable<Uri>;
							if (uri_list == null)
								str_list = data as IEnumerable<string>;
						}
					}
				}

				if (str_list != null)
					text = Encode (str_list, Seperator, Netscape);
				else if (uri_list != null)
					text = Encode (uri_list, Seperator, Netscape);
				else
					return false;

				return SetText (handler, text, display, requestor, property, target, selection);
			}


			internal static string Encode (IEnumerable<string> str_list, string seperator = "\r\n", bool netscape = false)
			{
				Uri uri;
				bool first = true;
				StringBuilder builder = new StringBuilder ();


				foreach (string str in str_list) {
					if (!string.IsNullOrEmpty (str)) {
						if (first)
							first = false;
						else
							builder.Append (seperator);

						try {
							uri = new Uri (str);
							builder.Append (uri.AbsoluteUri);
						} catch {
							builder.Append (str);
						}

						if (netscape) {
							builder.Append (seperator + builder);
							break;
						}
					}
				}

				return builder.ToString ( );
			}

			internal static string Encode (IEnumerable<Uri> uri_list, string seperator = "\r\n", bool netscape = false)
			{
				bool first = true;
				StringBuilder builder = new StringBuilder ();

				foreach (Uri uri in uri_list) {
					if (uri != null) {
						if (first)
							first = false;
						else
							builder.Append (seperator);

						try {
							builder.Append (uri.AbsoluteUri);
						} catch {
							builder.Append (uri);
						}

						if (netscape) {
							builder.Append (seperator + builder);
							break;
						}
					}
				}

				return builder.ToString ( );
			}
		}

		sealed class MultipleConverter : DataConverter
		{
			internal override void ImportFromX11 (X11SelectionHandler handler, IDataObject idata,
					IntPtr display, IntPtr requestor, IntPtr property)
			{
				var atoms =  X11SelectionHandler.GetDataPtrs (display, requestor, property, X11Selection.ATOM_PAIR);
				if (atoms == null || atoms.Count < 1) {
					return;
				}

				// call all supported converters
				for( int i = 0; i+1 < atoms.Count; i += 2){
					var singleFormat = atoms[i];
					var singleProperty = atoms[i+1];

					if ((singleFormat == IntPtr.Zero) || (singleProperty == IntPtr.Zero)){
						continue;
					}


					try {
						var singleHandler = X11SelectionHandler.Find (singleFormat);
						if (singleHandler == null){
							// should never happen
							continue;
						}

						singleHandler.Converter.ImportFromX11 (singleHandler, idata, display, requestor, singleProperty);
					} catch {
						// ignore - see X11SelectionHandler.GetData
					} finally {
						XplatUIX11.XDeleteProperty (display, requestor, singleProperty);
					}
				}
			}

			internal override bool ExportToX11 (X11SelectionHandler handler, object idata, IntPtr display,
					IntPtr requestor, IntPtr property, IntPtr target, IntPtr selection)
			{
				X11SelectionHandler singleHandler = null;

				var atoms =  X11SelectionHandler.GetDataPtrs (display, requestor, property, X11Selection.ATOM_PAIR);
				if (atoms == null || atoms.Count < 1) {
					return false;
				}

				for( int i = 0; i+1 < atoms.Count; i += 2){
					var singleFormat = atoms[i];
					var singleProperty = atoms[i+1];

					if ((singleFormat == IntPtr.Zero) || (singleProperty == IntPtr.Zero)) {
						// malformed request
						atoms[i] = IntPtr.Zero;
						atoms[i+1] = IntPtr.Zero;
						continue;
					}

					try {
						singleHandler = X11SelectionHandler.Find (singleFormat);
						if (singleHandler == null){
							// unsupported format
							atoms[i] = IntPtr.Zero;
							atoms[i+1] = IntPtr.Zero;
							continue;
						}

						bool success = singleHandler.Converter.ExportToX11 (singleHandler, idata, display, requestor, singleProperty, target, selection);
						if (!success) {
							// export failed
							atoms[i] = IntPtr.Zero;
							atoms[i+1] = IntPtr.Zero;
						}
					} catch {
						// export failed
						atoms[i] = IntPtr.Zero;
						atoms[i+1] = IntPtr.Zero;
					}
				}

				SetAtoms (atoms.ToArray(), display, requestor, property, X11Selection.ATOM_PAIR);

				return true;
			}
		}
	}
}
