//
// System.Security.Cryptography HashAlgorithm Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System.Globalization;
using System.IO;

namespace System.Security.Cryptography {

	public abstract class HashAlgorithm : ICryptoTransform {

		protected byte[] HashValue;
		protected int HashSizeValue;
		protected int State;
		private bool disposed;

		protected HashAlgorithm () 
		{
			disposed = false;
		}

		public virtual bool CanTransformMultipleBlocks {
			get { return true; }
		}

		public virtual bool CanReuseTransform {
			get { return true; }
		}

		public void Clear () 
		{
			// same as System.IDisposable.Dispose() which is documented
			Dispose (true);
		}

		public byte[] ComputeHash (byte[] input) 
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return ComputeHash (input, 0, input.Length);
		}

		public byte[] ComputeHash (byte[] buffer, int offset, int count) 
		{
			if (disposed)
				throw new ObjectDisposedException ("HashAlgorithm");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentException ("count", "< 0");
			// ordered to avoid possible integer overflow
			if (offset > buffer.Length - count) {
				throw new ArgumentException ("offset + count", 
					Locale.GetText ("Overflow"));
			}

			HashCore (buffer, offset, count);
			HashValue = HashFinal ();
			Initialize ();
			
			return HashValue;
		}

		public byte[] ComputeHash (Stream inputStream) 
		{
			// don't read stream unless object is ready to use
			if (disposed)
				throw new ObjectDisposedException ("HashAlgorithm");

			byte[] buffer = new byte [4096];
			int len = inputStream.Read (buffer, 0, 4096);
			while (len > 0) {
				HashCore (buffer, 0, len);
				len = inputStream.Read (buffer, 0, 4096);
			}
			HashValue = HashFinal ();
			Initialize ();
			return HashValue;
		}
	
		public static HashAlgorithm Create () 
		{
			return Create ("System.Security.Cryptography.HashAlgorithm");
		}
	
		public static HashAlgorithm Create (string hashName)
		{
			return (HashAlgorithm) CryptoConfig.CreateFromName (hashName);
		}
	
		public virtual byte[] Hash {
			get { 
				if (HashValue == null) {
					throw new CryptographicUnexpectedOperationException (
						Locale.GetText ("No hash value computed."));
				}
				return HashValue; 
			}
		}
	
		protected abstract void HashCore (byte[] rgb, int start, int size);

		protected abstract byte[] HashFinal ();

		public virtual int HashSize {
			get { return HashSizeValue; }
		}
	
		public abstract void Initialize ();

		protected virtual void Dispose (bool disposing)
		{
			disposed = true;
		}
	
		public virtual int InputBlockSize {
			get { return 1; }
		}
	
		public virtual int OutputBlockSize {
			get { return 1; }
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}
		
		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) 
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");

			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException ("inputOffset", "< 0");
			if (inputCount < 0)
				throw new ArgumentException ("inputCount");

			// ordered to avoid possible integer overflow
			if ((inputOffset < 0) || (inputOffset > inputBuffer.Length - inputCount))
				throw new ArgumentException ("inputBuffer");
			// ordered to avoid possible integer overflow
			if ((outputOffset < 0) || (outputOffset > outputBuffer.Length - inputCount))
				throw new IndexOutOfRangeException ("outputBuffer");

			// note: other exceptions are handled by Buffer.BlockCopy
			Buffer.BlockCopy (inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
			HashCore (inputBuffer, inputOffset, inputCount);

			return inputCount;
		}
	
		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (inputCount < 0)
				throw new ArgumentException ("inputCount");
			// ordered to avoid possible integer overflow
			if (inputOffset > inputBuffer.Length - inputCount) {
				throw new ArgumentException ("inputOffset + inputCount", 
					Locale.GetText ("Overflow"));
			}

			byte[] outputBuffer = new byte [inputCount];
			
			// note: other exceptions are handled by Buffer.BlockCopy
			Buffer.BlockCopy (inputBuffer, inputOffset, outputBuffer, 0, inputCount);
			
			HashCore (inputBuffer, inputOffset, inputCount);
			HashValue = HashFinal ();
			Initialize ();
			
			return outputBuffer;
		}
	}
}
