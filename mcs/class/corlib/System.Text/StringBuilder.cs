// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Text.StringBuilder
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Make sure the coding complies to the ECMA draft, there's some
// variable names that probably don't (like sString)
//
using System.Runtime.CompilerServices;

namespace System.Text {
	
	[Serializable]
	public sealed class StringBuilder {

		private const int defaultCapacity = 16;

		private int sCapacity;
		private int sLength;
		private char[] sString;
		private int sMaxCapacity = Int32.MaxValue;

		public StringBuilder(string value, int startIndex, int length, int capacity) {
			// first, check the parameters and throw appropriate exceptions if needed
			if(null==value) {
				throw new System.ArgumentNullException("value");
			}

			// make sure startIndex is zero or positive
			if(startIndex < 0) {
				throw new System.ArgumentOutOfRangeException("startIndex", startIndex, "StartIndex cannot be less than zero.");
			}

			// make sure length is zero or positive
			if(length < 0) {
				throw new System.ArgumentOutOfRangeException("length", length, "Length cannot be less than zero.");
			}

			// make sure startIndex and length give a valid substring of value
			if(startIndex + (length -1) > (value.Length - 1) ) {
				throw new System.ArgumentOutOfRangeException("startIndex", startIndex, "StartIndex and length must refer to a location within the string.");
			}
			
			// the capacity must be at least as big as the default capacity
			sCapacity = Math.Max(capacity, defaultCapacity);

			// LAMESPEC: what to do if capacity is too small to hold the substring?
			// Like the MS implementation, double the capacity until it is large enough
			while (sCapacity < length) {
				// However, take care not to double if that would make the number
				// larger than what an int can hold
				if (sCapacity <= Int32.MaxValue / 2) {
					sCapacity *= 2;
				}
				else{
					sCapacity = Int32.MaxValue;
				}
			}

			sString = new char[sCapacity];
			sLength = length;

			// if the length is not zero, then we have to copy some characters
			if (sLength > 0) {
				// Copy the correct number of characters into the internal array
				value.CopyTo (0, sString, 0, sLength);
			}
		}

		public StringBuilder () : this(String.Empty, 0, 0, 0) {}

		public StringBuilder( int capacity ) : this(String.Empty, 0, 0, capacity) {}

		public StringBuilder( int capacity, int maxCapacity ) : this(String.Empty, 0, 0, capacity) {
			if(capacity > maxCapacity) {
				throw new System.ArgumentOutOfRangeException("capacity", "Capacity exceeds maximum capacity.");
			}
			sMaxCapacity = maxCapacity;
		}

		public StringBuilder( string value ) : this(value, 0, value == null ? 0 : value.Length, value == null? 0 : value.Length) {
		}
	
		public StringBuilder( string value, int capacity) : this(value, 0, value.Length, capacity) {}
	
		public int MaxCapacity {
			get {
				// MS runtime always returns Int32.MaxValue.
				return sMaxCapacity;
			}
		}

		public int Capacity {
			get {
				return sCapacity;
			}

			set {
				if( value < sLength ) {
					throw new ArgumentException( "Capacity must be > length" );
				} else {
					char[] tString = new char[value];	       
					Array.Copy( sString, tString, sLength );
					sString = tString;
					sCapacity = sString.Length;
				}
			}
		}


		public int Length {
			get {
				return sLength;
			}

			set {
				if( value < 0 || value > MaxCapacity) {
					throw new ArgumentOutOfRangeException();
				} else {

					if( value < sLength ) {
						// Truncate current string at value

						// LAMESPEC:  The spec is unclear as to what to do
						// with the capacity when truncating the string.
						//
						// Don't change the capacity, as this is what
						// the MS implementation does.

						sLength = value;
					} else {
						// Expand the capacity to the new length and
						// pad the string with spaces.
						
						// LAMESPEC: The spec says to put the spaces on the
						// left of the string however the MS implementation
						// puts them on the right.  We'll do that for 
						// compatibility (!)

						char[] tString = new char[ value ];
						int padLength = value - sLength;
						
						string padding = new String( ' ', padLength );
						Array.Copy( sString, tString, sLength );
						padding.CopyTo (0, sString, sLength, padLength);
						sString = tString;
						sLength = sString.Length;
						sCapacity = value;
					}
				}
			}
		}

