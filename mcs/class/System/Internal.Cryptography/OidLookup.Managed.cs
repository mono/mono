//
// Copyright (c) 2018 Microsoft
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Internal.Cryptography
{
	/// <summary>
	/// Fully managed implementation of OidLookup for the commonly supported Oids.
	/// </summary>
	internal static partial class OidLookup
	{
		private static bool ShouldUseCache(OidGroup oidGroup)
		{
			return true;
		}

		private static string NativeOidToFriendlyName(string oid, OidGroup oidGroup, bool fallBackToAllGroups)
		{
			switch (oid) {
			case "1.2.840.113549.1.7.1": return "PKCS 7 Data";
			case "1.2.840.113549.1.9.3": return "Content Type";
			case "1.2.840.113549.1.9.4": return "Message Digest";
			case "1.2.840.113549.1.9.5": return "Signing Time";
			case "1.2.840.113549.1.9.16.3.3": return "id-smime-alg-3DESwrap";
			case "2.5.29.14": return "Subject Key Identifier";
			case "2.5.29.15": return "Key Usage";
			case "2.5.29.17": return "Subject Alternative Name";
			case "2.5.29.19": return "Basic Constraints";
			case "2.5.29.37": return "Extended Key Usage";
			case "2.16.840.1.113730.1.1": return "Netscape Cert Type";
			}
			return null;
		}

		private static string NativeFriendlyNameToOid(string friendlyName, OidGroup oidGroup, bool fallBackToAllGroups)
		{
			switch (friendlyName) {
			case "PKCS 7 Data": return "1.2.840.113549.1.7.1";
			case "Content Type": return "1.2.840.113549.1.9.3";
			case "Message Digest": return "1.2.840.113549.1.9.4";
			case "Signing Time": return "1.2.840.113549.1.9.5";
			case "id-smime-alg-3DESwrap": return "1.2.840.113549.1.9.16.3.3";
			case "Subject Key Identifier": return "2.5.29.14";
			case "Key Usage": return "2.5.29.15";
			case "Subject Alternative Name": return "2.5.29.17";
			case "Basic Constraints": return "2.5.29.19";
			case "Extended Key Usage": return "2.5.29.37";
			case "Netscape Cert Type": return  "2.16.840.1.113730.1.1";
			};
			return null;
		}
	}
}
