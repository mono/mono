
using System.Text;
using System.Diagnostics;

using System.Threading.Tasks;

namespace System.Xml {

    internal abstract partial class Base64Encoder {

        internal abstract Task WriteCharsAsync(char[] chars, int index, int count);

        internal async Task EncodeAsync( byte[] buffer, int index, int count ) {
            if ( buffer == null ) {
                throw new ArgumentNullException( "buffer" );
            }
            if ( index < 0 ) {
                throw new ArgumentOutOfRangeException( "index" );
            }
            if ( count < 0 ) {
                throw new ArgumentOutOfRangeException( "count" );
            }
            if ( count > buffer.Length - index ) {
                throw new ArgumentOutOfRangeException( "count" );
            }

            // encode left-over buffer
            if( leftOverBytesCount > 0 ) {
                int i = leftOverBytesCount;
                while ( i < 3 && count > 0 ) {
                    leftOverBytes[i++] = buffer[index++];
                    count--;
                }

                // the total number of buffer we have is less than 3 -> return
                if ( count == 0 && i < 3 ) {
                    leftOverBytesCount = i;
                    return;
                }

                // encode the left-over buffer and write out
                int leftOverChars = Convert.ToBase64CharArray( leftOverBytes, 0, 3, charsLine, 0 );
                await WriteCharsAsync( charsLine, 0, leftOverChars ).ConfigureAwait(false);
            }

            // store new left-over buffer
            leftOverBytesCount = count % 3;
            if ( leftOverBytesCount > 0 )  {
                count -= leftOverBytesCount;
                if ( leftOverBytes == null ) {
                    leftOverBytes = new byte[3];
                }
                for( int i = 0; i < leftOverBytesCount; i++ ) {
                    leftOverBytes[i] = buffer[ index + count + i ];
                }
            }

            // encode buffer in 76 character long chunks
            int endIndex = index + count;
            int chunkSize = LineSizeInBytes;
            while( index < endIndex ) {
                if ( index + chunkSize > endIndex ) {
                    chunkSize = endIndex - index;
                }
                int charCount = Convert.ToBase64CharArray( buffer, index, chunkSize, charsLine, 0 );
                await WriteCharsAsync( charsLine, 0, charCount ).ConfigureAwait(false);
    
                index += chunkSize;
            }
        }

        internal async Task FlushAsync() {
            if ( leftOverBytesCount > 0 ) {
                int leftOverChars = Convert.ToBase64CharArray( leftOverBytes, 0, leftOverBytesCount, charsLine, 0 );
                await WriteCharsAsync( charsLine, 0, leftOverChars ).ConfigureAwait(false);
                leftOverBytesCount = 0;
            }
        }
    }

    internal partial class XmlTextWriterBase64Encoder : Base64Encoder {
        internal override Task WriteCharsAsync(char[] chars, int index, int count) {
            throw new NotImplementedException();
        }
    }

    internal partial class XmlRawWriterBase64Encoder : Base64Encoder {

        internal override Task WriteCharsAsync( char[] chars, int index, int count ) {
            return rawWriter.WriteRawAsync( chars, index, count );
        }
    }

} 
