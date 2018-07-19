// PendingBuffer.cs
//
// Copyright (C) 2001 Mike Krueger
// Copyright (C) 2004 John Reilly
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

namespace ICSharpCode.SharpZipLib.Zip.Compression 
{
	
	/// <summary>
	/// This class is general purpose class for writing data to a buffer.
	/// 
	/// It allows you to write bits as well as bytes
	/// Based on DeflaterPending.java
	/// 
	/// author of the original java version : Jochen Hoenicke
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class PendingBuffer
	{
		/// <summary>Internal work buffer
		/// </summary>
		protected byte[] buf;
		
		int    start;
		int    end;
		
		uint    bits;
		int    bitCount;

		/// <summary>
		/// construct instance using default buffer size of 4096
		/// </summary>
		public PendingBuffer() : this( 4096 )
		{
			
		}
		
		/// <summary>
		/// construct instance using specified buffer size
		/// </summary>
		/// <param name="bufsize">
		/// size to use for internal buffer
		/// </param>
		public PendingBuffer(int bufsize)
		{
			buf = new byte[bufsize];
		}

		/// <summary>
		/// Clear internal state/buffers
		/// </summary>
		public void Reset() 
		{
			start = end = bitCount = 0;
		}

		/// <summary>
		/// write a byte to buffer
		/// </summary>
		/// <param name="b">
		/// value to write
		/// </param>
		public void WriteByte(int b)
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			buf[end++] = (byte) b;
		}

		/// <summary>
		/// Write a short value to buffer LSB first
		/// </summary>
		/// <param name="s">
		/// value to write
		/// </param>
		public void WriteShort(int s)
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			buf[end++] = (byte) s;
			buf[end++] = (byte) (s >> 8);
		}

		/// <summary>
		/// write an integer LSB first
		/// </summary>
		/// <param name="s">value to write</param>
		public void WriteInt(int s)
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			buf[end++] = (byte) s;
			buf[end++] = (byte) (s >> 8);
			buf[end++] = (byte) (s >> 16);
			buf[end++] = (byte) (s >> 24);
		}
		
		/// <summary>
		/// Write a block of data to buffer
		/// </summary>
		/// <param name="block">data to write</param>
		/// <param name="offset">offset of first byte to write</param>
		/// <param name="len">number of bytes to write</param>
		public void WriteBlock(byte[] block, int offset, int len)
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			System.Array.Copy(block, offset, buf, end, len);
			end += len;
		}

		/// <summary>
		/// The number of bits written to the buffer
		/// </summary>
		public int BitCount {
			get {
				return bitCount;
			}
		}
		
		/// <summary>
		/// Align internal buffer on a byte boundary
		/// </summary>
		public void AlignToByte() 
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			if (bitCount > 0) {
				buf[end++] = (byte) bits;
				if (bitCount > 8) {
					buf[end++] = (byte) (bits >> 8);
				}
			}
			bits = 0;
			bitCount = 0;
		}

		/// <summary>
		/// Write bits to internal buffer
		/// </summary>
		/// <param name="b">source of bits</param>
		/// <param name="count">number of bits to write</param>
		public void WriteBits(int b, int count)
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			//			if (DeflaterConstants.DEBUGGING) {
			//				//Console.WriteLine("writeBits("+b+","+count+")");
			//			}
			bits |= (uint)(b << bitCount);
			bitCount += count;
			if (bitCount >= 16) {
				buf[end++] = (byte) bits;
				buf[end++] = (byte) (bits >> 8);
				bits >>= 16;
				bitCount -= 16;
			}
		}

		/// <summary>
		/// Write a short value to internal buffer most significant byte first
		/// </summary>
		/// <param name="s">value to write</param>
		public void WriteShortMSB(int s) 
		{
			if (DeflaterConstants.DEBUGGING && start != 0) {
				throw new SharpZipBaseException();
			}
			buf[end++] = (byte) (s >> 8);
			buf[end++] = (byte) s;
		}
		
		/// <summary>
		/// Indicates if buffer has been flushed
		/// </summary>
		public bool IsFlushed {
			get {
				return end == 0;
			}
		}
		
		/// <summary>
		/// Flushes the pending buffer into the given output array.  If the
		/// output array is to small, only a partial flush is done.
		/// </summary>
		/// <param name="output">
		/// the output array;
		/// </param>
		/// <param name="offset">
		/// the offset into output array;
		/// </param>
		/// <param name="length">		
		/// length the maximum number of bytes to store;
		/// </param>
		/// <exception name="ArgumentOutOfRangeException">
		/// IndexOutOfBoundsException if offset or length are invalid.
		/// </exception>
		public int Flush(byte[] output, int offset, int length) 
		{
			if (bitCount >= 8) {
				buf[end++] = (byte) bits;
				bits >>= 8;
				bitCount -= 8;
			}
			if (length > end - start) {
				length = end - start;
				System.Array.Copy(buf, start, output, offset, length);
				start = 0;
				end = 0;
			} else {
				System.Array.Copy(buf, start, output, offset, length);
				start += length;
			}
			return length;
		}

		/// <summary>
		/// Convert internal buffer to byte array.
		/// Buffer is empty on completion
		/// </summary>
		/// <returns>
		/// converted buffer contents contents
		/// </returns>
		public byte[] ToByteArray()
		{
			byte[] ret = new byte[end - start];
			System.Array.Copy(buf, start, ret, 0, ret.Length);
			start = 0;
			end = 0;
			return ret;
		}
	}
}	
