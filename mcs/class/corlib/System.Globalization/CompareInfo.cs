//
// System.Globalization.CompareInfo
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc. 2002
//

using System.Reflection;
using System.Runtime.Serialization;

namespace System.Globalization
{
	[Serializable]
	public class CompareInfo : IDeserializationCallback
	{

		/* Hide the .ctor() */
		CompareInfo() {}
		
		public virtual int Compare (string string1, string string2)
		{
			return Compare (string1, string2, CompareOptions.None);
		}

		[MonoTODO]
		public virtual int Compare (string string1, string string2, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		public virtual int Compare (string string1, int offset1, string string2, int offset2)
		{
			return Compare (string1, offset1, string2, offset2, CompareOptions.None);
		}

		[MonoTODO]
		public virtual int Compare (string string1, int offset1, string string2, int offset2, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		public virtual int Compare (string string1, int offset1, int length1, string string2, int offset2, int length2)
		{
			return Compare (string1, offset1, length1, string2, offset2, length2, CompareOptions.None);
		}

		[MonoTODO]
		public virtual int Compare (string string1, int offset1, int length1, string string2, int offset2, int length2, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static CompareInfo GetCompareInfo(int culture)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static CompareInfo GetCompareInfo(string name)
		{
			if(name == null) {
				throw new ArgumentNullException("name is null");
			}
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static CompareInfo GetCompareInfo(int culture, Assembly assembly)
		{
			if(assembly == null) {
				throw new ArgumentNullException("assembly is null");
			}
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static CompareInfo GetCompareInfo(string name, Assembly assembly)
		{
			if(name == null) {
				throw new ArgumentNullException("name is null");
			}
			if(assembly == null) {
				throw new ArgumentNullException("assembly is null");
			}
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual SortKey GetSortKey(string source)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual SortKey GetSortKey(string source,
						  CompareOptions options)
		{
			throw new NotImplementedException();
		}

		public virtual int IndexOf (string source, char value)
		{
			if (source == null)
				throw new ArgumentNullException ();
			return IndexOf (source, value, CompareOptions.None);
		}

		public virtual int IndexOf (string source, string value)
		{
			if (source == null)
				throw new ArgumentNullException ();
			if (value == null)
				throw new ArgumentNullException ();
			return IndexOf (source, value, CompareOptions.None);
		}

		[MonoTODO]
		public virtual int IndexOf (string source, char value, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int IndexOf (string source, char value, int startIndex)
		{
			if (source == null)
				throw new ArgumentNullException ();
			if (startIndex < 0 || startIndex > source.Length)
				throw new ArgumentOutOfRangeException ();
			return IndexOf (source, value, startIndex, CompareOptions.None);
		}
		
		[MonoTODO]
		public virtual int IndexOf (string source, string value, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		public virtual int IndexOf (string source, string value, int startIndex)
		{
			if (source == null)
				throw new ArgumentNullException ();
			if (value == null)
				throw new ArgumentNullException ();
			if (startIndex < 0 || startIndex > source.Length)
				throw new ArgumentOutOfRangeException ();
			return IndexOf (source, value, startIndex, CompareOptions.None);
		}

		[MonoTODO]
		public virtual int IndexOf (string source, char value, int startIndex, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int IndexOf (string source, char value, int startIndex, int count)
		{
			if (source == null)
				throw new ArgumentNullException ();
			if (startIndex < 0 || startIndex > source.Length)
				throw new ArgumentOutOfRangeException ();
			if (count < 0)
				throw new ArgumentOutOfRangeException ();
			return IndexOf (source, value, startIndex, count, CompareOptions.None);
		}

		public virtual int IndexOf (string source, string value, int startIndex, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		public virtual int IndexOf (string source, string value, int startIndex, int count)
		{
			if (source == null)
				throw new ArgumentNullException ();
			if (value == null)
				throw new ArgumentNullException ();
			if (startIndex < 0 || startIndex > source.Length)
				throw new ArgumentOutOfRangeException ();
			if (count < 0)
				throw new ArgumentOutOfRangeException ();
			return IndexOf (source, value, startIndex, count, CompareOptions.None);
		}
		
		[MonoTODO]
		public virtual int IndexOf (string source, char value, int startIndex, int count, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int IndexOf (string source, string value, int startIndex, int count, CompareOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsPrefix(string source, string prefix)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(prefix == null) {
				throw new ArgumentNullException("prefix is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsPrefix(string source, string prefix, CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(prefix == null) {
				throw new ArgumentNullException("prefix is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSuffix(string source, string suffix)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(suffix == null) {
				throw new ArgumentNullException("suffix is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSuffix(string source, string suffix, CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(suffix == null) {
				throw new ArgumentNullException("suffix is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, char value)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, string value)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(value == null) {
				throw new ArgumentNullException("value is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, char value,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, char value,
					       int startIndex)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, string value,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(value == null) {
				throw new ArgumentNullException("value is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, string value,
					       int startIndex)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(value == null) {
				throw new ArgumentNullException("value is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, char value,
					       int startIndex,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, char value,
					       int startIndex, int count)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(count < 0) {
				throw new ArgumentOutOfRangeException("count is less than zero");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, string value,
					       int startIndex,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(value == null) {
				throw new ArgumentNullException("value is null");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, string value,
					       int startIndex, int count)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(value == null) {
				throw new ArgumentNullException("value is null");
			}
			if(count < 0) {
				throw new ArgumentOutOfRangeException("count is less than zero");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, char value,
					       int startIndex, int count,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(count < 0) {
				throw new ArgumentOutOfRangeException("count is less than zero");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int LastIndexOf(string source, string value,
					       int startIndex, int count,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source is null");
			}
			if(value == null) {
				throw new ArgumentNullException("value is null");
			}
			if(count < 0) {
				throw new ArgumentOutOfRangeException("count is less than zero");
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			throw new NotImplementedException ();
		}

		/* LAMESPEC: not mentioned in the spec, but corcompare
		 * shows it.  Some documentation about what it does
		 * would be nice.
		 */
		[MonoTODO]
		public Int32 LCID
		{
			get {
				throw new NotImplementedException();
			}
		}
	}
}
