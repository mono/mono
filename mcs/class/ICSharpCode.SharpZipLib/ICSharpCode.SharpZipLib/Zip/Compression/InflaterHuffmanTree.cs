// InflaterHuffmanTree.cs
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

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip.Compression 
{
	
	/// <summary>
	/// Huffman tree used for inflation
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class InflaterHuffmanTree 
	{
		static int MAX_BITLEN = 15;
		short[] tree;
		
		/// <summary>
		/// Literal length tree
		/// </summary>
		public static InflaterHuffmanTree defLitLenTree;
		
		/// <summary>
		/// Distance tree
		/// </summary>
		public static InflaterHuffmanTree defDistTree;
		
		static InflaterHuffmanTree()
		{
			try {
				byte[] codeLengths = new byte[288];
				int i = 0;
				while (i < 144) {
					codeLengths[i++] = 8;
				}
				while (i < 256) {
					codeLengths[i++] = 9;
				}
				while (i < 280) {
					codeLengths[i++] = 7;
				}
				while (i < 288) {
					codeLengths[i++] = 8;
				}
				defLitLenTree = new InflaterHuffmanTree(codeLengths);
				
				codeLengths = new byte[32];
				i = 0;
				while (i < 32) {
					codeLengths[i++] = 5;
				}
				defDistTree = new InflaterHuffmanTree(codeLengths);
			} catch (Exception) {
				throw new SharpZipBaseException("InflaterHuffmanTree: static tree length illegal");
			}
		}
		
		/// <summary>
		/// Constructs a Huffman tree from the array of code lengths.
		/// </summary>
		/// <param name = "codeLengths">
		/// the array of code lengths
		/// </param>
		public InflaterHuffmanTree(byte[] codeLengths)
		{
			BuildTree(codeLengths);
		}
		
		void BuildTree(byte[] codeLengths)
		{
			int[] blCount  = new int[MAX_BITLEN + 1];
			int[] nextCode = new int[MAX_BITLEN + 1];
			
			for (int i = 0; i < codeLengths.Length; i++) {
				int bits = codeLengths[i];
				if (bits > 0) {
					blCount[bits]++;
				}
			}
			
			int code = 0;
			int treeSize = 512;
			for (int bits = 1; bits <= MAX_BITLEN; bits++) {
				nextCode[bits] = code;
				code += blCount[bits] << (16 - bits);
				if (bits >= 10) {
					/* We need an extra table for bit lengths >= 10. */
					int start = nextCode[bits] & 0x1ff80;
					int end   = code & 0x1ff80;
					treeSize += (end - start) >> (16 - bits);
				}
			}
			
/* -jr comment this out! doesnt work for dynamic trees and pkzip 2.04g
			if (code != 65536) 
			{
				throw new SharpZipBaseException("Code lengths don't add up properly.");
			}
*/
			/* Now create and fill the extra tables from longest to shortest
			* bit len.  This way the sub trees will be aligned.
			*/
			tree = new short[treeSize];
			int treePtr = 512;
			for (int bits = MAX_BITLEN; bits >= 10; bits--) {
				int end   = code & 0x1ff80;
				code -= blCount[bits] << (16 - bits);
				int start = code & 0x1ff80;
				for (int i = start; i < end; i += 1 << 7) {
					tree[DeflaterHuffman.BitReverse(i)] = (short) ((-treePtr << 4) | bits);
					treePtr += 1 << (bits-9);
				}
			}
			
			for (int i = 0; i < codeLengths.Length; i++) {
				int bits = codeLengths[i];
				if (bits == 0) {
					continue;
				}
				code = nextCode[bits];
				int revcode = DeflaterHuffman.BitReverse(code);
				if (bits <= 9) {
					do {
						tree[revcode] = (short) ((i << 4) | bits);
						revcode += 1 << bits;
					} while (revcode < 512);
				} else {
					int subTree = tree[revcode & 511];
					int treeLen = 1 << (subTree & 15);
					subTree = -(subTree >> 4);
					do {
						tree[subTree | (revcode >> 9)] = (short) ((i << 4) | bits);
						revcode += 1 << bits;
					} while (revcode < treeLen);
				}
				nextCode[bits] = code + (1 << (16 - bits));
			}
			
		}
		
		/// <summary>
		/// Reads the next symbol from input.  The symbol is encoded using the
		/// huffman tree.
		/// </summary>
		/// <param name="input">
		/// input the input source.
		/// </param>
		/// <returns>
		/// the next symbol, or -1 if not enough input is available.
		/// </returns>
		public int GetSymbol(StreamManipulator input)
		{
			int lookahead, symbol;
			if ((lookahead = input.PeekBits(9)) >= 0) {
				if ((symbol = tree[lookahead]) >= 0) {
					input.DropBits(symbol & 15);
					return symbol >> 4;
				}
				int subtree = -(symbol >> 4);
				int bitlen = symbol & 15;
				if ((lookahead = input.PeekBits(bitlen)) >= 0) {
					symbol = tree[subtree | (lookahead >> 9)];
					input.DropBits(symbol & 15);
					return symbol >> 4;
				} else {
					int bits = input.AvailableBits;
					lookahead = input.PeekBits(bits);
					symbol = tree[subtree | (lookahead >> 9)];
					if ((symbol & 15) <= bits) {
						input.DropBits(symbol & 15);
						return symbol >> 4;
					} else {
						return -1;
					}
				}
			} else {
				int bits = input.AvailableBits;
				lookahead = input.PeekBits(bits);
				symbol = tree[lookahead];
				if (symbol >= 0 && (symbol & 15) <= bits) {
					input.DropBits(symbol & 15);
					return symbol >> 4;
				} else {
					return -1;
				}
			}
		}
	}
}

