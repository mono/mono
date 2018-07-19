//------------------------------------------------------------------------------
// <copyright file="CharEntitiesEncodingFallback.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace System.Xml {

//
// CharEntityEncoderFallback
//

    internal class CharEntityEncoderFallback : EncoderFallback {
        private CharEntityEncoderFallbackBuffer fallbackBuffer;

        private int[]   textContentMarks;
        private int     endMarkPos;
        private int     curMarkPos;
        private int     startOffset;

        internal CharEntityEncoderFallback() {
        }

        public override EncoderFallbackBuffer CreateFallbackBuffer() {
            if ( fallbackBuffer == null ) { 
                fallbackBuffer = new CharEntityEncoderFallbackBuffer( this );
            }
            return fallbackBuffer;
        }
 
        public override int MaxCharCount {
            get {
                return 12;
            }
        }

        internal int StartOffset {
            get {
                return startOffset;
            }
            set {
                startOffset = value;
            }
        }

        internal void Reset( int[] textContentMarks, int endMarkPos ) {
            this.textContentMarks = textContentMarks;
            this.endMarkPos = endMarkPos;
            curMarkPos = 0;
        }

        internal bool CanReplaceAt( int index ) {
            int mPos = curMarkPos;
            int charPos = startOffset + index;
            while ( mPos < endMarkPos && charPos >= textContentMarks[mPos+1] ) {
                mPos++;
            }
            curMarkPos = mPos;

            return (mPos & 1) != 0;
        }
    }
 
//
// CharEntityFallbackBuffer
//
    internal class CharEntityEncoderFallbackBuffer : EncoderFallbackBuffer {
        private CharEntityEncoderFallback parent;

        private string  charEntity = string.Empty;
        private int     charEntityIndex = -1;

        internal CharEntityEncoderFallbackBuffer( CharEntityEncoderFallback parent ) {
            this.parent = parent;
        }
 
        public override bool Fallback( char charUnknown, int index ) {
            // If we are already in fallback, throw, it's probably at the suspect character in charEntity
            if ( charEntityIndex >= 0 ) {
                (new EncoderExceptionFallback()).CreateFallbackBuffer().Fallback( charUnknown, index );
            }
 
            // find out if we can replace the character with entity
            if ( parent.CanReplaceAt( index ) ) {
                // Create the replacement character entity
                charEntity = string.Format( CultureInfo.InvariantCulture, "&#x{0:X};", new object[] { (int)charUnknown } );
                charEntityIndex = 0;
                return true;
            }
            else {
                EncoderFallbackBuffer errorFallbackBuffer = ( new EncoderExceptionFallback() ).CreateFallbackBuffer();
                errorFallbackBuffer.Fallback( charUnknown, index );
                return false;
            }
        }
 
        public override bool Fallback( char charUnknownHigh, char charUnknownLow, int index ) {
            // check input surrogate pair
            if ( !char.IsSurrogatePair( charUnknownHigh, charUnknownLow ) ) {
                throw XmlConvert.CreateInvalidSurrogatePairException( charUnknownHigh, charUnknownLow );
            }

            // If we are already in fallback, throw, it's probably at the suspect character in charEntity
            if ( charEntityIndex >= 0 ) {
                (new EncoderExceptionFallback()).CreateFallbackBuffer().Fallback( charUnknownHigh, charUnknownLow, index );
            }
 
            if ( parent.CanReplaceAt( index ) ) {
                // Create the replacement character entity
                charEntity = string.Format( CultureInfo.InvariantCulture, "&#x{0:X};", new object[] { SurrogateCharToUtf32( charUnknownHigh, charUnknownLow ) } );
                charEntityIndex = 0;
                return true;
            }
            else {
                EncoderFallbackBuffer errorFallbackBuffer = ( new EncoderExceptionFallback() ).CreateFallbackBuffer();
                errorFallbackBuffer.Fallback( charUnknownHigh, charUnknownLow, index );
                return false;
            }
        }
 
        public override char GetNextChar() {
            // Bug fix: 35637. The protocol using GetNextChar() and MovePrevious() called by Encoder is not well documented.
            // Here we have to to signal to Encoder that the previous read was last character. Only AFTER we can 
            // mark ourself as done (-1). Otherwise MovePrevious() can still be called, but -1 is already incorrectly set
            // and return false from MovePrevious(). Then Encoder ----ing the rest of the bytes.
            if (charEntityIndex == charEntity.Length)
            {
                charEntityIndex = -1;
            }
            if ( charEntityIndex == -1 ) {
                return (char)0;
            }
            else {
                Debug.Assert( charEntityIndex < charEntity.Length );
                char ch = charEntity[charEntityIndex++];
                return ch;
            }
        }
  
        public override bool MovePrevious() {
            if ( charEntityIndex == -1 ) {
                return false;
            }
            else {
                // Could be == length if just read the last character
                Debug.Assert(charEntityIndex <= charEntity.Length);
                if (charEntityIndex > 0)
                {
                    charEntityIndex--;
                    return true;
                }
                else {
                    return false;
                }
            }
        }
          

        public override int Remaining {
            get {
                if ( charEntityIndex == -1 ) {
                    return 0;
                }
                else {
                    return charEntity.Length - charEntityIndex;
                }
            }
        }
 
        public override void Reset() {
            charEntityIndex = -1;
        }

        private int SurrogateCharToUtf32(char highSurrogate, char lowSurrogate) {
            return XmlCharType.CombineSurrogateChar(lowSurrogate, highSurrogate);
        }
    }
}
