//
// Mono.Security.Protocol.Ntlm.NtlmFlags
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//
// References
// a.	NTLM Authentication Scheme for HTTP, Ronald Tschalär
//	http://www.innovation.ch/java/ntlm.html
// b.	The NTLM Authentication Protocol, Copyright © 2003 Eric Glass
//	http://davenport.sourceforge.net/ntlm.html
//

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

namespace Mono.Security.Protocol.Ntlm {

	[Flags]
#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	enum NtlmFlags : int {
		// The client sets this flag to indicate that it supports Unicode strings.
		NegotiateUnicode = 0x00000001,
		// This is set to indicate that the client supports OEM strings.
		NegotiateOem = 0x00000002,
		// This requests that the server send the authentication target with the Type 2 reply.
		RequestTarget = 0x00000004,
		// Indicates that NTLM authentication is supported.
		NegotiateNtlm = 0x00000200,
		// When set, the client will send with the message the name of the domain in which the workstation has membership.
		NegotiateDomainSupplied = 0x00001000,
		// Indicates that the client is sending its workstation name with the message.  
		NegotiateWorkstationSupplied = 0x00002000,
		// Indicates that communication between the client and server after authentication should carry a "dummy" signature.
		NegotiateAlwaysSign = 0x00008000,
		// Indicates that this client supports the NTLM2 signing and sealing scheme; if negotiated, this can also affect the response calculations.
		NegotiateNtlm2Key = 0x00080000,
		// Indicates that this client supports strong (128-bit) encryption.
		Negotiate128 = 0x20000000,
		// Indicates that this client supports medium (56-bit) encryption.
		Negotiate56 = (unchecked ((int) 0x80000000))
	}
}