		[IndexerName("Chars")]
		public char this[ int index ] {
			get {

				if( index >= sLength || index < 0 ) {
					throw new IndexOutOfRangeException();
				}
				return sString[ index ];
			} 

			set {
				if( index >= sLength || index < 0 ) {
					throw new IndexOutOfRangeException();
				}
				sString[ index ] = value;
			}
		}

		public override string ToString() {
			return ToString(0, sLength);
		}

		public string ToString( int startIndex, int length ) {
			if( startIndex < 0 || length < 0 || startIndex + length > sLength ) {
				throw new ArgumentOutOfRangeException();
			}
	
			return new String( sString, startIndex, length );
		}

		public int EnsureCapacity( int capacity ) {
			if( capacity < 0 ) {
				throw new ArgumentOutOfRangeException( 
					"Capacity must be greater than 0." );
			}

			if( capacity <= sCapacity ) {
				return sCapacity;
			} else {
				Capacity = capacity;
				return sCapacity;
			}
		}

		public bool Equals( StringBuilder sb ) {
			if(sLength == sb.Length && this.ToString() == sb.ToString() ) {
				return true;
			} else {
				return false;
			}
		}

		public StringBuilder Remove (int startIndex, int length)
		{
			if( startIndex < 0 || length < 0 || startIndex + length > sLength )
				throw new ArgumentOutOfRangeException();

			// Copy everything after the 'removed' part to the start 
			// of the removed part and truncate the sLength

			Array.Copy (sString, startIndex + length, sString, startIndex,
				    sLength - (startIndex + length));

			sLength -= length;
			return this;
		}			       

		public StringBuilder Replace( char oldChar, char newChar ) {
		
			return Replace( oldChar, newChar, 0, sLength);
		}

		public StringBuilder Replace( char oldChar, char newChar, int startIndex, int count ) {
			if( startIndex + count > sLength || startIndex < 0 || count < 0 ) {
				throw new ArgumentOutOfRangeException();
			}

			for( int replaceIterate = startIndex; replaceIterate < startIndex + count; replaceIterate++ ) {
				if( this[replaceIterate] == oldChar ) {
					this[replaceIterate] = newChar;
				}
			}

			return this;
		}

		public StringBuilder Replace( string oldValue, string newValue ) {
			return Replace( oldValue, newValue, 0, sLength );
		}

		public StringBuilder Replace( string oldValue, string newValue, int startIndex, int count ) {
			string startString = this.ToString();
			StringBuilder newStringB = new StringBuilder();

			if( oldValue == null ) { 
				throw new ArgumentNullException(
					"The old value cannot be null.");
			}

			if( startIndex < 0 || count < 0 || startIndex + count > sLength ) {
				throw new ArgumentOutOfRangeException();
			}

			if( oldValue.Length == 0 ) {
				throw new ArgumentException(
					"The old value cannot be zero length.");
			}

			int nextIndex = startIndex; // Where to start the next search
			int lastIndex = nextIndex;  // Where the last search finished

			while( nextIndex != -1 ) {
				nextIndex = startString.IndexOf( oldValue, lastIndex);				  
				if( nextIndex != -1 ) {
					// The MS implementation won't replace a substring 
					// if that substring goes over the "count"
					// boundary, so we'll make sure the behaviour 
					// here is the same.

					if( nextIndex + oldValue.Length <= startIndex + count ) {

						// Add everything to the left of the old 
						// string
						newStringB.Append( startString.Substring( lastIndex, nextIndex - lastIndex ) );
	
						// Add the replacement string
						newStringB.Append( newValue );
						
						// Set the next start point to the 
						// end of the last match
						lastIndex = nextIndex + oldValue.Length;
					} else {
						// We're past the "count" we're supposed to replace within
						nextIndex = -1;
						newStringB.Append( 
							startString.Substring( lastIndex ) );
					}

				} else {
					// Append everything left over
					newStringB.Append( startString.Substring( lastIndex ) );
				}
			} 

			sCapacity = newStringB.sCapacity;
			sString = newStringB.sString;
			sLength = newStringB.sLength;
			return this;
		}

		      
		/* The Append Methods */

