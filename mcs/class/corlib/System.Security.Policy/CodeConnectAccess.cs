//
// System.Security.Policy.CodeConnectAccess class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public class CodeConnectAccess {

		public static readonly string AnyScheme = "*";
		public static readonly int DefaultPort = -3;
		public static readonly int OriginPort = -4;
		public static readonly string OriginScheme = "$origin";

		private string _scheme;
		private int _port;

		[MonoTODO ("(2.0) validations incomplete")]
		public CodeConnectAccess (string allowScheme, int allowPort)
		{
			// LAME but as documented
			if ((allowScheme == null) || (allowScheme.Length == 0))
				throw new ArgumentOutOfRangeException ("allowScheme");
			// TODO : check for invalid characters in scheme
			if ((allowPort < 0) || (allowPort > 65535)) {
				throw new ArgumentOutOfRangeException ("allowPort");
			}

			_scheme = allowScheme;
			_port = allowPort;
		}

		public int Port {
			get { return _port; }
		}

		public string Scheme {
			get { return _scheme; }
		}

		public override bool Equals (object o)
		{
			CodeConnectAccess cca = (o as CodeConnectAccess);
			if (cca == null)
				return false;
			return ((_scheme == cca._scheme) && (_port == cca._port));
		}

		public override int GetHashCode ()
		{
			// return same hash code if objects are equals
			return (_scheme.GetHashCode () ^ _port);
		}

		public static CodeConnectAccess CreateAnySchemeAccess (int allowPort)
		{
			return new CodeConnectAccess (AnyScheme, allowPort);
		}

		public static CodeConnectAccess CreateOriginSchemeAccess (int allowPort)
		{
			return new CodeConnectAccess (OriginScheme, allowPort);
		}
	}
}

#endif
