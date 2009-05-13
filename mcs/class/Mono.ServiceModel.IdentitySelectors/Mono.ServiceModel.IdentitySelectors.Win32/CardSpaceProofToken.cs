//
// CardSpaceProofToken.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors.Win32
{
	class CardSpaceProofToken : SecurityToken
	{
		DateTime valid_to;
		ReadOnlyCollection<SecurityKey> keys;

		public CardSpaceProofToken (DateTime validTo, AsymmetricSecurityKey proofKey)
		{
			valid_to = validTo;
			keys = new ReadOnlyCollection<SecurityKey> (new SecurityKey [] {proofKey});
		}

		public override DateTime ValidFrom {
			get { return DateTime.MinValue.ToUniversalTime (); }
		}

		public override DateTime ValidTo {
			get { return valid_to; }
		}

		public override string Id {
			get { throw new NotImplementedException (); }
		}

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { return keys; }
		}
	}
}
