//
// System.Globalization.TextInfo.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Globalization {

	[Serializable]
	public class TextInfo: IDeserializationCallback {

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

		/*
		[MonoTODO]
		public override bool Equals(object obj)
		{
			return(false);
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			return(0);
		}
		*/

		[MonoTODO]
		public virtual char ToLower(char c)
		{
			return('X');
		}
		
		[MonoTODO]
		public virtual string ToLower(string str)
		{
			if(str==null) {
				throw new ArgumentNullException("string is null");
			}
			
			return("");
		}
		
		[MonoTODO]
		public override string ToString()
		{
			return("TextInfo");
		}

		[MonoTODO]
		public string ToTitleCase(string str)
		{
			if(str==null) {
				throw new ArgumentNullException("string is null");
			}
			
			return("");
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
