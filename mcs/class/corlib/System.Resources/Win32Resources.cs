//
// System.Resources/Win32Resources.cs
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// An incomplete set of classes for manipulating Win32 resources
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Resources {


internal enum Win32ResourceType {
	RT_CURSOR = 1,
	RT_FONT = 8,
	RT_BITMAP = 2,
	RT_ICON = 3,
	RT_MENU = 4,
	RT_DIALOG = 5,
	RT_STRING = 6,
	RT_FONTDIR = 7,
	RT_ACCELERATOR = 9,
	RT_RCDATA = 10,
	RT_MESSAGETABLE = 11,
	RT_GROUP_CURSOR = 12,
	RT_GROUP_ICON = 14,
	RT_VERSION = 16,
	RT_DLGINCLUDE = 17,
	RT_PLUGPLAY = 19,
	RT_VXD = 20,
	RT_ANICURSOR = 21,
	RT_ANIICON = 22,
	RT_HTML = 23,
}

internal class NameOrId {
	string name;
	int id;

	public NameOrId (string name) {
		this.name = name;
	}

	public NameOrId (int id) {
		this.id = id;
	}

	public bool IsName {
		get {
			return name != null;
		}
	}

	public string Name {
		get {
			return name;
		}
	}

	public int Id {
		get {
			return id;
		}
	}

	public override string ToString () {
		if (name != null)
			return "Name(" + name + ")";
		else
			return "Id(" + id + ")";
	}
}

internal abstract class Win32Resource {

	NameOrId type;
	NameOrId name;
	int language;

	internal Win32Resource (NameOrId type, NameOrId name, int language) {
		this.type = type;
		this.name = name;
		this.language = language;
	}

	internal Win32Resource (Win32ResourceType type, int name, int language) {
		this.type = new NameOrId ((int)type);
		this.name = new NameOrId (name);
		this.language = language;
	}

	public Win32ResourceType ResourceType {
		get {
			if (type.IsName)
				return (Win32ResourceType)(-1);
			else
				return (Win32ResourceType)type.Id;
		}
	}

	public NameOrId Name {
		get {
			return name;
		}
	}

	public NameOrId Type {
		get {
			return type;
		}
	}

	public int Language {
		get {
			return language;
		}
	}

	public abstract void WriteTo (Stream s);

	public override string ToString () {
		return "Win32Resource (Kind=" + ResourceType + ", Name=" + name + ")";
	}
}

//
// This class represents a Win32 resource in encoded format
//
internal class Win32EncodedResource : Win32Resource {

	byte[] data;

	internal Win32EncodedResource (NameOrId type, NameOrId name, int language, byte[] data) : base (type, name, language) {
		this.data = data;
	}

	public byte[] Data {
		get {
			return data;
		}
	}

	public override void WriteTo (Stream s) {
		s.Write (data, 0, data.Length);
	}
}

//
// This class represents a Win32 ICON resource
//
internal class Win32IconResource : Win32Resource {

	ICONDIRENTRY icon;

	public Win32IconResource (int id, int language, ICONDIRENTRY icon) : base (Win32ResourceType.RT_ICON, id, language) {
		this.icon = icon;
	}

	public ICONDIRENTRY Icon {
		get {
			return icon;
		}
	}

	public override void WriteTo (Stream s) {
		s.Write (icon.image, 0, icon.image.Length);
	}
}

internal class Win32GroupIconResource : Win32Resource {

	Win32IconResource[] icons;

	public Win32GroupIconResource (int id, int language, Win32IconResource[] icons) : base (Win32ResourceType.RT_GROUP_ICON, id, language) {
		this.icons = icons;
	}

	public override void WriteTo (Stream s) {
		using (BinaryWriter w = new BinaryWriter (s)) {
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write ((short)icons.Length);
			for (int i = 0; i < icons.Length; ++i) {
				Win32IconResource icon = icons [i];
				ICONDIRENTRY entry = icon.Icon;

				w.Write (entry.bWidth);
				w.Write (entry.bHeight);
				w.Write (entry.bColorCount);
				w.Write ((byte)0);
				w.Write (entry.wPlanes);
				w.Write (entry.wBitCount);
				w.Write ((int)entry.image.Length);
				w.Write ((short)icon.Name.Id);
			}
		}
	}
}

//
// This class represents a Win32 VERSION resource
//
internal class Win32VersionResource : Win32Resource {

	public string[] WellKnownProperties = {
		"Comments",
		"CompanyName",
		"FileDescription",
		"FileVersion",
		"InternalName",
		"LegalCopyright",
		"LegalTrademarks",
		"OriginalFilename",
		"ProductName",
		"ProductVersion"
	};

	long signature;
	int struct_version;
	long file_version;
	long product_version;
	int file_flags_mask;
	int file_flags;
	int file_os;
	int file_type;
	int file_subtype;
	long file_date;

	int file_lang;
	int file_codepage;

	Hashtable properties;

	public Win32VersionResource (int id, int language) : base (Win32ResourceType.RT_VERSION, id, language) {
		// Initialize non-public members to the usual values used in
		// resources
		signature = 0xfeef04bd;
		struct_version = 1 << 16; /* 1.0 */
		file_flags_mask = 63;
		file_flags = 0;
		file_os = 4; /* VOS_WIN32 */
		file_type = 2;
		file_subtype = 0;
		file_date = 0;

		file_lang = 0x7f;
		file_codepage = 1200;

		properties = new Hashtable ();

		// Well known properties
		foreach (string s in WellKnownProperties)
			// The value of properties can't be empty
			properties [s] = " ";
	}

	public string FileVersion {
		get {
			return 
				"" + (file_version >> 48) + 
				"." + ((file_version >> 32) & 0xffff) + 
				"." + ((file_version >> 16) & 0xffff) +
				"." + ((file_version >> 0) & 0xffff);
		}

		set {
			long[] ver = new long [4] { 0, 0, 0, 0 };
			if (value != null) {
				string[] parts = value.Split ('.');
				
				for (int i = 0; i < parts.Length; ++i) {
					try {
						if (i < ver.Length)
							ver [i] = Int32.Parse (parts [i]);
					}
					catch (FormatException) {
					}
				}
			}

			file_version = (ver [0] << 48) | (ver [1] << 32) | (ver [2] << 16) + ver [3];
		}
	}

	public virtual string this [string key] {
		set {
			properties [key] = value;
		}
	}

	// Accessors for well known properties

	public virtual string Comments {
		get {
			return (string)properties ["Comments"];
		}
		set {
			properties ["Comments"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string CompanyName {
		get {
			return (string)properties ["CompanyName"];
		}
		set {
			properties ["CompanyName"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string LegalCopyright {
		get {
			return (string)properties ["LegalCopyright"];
		}
		set {
			properties ["LegalCopyright"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string LegalTrademarks {
		get {
			return (string)properties ["LegalTrademarks"];
		}
		set {
			properties ["LegalTrademarks"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string OriginalFilename {
		get {
			return (string)properties ["OriginalFilename"];
		}
		set {
			properties ["OriginalFilename"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string ProductName {
		get {
			return (string)properties ["ProductName"];
		}
		set {
			properties ["ProductName"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string ProductVersion {
		get {
			return (string)properties ["ProductVersion"];
		}
		set {
			properties ["ProductVersion"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string InternalName {
		get {
			return (string)properties ["InternalName"];
		}
		set {
			properties ["InternalName"] = value == String.Empty ? " " : value;
		}
	}

	public virtual string FileDescription {
		get {
			return (string)properties ["FileDescription"];
		}
		set {
			properties ["FileDescription"] = value == String.Empty ? " " : value;
		}
	}

	public virtual int FileLanguage {
		get {
			return file_lang;
		}
		set {
			file_lang = value;
		}
	}

	private void emit_padding (BinaryWriter w) {
		Stream ms = w.BaseStream;

		if ((ms.Position % 4) != 0)
			w.Write ((short)0);
	}

	private void patch_length (BinaryWriter w, long len_pos) {
		Stream ms = w.BaseStream;

		long pos = ms.Position;
		ms.Position = len_pos;
		w.Write ((short)(pos - len_pos));
		ms.Position = pos;
	}

	public override void WriteTo (Stream ms)
	{
		using (BinaryWriter w = new BinaryWriter (ms, Encoding.Unicode)) {
			//
			// See the documentation for the VS_VERSIONINFO structure and
			// its children on MSDN
			//

			// VS_VERSIONINFO
			w.Write ((short)0);
			w.Write ((short)0x34);
			w.Write ((short)0);
			w.Write ("VS_VERSION_INFO".ToCharArray ());
			w.Write ((short)0);

			emit_padding (w);

			// VS_FIXEDFILEINFO
			w.Write ((uint)signature);
			w.Write ((int)struct_version);
			w.Write ((int)(file_version >> 32));
			w.Write ((int)((file_version & 0xffffffff)));

			w.Write ((int)(product_version >> 32));
			w.Write ((int)(product_version & 0xffffffff));
			w.Write ((int)file_flags_mask);
			w.Write ((int)file_flags);
			w.Write ((int)file_os);
			w.Write ((int)file_type);
			w.Write ((int)file_subtype);
			w.Write ((int)(file_date >> 32));
			w.Write ((int)(file_date & 0xffffffff));

			emit_padding (w);

			// VarFileInfo
			long var_file_info_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write ("VarFileInfo".ToCharArray ());
			w.Write ((short)0);

			if ((ms.Position % 4) != 0)
				w.Write ((short)0);

			// Var
			long var_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)4);
			w.Write ((short)0);
			w.Write ("Translation".ToCharArray ());
			w.Write ((short)0);

			if ((ms.Position % 4) != 0)
				w.Write ((short)0);

			w.Write ((short)file_lang);
			w.Write ((short)file_codepage);

			patch_length (w, var_pos);

			patch_length (w, var_file_info_pos);

			// StringFileInfo
			long string_file_info_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write ("StringFileInfo".ToCharArray ());

			emit_padding (w);

			// StringTable
			long string_table_pos = ms.Position;
			w.Write ((short)0);
			w.Write ((short)0);
			w.Write ((short)1);
			w.Write (String.Format ("{0:x4}{1:x4}", file_lang, file_codepage).ToCharArray ());

			emit_padding (w);

			// Strings
			foreach (string key in properties.Keys) {
				string value = (string)properties [key];

				long string_pos = ms.Position;
				w.Write ((short)0);
				w.Write ((short)(value.ToCharArray ().Length + 1));
				w.Write ((short)1);
				w.Write (key.ToCharArray ());
				w.Write ((short)0);

				emit_padding (w);

				w.Write (value.ToCharArray ());
				w.Write ((short)0);

				emit_padding (w);

				patch_length (w, string_pos);
			}

			patch_length (w, string_table_pos);

			patch_length (w, string_file_info_pos);

			patch_length (w, 0);
		}
	}
}

internal class Win32ResFileReader {

	Stream res_file;

	public Win32ResFileReader (Stream s) {
		res_file = s;
	}

	int read_int16 () {
		int b1 = res_file.ReadByte ();
		int b2 = res_file.ReadByte ();

		if ((b1 == -1) || (b2 == -1))
			return -1;
		else
			return b1 | (b2 << 8);
	}

	int read_int32 () {
		int w1 = read_int16 ();
		int w2 = read_int16 ();

		if ((w1 == -1) || (w2 == -1))
			return -1;
		return w1 | (w2 << 16);
	}

	private void read_padding () {
		while ((res_file.Position % 4) != 0)
			read_int16 ();
	}

	NameOrId read_ordinal () {
		int i = read_int16 ();
		if ((i & 0xffff) != 0) {
			int j = read_int16 ();
			return new NameOrId (j);
		}
		else {
			byte[] chars = new byte [16];
			int pos = 0;

			while (true) {
				int j = read_int16 ();
				if (j == 0)
					break;
				if (pos == chars.Length) {
					byte[] new_chars = new byte [chars.Length * 2];
					Array.Copy (chars, new_chars, chars.Length);
					chars = new_chars;
				}
				chars [pos] = (byte)(j >> 8);
				chars [pos + 1] = (byte)(j & 0xff);
				pos += 2;
			}

			return new NameOrId (new String (Encoding.Unicode.GetChars (chars, 0, pos)));
		}					
	}

	public ICollection ReadResources () {
		ArrayList resources = new ArrayList ();

		/* 
		 * We can't use a BinaryReader since we have to keep track of the 
		 * stream position for padding.
		 */

		while (true) {

			read_padding ();

			int data_size = read_int32 ();

			if (data_size == -1)
				/* EOF */
				break;

			int header_size = read_int32 ();
			NameOrId type = read_ordinal ();
			NameOrId name = read_ordinal ();

			read_padding ();

			//int data_version = 
			read_int32 ();
			//int memory_flags =
			read_int16 ();
			int language_id = read_int16 ();
			//int version =
			read_int32 ();
			//int characteristics =
			read_int32 ();

			if (data_size == 0)
				/* Empty resource entry */
				continue;

			byte[] data = new byte [data_size];
			res_file.Read (data, 0, data_size);

			resources.Add (new Win32EncodedResource (type, name, language_id, data));
		}

		return resources;
	}
}

//
// This class represents one icon image in an .ico file
//
internal class ICONDIRENTRY {

	public byte bWidth;
	public byte bHeight;
	public byte bColorCount;
	public byte bReserved;
	public Int16 wPlanes;
	public Int16 wBitCount;
	public Int32 dwBytesInRes;
	public Int32 dwImageOffset;

	public byte[] image;

	public override string ToString () {
		return "ICONDIRENTRY (" + bWidth + "x" + bHeight + " " + wBitCount + " bpp)";
	}
}

//
// This class represents a Reader for Win32 .ico files
//
internal class Win32IconFileReader {

	Stream iconFile;

	public Win32IconFileReader (Stream s) {
		iconFile = s;
	}

	public ICONDIRENTRY[] ReadIcons () {
		ICONDIRENTRY[] icons = null;

		using (BinaryReader r = new BinaryReader (iconFile)) {
			int idReserved = r.ReadInt16 ();
			int idType = r.ReadInt16 ();
			if ((idReserved != 0) || (idType != 1))
				throw new Exception ("Invalid .ico file format");
			long nitems = r.ReadInt16 ();

			icons = new ICONDIRENTRY [nitems];

			for (int i = 0; i < nitems; ++i) {
				ICONDIRENTRY entry = new ICONDIRENTRY ();

				entry.bWidth = r.ReadByte ();
				entry.bHeight = r.ReadByte ();
				entry.bColorCount = r.ReadByte ();
				entry.bReserved = r.ReadByte ();
				entry.wPlanes = r.ReadInt16 ();
				entry.wBitCount = r.ReadInt16 ();
				int dwBytesInRes = r.ReadInt32 ();
				int dwImageOffset = r.ReadInt32 ();

				/* Read image */
				entry.image = new byte [dwBytesInRes];

				long pos = iconFile.Position;
				iconFile.Position = dwImageOffset;
				iconFile.Read (entry.image, 0, dwBytesInRes);
				iconFile.Position = pos;

				/* 
				 * The wPlanes and wBitCount members in the ICONDIRENTRY
				 * structure can be 0, so we set them from the BITMAPINFOHEADER
				 * structure that follows
				 */

				if (entry.wPlanes == 0)
					entry.wPlanes = (short)(entry.image [12] | (entry.image [13] << 8));
				if (entry.wBitCount == 0)
					entry.wBitCount = (short)(entry.image [14] | (entry.image [15] << 8));

				icons [i] = entry;
			}

			return icons;
		}
	}
}	

}
