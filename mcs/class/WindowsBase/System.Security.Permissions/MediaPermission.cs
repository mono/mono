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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Security;

namespace System.Security.Permissions {

	public sealed class MediaPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		public MediaPermission ()
		{
			throw new NotImplementedException ();
		}

		public MediaPermission (MediaPermissionAudio permissionAudio)
		{
			throw new NotImplementedException ();
		}

		public MediaPermission (MediaPermissionImage permissionImage)
		{
			throw new NotImplementedException ();
		}

		public MediaPermission (MediaPermissionVideo permissionVideo)
		{
			throw new NotImplementedException ();
		}

		public MediaPermission (PermissionState state)
		{
			throw new NotImplementedException ();
		}

		public MediaPermission (MediaPermissionAudio permissionAudio, MediaPermissionVideo permissionVideo, MediaPermissionImage permissionImage)
		{
			throw new NotImplementedException ();
		}

		public MediaPermissionAudio Audio {
			get { throw new NotImplementedException (); }
		}

		public MediaPermissionVideo Video {
			get { throw new NotImplementedException (); }
		}

		public MediaPermissionImage Image {
			get { throw new NotImplementedException (); }
		}

		public override IPermission Copy ()
		{
			throw new NotImplementedException ();
		}

		public override void FromXml (SecurityElement securityElement)
		{
			throw new NotImplementedException ();
		}

		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted ()
		{
			throw new NotImplementedException ();
		}

		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}
	}

}