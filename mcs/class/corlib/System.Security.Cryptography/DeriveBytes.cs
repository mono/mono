//
// System.Security.Cryptography DeriveBytes.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
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

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public abstract class DeriveBytes : IDisposable {
		private bool m_disposed;

		protected DeriveBytes ()
		{
		}
		
		public abstract byte[] GetBytes (int cb);

		public abstract void Reset ();

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!m_disposed) {
				if (disposing) {
					// dispose managed objects
				}
				m_disposed = true;
			}
		}
	}
}
