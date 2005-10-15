// DeflaterConstants.cs
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
	/// This class contains constants used for deflation.
	/// </summary>
	public class DeflaterConstants 
	{
		/// <summary>
		/// Set to true to enable debugging
		/// </summary>
		public const bool DEBUGGING = false;
		
		/// <summary>
		/// Written to Zip file to identify a stored block
		/// </summary>
		public const int STORED_BLOCK = 0;
		
		/// <summary>
		/// Identifies static tree in Zip file
		/// </summary>
		public const int STATIC_TREES = 1;
		
		/// <summary>
		/// Identifies dynamic tree in Zip file
		/// </summary>
		public const int DYN_TREES    = 2;
		
		/// <summary>
		/// Header flag indicating a preset dictionary for deflation
		/// </summary>
		public const int PRESET_DICT  = 0x20;
		
		/// <summary>
		/// Sets internal buffer sizes for Huffman encoding
		/// </summary>
		public const int DEFAULT_MEM_LEVEL = 8;

		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int MAX_MATCH = 258;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int MIN_MATCH = 3;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int MAX_WBITS = 15;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int WSIZE = 1 << MAX_WBITS;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int WMASK = WSIZE - 1;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int HASH_BITS = DEFAULT_MEM_LEVEL + 7;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int HASH_SIZE = 1 << HASH_BITS;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int HASH_MASK = HASH_SIZE - 1;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int HASH_SHIFT = (HASH_BITS + MIN_MATCH - 1) / MIN_MATCH;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int MAX_DIST = WSIZE - MIN_LOOKAHEAD;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int PENDING_BUF_SIZE = 1 << (DEFAULT_MEM_LEVEL + 8);
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public static int MAX_BLOCK_SIZE = Math.Min(65535, PENDING_BUF_SIZE - 5);
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int DEFLATE_STORED = 0;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int DEFLATE_FAST   = 1;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public const int DEFLATE_SLOW   = 2;
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public static int[] GOOD_LENGTH = { 0, 4,  4,  4,  4,  8,   8,   8,   32,   32 };
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public static int[] MAX_LAZY    = { 0, 4,  5,  6,  4, 16,  16,  32,  128,  258 };
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public static int[] NICE_LENGTH = { 0, 8, 16, 32, 16, 32, 128, 128,  258,  258 };
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public static int[] MAX_CHAIN   = { 0, 4,  8, 32, 16, 32, 128, 256, 1024, 4096 };
		
		/// <summary>
		/// Internal compression engine constant
		/// </summary>		
		public static int[] COMPR_FUNC  = { 0, 1,  1,  1,  1,  2,   2,   2,    2,    2 };
		
	}
}
