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

// {{Aroush-2.9}} Port issue?  Both of those were treated as: System.IO.MemoryStream
//using CharBuffer = java.nio.CharBuffer;
//using ByteBuffer = java.nio.ByteBuffer;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Provides support for converting byte sequences to Strings and back again.
	/// The resulting Strings preserve the original byte sequences' sort order.
	/// 
	/// The Strings are constructed using a Base 8000h encoding of the original
	/// binary data - each char of an encoded String represents a 15-bit chunk
	/// from the byte sequence.  Base 8000h was chosen because it allows for all
	/// lower 15 bits of char to be used without restriction; the surrogate range 
	/// [U+D8000-U+DFFF] does not represent valid chars, and would require
	/// complicated handling to avoid them and allow use of char's high bit.
	/// 
	/// Although unset bits are used as padding in the final char, the original
	/// byte sequence could contain trailing bytes with no set bits (null bytes):
	/// padding is indistinguishable from valid information.  To overcome this
	/// problem, a char is appended, indicating the number of encoded bytes in the
	/// final content char.
	/// 
	/// This class's operations are defined over CharBuffers and ByteBuffers, to
	/// allow for wrapped arrays to be reused, reducing memory allocation costs for
	/// repeated operations.  Note that this class calls array() and arrayOffset()
	/// on the CharBuffers and ByteBuffers it uses, so only wrapped arrays may be
	/// used.  This class interprets the arrayOffset() and limit() values returned by
	/// its input buffers as beginning and end+1 positions on the wrapped array,
	/// resprectively; similarly, on the output buffer, arrayOffset() is the first
	/// position written to, and limit() is set to one past the final output array
	/// position.
	/// </summary>
	public class IndexableBinaryStringTools
	{
		
		private static readonly CodingCase[] CODING_CASES = new CodingCase[]{new CodingCase(7, 1), new CodingCase(14, 6, 2), new CodingCase(13, 5, 3), new CodingCase(12, 4, 4), new CodingCase(11, 3, 5), new CodingCase(10, 2, 6), new CodingCase(9, 1, 7), new CodingCase(8, 0)};
		
		// Export only static methods
		private IndexableBinaryStringTools()
		{
		}
		
		/// <summary> Returns the number of chars required to encode the given byte sequence.
		/// 
		/// </summary>
		/// <param name="original">The byte sequence to be encoded.  Must be backed by an array.
		/// </param>
		/// <returns> The number of chars required to encode the given byte sequence
		/// </returns>
		/// <throws>  IllegalArgumentException If the given ByteBuffer is not backed by an array </throws>
		public static int GetEncodedLength(System.Collections.Generic.List<byte> original)
		{
            return (original.Count == 0) ? 0 : ((original.Count * 8 + 14) / 15) + 1;
		}
		
		/// <summary> Returns the number of bytes required to decode the given char sequence.
		/// 
		/// </summary>
		/// <param name="encoded">The char sequence to be encoded.  Must be backed by an array.
		/// </param>
		/// <returns> The number of bytes required to decode the given char sequence
		/// </returns>
		/// <throws>  IllegalArgumentException If the given CharBuffer is not backed by an array </throws>
        public static int GetDecodedLength(System.Collections.Generic.List<char> encoded)
		{
            int numChars = encoded.Count - 1;
            if (numChars <= 0)
            {
                return 0;
            }
            else
            {
                int numFullBytesInFinalChar = encoded[encoded.Count - 1];
                int numEncodedChars = numChars - 1;
                return ((numEncodedChars * 15 + 7) / 8 + numFullBytesInFinalChar);
            }
		}
		
		/// <summary> Encodes the input byte sequence into the output char sequence.  Before
		/// calling this method, ensure that the output CharBuffer has sufficient
		/// capacity by calling {@link #GetEncodedLength(java.nio.ByteBuffer)}.
		/// 
		/// </summary>
		/// <param name="input">The byte sequence to encode
		/// </param>
		/// <param name="output">Where the char sequence encoding result will go.  The limit
		/// is set to one past the position of the final char.
		/// </param>
		/// <throws>  IllegalArgumentException If either the input or the output buffer </throws>
		/// <summary>  is not backed by an array
		/// </summary>
		public static void  Encode(System.Collections.Generic.List<byte> input, System.Collections.Generic.List<char> output)
		{
            int outputLength = GetEncodedLength(input);
            // only adjust capacity if needed
            if (output.Capacity < outputLength)
            {
                output.Capacity = outputLength;
            }

            // ensure the buffer we are writing into is occupied with nulls
            if (output.Count < outputLength)
            {
                for (int i = output.Count; i < outputLength; i++)
                {
                    output.Add(Char.MinValue);
                }
            }

            if (input.Count > 0)
            {
                int inputByteNum = 0;
                int caseNum = 0;
                int outputCharNum = 0;
                CodingCase codingCase;
                for (; inputByteNum + CODING_CASES[caseNum].numBytes <= input.Count; ++outputCharNum)
                {
                    codingCase = CODING_CASES[caseNum];
                    if (2 == codingCase.numBytes)
                    {
                        output[outputCharNum] = (char)(((input[inputByteNum] & 0xFF) << codingCase.initialShift) + ((SupportClass.Number.URShift((input[inputByteNum + 1] & 0xFF), codingCase.finalShift)) & codingCase.finalMask) & (short)0x7FFF);
                    }
                    else
                    {
                        // numBytes is 3
                        output[outputCharNum] = (char)(((input[inputByteNum] & 0xFF) << codingCase.initialShift) + ((input[inputByteNum + 1] & 0xFF) << codingCase.middleShift) + ((SupportClass.Number.URShift((input[inputByteNum + 2] & 0xFF), codingCase.finalShift)) & codingCase.finalMask) & (short)0x7FFF);
                    }
                    inputByteNum += codingCase.advanceBytes;
                    if (++caseNum == CODING_CASES.Length)
                    {
                        caseNum = 0;
                    }
                }
                // Produce final char (if any) and trailing count chars.
                codingCase = CODING_CASES[caseNum];
                
                if (inputByteNum + 1 < input.Count)
                {
                    // codingCase.numBytes must be 3
                    output[outputCharNum++] = (char) ((((input[inputByteNum] & 0xFF) << codingCase.initialShift) + ((input[inputByteNum + 1] & 0xFF) << codingCase.middleShift)) & (short) 0x7FFF);
                    // Add trailing char containing the number of full bytes in final char
                    output[outputCharNum++] = (char) 1;
                }
                else if (inputByteNum < input.Count)
                {
                    output[outputCharNum++] = (char) (((input[inputByteNum] & 0xFF) << codingCase.initialShift) & (short) 0x7FFF);
                    // Add trailing char containing the number of full bytes in final char
                    output[outputCharNum++] = caseNum == 0?(char) 1:(char) 0;
                }
                else
                {
                    // No left over bits - last char is completely filled.
                    // Add trailing char containing the number of full bytes in final char
                    output[outputCharNum++] = (char) 1;
                }
            }
		}
		
		/// <summary> Decodes the input char sequence into the output byte sequence.  Before
		/// calling this method, ensure that the output ByteBuffer has sufficient
		/// capacity by calling {@link #GetDecodedLength(java.nio.CharBuffer)}.
		/// 
		/// </summary>
		/// <param name="input">The char sequence to decode
		/// </param>
		/// <param name="output">Where the byte sequence decoding result will go.  The limit
		/// is set to one past the position of the final char.
		/// </param>
		/// <throws>  IllegalArgumentException If either the input or the output buffer </throws>
		/// <summary>  is not backed by an array
		/// </summary>
		public static void Decode(System.Collections.Generic.List<char> input, System.Collections.Generic.List<byte> output)
		{
            int numOutputBytes = GetDecodedLength(input);
            if (output.Capacity < numOutputBytes)
            {
                output.Capacity = numOutputBytes;
            }

            // ensure the buffer we are writing into is occupied with nulls
            if (output.Count < numOutputBytes)
            {
                for (int i = output.Count; i < numOutputBytes; i++)
                {
                    output.Add(Byte.MinValue);
                }
            }

            if (input.Count > 0)
            {
                int caseNum = 0;
                int outputByteNum = 0;
                int inputCharNum = 0;
                short inputChar;
                CodingCase codingCase;
                for (; inputCharNum < input.Count - 2; ++inputCharNum)
                {
                    codingCase = CODING_CASES[caseNum];
                    inputChar = (short) input[inputCharNum];
                    if (2 == codingCase.numBytes)
                    {
                        if (0 == caseNum)
                        {
                            output[outputByteNum] = (byte) (SupportClass.Number.URShift(inputChar, codingCase.initialShift));
                        }
                        else
                        {
                            output[outputByteNum] = (byte) (output[outputByteNum] + (byte) (SupportClass.Number.URShift(inputChar, codingCase.initialShift)));
                        }
                        output[outputByteNum + 1] = (byte) ((inputChar & codingCase.finalMask) << codingCase.finalShift);
                    }
                    else
                    {
                        // numBytes is 3
                        output[outputByteNum] = (byte) (output[outputByteNum] + (byte) (SupportClass.Number.URShift(inputChar, codingCase.initialShift)));
                        output[outputByteNum + 1] = (byte) (SupportClass.Number.URShift((inputChar & codingCase.middleMask), codingCase.middleShift));
                        output[outputByteNum + 2] = (byte) ((inputChar & codingCase.finalMask) << codingCase.finalShift);
                    }
                    outputByteNum += codingCase.advanceBytes;
                    if (++caseNum == CODING_CASES.Length)
                    {
                        caseNum = 0;
                    }
                }
                // Handle final char
                inputChar = (short) input[inputCharNum];
                codingCase = CODING_CASES[caseNum];
                if (0 == caseNum)
                {
                    output[outputByteNum] = 0;
                }
                output[outputByteNum] = (byte) (output[outputByteNum] + (byte) (SupportClass.Number.URShift(inputChar, codingCase.initialShift)));
                long bytesLeft = numOutputBytes - outputByteNum;
                if (bytesLeft > 1)
                {
                    if (2 == codingCase.numBytes)
                    {
                        output[outputByteNum + 1] = (byte) (SupportClass.Number.URShift((inputChar & codingCase.finalMask), codingCase.finalShift));
                    }
                    else
                    {
                        // numBytes is 3
                        output[outputByteNum + 1] = (byte) (SupportClass.Number.URShift((inputChar & codingCase.middleMask), codingCase.middleShift));
                        if (bytesLeft > 2)
                        {
                            output[outputByteNum + 2] = (byte) ((inputChar & codingCase.finalMask) << codingCase.finalShift);
                        }
                    }
                }
            }
		}
		
		/// <summary> Decodes the given char sequence, which must have been encoded by
		/// {@link #Encode(java.nio.ByteBuffer)} or 
		/// {@link #Encode(java.nio.ByteBuffer, java.nio.CharBuffer)}.
		/// 
		/// </summary>
		/// <param name="input">The char sequence to decode
		/// </param>
		/// <returns> A byte sequence containing the decoding result.  The limit
		/// is set to one past the position of the final char.
		/// </returns>
		/// <throws>  IllegalArgumentException If the input buffer is not backed by an </throws>
		/// <summary>  array
		/// </summary>
        public static System.Collections.Generic.List<byte> Decode(System.Collections.Generic.List<char> input)
		{
            System.Collections.Generic.List<byte> output = 
                new System.Collections.Generic.List<byte>(new byte[GetDecodedLength(input)]);
			Decode(input, output);
			return output;
		}
		
		/// <summary> Encodes the input byte sequence.
		/// 
		/// </summary>
		/// <param name="input">The byte sequence to encode
		/// </param>
		/// <returns> A char sequence containing the encoding result.  The limit is set
		/// to one past the position of the final char.
		/// </returns>
		/// <throws>  IllegalArgumentException If the input buffer is not backed by an </throws>
		/// <summary>  array
		/// </summary>
		public static System.Collections.Generic.List<char> Encode(System.Collections.Generic.List<byte> input)
		{
            System.Collections.Generic.List<char> output = 
                new System.Collections.Generic.List<char>(new char[GetEncodedLength(input)]);
			Encode(input, output);
			return output;
		}
		
		internal class CodingCase
		{
			internal int numBytes, initialShift, middleShift, finalShift, advanceBytes = 2;
			internal short middleMask, finalMask;
			
			internal CodingCase(int initialShift, int middleShift, int finalShift)
			{
				this.numBytes = 3;
				this.initialShift = initialShift;
				this.middleShift = middleShift;
				this.finalShift = finalShift;
				this.finalMask = (short) (SupportClass.Number.URShift((short) 0xFF, finalShift));
				this.middleMask = (short) ((short) 0xFF << middleShift);
			}
			
			internal CodingCase(int initialShift, int finalShift)
			{
				this.numBytes = 2;
				this.initialShift = initialShift;
				this.finalShift = finalShift;
				this.finalMask = (short) (SupportClass.Number.URShift((short) 0xFF, finalShift));
				if (finalShift != 0)
				{
					advanceBytes = 1;
				}
			}
		}
	}
}
