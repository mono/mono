//
// SecurityUtilites.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public static class SecurityUtilities {
	
		[MonoTODO]
		public static PermissionSet ComputeZonePermissionSet (string targetZone,
								      PermissionSet includedPermissionSet,
								      string[] excludedPermissions)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static PermissionSet IdentityListToPermissionSet (string[] ids)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static string[] PermissionSetToIdentityList (PermissionSet permissionSet)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void SignFile (string certThumbprint,
					     Uri timestampUrl,
					     string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void SignFile (X509Certificate2 cert,
					     Uri timestampUrl,
					     string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void SignFile (string certPath,
					     SecureString certPassword,
					     Uri timestampUrl,
					     string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static PermissionSet XmlToPermissionSet (XmlElement element)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
