//
// System.Security.Cryptography CryptoAPITransform.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Cryptography {

	// Note: This class isn't used by Mono as all algorithms are provided with
	// 100% managed implementations.

	[ComVisible (true)]
	public sealed class CryptoAPITransform : ICryptoTransform {

		private bool m_disposed;

		internal CryptoAPITransform () 
		{
			m_disposed = false;
		}


		public bool CanReuseTransform {
			get { return true; }
		}

		public bool CanTransformMultipleBlocks {
			get { return true; }
		}

		public int InputBlockSize {
			get { return 0;	}
		}

		public IntPtr KeyHandle {
			[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
			get { return IntPtr.Zero; }
		}

		public int OutputBlockSize {
			get { return 0; }
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose () 
#endif
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		public void Clear () 
		{
			Dispose (false);
		}

		private void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// dispose unmanaged objects
				if (disposing) {
					// dispose managed objects
				}
				m_disposed = true;
			}
		}

		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			return 0;
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			// Reset (); should be called here before returning final data
			return null;
		}

		[ComVisible (false)]
		public void Reset ()
		{
		}
	}
}
