//
// RecipientInfo.cs - System.Security.Cryptography.Pkcs.RecipientInfo
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System;

namespace System.Security.Cryptography.Pkcs {

	public abstract class RecipientInfo {

		private RecipientInfoType _type;

		// constructors

		// documented as protected at http://longhorn.msdn.microsoft.com
		// but not present in the 1.2 beta SDK
		internal RecipientInfo (RecipientInfoType recipInfoType) 
		{
			_type = recipInfoType;
		}

		// properties

		public abstract byte[] EncryptedKey { get; }

		public abstract AlgorithmIdentifier KeyEncryptionAlgorithm { get; }

		public abstract SubjectIdentifier RecipientIdentifier { get; }

		public RecipientInfoType Type {
			get { return _type; }
		}

		public abstract int Version { get; }
	}
}

#endif
