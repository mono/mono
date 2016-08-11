//
// ECCurve.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

#if NETSTANDARD

namespace System.Security.Cryptography
{
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct ECCurve
	{
		public byte[] A;
		public byte[] B;
		public byte[] Cofactor;
		public ECCurveType CurveType;
		public ECPoint G;
		public HashAlgorithmName? Hash;
		public byte[] Order;
		public byte[] Polynomial;
		public byte[] Prime;
		public byte[] Seed;
		public bool IsCharacteristic2 { get { throw new NotImplementedException (); } }
		public bool IsExplicit { get { throw new NotImplementedException (); } }
		public bool IsNamed { get { throw new NotImplementedException (); } }
		public bool IsPrime { get { throw new NotImplementedException (); } }
		public Oid Oid { get { throw new NotImplementedException (); } }
		public static ECCurve CreateFromFriendlyName (string oidFriendlyName) { throw new NotImplementedException (); }
		public static ECCurve CreateFromOid (Oid curveOid) { throw new NotImplementedException (); }
		public static ECCurve CreateFromValue (string oidValue) { throw new NotImplementedException (); }
		public void Validate () { throw new NotImplementedException (); }

		public enum ECCurveType
		{
			Implicit = 0,
			PrimeShortWeierstrass = 1,
			PrimeTwistedEdwards = 2,
			PrimeMontgomery = 3,
			Characteristic2 = 4,
			Named = 5,
		}
		
		public static class NamedCurves
		{
			public static ECCurve brainpoolP160r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP160t1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP192r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP192t1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP224r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP224t1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP256r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP256t1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP320r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP320t1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP384r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP384t1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP512r1 { get { throw new NotImplementedException (); } }
			public static ECCurve brainpoolP512t1 { get { throw new NotImplementedException (); } }
			public static ECCurve nistP256 { get { throw new NotImplementedException (); } }
			public static ECCurve nistP384 { get { throw new NotImplementedException (); } }
			public static ECCurve nistP521 { get { throw new NotImplementedException (); } }
		}
	}
}

#endif