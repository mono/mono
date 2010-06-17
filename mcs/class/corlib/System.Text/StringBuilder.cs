// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Text.StringBuilder
//
// Authors: 
//   Marcin Szczepanski (marcins@zipworld.com.au)
//   Paolo Molaro (lupus@ximian.com)
//   Patrik Torstensson
//
// NOTE: In the case the buffer is only filled by 50% a new string
//       will be returned by ToString() is cached in the '_cached_str'
//		 cache_string will also control if a string has been handed out
//		 to via ToString(). If you are chaning the code make sure that
//		 if you modify the string data set the cache_string to null.
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
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text {
	
	[Serializable]
	[ComVisible (true)]
        [MonoLimitation ("Serialization format not compatible with .NET")]
	public sealed class StringBuilder : ISerializable
	{
		private int _length;
		private string _str;
		private string _cached_str;
		
		private int _maxCapacity;
		private const int constDefaultCapacity = 16;

		public StringBuilder(string value, int startIndex, int length, int capacity) 
			: this (value, startIndex, length, capacity, Int32.MaxValue)
		{
		}

		private StringBuilder(string value, int startIndex, int length, int capacity, int maxCapacity)
		{
			// first, check the parameters and throw appropriate exceptions if needed
			if (null == value)
				value = "";

			// make sure startIndex is zero or positive
			if (startIndex < 0)
				throw new System.ArgumentOutOfRangeException ("startIndex", startIndex, "StartIndex cannot be less than zero.");

			// make sure length is zero or positive
			if(length < 0)
				throw new System.ArgumentOutOfRangeException ("length", length, "Length cannot be less than zero.");

			if (capacity < 0)
				throw new System.ArgumentOutOfRangeException ("capacity", capacity, "capacity must be greater than zero.");

			if (maxCapacity < 1)
				throw new System.ArgumentOutOfRangeException ("maxCapacity", "maxCapacity is less than one.");
			if (capacity > maxCapacity)
				throw new System.ArgumentOutOfRangeException ("capacity", "Capacity exceeds maximum capacity.");

			// make sure startIndex and length give a valid substring of value
			// re-ordered to avoid possible integer overflow
			if (startIndex > value.Length - length)
				throw new System.ArgumentOutOfRangeException ("startIndex", startIndex, "StartIndex and length must refer to a location within the string.");

			if (capacity == 0) {
				if (maxCapacity > constDefaultCapacity)
					capacity = constDefaultCapacity;
				else
					_str = _cached_str = String.Empty;
			}
			_maxCapacity = maxCapacity;

			if (_str == null)
				_str = String.InternalAllocateStr ((length > capacity) ? length : capacity);
			if (length > 0)
				String.CharCopy (_str, 0, value, startIndex, length);
			
			_length = length;
		}

		public StringBuilder () : this (null) {}

		public StringBuilder(int capacity) : this (String.Empty, 0, 0, capacity) {}

		public StringBuilder(int capacity, int maxCapacity) : this (String.Empty, 0, 0, capacity, maxCapacity) { }

		public StringBuilder (string value)
		{
			/*
			 * This is an optimization to avoid allocating the internal string
			 * until the first Append () call.
			 * The runtime pinvoke marshalling code needs to be aware of this.
			 */
			if (null == value)
				value = "";
			
			_length = value.Length;
			_str = _cached_str = value;
			_maxCapacity = Int32.MaxValue;
		}
	
		public StringBuilder( string value, int capacity) : this(value == null ? "" : value, 0, value == null ? 0 : value.Length, capacity) {}
	
		public int MaxCapacity {
			get {
				return _maxCapacity;
			}
		}

		public int Capacity {
			get {
				if (_str.Length == 0)
					return Math.Min (_maxCapacity, constDefaultCapacity);
				
				return _str.Length;
			}

			set {
				if (value < _length)
					throw new ArgumentException( "Capacity must be larger than length" );

				if (value > _maxCapacity)
					throw new ArgumentOutOfRangeException ("value", "Should be less than or equal to MaxCapacity");

				InternalEnsureCapacity(value);
			}
		}

		public int Length {
			get {
				return _length;
			}

			set {
				if( value < 0 || value > _maxCapacity)
					throw new ArgumentOutOfRangeException();

				if (value == _length)
					return;

				if (value < _length) {
					// LAMESPEC:  The spec is unclear as to what to do
					// with the capacity when truncating the string.

					// Do as MS, keep the capacity
					
					// Make sure that we invalidate any cached string.
					InternalEnsureCapacity (value);
					_length = value;
				} else {
					// Expand the capacity to the new length and
					// pad the string with NULL characters.
					Append('\0', value - _length);
				}
			}
		}

		[IndexerName("Chars")]
		public char this [int index] {
			get {
				if (index >= _length || index < 0)
					throw new IndexOutOfRangeException();

				return _str [index];
			} 

			set {
				if (index >= _length || index < 0)
					throw new IndexOutOfRangeException();

				if (null != _cached_str)
					InternalEnsureCapacity (_length);
				
				_str.InternalSetChar (index, value);
			}
		}

		public override string ToString () 
		{
			if (_length == 0)
				return String.Empty;

			if (null != _cached_str)
				return _cached_str;

			// If we only have a half-full buffer we return a new string.
			if (_length < (_str.Length >> 1) || (_str.Length > string.LOS_limit && _length <= string.LOS_limit))
			{
				// use String.SubstringUnchecked instead of String.Substring
				// as the former is guaranteed to create a new string object
				_cached_str = _str.SubstringUnchecked (0, _length);
				return _cached_str;
			}

			_cached_str = _str;
			_str.InternalSetLength(_length);

			return _str;
		}

		public string ToString (int startIndex, int length) 
		{
			// re-ordered to avoid possible integer overflow
			if (startIndex < 0 || length < 0 || startIndex > _length - length)
				throw new ArgumentOutOfRangeException();

			// use String.SubstringUnchecked instead of String.Substring
			// as the former is guaranteed to create a new string object
			if (startIndex == 0 && length == _length)
				return ToString ();
			else
				return _str.SubstringUnchecked (startIndex, length);
		}

		public int EnsureCapacity (int capacity) 
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("Capacity must be greater than 0." );

			if( capacity <= _str.Length )
				return _str.Length;

			InternalEnsureCapacity (capacity);

			return _str.Length;
		}

		public bool Equals (StringBuilder sb) 
		{
			if (((object)sb) == null)
				return false;
			
			if (_length == sb.Length && _str == sb._str )
				return true;

			return false;
		}

		public StringBuilder Remove (int startIndex, int length)
		{
			// re-ordered to avoid possible integer overflow
			if (startIndex < 0 || length < 0 || startIndex > _length - length)
				throw new ArgumentOutOfRangeException();
			
			if (null != _cached_str)
				InternalEnsureCapacity (_length);
			
			// Copy everything after the 'removed' part to the start 
			// of the removed part and truncate the sLength
			if (_length - (startIndex + length) > 0)
				String.CharCopy (_str, startIndex, _str, startIndex + length, _length - (startIndex + length));

			_length -= length;

			return this;
		}			       

		public StringBuilder Replace (char oldChar, char newChar) 
		{
			return Replace( oldChar, newChar, 0, _length);
		}

		public StringBuilder Replace (char oldChar, char newChar, int startIndex, int count) 
		{
			// re-ordered to avoid possible integer overflow
			if (startIndex > _length - count || startIndex < 0 || count < 0)
				throw new ArgumentOutOfRangeException();

			if (null != _cached_str)
				InternalEnsureCapacity (_str.Length);

			for (int replaceIterate = startIndex; replaceIterate < startIndex + count; replaceIterate++ ) {
				if( _str [replaceIterate] == oldChar )
					_str.InternalSetChar (replaceIterate, newChar);
			}

			return this;
		}

		public StringBuilder Replace( string oldValue, string newValue ) {
			return Replace (oldValue, newValue, 0, _length);
		}

		public StringBuilder Replace( string oldValue, string newValue, int startIndex, int count ) 
		{
			if (oldValue == null)
				throw new ArgumentNullException ("The old value cannot be null.");

			if (startIndex < 0 || count < 0 || startIndex > _length - count)
				throw new ArgumentOutOfRangeException ();

			if (oldValue.Length == 0)
				throw new ArgumentException ("The old value cannot be zero length.");

			string substr = _str.Substring(startIndex, count);
			string replace = substr.Replace(oldValue, newValue);
			// return early if no oldValue was found
			if ((object) replace == (object) substr)
				return this;

			InternalEnsureCapacity (replace.Length + (_length - count));

			// shift end part
			if (replace.Length < count)
				String.CharCopy (_str, startIndex + replace.Length, _str, startIndex + count, _length - startIndex  - count);
			else if (replace.Length > count)
				String.CharCopyReverse (_str, startIndex + replace.Length, _str, startIndex + count, _length - startIndex  - count);

			// copy middle part back into _str
			String.CharCopy (_str, startIndex, replace, 0, replace.Length);
			
			_length = replace.Length + (_length - count);

			return this;
		}

		      
		/* The Append Methods */
		public StringBuilder Append (char[] value) 
		{
			if (value == null)
				return this;

			int needed_cap = _length + value.Length;
			if (null != _cached_str || _str.Length < needed_cap)
				InternalEnsureCapacity (needed_cap);
			
			String.CharCopy (_str, _length, value, 0, value.Length);
			_length = needed_cap;

			return this;
		} 
		
		public StringBuilder Append (string value) 
		{
			if (value == null)
				return this;
			
			if (_length == 0 && value.Length < _maxCapacity && value.Length > _str.Length) {
				_length = value.Length;
				_str = _cached_str = value;
				return this;
			}

			int needed_cap = _length + value.Length;
			if (null != _cached_str || _str.Length < needed_cap)
				InternalEnsureCapacity (needed_cap);

			String.CharCopy (_str, _length, value, 0, value.Length);
			_length = needed_cap;
			return this;
		}

		public StringBuilder Append (bool value) {
			return Append (value.ToString());
		}
		
		public StringBuilder Append (byte value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (decimal value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (double value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (short value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (int value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (long value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (object value) {
			if (value == null)
				return this;

			return Append (value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append (sbyte value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (float value) {
			return Append (value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append (ushort value) {
			return Append (value.ToString());
		}	
		
		[CLSCompliant(false)]
		public StringBuilder Append (uint value) {
			return Append (value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append (ulong value) {
			return Append (value.ToString());
		}

		public StringBuilder Append (char value) 
		{
			int needed_cap = _length + 1;
			if (null != _cached_str || _str.Length < needed_cap)
				InternalEnsureCapacity (needed_cap);

			_str.InternalSetChar(_length, value);
			_length = needed_cap;

			return this;
		}

		public StringBuilder Append (char value, int repeatCount) 
		{
			if( repeatCount < 0 )
				throw new ArgumentOutOfRangeException();

			InternalEnsureCapacity (_length + repeatCount);
			
			for (int i = 0; i < repeatCount; i++)
				_str.InternalSetChar (_length++, value);

			return this;
		}

		public StringBuilder Append( char[] value, int startIndex, int charCount ) 
		{
			if (value == null) {
				if (!(startIndex == 0 && charCount == 0))
					throw new ArgumentNullException ("value");

				return this;
			}

			if ((charCount < 0 || startIndex < 0) || (startIndex > value.Length - charCount)) 
				throw new ArgumentOutOfRangeException();
			
			int needed_cap = _length + charCount;
			InternalEnsureCapacity (needed_cap);

			String.CharCopy (_str, _length, value, startIndex, charCount);
			_length = needed_cap;

			return this;
		}

		public StringBuilder Append (string value, int startIndex, int count) 
		{
			if (value == null) {
				if (startIndex != 0 && count != 0)
					throw new ArgumentNullException ("value");
					
				return this;
			}

			if ((count < 0 || startIndex < 0) || (startIndex > value.Length - count))
				throw new ArgumentOutOfRangeException();
			
			int needed_cap = _length + count;
			if (null != _cached_str || _str.Length < needed_cap)
				InternalEnsureCapacity (needed_cap);

			String.CharCopy (_str, _length, value, startIndex, count);
			
			_length = needed_cap;

			return this;
		}

#if NET_4_0 || MOONLIGHT
		public StringBuilder Clear ()
		{
			_length = 0;
			return this;
		}
#endif

		[ComVisible (false)]
		public StringBuilder AppendLine ()
		{
			return Append (System.Environment.NewLine);
		}

		[ComVisible (false)]
		public StringBuilder AppendLine (string value)
		{
			return Append (value).Append (System.Environment.NewLine);
		}

		public StringBuilder AppendFormat (string format, params object[] args)
		{
			return AppendFormat (null, format, args);
		}

		public StringBuilder AppendFormat (IFormatProvider provider,
						   string format,
						   params object[] args)
		{
			String.FormatHelper (this, provider, format, args);
			return this;
		}

#if MOONLIGHT
		internal
#else
		public
#endif
		StringBuilder AppendFormat (string format, object arg0)
		{
			return AppendFormat (null, format, new object [] { arg0 });
		}

#if MOONLIGHT
		internal
#else
		public
#endif
		StringBuilder AppendFormat (string format, object arg0, object arg1)
		{
			return AppendFormat (null, format, new object [] { arg0, arg1 });
		}

#if MOONLIGHT
		internal
#else
		public
#endif
		StringBuilder AppendFormat (string format, object arg0, object arg1, object arg2)
		{
			return AppendFormat (null, format, new object [] { arg0, arg1, arg2 });
		}

		/*  The Insert Functions */
		
		public StringBuilder Insert (int index, char[] value) 
		{
			return Insert (index, new string (value));
		}
				
		public StringBuilder Insert (int index, string value) 
		{
			if( index > _length || index < 0)
				throw new ArgumentOutOfRangeException();

			if (value == null || value.Length == 0)
				return this;

			InternalEnsureCapacity (_length + value.Length);

			// Move everything to the right of the insert point across
			String.CharCopyReverse (_str, index + value.Length, _str, index, _length - index);
			
			// Copy in stuff from the insert buffer
			String.CharCopy (_str, index, value, 0, value.Length);
			
			_length += value.Length;

			return this;
		}

		public StringBuilder Insert( int index, bool value ) {
			return Insert (index, value.ToString());
		}
		
		public StringBuilder Insert( int index, byte value ) {
			return Insert (index, value.ToString());
		}

		public StringBuilder Insert( int index, char value) 
		{
			if (index > _length || index < 0)
				throw new ArgumentOutOfRangeException ("index");

			InternalEnsureCapacity (_length + 1);
			
			// Move everything to the right of the insert point across
			String.CharCopyReverse (_str, index + 1, _str, index, _length - index);
			
			_str.InternalSetChar (index, value);
			_length++;

			return this;
		}

		public StringBuilder Insert( int index, decimal value ) {
			return Insert (index, value.ToString());
		}

		public StringBuilder Insert( int index, double value ) {
			return Insert (index, value.ToString());
		}
		
		public StringBuilder Insert( int index, short value ) {
			return Insert (index, value.ToString());
		}

		public StringBuilder Insert( int index, int value ) {
			return Insert (index, value.ToString());
		}

		public StringBuilder Insert( int index, long value ) {
			return Insert (index, value.ToString());
		}
	
		public StringBuilder Insert( int index, object value ) {
			return Insert (index, value.ToString());
		}
		
		[CLSCompliant(false)]
		public StringBuilder Insert( int index, sbyte value ) {
			return Insert (index, value.ToString() );
		}

		public StringBuilder Insert (int index, float value) {
			return Insert (index, value.ToString() );
		}

		[CLSCompliant(false)]
		public StringBuilder Insert (int index, ushort value) {
			return Insert (index, value.ToString() );
		}

		[CLSCompliant(false)]
		public StringBuilder Insert (int index, uint value) {
			return Insert ( index, value.ToString() );
		}
		
		[CLSCompliant(false)]
		public StringBuilder Insert (int index, ulong value) {
			return Insert ( index, value.ToString() );
		}

		public StringBuilder Insert (int index, string value, int count) 
		{
			// LAMESPEC: The spec says to throw an exception if 
			// count < 0, while MS throws even for count < 1!
			if ( count < 0 )
				throw new ArgumentOutOfRangeException();

			if (value != null && value != String.Empty)
				for (int insertCount = 0; insertCount < count; insertCount++)
					Insert( index, value );

			return this;
		}

		public StringBuilder Insert (int index, char [] value, int startIndex, int charCount)
		{
			if (value == null) {
				if (startIndex == 0 && charCount == 0)
					return this;

				throw new ArgumentNullException ("value");
			}

			if (charCount < 0 || startIndex < 0 || startIndex > value.Length - charCount)
				throw new ArgumentOutOfRangeException ();

			return Insert (index, new String (value, startIndex, charCount));
		}
	
		private void InternalEnsureCapacity (int size) 
		{
			if (size > _str.Length || (object) _cached_str == (object) _str) {
				int capacity = _str.Length;

				// Try double buffer, if that doesn't work, set the length as capacity
				if (size > capacity) {
					
					// The first time a string is appended, we just set _cached_str
					// and _str to it. This allows us to do some optimizations.
					// Below, we take this into account.
					if ((object) _cached_str == (object) _str && capacity < constDefaultCapacity)
						capacity = constDefaultCapacity;
					
					capacity = capacity << 1;
					if (size > capacity)
						capacity = size;

					if (capacity >= Int32.MaxValue || capacity < 0)
						capacity = Int32.MaxValue;

					if (capacity > _maxCapacity && size <= _maxCapacity)
						capacity = _maxCapacity;
					
					if (capacity > _maxCapacity)
						throw new ArgumentOutOfRangeException ("size", "capacity was less than the current size.");
				}

				string tmp = String.InternalAllocateStr (capacity);
				if (_length > 0)
					String.CharCopy (tmp, 0, _str, 0, _length);

				_str = tmp;
			}

			_cached_str = null;
		}

		[ComVisible (false)]
		public void CopyTo (int sourceIndex, char [] destination, int destinationIndex, int count)
		{
			if (destination == null)
				throw new ArgumentNullException ("destination");
			if ((Length - count < sourceIndex) ||
			    (destination.Length -count < destinationIndex) ||
			    (sourceIndex < 0 || destinationIndex < 0 || count < 0))
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < count; i++)
				destination [destinationIndex+i] = _str [sourceIndex+i];
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("m_MaxCapacity", _maxCapacity);
			info.AddValue ("Capacity", Capacity);
			info.AddValue ("m_StringValue", ToString ());
			info.AddValue ("m_currentThread", 0);
		}

		StringBuilder (SerializationInfo info, StreamingContext context)
		{
			string s = info.GetString ("m_StringValue");
			if (s == null)
				s = "";
			_length = s.Length;
			_str = _cached_str = s;
			
			_maxCapacity = info.GetInt32 ("m_MaxCapacity");
			if (_maxCapacity < 0)
				_maxCapacity = Int32.MaxValue;
			Capacity = info.GetInt32 ("Capacity");
		}
	}
}       
