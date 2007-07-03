//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNegativeInteger
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public sealed class SoapNegativeInteger : ISoapXsd
	{
		decimal _value;
		
		public SoapNegativeInteger()
		{
		}
		
		public SoapNegativeInteger(decimal value)
		{
			if (value >= 0) 
				throw SoapHelper.GetException (this, "invalid " + value);
			_value = value;
		}
		
		public decimal Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "negativeInteger"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapNegativeInteger Parse (string value)
		{
			return new SoapNegativeInteger (decimal.Parse (value));
		}

		public override string ToString()
		{
			return _value.ToString ();
		}
	}
}
