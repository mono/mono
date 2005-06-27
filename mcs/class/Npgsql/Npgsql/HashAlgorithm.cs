//
// System.Security.Cryptography HashAlgorithm Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//
// Comment: Adapted to the Project from Mono CVS as Sebastien Pouliot suggested to enable
// support of Npgsql MD5 authentication in platforms which don't have support for MD5 algorithm.
//


using System;
using System.IO;


namespace Npgsql
{


    // Comment: Removed the ICryptoTransform implementation as this interface may be not supported by
    // all platforms.

    internal abstract class HashAlgorithm : IDisposable
    {
        protected byte[] HashValue; // Caches the hash after it is calculated.  Accessed through the Hash property.
        protected int HashSizeValue; // The size of the hash in bits.
        protected int State;  // nonzero when in use;  zero when not in use
        private bool disposed;

        /// <summary>
        /// Called from constructor of derived class.
        /// </summary>
        protected HashAlgorithm ()
        {
            disposed = false;
        }

        /// <summary>
        /// Finalizer for HashAlgorithm
        /// </summary>
        ~HashAlgorithm ()
        {
            Dispose(false);
        }

        /// <summary>
        /// Get whether or not the hash can transform multiple blocks at a time.
        /// Note: MUST be overriden if descendant can transform multiple block
        /// on a single call!
        /// </summary>
        public virtual bool CanTransformMultipleBlocks {
            get
            {
                return true;
            }
        }

        public virtual bool CanReuseTransform {
            get
            {
                return true;
            }
        }

        public void Clear()
        {
            // same as System.IDisposable.Dispose() which is documented
            Dispose (true);
        }

        /// <summary>
        /// Computes the entire hash of all the bytes in the byte array.
        /// </summary>
        public byte[] ComputeHash (byte[] input)
        {
            return ComputeHash (input, 0, input.Length);
        }

        public byte[] ComputeHash (byte[] buffer, int offset, int count)
        {
            if (disposed)
                throw new ObjectDisposedException ("HashAlgorithm");

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

            int l = (int) (inputStream.Length - inputStream.Position);
            byte[] buffer = new byte [l];
            inputStream.Read (buffer, 0, l);

            return ComputeHash (buffer, 0, l);
        }

        // Commented out because it uses the CryptoConfig which can't be available in all platforms

        /*
        /// <summary>
        /// Creates the default implementation of the default hash algorithm (SHA1).
        /// </summary>
        public static HashAlgorithm Create ()
        {
        	return Create ("System.Security.Cryptography.HashAlgorithm");
        }*/

        /*
        /// <summary>
        /// Creates a specific implementation of the general hash idea.
        /// </summary>
        /// <param name="hashName">Specifies which derived class to create.</param>
        public static HashAlgorithm Create (string hashName)
        {
        	return (HashAlgorithm) CryptoConfig.CreateFromName (hashName);
        }*/



        // Changed Exception type because it uses the CryptographicUnexpectedOperationException
        // which can't be available in all platforms.
        /// <summary>
        /// Gets the previously computed hash.
        /// </summary>
        public virtual byte[] Hash {
            get
            {
                if (HashValue == null)
                    throw new NullReferenceException("HashValue is null");
                return HashValue;
            }
        }

        /// <summary>
        /// When overridden in a derived class, drives the hashing function.
        /// </summary>
        /// <param name="rgb"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        protected abstract void HashCore (byte[] rgb, int start, int size);

        /// <summary>
        /// When overridden in a derived class, this pads and hashes whatever data might be left in the buffers and then returns the hash created.
        /// </summary>
        protected abstract byte[] HashFinal ();

        /// <summary>
        /// Returns the size in bits of the hash.
        /// </summary>
        public virtual int HashSize {
            get
            {
                return HashSizeValue;
            }
        }

        /// <summary>
        /// When overridden in a derived class, initializes the object to prepare for hashing.
        /// </summary>
        public abstract void Initialize ();

        protected virtual void Dispose (bool disposing)
        {
            disposed = true;
        }

        /// <summary>
        /// Must be overriden if not 1
        /// </summary>
        public virtual int InputBlockSize {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Must be overriden if not 1
        /// </summary>
        public virtual int OutputBlockSize {
            get
            {
                return 1;
            }
        }

        void IDisposable.Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);  // Finalization is now unnecessary
        }

        /// <summary>
        /// Used for stream chaining.  Computes hash as data passes through it.
        /// </summary>
        /// <param name="inputBuffer">The buffer from which to grab the data to be copied.</param>
        /// <param name="inputOffset">The offset into the input buffer to start reading at.</param>
        /// <param name="inputCount">The number of bytes to be copied.</param>
        /// <param name="outputBuffer">The buffer to write the copied data to.</param>
        /// <param name="outputOffset">At what point in the outputBuffer to write the data at.</param>
        public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Buffer.BlockCopy (inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            HashCore (inputBuffer, inputOffset, inputCount);

            return inputCount;
        }

        /// <summary>
        /// Used for stream chaining.  Computes hash as data passes through it.  Finishes off the hash.
        /// </summary>
        /// <param name="inputBuffer">The buffer from which to grab the data to be copied.</param>
        /// <param name="inputOffset">The offset into the input buffer to start reading at.</param>
        /// <param name="inputCount">The number of bytes to be copied.</param>
        public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[inputCount];

            Buffer.BlockCopy (inputBuffer, inputOffset, outputBuffer, 0, inputCount);

            HashCore (inputBuffer, inputOffset, inputCount);
            HashValue = HashFinal ();
            Initialize ();

            return outputBuffer;
        }
    }

}
