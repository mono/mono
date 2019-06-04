// DeflaterEngine.cs
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

using ICSharpCode.SharpZipLib.Checksums;

namespace ICSharpCode.SharpZipLib.Zip.Compression 
{
	
	/// <summary>
	/// Strategies for deflater
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public enum DeflateStrategy 
	{
		/// <summary>
		/// The default strategy
		/// </summary>
		Default  = 0,
		
		/// <summary>
		/// This strategy will only allow longer string repetitions.  It is
		/// useful for random data with a small character set.
		/// </summary>
		Filtered = 1,

			
		/// <summary>
		/// This strategy will not look for string repetitions at all.  It
		/// only encodes with Huffman trees (which means, that more common
		/// characters get a smaller encoding.
		/// </summary>
		HuffmanOnly = 2
	}

	// DEFLATE ALGORITHM:
	// 
	// The uncompressed stream is inserted into the window array.  When
	// the window array is full the first half is thrown away and the
	// second half is copied to the beginning.
	//
	// The head array is a hash table.  Three characters build a hash value
	// and they the value points to the corresponding index in window of 
	// the last string with this hash.  The prev array implements a
	// linked list of matches with the same hash: prev[index & WMASK] points
	// to the previous index with the same hash.
	// 

	
	/// <summary>
	/// Low level compression engine for deflate algorithm which uses a 32K sliding window
	/// with secondary compression from Huffman/Shannon-Fano codes.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class DeflaterEngine : DeflaterConstants 
	{
		static int TOO_FAR = 4096;
		
		int ins_h;

		/// <summary>
		/// Hashtable, hashing three characters to an index for window, so
		/// that window[index]..window[index+2] have this hash code.  
		/// Note that the array should really be unsigned short, so you need
		/// to and the values with 0xffff.
		/// </summary>
		short[] head;

		/// <summary>
		/// <code>prev[index &amp; WMASK]</code> points to the previous index that has the
		/// same hash code as the string starting at index.  This way 
		/// entries with the same hash code are in a linked list.
		/// Note that the array should really be unsigned short, so you need
		/// to and the values with 0xffff.
		/// </summary>
		short[] prev;
		
		int    matchStart;
		int    matchLen;
		bool   prevAvailable;
		int    blockStart;

		/// <summary>
		/// Points to the current character in the window.
		/// </summary>
		int    strstart;

		/// <summary>
		/// lookahead is the number of characters starting at strstart in
		/// window that are valid.
		/// So window[strstart] until window[strstart+lookahead-1] are valid
		/// characters.
		/// </summary>
		int    lookahead;

		/// <summary>
		/// This array contains the part of the uncompressed stream that 
		/// is of relevance.  The current character is indexed by strstart.
		/// </summary>
		byte[] window;
		
		DeflateStrategy strategy;
		int max_chain, max_lazy, niceLength, goodLength;
		
		/// <summary>
		/// The current compression function.
		/// </summary>
		int comprFunc;
		
		/// <summary>
		/// The input data for compression.
		/// </summary>
		byte[] inputBuf;
		
		/// <summary>
		/// The total bytes of input read.
		/// </summary>
		int totalIn;
		
		/// <summary>
		/// The offset into inputBuf, where input data starts.
		/// </summary>
		int inputOff;
		
		/// <summary>
		/// The end offset of the input data.
		/// </summary>
		int inputEnd;
		
		DeflaterPending pending;
		DeflaterHuffman huffman;
		
		/// <summary>
		/// The adler checksum
		/// </summary>
		Adler32 adler;
		
		/// <summary>
		/// Construct instance with pending buffer
		/// </summary>
		/// <param name="pending">
		/// Pending buffer to use
		/// </param>>
		public DeflaterEngine(DeflaterPending pending) 
		{
			this.pending = pending;
			huffman = new DeflaterHuffman(pending);
			adler = new Adler32();
			
			window = new byte[2 * WSIZE];
			head   = new short[HASH_SIZE];
			prev   = new short[WSIZE];
			
			// We start at index 1, to avoid an implementation deficiency, that
		   // we cannot build a repeat pattern at index 0.
			blockStart = strstart = 1;
		}

		/// <summary>
		/// Reset internal state
		/// </summary>		
		public void Reset()
		{
			huffman.Reset();
			adler.Reset();
			blockStart = strstart = 1;
			lookahead = 0;
			totalIn   = 0;
			prevAvailable = false;
			matchLen = MIN_MATCH - 1;
			
			for (int i = 0; i < HASH_SIZE; i++) {
				head[i] = 0;
			}
			
			for (int i = 0; i < WSIZE; i++) {
				prev[i] = 0;
			}
		}

		/// <summary>
		/// Reset Adler checksum
		/// </summary>		
		public void ResetAdler()
		{
			adler.Reset();
		}

		/// <summary>
		/// Get current value of Adler checksum
		/// </summary>		
		public int Adler {
			get {
				return (int)adler.Value;
			}
		}

		/// <summary>
		/// Total data processed
		/// </summary>		
		public int TotalIn {
			get {
				return totalIn;
			}
		}

		/// <summary>
		/// Get/set the <see cref="DeflateStrategy">deflate strategy</see>
		/// </summary>		
		public DeflateStrategy Strategy {
			get {
				return strategy;
			}
			set {
				strategy = value;
			}
		}
		
		/// <summary>
		/// Set the deflate level (0-9)
		/// </summary>
		public void SetLevel(int lvl)
		{
			goodLength = DeflaterConstants.GOOD_LENGTH[lvl];
			max_lazy   = DeflaterConstants.MAX_LAZY[lvl];
			niceLength = DeflaterConstants.NICE_LENGTH[lvl];
			max_chain  = DeflaterConstants.MAX_CHAIN[lvl];
			
			if (DeflaterConstants.COMPR_FUNC[lvl] != comprFunc) {
/*
				if (DeflaterConstants.DEBUGGING) {
				   Console.WriteLine("Change from " + comprFunc + " to "
									      + DeflaterConstants.COMPR_FUNC[lvl]);
				}
*/
				switch (comprFunc) {
					case DEFLATE_STORED:
						if (strstart > blockStart) {
							huffman.FlushStoredBlock(window, blockStart,
								strstart - blockStart, false);
							blockStart = strstart;
						}
						UpdateHash();
						break;
					case DEFLATE_FAST:
						if (strstart > blockStart) {
							huffman.FlushBlock(window, blockStart, strstart - blockStart,
								false);
							blockStart = strstart;
						}
						break;
					case DEFLATE_SLOW:
						if (prevAvailable) {
							huffman.TallyLit(window[strstart-1] & 0xff);
						}
						if (strstart > blockStart) {
							huffman.FlushBlock(window, blockStart, strstart - blockStart, false);
							blockStart = strstart;
						}
						prevAvailable = false;
						matchLen = MIN_MATCH - 1;
						break;
				}
				comprFunc = COMPR_FUNC[lvl];
			}
		}
		
		void UpdateHash() 
		{
/*
			if (DEBUGGING) {
				Console.WriteLine("updateHash: "+strstart);
			}
*/
			ins_h = (window[strstart] << HASH_SHIFT) ^ window[strstart + 1];
		}
		
		/// <summary>
		/// Inserts the current string in the head hash and returns the previous
		/// value for this hash.
		/// </summary>
		/// <returns>The previous hash value</returns>
		int InsertString() 
		{
			short match;
			int hash = ((ins_h << HASH_SHIFT) ^ window[strstart + (MIN_MATCH -1)]) & HASH_MASK;
/*
			if (DeflaterConstants.DEBUGGING) {
				if (hash != (((window[strstart] << (2*HASH_SHIFT)) ^ 
							      (window[strstart + 1] << HASH_SHIFT) ^ 
							      (window[strstart + 2])) & HASH_MASK)) {
						throw new SharpZipBaseException("hash inconsistent: " + hash + "/"
									            +window[strstart] + ","
									            +window[strstart+1] + ","
									            +window[strstart+2] + "," + HASH_SHIFT);
					}
			}
*/
			prev[strstart & WMASK] = match = head[hash];
			head[hash] = (short)strstart;
			ins_h = hash;
			return match & 0xffff;
		}
		
		void SlideWindow()
		{
			Array.Copy(window, WSIZE, window, 0, WSIZE);
			matchStart -= WSIZE;
			strstart   -= WSIZE;
			blockStart -= WSIZE;
			
			/* Slide the hash table (could be avoided with 32 bit values
			 * at the expense of memory usage).
			 */
			for (int i = 0; i < HASH_SIZE; ++i) {
				int m = head[i] & 0xffff;
				head[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
			}
			
			/* Slide the prev table. */
			for (int i = 0; i < WSIZE; i++) {
				int m = prev[i] & 0xffff;
				prev[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
			}
		}
		
		/// <summary>
		/// Fill the window
		/// </summary>
		public void FillWindow()
		{
			/* If the window is almost full and there is insufficient lookahead,
			 * move the upper half to the lower one to make room in the upper half.
			 */
			if (strstart >= WSIZE + MAX_DIST) {
				SlideWindow();
			}
			
			/* If there is not enough lookahead, but still some input left,
			 * read in the input
			 */
			while (lookahead < DeflaterConstants.MIN_LOOKAHEAD && inputOff < inputEnd) {
				int more = 2 * WSIZE - lookahead - strstart;
				
				if (more > inputEnd - inputOff) {
					more = inputEnd - inputOff;
				}
				
				System.Array.Copy(inputBuf, inputOff, window, strstart + lookahead, more);
				adler.Update(inputBuf, inputOff, more);
				
				inputOff += more;
				totalIn  += more;
				lookahead += more;
			}
			
			if (lookahead >= MIN_MATCH) {
				UpdateHash();
			}
		}
		
		/// <summary>
		/// Find the best (longest) string in the window matching the 
		/// string starting at strstart.
		///
		/// Preconditions:
		/// <code>
		/// strstart + MAX_MATCH &lt;= window.length.</code>
		/// </summary>
		/// <param name="curMatch"></param>
		/// <returns>True if a match greater than the minimum length is found</returns>
		bool FindLongestMatch(int curMatch) 
		{
			int chainLength = this.max_chain;
			int niceLength  = this.niceLength;
			short[] prev    = this.prev;
			int scan        = this.strstart;
			int match;
			int best_end = this.strstart + matchLen;
			int best_len = Math.Max(matchLen, MIN_MATCH - 1);
			
			int limit = Math.Max(strstart - MAX_DIST, 0);
			
			int strend = strstart + MAX_MATCH - 1;
			byte scan_end1 = window[best_end - 1];
			byte scan_end  = window[best_end];
			
			/* Do not waste too much time if we already have a good match: */
			if (best_len >= this.goodLength) {
				chainLength >>= 2;
			}
			
			/* Do not look for matches beyond the end of the input. This is necessary
			* to make deflate deterministic.
			*/
			if (niceLength > lookahead) {
				niceLength = lookahead;
			}
			
/*
			if (DeflaterConstants.DEBUGGING && strstart > 2 * WSIZE - MIN_LOOKAHEAD) {
				throw new InvalidOperationException("need lookahead");
			}
*/			
			do {
/*
				if (DeflaterConstants.DEBUGGING && curMatch >= strstart) {
					throw new InvalidOperationException("future match");
				}
*/            
				if (window[curMatch + best_len] != scan_end      || 
					window[curMatch + best_len - 1] != scan_end1 || 
					window[curMatch] != window[scan]             || 
					window[curMatch + 1] != window[scan + 1]) {
					continue;
				}
				
				match = curMatch + 2;
				scan += 2;
				
				/* We check for insufficient lookahead only every 8th comparison;
				* the 256th check will be made at strstart + 258.
				*/
			while (window[++scan] == window[++match] && 
				window[++scan] == window[++match] && 
				window[++scan] == window[++match] && 
				window[++scan] == window[++match] && 
				window[++scan] == window[++match] && 
				window[++scan] == window[++match] && 
				window[++scan] == window[++match] && 
				window[++scan] == window[++match] && scan < strend) ;
				
				if (scan > best_end) {
/*
					if (DeflaterConstants.DEBUGGING && ins_h == 0)
						System.err.println("Found match: "+curMatch+"-"+(scan-strstart));
*/
					matchStart = curMatch;
					best_end = scan;
					best_len = scan - strstart;
					
					if (best_len >= niceLength) {
						break;
					}
					
					scan_end1  = window[best_end - 1];
					scan_end   = window[best_end];
				}
				scan = strstart;
			} while ((curMatch = (prev[curMatch & WMASK] & 0xffff)) > limit && --chainLength != 0);
			
			matchLen = Math.Min(best_len, lookahead);
			return matchLen >= MIN_MATCH;
		}
		
		/// <summary>
		/// Set compression dictionary
		/// </summary>
		public void SetDictionary(byte[] buffer, int offset, int length) 
		{
/*
			if (DeflaterConstants.DEBUGGING && strstart != 1) {
				throw new InvalidOperationException("strstart not 1");
			}
*/
			adler.Update(buffer, offset, length);
			if (length < MIN_MATCH) {
				return;
			}
			if (length > MAX_DIST) {
				offset += length - MAX_DIST;
				length = MAX_DIST;
			}
			
			System.Array.Copy(buffer, offset, window, strstart, length);
			
			UpdateHash();
			--length;
			while (--length > 0) {
				InsertString();
				strstart++;
			}
			strstart += 2;
			blockStart = strstart;
		}
		
		bool DeflateStored(bool flush, bool finish)
		{
			if (!flush && lookahead == 0) {
				return false;
			}
			
			strstart += lookahead;
			lookahead = 0;
			
			int storedLen = strstart - blockStart;
			
			if ((storedLen >= DeflaterConstants.MAX_BLOCK_SIZE) || /* Block is full */
				(blockStart < WSIZE && storedLen >= MAX_DIST) ||   /* Block may move out of window */
				flush) {
				bool lastBlock = finish;
				if (storedLen > DeflaterConstants.MAX_BLOCK_SIZE) {
					storedLen = DeflaterConstants.MAX_BLOCK_SIZE;
					lastBlock = false;
				}
				
/*
				if (DeflaterConstants.DEBUGGING) {
				   Console.WriteLine("storedBlock["+storedLen+","+lastBlock+"]");
				}
*/
	
				huffman.FlushStoredBlock(window, blockStart, storedLen, lastBlock);
				blockStart += storedLen;
				return !lastBlock;
			}
			return true;
		}
		
		private bool DeflateFast(bool flush, bool finish)
		{
			if (lookahead < MIN_LOOKAHEAD && !flush) {
				return false;
			}
			
			while (lookahead >= MIN_LOOKAHEAD || flush) {
				if (lookahead == 0) {
					/* We are flushing everything */
					huffman.FlushBlock(window, blockStart, strstart - blockStart, finish);
					blockStart = strstart;
					return false;
				}
				
				if (strstart > 2 * WSIZE - MIN_LOOKAHEAD) {
					/* slide window, as findLongestMatch needs this.
					 * This should only happen when flushing and the window
					 * is almost full.
					 */
					SlideWindow();
				}
				
				int hashHead;
				if (lookahead >= MIN_MATCH && 
					(hashHead = InsertString()) != 0 && 
					strategy != DeflateStrategy.HuffmanOnly &&
					strstart - hashHead <= MAX_DIST && 
					FindLongestMatch(hashHead)) {
					/* longestMatch sets matchStart and matchLen */
/*
					if (DeflaterConstants.DEBUGGING) {
						for (int i = 0 ; i < matchLen; i++) {
							if (window[strstart+i] != window[matchStart + i]) {
								throw new SharpZipBaseException("Match failure");
							}
						}
					}
*/					
					// -jr- Hak hak hak this stops problems with fast/low compression and index out of range
					if (huffman.TallyDist(strstart - matchStart, matchLen)) {
						bool lastBlock = finish && lookahead == 0;
						huffman.FlushBlock(window, blockStart, strstart - blockStart, lastBlock);
						blockStart = strstart;
					}
					
					lookahead -= matchLen;
					if (matchLen <= max_lazy && lookahead >= MIN_MATCH) {
						while (--matchLen > 0) {
							++strstart;
							InsertString();
						}
						++strstart;
					} else {
						strstart += matchLen;
						if (lookahead >= MIN_MATCH - 1) {
							UpdateHash();
						}
					}
					matchLen = MIN_MATCH - 1;
					continue;
				} else {
					/* No match found */
					huffman.TallyLit(window[strstart] & 0xff);
					++strstart;
					--lookahead;
				}
				
				if (huffman.IsFull()) {
					bool lastBlock = finish && lookahead == 0;
					huffman.FlushBlock(window, blockStart, strstart - blockStart, lastBlock);
					blockStart = strstart;
					return !lastBlock;
				}
			}
			return true;
		}
		
		bool DeflateSlow(bool flush, bool finish)
		{
			if (lookahead < MIN_LOOKAHEAD && !flush) {
				return false;
			}
			
			while (lookahead >= MIN_LOOKAHEAD || flush) {
				if (lookahead == 0) {
					if (prevAvailable) {
						huffman.TallyLit(window[strstart-1] & 0xff);
					}
					prevAvailable = false;
					
					// We are flushing everything
/*
					if (DeflaterConstants.DEBUGGING && !flush) {
						throw new SharpZipBaseException("Not flushing, but no lookahead");
					}
*/               
					huffman.FlushBlock(window, blockStart, strstart - blockStart,
						finish);
					blockStart = strstart;
					return false;
				}
				
				if (strstart >= 2 * WSIZE - MIN_LOOKAHEAD) {
					/* slide window, as findLongestMatch need this.
					 * This should only happen when flushing and the window
					 * is almost full.
					 */
					SlideWindow();
				}
				
				int prevMatch = matchStart;
				int prevLen = matchLen;
				if (lookahead >= MIN_MATCH) {
					int hashHead = InsertString();
					if (strategy != DeflateStrategy.HuffmanOnly && hashHead != 0 && strstart - hashHead <= MAX_DIST && FindLongestMatch(hashHead)) {
						/* longestMatch sets matchStart and matchLen */
							
						/* Discard match if too small and too far away */
						if (matchLen <= 5 && (strategy == DeflateStrategy.Filtered || (matchLen == MIN_MATCH && strstart - matchStart > TOO_FAR))) {
							matchLen = MIN_MATCH - 1;
						}
					}
				}
				
				/* previous match was better */
				if (prevLen >= MIN_MATCH && matchLen <= prevLen) {
/*
					if (DeflaterConstants.DEBUGGING) {
					   for (int i = 0 ; i < matchLen; i++) {
					      if (window[strstart-1+i] != window[prevMatch + i])
					         throw new SharpZipBaseException();
						}
					}
*/               
					huffman.TallyDist(strstart - 1 - prevMatch, prevLen);
					prevLen -= 2;
					do {
						strstart++;
						lookahead--;
						if (lookahead >= MIN_MATCH) {
							InsertString();
						}
					} while (--prevLen > 0);
					strstart ++;
					lookahead--;
					prevAvailable = false;
					matchLen = MIN_MATCH - 1;
				} else {
					if (prevAvailable) {
						huffman.TallyLit(window[strstart-1] & 0xff);
					}
					prevAvailable = true;
					strstart++;
					lookahead--;
				}
				
				if (huffman.IsFull()) {
					int len = strstart - blockStart;
					if (prevAvailable) {
						len--;
					}
					bool lastBlock = (finish && lookahead == 0 && !prevAvailable);
					huffman.FlushBlock(window, blockStart, len, lastBlock);
					blockStart += len;
					return !lastBlock;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Deflate drives actual compression of data
		/// </summary>
		public bool Deflate(bool flush, bool finish)
		{
			bool progress;
			do {
				FillWindow();
				bool canFlush = flush && inputOff == inputEnd;
				//				if (DeflaterConstants.DEBUGGING) {
				//					//Console.WriteLine("window: ["+blockStart+","+strstart+","
				//					                  +lookahead+"], "+comprFunc+","+canFlush);
				//				}
				switch (comprFunc) {
					case DEFLATE_STORED:
						progress = DeflateStored(canFlush, finish);
						break;
					case DEFLATE_FAST:
						progress = DeflateFast(canFlush, finish);
						break;
					case DEFLATE_SLOW:
						progress = DeflateSlow(canFlush, finish);
						break;
					default:
						throw new InvalidOperationException("unknown comprFunc");
				}
			} while (pending.IsFlushed && progress); /* repeat while we have no pending output and progress was made */
			return progress;
		}


		/// <summary>
		/// Sets input data to be deflated.  Should only be called when <code>NeedsInput()</code>
		/// returns true
		/// </summary>
		/// <param name="buf">The buffer containing input data.</param>
		/// <param name="off">The index of the first byte of data.</param>
		/// <param name="len">The number of bytes of data to use as input.</param>
		public void SetInput(byte[] buf, int off, int len)
		{
			if (inputOff < inputEnd) {
				throw new InvalidOperationException("Old input was not completely processed");
			}
			
			int end = off + len;
			
			/* We want to throw an ArrayIndexOutOfBoundsException early.  The
			* check is very tricky: it also handles integer wrap around.
			*/
			if (0 > off || off > end || end > buf.Length) {
				throw new ArgumentOutOfRangeException();
			}
			
			inputBuf = buf;
			inputOff = off;
			inputEnd = end;
		}

		/// <summary>
		/// Return true if input is needed via <see cref="SetInput"> SetInput</see>
		/// </summary>		
		public bool NeedsInput()
		{
			return inputEnd == inputOff;
		}
	}
}
