//
// System.Globalization.TextInfo.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
// 	Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc.
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

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Globalization {

	[Serializable]
	public class TextInfo: IDeserializationCallback
	{
		private int m_win32LangID;
		int m_nDataItem;
		bool m_useUserOverride;
		
		internal TextInfo ()
		{
		}

		internal TextInfo (int lcid)
		{
			this.m_win32LangID=lcid;
		}

		[MonoTODO]
		public virtual int ANSICodePage
		{
			get {
				return(0);
			}
		}

		[MonoTODO]
		public virtual int EBCDICCodePage
		{
			get {
				return(0);
			}
		}
		
		[MonoTODO]
		public virtual string ListSeparator 
		{
			get {
				return(",");
			}
		}

		[MonoTODO]
		public virtual int MacCodePage
		{
			get {
				return(0);
			}
		}
		
		[MonoTODO]
		public virtual int OEMCodePage
		{
			get {
				return(0);
			}
		}

		[MonoTODO]
		public override bool Equals(object obj)
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return(m_win32LangID);
		}

		[MonoTODO]
		public virtual char ToLower(char c)
		{
			return Char.ToLower (c);
		}
		
		[MonoTODO]
		public virtual string ToLower(string str)
		{
			if(str==null) {
				throw new ArgumentNullException("string is null");
			}
			
			Text.StringBuilder s = new Text.StringBuilder ();

			foreach (char c in str) {
				s.Append (Char.ToLower (c));
			}

			return s.ToString ();
		}
		
		[MonoTODO]
		public override string ToString()
		{
			return("TextInfo");
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
						s.Append (Char.ToUpper (c));
					else
						s.Append (Char.ToLower (c));
					space_seen = false;
				} else {
					s.Append (c);
					if (Char.IsWhiteSpace (c))
						space_seen = true;
				}
			}

			return s.ToString ();
		}

		[MonoTODO]
		public virtual char ToUpper(char c)
		{
			return('X');
		}

		[MonoTODO]
		public virtual string ToUpper(string str)
		{
			if(str==null) {
				throw new ArgumentNullException("string is null");
			}
			
			return("");
		}

		/* IDeserialization interface */
		[MonoTODO]
		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}
	}
}
