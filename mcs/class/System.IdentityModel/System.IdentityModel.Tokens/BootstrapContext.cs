//
// BootstrapContext.cs
//
// Author:
//   Robert J. van der Boon (rjvdboon@gmail.com)
//
// Copyright (C) 2014 Robert J. van der Boon
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
#if NET_4_5
using System;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
	[Serializable]
	public class BootstrapContext : ISerializable
	{
		/// <summary>Gets the string that was used to initialize the context.</summary>
		public string Token { get; private set; }
		/// <summary>Gets the array that was used to initialize the context.</summary>
		public byte[] TokenBytes { get; private set; }
		/// <summary>Gets the security token that was used to initialize the context.</summary>
		public SecurityToken SecurityToken { get; private set; }
		/// <summary>Gets the token handler that was used to initialize the context.</summary>
		public SecurityTokenHandler SecurityTokenHandler { get; private set; }

		/// <summary>Initializes a new instance of the <see cref="BootstrapContext"/> class by using the specified string.</summary>
		public BoostrapContext (string token) {
			if (token == null) throw new ArgumentNullException("token");
			Token = token;
		}

		/// <summary>Initializes a new instance of the <see cref="BootstrapContext"/> class by using the specified array.</summary>
		public BoostrapContext (byte[] token) {
			if (token == null) throw new ArgumentNullException("token");
			TokenBytes = token;
		}

		/// <summary>Initializes a new instance of the <see cref="BootstrapContext"/> class by using the specified security token and token handler.</summary>
		public BoostrapContext (SecurityToken token, SecurityTokenHandler handler) {
			if (token == null) throw new ArgumentNullException("token");
			if (handler == null) throw new ArgumentNullException("handler");
			SecurityToken = token;
			SecurityTokenHandler = handler;
		}
		
		/// <summary>Initializes a new instance of the <see cref="BootstrapContext"/> class from a stream.</summary>
		protected BootstrapContext(SerializationInfo info, SerializationContext context) {
			if (info == null) throw new ArgumentNullException("info");
			char type = info.GetChar("K");
			switch (type) {
			case 'S':
				Token = info.GetString("V");
				break;
			case 'B':
				TokenBytes = (byte[])info.GetValue("V");
				break;
			case 'T':
				Token = Convert.FromBase64String(info.GetString("V"));
				break;
			}
		}
		
		/// <summary>Populates the <see cref="SerializationInfo"/> with data needed to serialize the current <see cref="BootstrapContext"/> object.</summary>
		public void GetObjectData(SerializationInfo info, SerializationContext context) {
			if (info == null) throw new ArgumentNullException("info");
			if (Token != null) {
				info.AddValue("K", 'S');
				info.AddValue("T", Token);
			} else if (TokenBytes != null) {
				info.AddValue("K", 'B');
				info.AddValue("T", TokenBytes);
			} else if (SecurityToken != null && SecurityTokenHandler != null) {
				info.AddValue("K", 'T');
				// Serialize SecurityToken using SecurityTokenHandler
				info.AddValue("T", Token);
			}
		}

	}
}
#endif