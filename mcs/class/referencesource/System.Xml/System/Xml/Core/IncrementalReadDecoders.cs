//------------------------------------------------------------------------------
// <copyright file="IncrementalReadDecoder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.Diagnostics;

namespace System.Xml {

//
//  IncrementalReadDecoder abstract class
//
    internal abstract class IncrementalReadDecoder {
        internal abstract int DecodedCount { get; }
        internal abstract bool IsFull { get; }
        internal abstract void SetNextOutputBuffer( Array array, int offset, int len );
        internal abstract int Decode( char[] chars, int startPos, int len );
        internal abstract int Decode( string str, int startPos, int len );
        internal abstract void Reset();
    }

#if !SILVERLIGHT // Needed only for XmlTextReader
//
//  Dummy IncrementalReadDecoder
//
    internal class IncrementalReadDummyDecoder : IncrementalReadDecoder {
        internal override int DecodedCount { get { return -1; } }
        internal override bool IsFull { get { return false; } }
        internal override void SetNextOutputBuffer( Array array, int offset, int len ) {}
        internal override int Decode( char[] chars, int startPos, int len ) { return len; }
        internal override int Decode( string str, int startPos, int len ) { return len; }
        internal override void Reset() {}
    }

//
//  IncrementalReadDecoder for ReadChars
//
    internal class IncrementalReadCharsDecoder : IncrementalReadDecoder {
        char[]  buffer;
        int     startIndex;
        int     curIndex;
        int     endIndex;

        internal IncrementalReadCharsDecoder() {
        }

        internal override int DecodedCount { 
            get { 
                return curIndex - startIndex; 
            } 
        }

        internal override bool IsFull { 
            get { 
                return curIndex == endIndex; 
            } 
        }

        internal override int Decode( char[] chars, int startPos, int len ) {
            Debug.Assert( chars != null );
            Debug.Assert( len >= 0 );
            Debug.Assert( startPos >= 0 );
            Debug.Assert( chars.Length - startPos >= len );

            Debug.Assert( len > 0 );

            int copyCount = endIndex - curIndex;
            if ( copyCount > len ) {
                copyCount = len;
            }
            Buffer.BlockCopy( chars, startPos * 2, buffer, curIndex * 2, copyCount * 2 );
            curIndex += copyCount;

            return copyCount;
        }

        internal override int Decode( string str, int startPos, int len ) { 
            Debug.Assert( str != null );
            Debug.Assert( len >= 0 );
            Debug.Assert( startPos >= 0 );
            Debug.Assert( str.Length - startPos >= len );

            Debug.Assert( len > 0 );

            int copyCount = endIndex - curIndex;
            if ( copyCount > len ) {
                copyCount = len;
            }
            str.CopyTo( startPos, buffer, curIndex, copyCount );
            curIndex += copyCount;

            return copyCount;
        }

        internal override void Reset() {
        }

        internal override void SetNextOutputBuffer( Array buffer, int index, int count ) {
            Debug.Assert( buffer != null );
            Debug.Assert( count >= 0 );
            Debug.Assert( index >= 0 );
            Debug.Assert( buffer.Length - index >= count );

            Debug.Assert( ( buffer as char[] ) != null );
            this.buffer = (char[])buffer;
            this.startIndex = index;
            this.curIndex = index;
            this.endIndex = index + count;
        }
    }
#endif
}
