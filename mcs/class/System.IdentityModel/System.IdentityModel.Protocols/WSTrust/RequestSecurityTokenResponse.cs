﻿//
// RequestSecurityTokenResponse.cs
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
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class RequestSecurityTokenResponse : WSTrustMessage
	{
		public bool IsFinal { get; set; }
		public SecurityKeyIdentifierClause RequestedAttachedReference { get; set; }
		public RequestedProofToken RequestedProofToken { get; set; }
		public RequestedSecurityToken RequestedSecurityToken { get; set; }
		public bool RequestedTokenCancelled { get; set; }
		public SecurityKeyIdentifierClause RequestedUnattachedReference { get; set; }
		public Status Status { get; set; }

		public RequestSecurityTokenResponse ()
		{ }

		public RequestSecurityTokenResponse (WSTrustMessage message) {
			Context = message.Context;
			KeyType = message.KeyType;
			KeySizeInBits = message.KeySizeInBits;
			RequestType = message.RequestType;
		}
	}
}