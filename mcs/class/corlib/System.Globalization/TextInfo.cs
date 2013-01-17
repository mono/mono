//
// System.Globalization.TextInfo.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
// 	Duncan Mak (duncan@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.
// (C) 2005 Novell, Inc.
//
// TODO:
//   Missing the various code page mappings.
//   Missing the OnDeserialization implementation.
//
// Copyright (C) 2004, 2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Globalization {

	[Serializable]
	[ComVisible (true)]
	[MonoTODO ("IDeserializationCallback isn't implemented.")]
	public class TextInfo: IDeserializationCallback, ICloneable
	{
		[StructLayout (LayoutKind.Sequential)]
		struct Data {
			public int ansi;
			public int ebcdic;
			public int mac;
			public int oem;
			public bool right_to_left;
			public byte list_sep;
		}

		string m_listSeparator;
		bool m_isReadOnly;
		string customCultureName;

#pragma warning disable 169
		[NonSerialized]
		int m_nDataItem;
		bool m_useUserOverride;
#pragma warning restore 169		

		int m_win32LangID;

		[NonSerialized]
		readonly CultureInfo ci;

		[NonSerialized]
		readonly bool handleDotI;

		[NonSerialized]
		readonly Data data;

		internal unsafe TextInfo (CultureInfo ci, int lcid, void* data, bool read_only)
		{
			this.m_isReadOnly = read_only;
			this.m_win32LangID = lcid;
			this.ci = ci;
			if (data != null)
				this.data = *(Data*) data;
			else {
				this.data = new Data ();
				this.data.list_sep = (byte) ',';
			}

			CultureInfo tmp = ci;
			while (tmp.Parent != null && tmp.Parent.LCID != 0x7F && tmp.Parent != tmp)
				tmp = tmp.Parent;

			if (tmp != null) {
				switch (tmp.LCID) {
				case 44: // Azeri (az)
				case 31: // Turkish (tr)
					handleDotI = true;
					break;
				}
			}
		}

		private TextInfo (TextInfo textInfo)
		{
			m_win32LangID = textInfo.m_win32LangID;
			m_nDataItem = textInfo.m_nDataItem;
			m_useUserOverride = textInfo.m_useUserOverride;
			m_listSeparator = textInfo.ListSeparator;
			customCultureName = textInfo.CultureName;
			ci = textInfo.ci;
			handleDotI = textInfo.handleDotI;
			data = textInfo.data;
		}

		public virtual int ANSICodePage
		{
			get {
				return data.ansi;
			}
		}

		public virtual int EBCDICCodePage
		{
			get {
				return data.ebcdic;
			}
		}

		[ComVisible (false)]
		public int LCID {
			get { return m_win32LangID; }
		}

		public virtual string ListSeparator {
			get {
				if (m_listSeparator == null)
					m_listSeparator = ((char) data.list_sep).ToString ();
				return m_listSeparator;
			}
			[ComVisible (false)]
			set { m_listSeparator = value; }
		}

		public virtual int MacCodePage
		{
			get {
				return data.mac;
			}
		}

		public virtual int OEMCodePage
		{
			get {
				return data.oem;
			}
		}

		[ComVisible (false)]
		public string CultureName {
			get {
				if (customCultureName == null)
					customCultureName = ci == null ? String.Empty : ci.Name;
				return customCultureName;
			}
		}

		[ComVisible (false)]
		public bool IsReadOnly {
			get { return m_isReadOnly; }
		}

		[ComVisible (false)]
		public bool IsRightToLeft {
			get {
				return data.right_to_left;
			}
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			TextInfo other = obj as TextInfo;
			if (other == null)
				return false;
			if (other.m_win32LangID != m_win32LangID)
				return false;
			if (other.ci != ci)
				return false;
			return true;
		}

		public override int GetHashCode()
		{
			return (m_win32LangID);
		}
		
		public override string ToString()
		{
			return "TextInfo - " + m_win32LangID;
		}

		public string ToTitleCase (string str)
		{
			if(str == null)
				throw new ArgumentNullException ("str");

			StringBuilder sb = null;
			int i = 0;
			int start = 0;
			while (i < str.Length) {
				if (!Char.IsLetter (str [i++]))
					continue;
				i--;
				char t = ToTitleCase (str [i]);
				bool capitalize = true;
				if (t == str [i]) {
					capitalize = false;
					bool allTitle = true;
					// if the word is all titlecase,
					// then don't capitalize it.
					int saved = i;
					while (++i < str.Length) {
						if (Char.IsWhiteSpace (str [i]))
							break;
						t = ToTitleCase (str [i]);
						if (t != str [i]) {
							allTitle = false;
							break;
						}
					}
					if (allTitle)
						continue;
					i = saved;

					// still check if all remaining
					// characters are lowercase,
					// where we don't have to modify
					// the source word.
					while (++i < str.Length) {
						if (Char.IsWhiteSpace (str [i]))
							break;
						if (ToLower (str [i]) != str [i]) {
							capitalize = true;
							i = saved;
							break;
						}
					}
				}

				if (capitalize) {
					if (sb == null)
						sb = new StringBuilder (str.Length);
					sb.Append (str, start, i - start);
					sb.Append (ToTitleCase (str [i]));
					start = i + 1;
					while (++i < str.Length) {
						if (Char.IsWhiteSpace (str [i]))
							break;
						sb.Append (ToLower (str [i]));
					}
					start = i;
				}
			}
			if (sb != null)
				sb.Append (str, start, str.Length - start);

			return sb != null ? sb.ToString () : str;
		}

		// Only Azeri and Turkish have their own special cases.
		// Other than them, all languages have common special case
		// (enumerable enough).
		public virtual char ToLower (char c)
		{
			// quick ASCII range check
			if (c < 0x40 || 0x60 < c && c < 128)
				return c;
			else if ('A' <= c && c <= 'Z' && (!handleDotI || c != 'I'))
				return (char) (c + 0x20);

			if (ci == null || ci.LCID == 0x7F)
				return Char.ToLowerInvariant (c);

			switch (c) {
			case '\u0049': // Latin uppercase I
				if (handleDotI)
					return '\u0131'; // I becomes dotless i
				break;
			case '\u0130': // I-dotted
				return '\u0069'; // i

			case '\u01c5': // LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON
				return '\u01c6';
			// \u01c7 -> \u01c9 (LJ) : invariant
			case '\u01c8': // LATIN CAPITAL LETTER L WITH SMALL LETTER J
				return '\u01c9';
			// \u01ca -> \u01cc (NJ) : invariant
			case '\u01cb': // LATIN CAPITAL LETTER N WITH SMALL LETTER J
				return '\u01cc';
			// WITH CARON : invariant
			// WITH DIAERESIS AND * : invariant

			case '\u01f2': // LATIN CAPITAL LETTER D WITH SMALL LETTER Z
				return '\u01f3';
			case '\u03d2':  // ? it is not in ICU
				return '\u03c5';
			case '\u03d3':  // ? it is not in ICU
				return '\u03cd';
			case '\u03d4':  // ? it is not in ICU
				return '\u03cb';
			}
			return Char.ToLowerInvariant (c);
		}

		public virtual char ToUpper (char c)
		{
			// quick ASCII range check
			if (c < 0x60)
				return c;
			else if ('a' <= c && c <= 'z' && (!handleDotI || c != 'i'))
				return (char) (c - 0x20);

			if (ci == null || ci.LCID == 0x7F)
				return Char.ToUpperInvariant (c);

			switch (c) {
			case '\u0069': // Latin lowercase i
				if (handleDotI)
					return '\u0130'; // dotted capital I
				break;
			case '\u0131': // dotless i
				return '\u0049'; // I

			case '\u01c5': // see ToLower()
				return '\u01c4';
			case '\u01c8': // see ToLower()
				return '\u01c7';
			case '\u01cb': // see ToLower()
				return '\u01ca';
			case '\u01f2': // see ToLower()
				return '\u01f1';
			case '\u0390': // GREEK SMALL LETTER IOTA WITH DIALYTIKA AND TONOS
				return '\u03aa'; // it is not in ICU
			case '\u03b0': // GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND TONOS
				return '\u03ab'; // it is not in ICU
			case '\u03d0': // GREEK BETA
				return '\u0392';
			case '\u03d1': // GREEK THETA
				return '\u0398';
			case '\u03d5': // GREEK PHI
				return '\u03a6';
			case '\u03d6': // GREEK PI
				return '\u03a0';
			case '\u03f0': // GREEK KAPPA
				return '\u039a';
			case '\u03f1': // GREEK RHO
				return '\u03a1';
			// am not sure why miscellaneous GREEK symbols are 
			// not handled here.
			}

			return Char.ToUpperInvariant (c);
		}

		private char ToTitleCase (char c)
		{
			// Handle some Latin characters.
			switch (c) {
			case '\u01c4':
			case '\u01c5':
			case '\u01c6':
				return '\u01c5';
			case '\u01c7':
			case '\u01c8':
			case '\u01c9':
				return '\u01c8';
			case '\u01ca':
			case '\u01cb':
			case '\u01cc':
				return '\u01cb';
			case '\u01f1':
			case '\u01f2':
			case '\u01f3':
				return '\u01f2';
			}
			if ('\u2170' <= c && c <= '\u217f' || // Roman numbers
				'\u24d0' <= c && c <= '\u24e9')
				return c;
			return ToUpper (c);
		}

		public unsafe virtual string ToLower (string str)
		{
			// In ICU (3.2) there are a few cases that one single
			// character results in multiple characters in e.g.
			// tr-TR culture. So I tried brute force conversion
			// test with single character as a string input, but 
			// there was no such conversion. So I think it just
			// invokes ToLower(char).
			if (str == null)
				throw new ArgumentNullException ("str");

			if (str.Length == 0)
				return String.Empty;

			string tmp = String.InternalAllocateStr (str.Length);
			fixed (char* source = str, dest = tmp) {

				char* destPtr = (char*)dest;
				char* sourcePtr = (char*)source;

				for (int n = 0; n < str.Length; n++) {
					*destPtr = ToLower (*sourcePtr);
					sourcePtr++;
					destPtr++;
				}
			}
			return tmp;
		}

		public unsafe virtual string ToUpper (string str)
		{
			// In ICU (3.2) there is a case that string
			// is handled beyond per-character conversion, but
			// it is only lt-LT culture where MS.NET does not
			// handle any special transliteration. So I keep
			// ToUpper() just as character conversion.
			if (str == null)
				throw new ArgumentNullException ("str");

			if (str.Length == 0)
				return String.Empty;

			string tmp = String.InternalAllocateStr (str.Length);
			fixed (char* source = str, dest = tmp) {

				char* destPtr = (char*)dest;
				char* sourcePtr = (char*)source;

				for (int n = 0; n < str.Length; n++) {
					*destPtr = ToUpper (*sourcePtr);
					sourcePtr++;
					destPtr++;
				}
			}
			return tmp;
		}

		[ComVisible (false)]
		public static TextInfo ReadOnly (TextInfo textInfo)
		{
			if (textInfo == null)
				throw new ArgumentNullException ("textInfo");

			TextInfo ti = new TextInfo (textInfo);
			ti.m_isReadOnly = true;
			return ti;
		}

		/* IDeserialization interface */
		[MonoTODO]
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			// FIXME: we need to re-create "data" in order to get most properties working
		}

		/* IClonable */
		[ComVisible (false)]
		public virtual object Clone ()
		{
			return new TextInfo (this);
		}
	}
}
