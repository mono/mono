//
// System.Globalization.SortKey.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

namespace System.Globalization {

	[Serializable]
	public class SortKey {
		private string str;
		private CompareOptions options;
		private byte[] key;
		private int lcid;
		
		/* Hide the .ctor() */
		SortKey() {}

		internal SortKey (int lcid, string source,
				  CompareOptions options)
		{
			this.lcid=lcid;
			str=source;
			this.options=options;
		}

		public virtual byte[] KeyData
		{
			get {
				return(key);
			}
		}

		public virtual string OriginalString
		{
			get {
				return(str);
			}
		}

		public static int Compare(SortKey sortkey1, SortKey sortkey2)
		{
			if(sortkey1==null) {
				throw new ArgumentNullException ("sortkey1");
			}
			if(sortkey2==null) {
				throw new ArgumentNullException ("sortkey2");
			}

			byte[] keydata1=sortkey1.key;
			byte[] keydata2=sortkey2.key;

			if(keydata1.Length==0) {
				if(keydata2.Length==0) {
					return(0);
				}
				return(-1);
			}
			
			int min_len=(keydata1.Length < keydata2.Length)?
				keydata1.Length:keydata2.Length;

			for(int i=0; i<min_len; i++) {
				if(keydata1[i] > keydata2[i]) {
					return(1);
				} else if(keydata1[i] < keydata2[i]) {
					return(-1);
				}
			}

			if(keydata1.Length < keydata2.Length) {
				return(-1);
			} else if (keydata1.Length > keydata2.Length) {
				return(1);
			} else {
				return(0);
			}
		}

		public override bool Equals(object value)
		{
			SortKey other=(value as SortKey);
			if(other!=null) {
				if((this.lcid==other.lcid) &&
				   (this.options==other.options) &&
				   (Compare (this, other)==0)) {
					return(true);
				}
			}

			return(false);
		}

		public override int GetHashCode()
		{
			return(str.GetHashCode ());
		}

		public override string ToString()
		{
			return("SortKey - "+lcid+", "+options+", "+str);
		}
	}
}
