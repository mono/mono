//
// System.Globalization.TextInfo.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
// 	Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Globalization {

	[Serializable]
	public class TextInfo: IDeserializationCallback
	{
		private int lcid;
		
		internal TextInfo ()
		{
		}

		internal TextInfo (int lcid)
		{
			this.lcid=lcid;
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
			return(lcid);
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

			s.Append (Char.ToUpper (str [0]));

			for (int i = 1; i < str.Length; i ++)
				s.Append (str [i]);

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
