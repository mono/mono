//
// System.Security.Cryptography CryptoAPITransform.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.IO;

namespace System.Security.Cryptography
{

	public sealed class CryptoAPITransform : ICryptoTransform
	{
		public CryptoAPITransform() 
		{
		}
		
		/// <summary>
		/// Indicates if the Transform object can transform multiple blocks
		/// </summary>
		public bool CanTransformMultipleBlocks
		{
			get 
			{
				// FIXME: should not be always true
				return true;
			}
		}

		public int InputBlockSize
		{
			get {
				// TODO: implement
				return 0;
			}
		}
		
		public IntPtr KeyHandle 
		{
			get {
				// TODO: implement
				return IntPtr.Zero;
			}
		}
		
		public int OutputBlockSize 
		{
			get {
				// TODO: implement
				return 0;
			}
		}
		
		public int TransformBlock(byte[] inputBuffer, int inputOffset, 
		                          int inputCount, byte[] outputBuffer, int outputOffset)
		{
			// TODO: implement
			return 0;
		}
		
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			// TODO: implement
			return null;
		}
		
	} // CryptoAPITransform
	
} // System.Security.Cryptography
