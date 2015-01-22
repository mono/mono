// GZipOutputStream.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;

using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.GZip 
{
	
	/// <summary>
	/// This filter stream is used to compress a stream into a "GZIP" stream.
	/// The "GZIP" format is described in RFC 1952.
	///
	/// author of the original java version : John Leuner
	/// </summary>
	/// <example> This sample shows how to gzip a file
	/// <code>
	/// using System;
	/// using System.IO;
	/// 
	/// using ICSharpCode.SharpZipLib.GZip;
	/// using ICSharpCode.SharpZipLib.Core;
	/// 
	/// class MainClass
	/// {
	/// 	public static void Main(string[] args)
	/// 	{
	/// 			using (Stream s = new GZipOutputStream(File.Create(args[0] + ".gz")))
	/// 			using (FileStream fs = File.OpenRead(args[0])) {
	/// 				byte[] writeData = new byte[4096];
	/// 				Streamutils.Copy(s, fs, writeData);
	/// 			}
	/// 		}
	/// 	}
	/// }	
	/// </code>
	/// </example>
	public class GZipOutputStream : DeflaterOutputStream
	{
        enum OutputState
        {
            Header,
            Footer,
            Finished,
            Closed,
        };

		#region Instance Fields
		/// <summary>
		/// CRC-32 value for uncompressed data
		/// </summary>
		protected Crc32 crc = new Crc32();
        OutputState state_ = OutputState.Header;
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a GzipOutputStream with the default buffer size
		/// </summary>
		/// <param name="baseOutputStream">
		/// The stream to read data (to be compressed) from
		/// </param>
		public GZipOutputStream(Stream baseOutputStream)
			: this(baseOutputStream, 4096)
		{
		}
		
		/// <summary>
		/// Creates a GZipOutputStream with the specified buffer size
		/// </summary>
		/// <param name="baseOutputStream">
		/// The stream to read data (to be compressed) from
		/// </param>
		/// <param name="size">
		/// Size of the buffer to use
		/// </param>
		public GZipOutputStream(Stream baseOutputStream, int size) : base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true), size)
		{
		}
		#endregion

		#region Public API
		/// <summary>
		/// Sets the active compression level (1-9).  The new level will be activated
		/// immediately.
		/// </summary>
		/// <param name="level">The compression level to set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Level specified is not supported.
		/// </exception>
		/// <see cref="Deflater"/>
		public void SetLevel(int level)
		{
			if (level < Deflater.BEST_SPEED) {
				throw new ArgumentOutOfRangeException("level");
			}
			deflater_.SetLevel(level);
		}
		
		/// <summary>
		/// Get the current compression level.
		/// </summary>
		/// <returns>The current compression level.</returns>
		public int GetLevel()
		{
			return deflater_.GetLevel();
		}
		#endregion

		#region Stream overrides
		/// <summary>
		/// Write given buffer to output updating crc
		/// </summary>
		/// <param name="buffer">Buffer to write</param>
		/// <param name="offset">Offset of first byte in buf to write</param>
		/// <param name="count">Number of bytes to write</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if ( state_ == OutputState.Header ) {
				WriteHeader();
			}

            if( state_!=OutputState.Footer )
            {
                throw new InvalidOperationException("Write not permitted in current state");
            }

			crc.Update(buffer, offset, count);
			base.Write(buffer, offset, count);
		}
		
		/// <summary>
		/// Writes remaining compressed output data to the output stream
		/// and closes it.
		/// </summary>
		public override void Close()
		{
			try {
				Finish();
			}
			finally {
                if ( state_ != OutputState.Closed ) {
                    state_ = OutputState.Closed;
				    if( IsStreamOwner ) {
					    baseOutputStream_.Close();
				    }
                }
			}
		}
		#endregion

		#region DeflaterOutputStream overrides
		/// <summary>
		/// Finish compression and write any footer information required to stream
		/// </summary>
		public override void Finish()
		{
			// If no data has been written a header should be added.
			if ( state_ == OutputState.Header ) {
				WriteHeader();
			}

            if( state_ == OutputState.Footer)
            {
                state_=OutputState.Finished;
                base.Finish();

                uint totalin=(uint)(deflater_.TotalIn&0xffffffff);
                uint crcval=(uint)(crc.Value&0xffffffff);

                byte[] gzipFooter;

                unchecked
                {
                    gzipFooter=new byte[] {
					(byte) crcval, (byte) (crcval >> 8),
					(byte) (crcval >> 16), (byte) (crcval >> 24),

					(byte) totalin, (byte) (totalin >> 8),
					(byte) (totalin >> 16), (byte) (totalin >> 24)
				};
                }

                baseOutputStream_.Write(gzipFooter, 0, gzipFooter.Length);
            }
		}
		#endregion

		#region Support Routines
		void WriteHeader()
		{
			if ( state_ == OutputState.Header )
			{
                state_=OutputState.Footer;

				int mod_time = (int)((DateTime.Now.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000L);  // Ticks give back 100ns intervals
				byte[] gzipHeader = {
					// The two magic bytes
					(byte) (GZipConstants.GZIP_MAGIC >> 8), (byte) (GZipConstants.GZIP_MAGIC & 0xff),

					// The compression type
					(byte) Deflater.DEFLATED,

					// The flags (not set)
					0,

					// The modification time
					(byte) mod_time, (byte) (mod_time >> 8),
					(byte) (mod_time >> 16), (byte) (mod_time >> 24),

					// The extra flags
					0,

					// The OS type (unknown)
					(byte) 255
				};
				baseOutputStream_.Write(gzipHeader, 0, gzipHeader.Length);
			}
		}
		#endregion
	}
}
