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

namespace System.Globalization {

	[Serializable]
	public class TextInfo: IDeserializationCallback
	{
		
		[StructLayout (LayoutKind.Sequential)]
		struct Data {
			public int ansi;
			public int ebcdic;
			public int mac;
			public int oem;
			public byte list_sep;
		}
		
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

		public virtual char ToLower(char c)
		{
			return Char.ToLower (c);
		}
		
		public virtual string ToLower(string str)
		{
			if(str==null) 
				throw new ArgumentNullException("string is null");

			return str.ToLower (ci);
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

		public virtual char ToUpper (char c)
		{
			return Char.ToUpper (c, ci);
		}

		public virtual string ToUpper (string str)
		{
			if(str==null)
				throw new ArgumentNullException("string is null");
			
			return str.ToUpper (ci);
		}

		/* IDeserialization interface */
		[MonoTODO]
		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}
	}
}
