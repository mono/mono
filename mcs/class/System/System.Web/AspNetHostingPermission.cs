//
// System.Web.AspNetHostingPermission.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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

#if NET_1_1

using System.Security;
using System.Security.Permissions;

namespace System.Web
{
	[Serializable]
	public sealed class AspNetHostingPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		AspNetHostingPermissionLevel _level;

		public AspNetHostingPermission (AspNetHostingPermissionLevel level)
		{
			_level = level;
		}
		public AspNetHostingPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					_level = AspNetHostingPermissionLevel.None;
					break;
				case PermissionState.Unrestricted:
					_level = AspNetHostingPermissionLevel.Unrestricted;
					break;
			}
		}

		public AspNetHostingPermissionLevel Level {
			get { return _level; }
			set { _level = value; }
		}

		public bool IsUnrestricted ()
		{
			return (_level == AspNetHostingPermissionLevel.Unrestricted);
		}

		public override IPermission Copy ()
		{
			return new AspNetHostingPermission (_level);
		}

		[MonoTODO ("implement")]
		public override void FromXml (SecurityElement securityElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif