//
// System.IO.MemoryStream 
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Clarify some of the lamespec issues
//

namespace System.IO {
		[Serializable]
        public class MemoryStream : Stream {
                private bool canRead;
                private bool canSeek;
                private bool canWrite;
                
                private bool allowGetBuffer;

                private int capacity;

                private byte[] internalBuffer;

                private int initialLength;
                private bool expandable;

                private bool streamClosed = false;

                private long position = 0;
                
                public MemoryStream() {
                        canRead = true;
                        canSeek = true;
                        canWrite = true;

                        capacity = 0;

                        internalBuffer = new byte[0];   

                        allowGetBuffer = true;
                        expandable = true;
                }

                public MemoryStream( byte[] buffer ) {
                        InternalConstructor( buffer, 0, buffer.Length, true, false );                        
                }

                public MemoryStream( int capacity ) {
                        
                        canRead = true;
                        canSeek = true;
                        canWrite = true;
                        
                        this.capacity = capacity;
                        initialLength = 0;
                        internalBuffer = new byte[ 0 ];

                        expandable = true;
                        allowGetBuffer = true;
                }

                public MemoryStream( byte[] buffer, bool writeable ) {
                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        }

                        InternalConstructor( buffer, 0, buffer.Length, writeable, true );

                }
 
                public MemoryStream( byte[] buffer, int index, int count ) { 
                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        }
                        
                        InternalConstructor( buffer, index, count, true, false );                                        
                }
                
                public MemoryStream( byte[] buffer, int index, int count, bool writeable ) { 
                        
                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        }
                        
                        InternalConstructor( buffer, index, count, writeable, true );        
                }

                public MemoryStream( byte[] buffer, int index, int count, bool writeable, bool publicallyVisible ) {
                        InternalConstructor( buffer, index, count, writeable, publicallyVisible );
                }

                private void InternalConstructor( byte[] buffer, int index, int count, bool writeable, bool publicallyVisible ) {
                
                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        } else if ( index < 0 || count < 0 ) {
                                throw new ArgumentOutOfRangeException();
                        } else if ( buffer.Length - index < count ) {
                                throw new ArgumentException();
                        }

                        // LAMESPEC: The spec says to throw an UnauthorisedAccessException if
                        // publicallyVisibile is fale?!  Doesn't that defy the point of having
                        // it there in the first place.  I'll leave it out for now.
                        
                        canRead = true;
                        canSeek = true;
                        canWrite = writeable;

                        initialLength = count;

                        internalBuffer = new byte[ count ];
                        capacity = count;

                        Array.Copy( buffer, index, internalBuffer, 0, count );

                        allowGetBuffer = publicallyVisible;
                        expandable = false;                
                 }

                 public override bool CanRead {
                        get {
                                return this.canRead;
                        }
                }

                public override bool CanSeek {
                        get {
                                return this.canSeek;
                        }
                }

                public override bool CanWrite {
                        get {
                                return this.canWrite;
                        }
                }

                public virtual int Capacity {
                        get {
                                return this.capacity;
                        }

                        set {
                                if( value < 0 || value < capacity ) {
                                        throw new ArgumentOutOfRangeException("value",
						"New capacity cannot be negative or less than the current capacity" );
                                } else if( !expandable ) {
                                        throw new NotSupportedException( "Cannot expand this MemoryStream" );
                                }

                                byte[] newBuffer = new byte[ value ];
                                Array.Copy( internalBuffer, 0, newBuffer, 0, capacity );
                                capacity = value;
                        }
                }

                public override long Length {
                        get {
                                // LAMESPEC: The spec says to throw an IOException if the
                                // stream is closed and an ObjectDisposedException if
                                // "methods were called after the stream was closed".  What
                                // is the difference?

                                if( streamClosed ) {
                                        throw new IOException( "MemoryStream is closed" );
                                }
                                
                                return internalBuffer.Length;
                        }
                }

                public override long Position {
                        get {
                                if( streamClosed ) {
                                        throw new IOException( "MemoryStream is closed" );
                                }

                                return position;
                        }

                        set {

                                if( position < 0 ) {
                                        throw new ArgumentOutOfRangeException ("value", "Position cannot be negative" );
				} else if (position > Int32.MaxValue) {
                                        throw new ArgumentOutOfRangeException ("value",
							"Length must be non-negative and less than 2^31 - 1 - origin");
                                } else if( streamClosed ) {
                                        throw new IOException( "MemoryStream is closed" );
                                }
                                
                                position = value;
                        }
                }
                
                public override void Close() {
                        if( streamClosed ) {
                                return;
                        }

			streamClosed = true;
			internalBuffer = null;
                }

                public override void Flush() { }

                public virtual byte[] GetBuffer() {
                        if( !allowGetBuffer ) {
                                throw new UnauthorizedAccessException();
                        }  

                        return internalBuffer;
                }

                public override int Read( byte[] buffer, int offset, int count ) {
                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        } else if( offset < 0 || count < 0 ) {
                                throw new ArgumentOutOfRangeException();
                        } else if( buffer.Length - offset < count ) {
                                throw new ArgumentException();
                        } else if ( streamClosed ) {
                                throw new ObjectDisposedException( "MemoryStream" );
                        }

                        long ReadTo;

                        if( position + count > internalBuffer.Length ) {
                                ReadTo = internalBuffer.Length;
                        } else {
                                ReadTo = position + (long)count;
                        }

                        Array.Copy( internalBuffer, (int)position, buffer, offset, (int)(ReadTo - position) );

                        int bytesRead = (int)(ReadTo - position);

                        position = ReadTo;

                        return bytesRead;
                }

                public override int ReadByte( ) {
                        if( streamClosed ) {
                                throw new ObjectDisposedException( "MemoryStream" );
                        }


                        // LAMESPEC: What happens if we're at the end of the stream?  It's unspecified in the
                        // docs but tests against the MS impl. show it returns -1
                        //

                        if( position >= internalBuffer.Length ) {
                                return -1;
                        } else {
                                return internalBuffer[ position++ ];
                        }
                }
                 
                public override long Seek( long offset, SeekOrigin loc ) { 
                        long refPoint;

                        if( streamClosed ) {
                                throw new ObjectDisposedException( "MemoryStream" );
                        }

                        switch( loc ) {
                                case SeekOrigin.Begin:
                                        refPoint = 0;
                                        break;
                                case SeekOrigin.Current:
                                        refPoint = position;
                                        break;
                                case SeekOrigin.End:
                                        refPoint = internalBuffer.Length;
                                        break;
                                default:
                                        throw new ArgumentException( "Invalid SeekOrigin" );
                        }

                        // LAMESPEC: My goodness, how may LAMESPECs are there in this
                        // class! :)  In the spec for the Position property it's stated
                        // "The position must not be more than one byte beyond the end of the stream."
                        // In the spec for seek it says "Seeking to any location beyond the length of the 
                        // stream is supported."  That's a contradiction i'd say.
                        // I guess seek can go anywhere but if you use position it may get moved back.

                        if( refPoint + offset < 0 ) {
                                throw new IOException( "Attempted to seek before start of MemoryStream" );
                        } else if( offset > internalBuffer.Length ) {
                                throw new ArgumentOutOfRangeException("offset",
						"Offset cannot be greater than length of MemoryStream" );
                        }

                        position = refPoint + offset;

                        return position;
                }
                
                
                public override void SetLength( long value ) { 
                        if( streamClosed ) {
                                throw new ObjectDisposedException( "MemoryStream" );                        
                        } else if( !expandable && value > capacity ) {
                                throw new NotSupportedException( "Expanding this MemoryStream is not supported" );
                        } else if( !canWrite ) {
                                throw new IOException( "Cannot write to this MemoryStream" );
                        } else if( value < 0 ) {

                                // LAMESPEC: AGAIN! It says to throw this exception if value is
                                // greater than "the maximum length of the MemoryStream".  I haven't
                                // seen anywhere mention what the maximum length of a MemoryStream is and
                                // since we're this far this memory stream is expandable.

                                throw new ArgumentOutOfRangeException();
                        } 

                        byte[] newBuffer;
			newBuffer = new byte[ value ];
                        
                        if (value < internalBuffer.Length) {
                                // truncate
                                Array.Copy( internalBuffer, 0, newBuffer, 0, (int)value );                              
                        } else {
                                // expand
                                 Array.Copy( internalBuffer, 0, newBuffer, 0, internalBuffer.Length );
                        }
                        internalBuffer = newBuffer;
                        capacity = (int)value;

                }
                
                
                public virtual byte[] ToArray() { 
                        byte[] outBuffer = new byte[capacity];
                        Array.Copy( internalBuffer, 0, outBuffer, 0, capacity);
                        return outBuffer; 
                }

                public override void Write( byte[] buffer, int offset, int count ) { 
                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        } else if( !canWrite ) {
                                throw new NotSupportedException();
                        } else if( buffer.Length - offset < count ) {
                                throw new ArgumentException();
                        } else if( offset < 0 || count < 0 ) {
                                throw new ArgumentOutOfRangeException();
                        } else if( streamClosed ) {
                                throw new ObjectDisposedException( "MemoryStream" );
                        }

			if( position + count > capacity ) {
				if( expandable ) {
					// expand the buffer
					SetLength( position + count );                       
				} else {
					// only write as many bytes as will fit
					count = (int)((long)capacity - position);
				}
			}

			// internal buffer may not be allocated all the way up to capacity
			// count will already be limited to capacity above if non-expandable
			if( position + count >= internalBuffer.Length )
				SetLength( position + count );

                        Array.Copy( buffer, offset, internalBuffer, (int)position, count );
                        position += count;
                }


                public override void WriteByte( byte value ) { 
                        if ( streamClosed )
                                throw new ObjectDisposedException( "MemoryStream" );
			else if( !canWrite || (position >= capacity && !expandable))
                                throw new NotSupportedException();

			if( position >= internalBuffer.Length )
				SetLength ( position + 1 );

			internalBuffer[ position++ ] = value;
                }
                

                public virtual void WriteTo( Stream stream ) { 
                        if( stream == null ) {
                                throw new ArgumentNullException();
                        }

                        stream.Write( internalBuffer, 0, internalBuffer.Length );
                
                }
                               
                       
        }               


}                      
