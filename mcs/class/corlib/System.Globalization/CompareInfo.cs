//
// System.Globalization.CompareInfo
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc. 2002
//

using System.Runtime.Serialization;

namespace System.Globalization
{
	public class CompareInfo : IDeserializationCallback
	{

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
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			throw new NotImplementedException ();
		}
		       
	}
}
