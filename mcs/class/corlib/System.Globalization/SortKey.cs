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
		/* Hide the .ctor() */
		SortKey() {}

		[MonoTODO]
		public virtual byte[] KeyData
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public virtual string OriginalString
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public static int Compare(SortKey sortkey1, SortKey sortkey2)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool Equals(object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}
	}
}
