//
// MACTripleDES.cs: Handles MAC with TripleDES
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 81: DES MODES OF OPERATION 
	//	MAC: Appendix F (MACDES not MACTripleDES but close enough ;-)
	//	http://www.itl.nist.gov/fipspubs/fip81.htm
	
	// LAMESPEC: MACTripleDES == MAC-CBC using TripleDES (not MAC-CFB).
	[ComVisible (true)]
	public class MACTripleDES: KeyedHashAlgorithm {
	
		private TripleDES tdes;
		private MACAlgorithm mac;
		private bool m_disposed;
	
		public MACTripleDES ()
		{
			Setup ("TripleDES", null);
		}
	
		public MACTripleDES (byte[] rgbKey)
		{
			if (rgbKey == null)
				throw new ArgumentNullException ("rgbKey");
			Setup ("TripleDES", rgbKey);
		}
	
		public MACTripleDES (string strTripleDES, byte[] rgbKey) 
		{
			if (rgbKey == null)
				throw new ArgumentNullException ("rgbKey");
			if (strTripleDES == null)
				Setup ("TripleDES", rgbKey);
			else
				Setup (strTripleDES, rgbKey);
		}
	
		private void Setup (string strTripleDES, byte[] rgbKey) 
		{
			tdes = TripleDES.Create (strTripleDES);
			// default padding (as using in Fx 1.0 and 1.1)
			tdes.Padding = PaddingMode.Zeros;
			// if rgbKey is null we keep the randomly generated key
			if (rgbKey != null) {
				// this way we get the TripleDES key validation (like weak
				// and semi-weak keys)
				tdes.Key = rgbKey;
			}
			HashSizeValue = tdes.BlockSize;
			// we use Key property to get the additional validations 
			// (from KeyedHashAlgorithm ancestor)
			Key = tdes.Key;
			mac = new MACAlgorithm (tdes);
			m_disposed = false;
		}

		[ComVisible (false)]
		public PaddingMode Padding {
			get { return tdes.Padding; }
			set { tdes.Padding = value; }
		}

		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// note: we ALWAYS zeroize keys (disposing or not)
	
				// clear our copy of the secret key
				if (KeyValue != null)
					Array.Clear (KeyValue, 0, KeyValue.Length);
				// clear the secret key (inside TripleDES)
				if (tdes != null)
					tdes.Clear ();
	
				if (disposing) {
					// disposed managed stuff
					KeyValue = null;
					tdes = null;
				}
				// ancestor
				base.Dispose (disposing);
				m_disposed = true;
			}
		}
	
		public override void Initialize () 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("MACTripleDES");
			State = 0;
			mac.Initialize (KeyValue);
		}
	
		protected override void HashCore (byte[] rgbData, int ibStart, int cbSize) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("MACTripleDES");
			if (State == 0) {
				Initialize ();
				State = 1;
			}
			mac.Core (rgbData, ibStart, cbSize);
		}
	
		protected override byte[] HashFinal () 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("MACTripleDES");
			State = 0;
			return mac.Final ();
		}
	}
}
