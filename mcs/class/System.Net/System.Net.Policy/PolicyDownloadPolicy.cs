//
// System.Windows.Browser.Net.PolicyDownloadPolicy class
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright (C) 2009-2010 Novell, Inc (http://www.novell.com)
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

#if NET_2_1

namespace System.Net.Policy {

	sealed class PolicyDownloadPolicy : ICrossDomainPolicy {

		public bool IsAllowed (WebRequest request)
		{
			return IsLocalPathPolicy (request.RequestUri);
		}

		static public bool IsLocalPathPolicy (Uri uri)
		{
			string local = uri.LocalPath;
			if (String.CompareOrdinal (local, CrossDomainPolicyManager.ClientAccessPolicyFile) == 0)
				return true;
			if (String.CompareOrdinal (local, CrossDomainPolicyManager.CrossDomainFile) == 0)
				return true;

			return false;
		}

		public Exception Exception {
			get { return null; }
		}
	}
}

#endif

