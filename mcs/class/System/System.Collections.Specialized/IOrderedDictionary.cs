//
// System.Collections.Specialized/IOrderedDictionary.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

#if NET_1_2

namespace System.Collections.Specialized
{
	public interface IOrderedDictionary : IDictionary
	{
		void Insert (int idx, object key, object value);
		void RemoveAt (int idx);
		
		object this[int idx] {
			get; set;
		}
	}
}

#endif
