/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using RAMOutputStream = Mono.Lucene.Net.Store.RAMOutputStream;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> This abstract class writes skip lists with multiple levels.
	/// 
	/// Example for skipInterval = 3:
	/// c            (skip level 2)
	/// c                 c                 c            (skip level 1) 
	/// x     x     x     x     x     x     x     x     x     x      (skip level 0)
	/// d d d d d d d d d d d d d d d d d d d d d d d d d d d d d d d d  (posting list)
	/// 3     6     9     12    15    18    21    24    27    30     (df)
	/// 
	/// d - document
	/// x - skip data
	/// c - skip data with child pointer
	/// 
	/// Skip level i contains every skipInterval-th entry from skip level i-1.
	/// Therefore the number of entries on level i is: floor(df / ((skipInterval ^ (i + 1))).
	/// 
	/// Each skip entry on a level i>0 contains a pointer to the corresponding skip entry in list i-1.
	/// This guarantess a logarithmic amount of skips to find the target document.
	/// 
	/// While this class takes care of writing the different skip levels,
	/// subclasses must define the actual format of the skip data.
	/// 
	/// </summary>
	abstract class MultiLevelSkipListWriter
	{
		// number of levels in this skip list
		private int numberOfSkipLevels;
		
		// the skip interval in the list with level = 0
		private int skipInterval;
		
		// for every skip level a different buffer is used 
		private RAMOutputStream[] skipBuffer;
		
		protected internal MultiLevelSkipListWriter(int skipInterval, int maxSkipLevels, int df)
		{
			this.skipInterval = skipInterval;
			
			// calculate the maximum number of skip levels for this document frequency
			numberOfSkipLevels = df == 0?0:(int) System.Math.Floor(System.Math.Log(df) / System.Math.Log(skipInterval));
			
			// make sure it does not exceed maxSkipLevels
			if (numberOfSkipLevels > maxSkipLevels)
			{
				numberOfSkipLevels = maxSkipLevels;
			}
		}
		
		protected internal virtual void  Init()
		{
			skipBuffer = new RAMOutputStream[numberOfSkipLevels];
			for (int i = 0; i < numberOfSkipLevels; i++)
			{
				skipBuffer[i] = new RAMOutputStream();
			}
		}
		
		protected internal virtual void  ResetSkip()
		{
			// creates new buffers or empties the existing ones
			if (skipBuffer == null)
			{
				Init();
			}
			else
			{
				for (int i = 0; i < skipBuffer.Length; i++)
				{
					skipBuffer[i].Reset();
				}
			}
		}
		
		/// <summary> Subclasses must implement the actual skip data encoding in this method.
		/// 
		/// </summary>
		/// <param name="level">the level skip data shall be writting for
		/// </param>
		/// <param name="skipBuffer">the skip buffer to write to
		/// </param>
		protected internal abstract void  WriteSkipData(int level, IndexOutput skipBuffer);
		
		/// <summary> Writes the current skip data to the buffers. The current document frequency determines
		/// the max level is skip data is to be written to. 
		/// 
		/// </summary>
		/// <param name="df">the current document frequency 
		/// </param>
		/// <throws>  IOException </throws>
		internal virtual void  BufferSkip(int df)
		{
			int numLevels;
			
			// determine max level
			for (numLevels = 0; (df % skipInterval) == 0 && numLevels < numberOfSkipLevels; df /= skipInterval)
			{
				numLevels++;
			}
			
			long childPointer = 0;
			
			for (int level = 0; level < numLevels; level++)
			{
				WriteSkipData(level, skipBuffer[level]);
				
				long newChildPointer = skipBuffer[level].GetFilePointer();
				
				if (level != 0)
				{
					// store child pointers for all levels except the lowest
					skipBuffer[level].WriteVLong(childPointer);
				}
				
				//remember the childPointer for the next level
				childPointer = newChildPointer;
			}
		}
		
		/// <summary> Writes the buffered skip lists to the given output.
		/// 
		/// </summary>
		/// <param name="output">the IndexOutput the skip lists shall be written to 
		/// </param>
		/// <returns> the pointer the skip list starts
		/// </returns>
		internal virtual long WriteSkip(IndexOutput output)
		{
			long skipPointer = output.GetFilePointer();
			if (skipBuffer == null || skipBuffer.Length == 0)
				return skipPointer;
			
			for (int level = numberOfSkipLevels - 1; level > 0; level--)
			{
				long length = skipBuffer[level].GetFilePointer();
				if (length > 0)
				{
					output.WriteVLong(length);
					skipBuffer[level].WriteTo(output);
				}
			}
			skipBuffer[0].WriteTo(output);
			
			return skipPointer;
		}
	}
}
