//
// ServiceModelSecurityTokenTypes.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;

namespace System.ServiceModel.Security.Tokens
{
	public static class ServiceModelSecurityTokenTypes
	{
		public static string AnonymousSslnego {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/AnonymousSslnego"; }
		}

		public static string MutualSslnego {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/MutualSslnego"; }
		}

		public static string SecureConversation {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SecureConversation"; }
		}

		public static string SecurityContext {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SecurityContextToken"; }
		}

		public static string Spnego {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/Spnego"; }
		}

		public static string SspiCredential {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SspiCredential"; }
		}
	}
}
