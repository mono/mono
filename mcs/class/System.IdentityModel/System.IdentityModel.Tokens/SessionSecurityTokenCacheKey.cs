//
// SessionSecurityTokenCacheKey.cs
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
using System.Xml;

namespace System.IdentityModel.Tokens
{
	public class SessionSecurityTokenCacheKey
	{
		[MonoTODO]
		public static bool operator !=(SessionSecurityTokenCacheKey first, SessionSecurityTokenCacheKey second) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool operator ==(SessionSecurityTokenCacheKey first, SessionSecurityTokenCacheKey second) {
			throw new NotImplementedException ();
		}

		public UniqueId ContextId { get; private set; }
		public string EndpointId { get; private set; }
		public bool IgnoreKeyGeneration { get; set; }
		public UniqueId KeyGeneration { get; private set; }

		public SessionSecurityTokenCacheKey (string endpointId, UniqueId contextId, UniqueId keyGeneration) {
			EndpointId = endpointId;
			ContextId = contextId;
			KeyGeneration = keyGeneration;
		}

		[MonoTODO]
		public override bool Equals (System.Object obj) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString () {
			throw new NotImplementedException ();
		}
	}
}
