//
// WSTrustMessage.cs
//
// Author:
//   Noesis Labs (Ryan.Melena@noesislabs.com)
//
// Copyright (C) 2014 Noesis Labs, LLC  https://noesislabs.com
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
using System.IdentityModel;

namespace System.IdentityModel.Protocols.WSTrust
{
	public abstract class WSTrustMessage : OpenObject
	{
		public bool AllowPostdating { get; set; }
		public EndpointReference AppliesTo { get; set; }
		public string AuthenticationType { get; set; }
		public BinaryExchange BinaryExchange { get; set; }
		public string CanonicalizationAlgorithm { get; set; }
		public string Context { get; set; }
		public string EncryptionAlgorithm { get; set; }
		public string EncryptWith { get; set; }
		public Entropy Entropy { get; set; }
		public int? KeySizeInBits { get; set; }
		public string KeyType { get; set; }
		public string KeyWrapAlgorithm { get; set; }
		public Lifetime Lifetime { get; set; }
		public string ReplyTo { get; set; }
		public string RequestType { get; set; }
		public string SignatureAlgorithm { get; set; }
		public string SignWith { get; set; }
		public string TokenType { get; set; }
		public UseKey UseKey { get; set; }
	}
}
