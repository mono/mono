//
// AsnEncodedData.cs - System.Security.Cryptography.AsnEncodedData
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

#if NET_2_0

using System.Text;

namespace System.Security.Cryptography {

	public class AsnEncodedData {

		private Oid _oid;
		private byte[] _raw;

		// constructors

		protected AsnEncodedData ()
		{
		}
	
		public AsnEncodedData (string oid, byte[] rawData)
		{
			_oid = new Oid (oid);
			RawData = rawData;
		}

		public AsnEncodedData (Oid oid, byte[] rawData)
		{
			Oid = oid;
			RawData = rawData;

			// yes, here oid == null is legal (by design), 
			// but no, it would not be legal for an oid string
			// see MSDN FDBK11479
		}

		public AsnEncodedData (AsnEncodedData asnEncodedData)
		{
			CopyFrom (asnEncodedData);
		}

		public AsnEncodedData (byte[] rawData)
		{
			RawData = rawData;
		}

		// properties

		public Oid Oid {
			get { return _oid; }
			set {
				if (value == null)
					_oid = null;
				else
					_oid = new Oid (value);
			}
		}

		public byte[] RawData { 
			get { return _raw; }
			set {
				if (value == null)
					throw new ArgumentNullException ("RawData");
				_raw = (byte[])value.Clone ();
			}
		}

		// methods

		public virtual void CopyFrom (AsnEncodedData asnEncodedData)
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("asnEncodedData");

			Oid = new Oid (asnEncodedData._oid);
			RawData = asnEncodedData._raw;
		}

		public virtual string Format (bool multiLine) 
		{
			StringBuilder sb = new StringBuilder ();
			for (int i=0; i < _raw.Length; i++) {
				sb.Append (_raw [i].ToString ("x2"));
				if (i != _raw.Length - 1)
					sb.Append (" ");
			}
			return sb.ToString ();
		}
	}
}

#endif
