//
// System.Globalization.TextInfo.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
// 	Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc.
//
// TODO:
//   Missing the various code page mappings.
//   Missing the OnDeserialization implementation.
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
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Globalization {

	[Serializable]
	public class TextInfo: IDeserializationCallback
	{
		private delegate char CharConverter (char c);
		
		[StructLayout (LayoutKind.Sequential)]
		struct Data {
			public int ansi;
			public int ebcdic;
			public int mac;
			public int oem;
			public byte list_sep;
		}

		CharConverter toLower;
		CharConverter toUpper;

		int m_win32LangID;
		int m_nDataItem;
		bool m_useUserOverride;

		[NonSerialized]
		readonly CultureInfo ci;
		
		[NonSerialized]
		readonly Data data;

		internal unsafe TextInfo (CultureInfo ci, int lcid, void* data)
		{
			this.m_win32LangID = lcid;
			this.ci = ci;
			if (data != null)
				this.data = *(Data*) data;
			else {
				this.data = new Data ();
				this.data.list_sep = (byte) '.';
			}
			toLower = new CharConverter (ToLower);
			toUpper = new CharConverter (ToUpper);
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

		public virtual string ListSeparator 
		{
			get {
				
				return ((char) data.list_sep).ToString ();
			}
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
				throw new ArgumentNullException("string is null");
			
			Text.StringBuilder s = new Text.StringBuilder ();
			bool space_seen = true;
				
			for (int i = 0; i < str.Length; i ++){
				char c = str [i];
				if (Char.IsLetter (c)){
					if (space_seen)
						s.Append (Char.ToUpper (c, ci));
					else
						s.Append (Char.ToLower (c, ci));
					space_seen = false;
				} else {
					s.Append (c);
					if (Char.IsWhiteSpace (c))
						space_seen = true;
				}
			}

			return s.ToString ();
		}

		// Only Azeri and Turkish have their own special cases.
		// Other than them, all languages have common special case
		// (enumerable enough).
		public virtual char ToLower (char c)
		{
			if (ci == CultureInfo.InvariantCulture)
				return Char.ToLowerInvariant (c);

			switch ((int) c) {
			case '\u0049': // Latin uppercase I
				CultureInfo tmp = ci;
				while (tmp.Parent != tmp && tmp.Parent != CultureInfo.InvariantCulture)
					tmp = tmp.Parent;
				switch (tmp.LCID) {
				case 44: // Azeri (az)
				case 31: // Turkish (tr)
					return '\u0131'; // I becomes dotless i
				}
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
			if (ci == CultureInfo.InvariantCulture)
				return Char.ToUpperInvariant (c);

			switch (c) {
			case '\u0069': // Latin lowercase i
				CultureInfo tmp = ci;
				while (tmp.Parent != tmp && tmp.Parent != CultureInfo.InvariantCulture)
					tmp = tmp.Parent;
				switch (tmp.LCID) {
				case 44: // Azeri (az)
				case 31: // Turkish (tr)
					return '\u0130'; // dotted capital I
				}
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

		public virtual string ToLower (string s)
		{
			// In ICU (3.2) there are a few cases that one single
			// character results in multiple characters in e.g.
			// tr-TR culture. So I tried brute force conversion
			// test with single character as a string input, but 
			// there was no such conversion. So I think it just
			// invokes ToLower(char).
			return Transliterate (s, toLower);
		}

		public virtual string ToUpper (string s)
		{
			// In ICU (3.2) there is a case that string
			// is handled beyond per-character conversion, but
			// it is only lt-LT culture where MS.NET does not
			// handle any special transliteration. So I keep
			// ToUpper() just as character conversion.
			return Transliterate (s, toUpper);
		}

		private string Transliterate (string s, CharConverter convert)
		{
			if (s == null)
				throw new ArgumentNullException("string is null");
			StringBuilder sb = null;
			int start = 0;
			for (int i = 0; i < s.Length; i++) {
				if (s [i] != convert (s [i])) {
					if (sb == null)
						sb = new StringBuilder (s.Length);
					sb.Append (s.Substring (start, i - start));
					sb.Append (convert (s [i]));
					start = i + 1;
				}
			}
			if (sb != null && start < s.Length)
				sb.Append (s.Substring (start));
			return sb == null ? s : sb.ToString ();
		}

		/* IDeserialization interface */
		[MonoTODO]
		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}
	}
}
