//
// System.Text.StringBuilder
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Implement the AppendFormat methods.  Wasn't sure how
// best to do this at this early stage, might want to see
// how the String class and the IFormatProvide / IFormattable interfaces
// pan out first.
//  
// TODO: Make sure the coding complies to the ECMA draft, there's some
// variable names that probably don't (like sString)
//

namespace System.Text {
        public sealed class StringBuilder {

                const int defaultCapacity = 16;

                private int sCapacity;
                private int sLength;
                private char[] sString;

                public StringBuilder() {

                        // The MS Implementation uses the default
                        // capacity for a StringBuilder.  The spec
                        // says it's up to the implementer, but 
                        // we'll do it the MS way just in case.
                                
                        sString = new char[ defaultCapacity ];
                        sCapacity = defaultCapacity;
                        sLength = 0;
                }

                public StringBuilder( int capacity ) {
                        if( capacity < defaultCapacity ) {
                                        // The spec says that the capacity
                                        // has to be at least the default capacity
                                capacity = defaultCapacity;
                        }

                        sString = new char[capacity];
                        sCapacity = capacity;
                        sLength = 0;
                }

                public StringBuilder( string str ) {
                
                        if( str.Length < defaultCapacity ) {    
                                char[] tString = str.ToCharArray();
                                sString = new char[ defaultCapacity ];
                                Array.Copy( tString, sString, str.Length );
                                sLength = str.Length;
                                sCapacity = defaultCapacity;
                        } else {
                                sString = str.ToCharArray();
                                sCapacity = sString.Length;
                                sLength = sString.Length;
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
                                if( value < 0 || value > Int32.MaxValue) {
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
                                                Array.Copy( padding.ToCharArray(), 0, tString, sLength, padLength );
                                                sString = tString;
                                                sLength = sString.Length;
                                                sCapacity = value;
                                        }
                                }
                        }
                }

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
                        if( this.ToString() == sb.ToString() ) {
                                return true;
                        } else {
                                return false;
                        }
                }

                public StringBuilder Remove( int startIndex, int length ) {
                        if( startIndex < 0 || length < 0 || startIndex + length > sLength ) {
                                throw new ArgumentOutOfRangeException();
                        }

                        // Copy everything after the 'removed' part to the start 
						// of the removed part and truncate the sLength

                        Array.Copy( sString, startIndex + length, sString, 
							startIndex, length );

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
                        string newString;

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

                                        if( nextIndex + oldValue.Length < startIndex + count ) {

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

                        newString = newStringB.ToString();

                        EnsureCapacity( newString.Length );
                        sString = newString.ToCharArray();
                        sLength = newString.Length;
                        return this;
                }

                      
	            /* The Append Methods */

                // TODO: Currently most of these methods convert the 
                // parameter to a CharArray (via a String) and then pass
                // it to Append( char[] ).  There might be a faster way
                // of doing this, but it's probably adequate and anything else
                // would make it too messy.
                //
                // As an example, a sample test run of appending a 100 character
                // string to the StringBuilder, and loooping this 50,000 times
                // results in an elapsed time of 2.4s using the MS StringBuilder
                // and 2.7s using this StringBuilder.  Note that this results
                // in a 5 million character string.  I believe MS uses a lot
				// of "native" DLLs for the "meat" of the base classes.


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
                                return Append( value.ToCharArray() );
                        } else {
                                return null;
                        }
                }

                public StringBuilder Append( bool value ) {
                        return Append( value.ToString().ToCharArray() );
                }
                
                public StringBuilder Append( byte value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( int index, char value) {
                        char[] appendChar = new char[1];
                        
                        appendChar[0] = value;
                        return Append( appendChar );
                }


                public StringBuilder Append( decimal value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( double value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( short value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( int value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( long value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( object value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( sbyte value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( float value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( ushort value ) {
                        return Append( value.ToString().ToCharArray() );
                }        
                
                public StringBuilder Append( uint value ) {
                        return Append( value.ToString().ToCharArray() );
                }

                public StringBuilder Append( ulong value ) {
                        return Append( value.ToString().ToCharArray() );
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

                        return Append( value.Substring( startIndex, count ).ToCharArray() );
                }

                public StringBuilder AppendFormat( string format, object arg0 ) {
                        // TODO: Implement
                        return this;
                }

                public StringBuilder AppendFormat( string format, params object[] args ) {
                        // TODO: Implement
                        return this;
                }

                public StringBuilder AppendFormat( IFormatProvider provider, string format,
                                                                params object[] args ) {
                        // TODO: Implement
                        return this;
                }

                public StringBuilder AppendFormat( string format, object arg0, object arg1 ) {
                        // TODO: Implement;
                        return this;
                }

                public StringBuilder AppendFormat( string format, object arg0, object arg1, object arg2 ) {
                        // TODO Implement
                        return this;
                }

                /*  The Insert Functions */
                
                // Similarly to the Append functions, get everything down to a CharArray 
				// and insert that.
                
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
                        return Insert( index, value.ToCharArray() );
                }

                public StringBuilder Insert( int index, bool value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }
                
                public StringBuilder Insert( int index, byte value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, char value) {
                        char[] insertChar = new char[1];
                        
                        insertChar[0] = value;
                        return Insert( index, insertChar );
                }

                public StringBuilder Insert( int index, decimal value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, double value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }
                
                public StringBuilder Insert( int index, short value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, int value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, long value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }
        
                public StringBuilder Insert( int index, object value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, sbyte value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, float value ) {
                return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, ushort value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, uint value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, ulong value ) {
                        return Insert( index, value.ToString().ToCharArray() );
                }

                public StringBuilder Insert( int index, string value, int count ) {
                        if ( count < 0 ) {
                                throw new ArgumentOutOfRangeException();
                        }

                        if( value != null ) {
                                if( value != "" ) {
                                        for( int insertCount = 0; insertCount < count; 
											insertCount++ ) {
                                                Insert( index, value.ToCharArray() );           
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
