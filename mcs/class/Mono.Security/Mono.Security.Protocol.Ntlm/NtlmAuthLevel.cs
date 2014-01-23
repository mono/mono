//
// NtlmAuthLevel.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Mono.Security.Protocol.Ntlm {
	
	/*
	 * On Windows, this is controlled by a registry setting
	 * (http://msdn.microsoft.com/en-us/library/ms814176.aspx)
	 *
	 * This can be configured by setting the static
	 * Type3Message.DefaultAuthLevel property, the default value
	 * is LM_and_NTLM_and_try_NTLMv2_Session.
	 */
	
#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	enum NtlmAuthLevel {
		/* Use LM and NTLM, never use NTLMv2 session security. */
		LM_and_NTLM,
		
		/* Use NTLMv2 session security if the server supports it,
		 * otherwise fall back to LM and NTLM. */
		LM_and_NTLM_and_try_NTLMv2_Session,
		
		/* Use NTLMv2 session security if the server supports it,
		 * otherwise fall back to NTLM.  Never use LM. */
		NTLM_only,
		
		/* Use NTLMv2 only. */
		NTLMv2_only,
	}
}

