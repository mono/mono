//
// System.Security.Cryptography.RijndaelManagedTransform
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
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

#if  !MOONLIGHT

using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

	// Notes: This class is "publicly" new in Fx 2.0 but was already 
	// existing in Fx 1.0. So this new class is only calling the old
	// (and more general) one (RijndaelTransform) located in 
	// RijndaelManaged.cs.

	[ComVisible (true)]
	public sealed class RijndaelManagedTransform: ICryptoTransform, IDisposable {

		private RijndaelTransform _st;
		private int _bs;

		internal RijndaelManagedTransform (Rijndael algo, bool encryption, byte[] key, byte[] iv)
		{
			_st = new RijndaelTransform (algo, encryption, key, iv);
			_bs = algo.BlockSize;
		}

		public int BlockSizeValue {
			get { return _bs; }
		}

		public bool CanTransformMultipleBlocks {
			get { return _st.CanTransformMultipleBlocks; }
		}

		public bool CanReuseTransform {
			get { return _st.CanReuseTransform; }
		}

		public int InputBlockSize {
			get { return _st.InputBlockSize; }
		}

		public int OutputBlockSize {
			get { return _st.OutputBlockSize; }
		}

		public void Clear ()
		{
			_st.Clear ();
		}

		[MonoTODO ("Reset does nothing since CanReuseTransform return false.")]
		public void Reset ()
		{
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose () 
#endif
		{
			_st.Clear ();
		}

		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			return _st.TransformBlock (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			return _st.TransformFinalBlock (inputBuffer, inputOffset, inputCount);
		}
	}
}

#endif