		public StringBuilder Append( char[] value ) {
			if( sLength + value.Length > sCapacity ) {
				// Need more capacity, double the capacity StringBuilder 
				// and make sure we have at least enough for the value 
				// if that's going to go over double. 
					 
				Capacity = value.Length + ( sCapacity + sCapacity);
			}

			Array.Copy( value, 0, sString, sLength, value.Length );
			sLength += value.Length;

			return this;
		} 
		
		public StringBuilder Append( string value ) {
			if( value != null ) {
				int new_size = sLength + value.Length;
				if (new_size > sCapacity)
					Capacity = value.Length + sCapacity * 2;

				value.CopyTo (0, sString, sLength, value.Length);
				sLength = new_size;
				return this;
			} else {
				return null;
			}
		}

		public StringBuilder Append( bool value ) {
			return Append (value.ToString());
		}
		
		public StringBuilder Append( byte value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( decimal value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( double value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( short value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( int value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( long value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( object value ) {
			return Append (value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append( sbyte value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( float value ) {
			return Append (value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append( ushort value ) {
			return Append (value.ToString());
		}	
		
		[CLSCompliant(false)]
		public StringBuilder Append( uint value ) {
			return Append (value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append( ulong value ) {
			return Append (value.ToString());
		}

		public StringBuilder Append( char value ) {
			if( sLength + 1 > sCapacity ) {
				// Need more capacity, double the capacity StringBuilder 
				// and make sure we have at least enough for the value 
				// if that's going to go over double. 
					 
				Capacity = 1 + ( sCapacity + sCapacity);
			}
			sString [sLength] = value;
			sLength++;

			return this;
		}

		public StringBuilder Append( char value, int repeatCount ) {
			if( repeatCount < 0 ) {
				throw new ArgumentOutOfRangeException();
			}

			return Append( new String( value, repeatCount) );
		}

		public StringBuilder Append( char[] value, int startIndex, int charCount ) {

			if( (charCount < 0 || startIndex < 0) || 
				( charCount + startIndex > value.Length ) ) {
				throw new ArgumentOutOfRangeException();
			}
			
			if( value == null ) {
				if( !(startIndex == 0 && charCount == 0) ) {
					throw new ArgumentNullException();
				} else {
					return this;
				}
			} else {
				char[] appendChars = new char[ charCount ];
			
				Array.Copy( value, startIndex, appendChars, 0, charCount );
				return Append( appendChars );
			}
		}

		public StringBuilder Append( string value, int startIndex, int count ) {
			if( (count < 0 || startIndex < 0) || 
				( startIndex + count > value.Length ) ) { 
				throw new ArgumentOutOfRangeException();
			}

			return Append (value.Substring (startIndex, count));
		}

		public StringBuilder AppendFormat (string format, object arg0 )
		{
			string result = String.Format (format, arg0);
			return Append (result);
		}

		public StringBuilder AppendFormat (string format, params object[] args )
		{
			string result = String.Format (format, args);
			return Append (result);
		}

		public StringBuilder AppendFormat (IFormatProvider provider,
						   string format,
						   params object[] args)
		{
			string result = String.Format (provider, format, args);
			return Append (result);
		}

		public StringBuilder AppendFormat (string format, object arg0, object arg1 )
		{
			string result = String.Format (format, arg0, arg1);
			return Append (result);
		}

		public StringBuilder AppendFormat (string format, object arg0, object arg1, object arg2 )
		{
			string result = String.Format (format, arg0, arg1, arg2);
			return Append (result);
		}

		/*  The Insert Functions */
		
		public StringBuilder Insert( int index, char[] value ) {
			if( index > sLength || index < 0) {
				throw new ArgumentOutOfRangeException();
			}

			if( value == null || value.Length == 0 ) {
				return this;
			} else {
				// Check we have the capacity to insert this array
				if( sCapacity < sLength + value.Length ) {
					Capacity = value.Length + ( sCapacity + sCapacity );
				}

				// Move everything to the right of the insert point across
				Array.Copy( sString, index, sString, index + value.Length, sLength - index);
				
				// Copy in stuff from the insert buffer
				Array.Copy( value, 0, sString, index, value.Length );
				
				sLength += value.Length;
				return this;
			}
		}
				
		public StringBuilder Insert( int index, string value ) {
			if (index > sLength || index < 0)
				throw new ArgumentOutOfRangeException ("index");

			if (value == null || value.Length == 0)
				return this;

			int len = value.Length;
			// Check we have the capacity to insert this array
			if (sCapacity < sLength + len)
				Capacity = len + ( sCapacity + sCapacity );

			// Move everything to the right of the insert point across
			Array.Copy (sString, index, sString, index + len, sLength - index);
			
			value.CopyTo (0, sString, index, len);
			
			sLength += len;
			return this;
		}

		public StringBuilder Insert( int index, bool value ) {
			return Insert( index, value.ToString());
		}
		
		public StringBuilder Insert( int index, byte value ) {
			return Insert( index, value.ToString());
		}

		public StringBuilder Insert( int index, char value) {
			char[] insertChar = new char[1];
			
			insertChar[0] = value;
			return Insert( index, insertChar );
		}

		public StringBuilder Insert( int index, decimal value ) {
			return Insert( index, value.ToString() );
		}

		public StringBuilder Insert( int index, double value ) {
			return Insert( index, value.ToString() );
		}
		
		public StringBuilder Insert( int index, short value ) {
			return Insert( index, value.ToString() );
		}

		public StringBuilder Insert( int index, int value ) {
			return Insert( index, value.ToString() );
		}

		public StringBuilder Insert( int index, long value ) {
			return Insert( index, value.ToString() );
		}
	
		public StringBuilder Insert( int index, object value ) {
			return Insert( index, value.ToString() );
		}
		
		[CLSCompliant(false)]
		public StringBuilder Insert( int index, sbyte value ) {
			return Insert( index, value.ToString() );
		}

		public StringBuilder Insert( int index, float value ) {
			return Insert( index, value.ToString() );
		}

		[CLSCompliant(false)]
		public StringBuilder Insert( int index, ushort value ) {
			return Insert( index, value.ToString() );
		}

		[CLSCompliant(false)]
		public StringBuilder Insert( int index, uint value ) {
			return Insert( index, value.ToString() );
		}
		
		[CLSCompliant(false)]
		public StringBuilder Insert( int index, ulong value ) {
			return Insert( index, value.ToString() );
		}

		public StringBuilder Insert( int index, string value, int count ) {
			if ( count < 0 ) {
				throw new ArgumentOutOfRangeException();
			}

			if( value != null ) {
				if( value != "" ) {
					for( int insertCount = 0; insertCount < count; 
						insertCount++ ) {
						Insert( index, value );	   
					}
				}
			}
			return this;
		}

		public StringBuilder Insert( int index, char[] value, int startIndex, 
			int charCount ) {

			if( value != null ) {
				if( charCount < 0 || startIndex < 0 || startIndex + charCount > value.Length ) {
					throw new ArgumentOutOfRangeException();
				}
					
				char[] insertChars = new char[ charCount  ];
				Array.Copy( value, startIndex, insertChars, 0, charCount );
				return Insert( index, insertChars );
			} else {
				return this;
			}
		}
	}
}       
