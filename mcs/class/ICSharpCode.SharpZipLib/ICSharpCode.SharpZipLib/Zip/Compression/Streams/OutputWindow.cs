// OutputWindow.cs
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

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams {
	
	/// <summary>
	/// Contains the output from the Inflation process.
	/// We need to have a window so that we can refer backwards into the output stream
	/// to repeat stuff.
	///
	/// author of the original java version : John Leuner
	/// </summary>
	public class OutputWindow
	{
		private static int WINDOW_SIZE = 1 << 15;
		private static int WINDOW_MASK = WINDOW_SIZE - 1;
		
		private byte[] window = new byte[WINDOW_SIZE]; //The window is 2^15 bytes
		private int window_end  = 0;
		private int window_filled = 0;
		
		public void Write(int abyte)
		{
			if (window_filled++ == WINDOW_SIZE) {
				throw new InvalidOperationException("Window full");
			}
			window[window_end++] = (byte) abyte;
			window_end &= WINDOW_MASK;
		}
		
		
		private void SlowRepeat(int rep_start, int len, int dist)
		{
			while (len-- > 0) {
				window[window_end++] = window[rep_start++];
				window_end &= WINDOW_MASK;
				rep_start &= WINDOW_MASK;
			}
		}
		
		public void Repeat(int len, int dist)
		{
			if ((window_filled += len) > WINDOW_SIZE) {
				throw new InvalidOperationException("Window full");
			}
			
			int rep_start = (window_end - dist) & WINDOW_MASK;
			int border = WINDOW_SIZE - len;
			if (rep_start <= border && window_end < border) {
				if (len <= dist) {
					System.Array.Copy(window, rep_start, window, window_end, len);
					window_end += len;
				}				else {
					/* We have to copy manually, since the repeat pattern overlaps.
					*/
					while (len-- > 0) {
						window[window_end++] = window[rep_start++];
					}
				}
			} else {
				SlowRepeat(rep_start, len, dist);
			}
		}
		
		public int CopyStored(StreamManipulator input, int len)
		{
			len = Math.Min(Math.Min(len, WINDOW_SIZE - window_filled), input.AvailableBytes);
			int copied;
			
			int tailLen = WINDOW_SIZE - window_end;
			if (len > tailLen) {
				copied = input.CopyBytes(window, window_end, tailLen);
				if (copied == tailLen) {
					copied += input.CopyBytes(window, 0, len - tailLen);
				}
			} else {
				copied = input.CopyBytes(window, window_end, len);
			}
			
			window_end = (window_end + copied) & WINDOW_MASK;
			window_filled += copied;
			return copied;
		}
		
		public void CopyDict(byte[] dict, int offset, int len)
		{
			if (window_filled > 0) {
				throw new InvalidOperationException();
			}
			
			if (len > WINDOW_SIZE) {
				offset += len - WINDOW_SIZE;
				len = WINDOW_SIZE;
			}
			System.Array.Copy(dict, offset, window, 0, len);
			window_end = len & WINDOW_MASK;
		}
		
		public int GetFreeSpace()
		{
			return WINDOW_SIZE - window_filled;
		}
		
		public int GetAvailable()
		{
			return window_filled;
		}
		
		public int CopyOutput(byte[] output, int offset, int len)
		{
			int copy_end = window_end;
			if (len > window_filled) {
				len = window_filled;
			} else {
				copy_end = (window_end - window_filled + len) & WINDOW_MASK;
			}
			
			int copied = len;
			int tailLen = len - copy_end;
			
			if (tailLen > 0) {
				System.Array.Copy(window, WINDOW_SIZE - tailLen,
				                  output, offset, tailLen);
				offset += tailLen;
				len = copy_end;
			}
			System.Array.Copy(window, copy_end - len, output, offset, len);
			window_filled -= copied;
			if (window_filled < 0) {
				throw new InvalidOperationException();
			}
			return copied;
		}
		
		public void Reset()
		{
			window_filled = window_end = 0;
		}
	}
}
