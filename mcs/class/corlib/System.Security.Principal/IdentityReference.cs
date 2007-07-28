//
// System.Security.Policy.IdentityReference.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Principal {

	[ComVisible (false)]
	public abstract class IdentityReference {

		// yep, this means it cannot be inherited outside corlib
		// not sure if this is "by design" reported as FDBK30180
		internal IdentityReference ()
		{
		}


		public abstract string Value { 
			get;
		}


		public abstract override bool Equals (object o);

		public abstract override int GetHashCode ();

		public abstract bool IsValidTargetType (Type targetType);

		public abstract override string ToString ();

		public abstract IdentityReference Translate (Type targetType);


		public static bool operator== (IdentityReference left, IdentityReference right)
		{
			if (((object)left) == null)
				return (((object)right) == null);
			if (((object)right) == null)
				return false;
			return (left.Value == right.Value);
		}

		public static bool operator!= (IdentityReference left, IdentityReference right)
		{
			if (((object)left) == null)
				return (((object)right) != null);
			if (((object)right) == null)
				return true;
			return (left.Value != right.Value);
		}
	}
}

#endif
