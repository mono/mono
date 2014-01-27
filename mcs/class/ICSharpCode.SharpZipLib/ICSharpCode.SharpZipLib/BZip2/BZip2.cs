// BZip2.cs
//
// Copyright (C) 2010 David Pierson
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

// Suppress this in CF and 1.1, not needed. Static classes introduced in C# version 2.0
#if !NETCF_2_0 && !NET_1_1

using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.BZip2 {
	
	/// <summary>
	/// An example class to demonstrate compression and decompression of BZip2 streams.
	/// </summary>
	public static class BZip2
	{
		/// <summary>
		/// Decompress the <paramref name="inStream">input</paramref> writing
		/// uncompressed data to the <paramref name="outStream">output stream</paramref>
		/// </summary>
		/// <param name="inStream">The readable stream containing data to decompress.</param>
		/// <param name="outStream">The output stream to receive the decompressed data.</param>
		/// <param name="isStreamOwner">Both streams are closed on completion if true.</param>
		public static void Decompress(Stream inStream, Stream outStream, bool isStreamOwner)
		{
			if (inStream == null || outStream == null) {
				throw new Exception("Null Stream");
			}

			try {
				using (BZip2InputStream bzipInput = new BZip2InputStream(inStream)) {
					bzipInput.IsStreamOwner = isStreamOwner;
					Core.StreamUtils.Copy(bzipInput, outStream, new byte[4096]);
				}
			} finally {
				if (isStreamOwner) {
					// inStream is closed by the BZip2InputStream if stream owner
					outStream.Close();
				}
			}
		}
		
		/// <summary>
		/// Compress the <paramref name="inStream">input stream</paramref> sending
		/// result data to <paramref name="outStream">output stream</paramref>
		/// </summary>
		/// <param name="inStream">The readable stream to compress.</param>
		/// <param name="outStream">The output stream to receive the compressed data.</param>
		/// <param name="isStreamOwner">Both streams are closed on completion if true.</param>
		/// <param name="level">Block size acts as compression level (1 to 9) with 1 giving
		/// the lowest compression and 9 the highest.</param>
		public static void Compress(Stream inStream, Stream outStream, bool isStreamOwner, int level)
		{
			if (inStream == null || outStream == null) {
				throw new Exception("Null Stream");
			}

			try {
				using (BZip2OutputStream bzipOutput = new BZip2OutputStream(outStream, level)) {
					bzipOutput.IsStreamOwner = isStreamOwner;
					Core.StreamUtils.Copy(inStream, bzipOutput, new byte[4096]);
				}
			} finally {
				if (isStreamOwner) {
					// outStream is closed by the BZip2OutputStream if stream owner
					inStream.Close();
				}
			}
		}

	}
}
#endif
