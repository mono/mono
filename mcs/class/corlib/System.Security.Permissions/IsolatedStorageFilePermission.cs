//
// System.Security.Permissions.IsolatedStorageFilePermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Permissions {

	[Serializable]
	public sealed class IsolatedStorageFilePermission : IsolatedStoragePermission, IBuiltInPermission {

		private const int version = 1;

		// Constructors

		[MonoTODO ("usage/quota calculated from evidences/policy")]
		public IsolatedStorageFilePermission (PermissionState state) : base (state)
		{
			if (!IsUnrestricted ()) {
				// TODO
			}
		}

		// Properties

		// Methods

		public override IPermission Copy () 
		{
			IsolatedStorageFilePermission p = new IsolatedStorageFilePermission (PermissionState.None);
			p.m_userQuota = m_userQuota;
			p.m_machineQuota = m_machineQuota;
			p.m_expirationDays = m_expirationDays;
			p.m_permanentData = m_permanentData;
			p.m_allowed = m_allowed;
			return p;
		}

		public override IPermission Intersect (IPermission target) 
		{
			IsolatedStorageFilePermission isfp = Cast (target);
			if (isfp == null)
				return null;
			if (IsEmpty () && isfp.IsEmpty ())
				return null;

			IsolatedStorageFilePermission p = new IsolatedStorageFilePermission (PermissionState.None);
			p.m_userQuota = (m_userQuota < isfp.m_userQuota) ? m_userQuota : isfp.m_userQuota;
			p.m_machineQuota = (m_machineQuota < isfp.m_machineQuota) ? m_machineQuota : isfp.m_machineQuota;
			p.m_expirationDays = (m_expirationDays < isfp.m_expirationDays) ? m_expirationDays : isfp.m_expirationDays;
			p.m_permanentData = (m_permanentData && isfp.m_permanentData);
			// UsageAllowed == Unrestricted is a special case handled by the property
			p.UsageAllowed = (m_allowed < isfp.m_allowed) ? m_allowed : isfp.m_allowed;
			return p;
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			IsolatedStorageFilePermission isfp = Cast (target);
			if (isfp == null)
				return IsEmpty ();
			if (isfp.IsUnrestricted ())
				return true;

			if (m_userQuota > isfp.m_userQuota)
				return false;
			if (m_machineQuota > isfp.m_machineQuota)
				return false;
			if (m_expirationDays > isfp.m_expirationDays)
				return false;
			if (m_permanentData != isfp.m_permanentData)
				return false;
			if (m_allowed > isfp.m_allowed)
				return false;
			return true;
		}

		public override IPermission Union (IPermission target)
		{
			IsolatedStorageFilePermission isfp = Cast (target);
			if (isfp == null)
				return Copy ();

			IsolatedStorageFilePermission p = new IsolatedStorageFilePermission (PermissionState.None);
			p.m_userQuota = (m_userQuota > isfp.m_userQuota) ? m_userQuota : isfp.m_userQuota;
			p.m_machineQuota = (m_machineQuota > isfp.m_machineQuota) ? m_machineQuota : isfp.m_machineQuota;
			p.m_expirationDays = (m_expirationDays > isfp.m_expirationDays) ? m_expirationDays : isfp.m_expirationDays;
			p.m_permanentData = (m_permanentData || isfp.m_permanentData);
			// UsageAllowed == Unrestricted is a special case handled by the property
			p.UsageAllowed = (m_allowed > isfp.m_allowed) ? m_allowed : isfp.m_allowed;
			return p;
		}

#if NET_2_0
		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			SecurityElement se = base.ToXml ();
			// TODO - something must have been added ???
			return se;
		}
#endif

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.IsolatedStorageFile;
		}

		// helpers

		private IsolatedStorageFilePermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			IsolatedStorageFilePermission isfp = (target as IsolatedStorageFilePermission);
			if (isfp == null) {
				ThrowInvalidPermission (target, typeof (IsolatedStorageFilePermission));
			}

			return isfp;
		}
	}
}
