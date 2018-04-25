//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Diagnostics;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Provides cookie compression using <see cref="DeflateStream"/>.
    /// </summary>
    public sealed class DeflateCookieTransform : CookieTransform
    {
        int _maxDecompressedSize = 1024 * 1024;     // Default maximum of 1MB

        /// <summary>
        /// Creates a new instance of <see cref="DeflateCookieTransform"/>.
        /// </summary>
        public DeflateCookieTransform()
        {
        }

        /// <summary>
        /// Gets or sets the maximum size (in bytes) of a decompressed cookie.
        /// </summary>
        public int MaxDecompressedSize
        {
            get { return _maxDecompressedSize; }
            set { _maxDecompressedSize = value; }
        }

        /// <summary>
        /// Inflates data.
        /// </summary>
        /// <param name="encoded">Data previously returned from <see cref="Encode"/></param>
        /// <returns>Decoded data.</returns>
        /// <exception cref="ArgumentNullException">The argument 'value' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'value' contains zero bytes.</exception>
        /// <exception cref="SecurityTokenException">The decompressed length is larger than MaxDecompressedSize.</exception>
        public override byte[] Decode( byte[] encoded )
        {
            if ( null == encoded )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "encoded" );
            }

            if ( 0 == encoded.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "encoded", SR.GetString( SR.ID6045 ) );
            }

            MemoryStream compressedStream = new MemoryStream( encoded );
            using ( DeflateStream deflateStream = new DeflateStream( compressedStream, CompressionMode.Decompress, false ) )
            {
                using ( MemoryStream decompressedStream = new MemoryStream() )
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    do
                    {
                        bytesRead = deflateStream.Read( buffer, 0, buffer.Length );
                        decompressedStream.Write( buffer, 0, bytesRead );

                        // check length against configured maximum to prevevent decompression bomb attacks
                        if ( decompressedStream.Length > MaxDecompressedSize )
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new SecurityTokenException( SR.GetString( SR.ID1068, MaxDecompressedSize ) ) );
                        }
                    } while ( bytesRead > 0 );

                    return decompressedStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Deflates data.
        /// </summary>
        /// <param name="value">Data to be compressed.</param>
        /// <returns>Compressed data.</returns>
        /// <exception cref="ArgumentNullException">The argument 'value' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'value' contains zero bytes.</exception>
        public override byte[] Encode( byte[] value )
        {
            if ( null == value )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "value" );
            }

            if ( 0 == value.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "value", SR.GetString( SR.ID6044 ) );
            }

            using ( MemoryStream compressedStream = new MemoryStream() )
            {
                using ( DeflateStream deflateStream = new DeflateStream( compressedStream, CompressionMode.Compress, true ) )
                {
                    deflateStream.Write( value, 0, value.Length );
                }

                byte[] encoded = compressedStream.ToArray();

                if ( DiagnosticUtility.ShouldTrace( TraceEventType.Information ) )
                {
                    TraceUtility.TraceEvent( 
                            TraceEventType.Information,
                            TraceCode.Diagnostics,
                            SR.GetString(SR.TraceDeflateCookieEncode),
                            new DeflateCookieTraceRecord( value.Length, encoded.Length ),
                            null,
                            null );
                }

                return encoded;
            }
        }
    }
}
