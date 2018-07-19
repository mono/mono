// BZip2.cs
//
// Copyright (C) 2001 Mike Krueger
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

namespace ICSharpCode.SharpZipLib.BZip2
{
	
	/// <summary>
	/// Does all the compress and decompress pre-operation stuff.
	/// Sets up the streams and file header characters.
	/// Uses multiply overloaded methods to call for the compress/decompress.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public sealed class BZip2
	{
		/// <summary>
		/// Decompress <paramref name="instream">input</paramref> writing 
		/// decompressed data to <paramref name="outstream">output stream</paramref>
		/// </summary>
		public static void Decompress(Stream instream, Stream outstream) 
		{
			System.IO.Stream bos = outstream;
			System.IO.Stream bis = instream;
			BZip2InputStream bzis = new BZip2InputStream(bis);
			int ch = bzis.ReadByte();
			while (ch != -1) {
				bos.WriteByte((byte)ch);
				ch = bzis.ReadByte();
			}
			bos.Flush();
		}
		
		/// <summary>
		/// Compress <paramref name="instream">input stream</paramref> sending 
		/// result to <paramref name="outputstream">output stream</paramref>
		/// </summary>
		public static void Compress(Stream instream, Stream outstream, int blockSize) 
		{			
			System.IO.Stream bos = outstream;
			System.IO.Stream bis = instream;
			int ch = bis.ReadByte();
			BZip2OutputStream bzos = new BZip2OutputStream(bos, blockSize);
			while (ch != -1) {
				bzos.WriteByte((byte)ch);
				ch = bis.ReadByte();
			}
			bis.Close();
			bzos.Close();
		}
	}
}

/* derived from a file which contained this license :
 * Copyright (c) 1999-2001 Keiron Liddle, Aftex Software
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
*/
