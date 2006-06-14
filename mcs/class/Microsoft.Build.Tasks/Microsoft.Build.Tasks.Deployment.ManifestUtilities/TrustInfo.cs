//
// TrustInfo.cs
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
using System.Security;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public sealed class TrustInfo {
	
		bool		hasUnmanagedCodePermission;
		bool		isFullTrust;
		PermissionSet	permissionSet;
		bool		preserveFullTrustPermissionSet;
		string		sameSiteAccess;
	
		[MonoTODO]
		public TrustInfo ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Read (Stream input)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Read (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ReadManifest (Stream input)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ReadManifest (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Write (Stream output)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Write (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void WriteManifest (Stream output)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void WriteManifest (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void WriteManifest (Stream input, Stream output)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool HasUnmanagedCodePermission {
			get { return hasUnmanagedCodePermission; }
		}
		
		[MonoTODO]
		public bool IsFullTrust {
			get { return isFullTrust; }
			set { isFullTrust = value; }
		}
		
		[MonoTODO]
		public PermissionSet PermissionSet {
			get { return permissionSet; }
			set { permissionSet = value; }
		}
		
		[MonoTODO]
		public bool PreserveFullTrustPermissionSet {
			get { return preserveFullTrustPermissionSet; }
			set { preserveFullTrustPermissionSet = value; }
		}
		
		[MonoTODO]
		public string SameSiteAccess {
			get { return sameSiteAccess; }
			set { sameSiteAccess = value; }
		}
	}
}

#endif
