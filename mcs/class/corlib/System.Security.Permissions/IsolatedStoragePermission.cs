//
// System.Security.Permissions.IsolatedStoragePermission.cs
//
// Authors:
//	Piers Haken <piersh@friskit.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions {

	[Serializable]
	[SecurityPermission (SecurityAction.InheritanceDemand, ControlEvidence = true, ControlPolicy = true)]
#if NET_2_0
	[ComVisible (true)]
#endif
	public abstract class IsolatedStoragePermission : CodeAccessPermission, IUnrestrictedPermission	{

		private const int version = 1;

		internal long m_userQuota;
		internal long m_machineQuota;
		internal long m_expirationDays;
		internal bool m_permanentData;
		internal IsolatedStorageContainment m_allowed;

#if NET_2_0
		protected IsolatedStoragePermission (PermissionState state)
#else
		public IsolatedStoragePermission (PermissionState state)
#endif
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted) {
				UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			}
		}

		public long UserQuota {
			get { return m_userQuota; }
			set { m_userQuota = value; }
		}

		public IsolatedStorageContainment UsageAllowed {
			get { return m_allowed; }
			set {
				if (!Enum.IsDefined (typeof (IsolatedStorageContainment), value)) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "IsolatedStorageContainment");
				}
				m_allowed = value;
				if (m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage) {
					m_userQuota = Int64.MaxValue;
					m_machineQuota = Int64.MaxValue;
					m_expirationDays = Int64.MaxValue ;
					m_permanentData = true;
				}
			}
		}


		public bool IsUnrestricted ()
		{
			return IsolatedStorageContainment.UnrestrictedIsolatedStorage == m_allowed;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);

			if (m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage)
				se.AddAttribute ("Unrestricted", "true");
			else {
				se.AddAttribute ("Allowed", m_allowed.ToString ());
				if (m_userQuota > 0)
					se.AddAttribute ("UserQuota", m_userQuota.ToString ());
			}
			
			return se;
		}

		public override void FromXml (SecurityElement esd)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			m_userQuota = 0;
			m_machineQuota = 0;
			m_expirationDays = 0;
			m_permanentData = false;
			m_allowed = IsolatedStorageContainment.None;

			if (IsUnrestricted (esd)) {
				UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			} else {
				string a = esd.Attribute ("Allowed");
				if (a != null) {
					UsageAllowed = (IsolatedStorageContainment) Enum.Parse (
						typeof (IsolatedStorageContainment), a);
				}
				a = esd.Attribute ("UserQuota");
				if (a != null) {
					Exception exc;
					Int64.Parse (a, true, out m_userQuota, out exc);
				}
			}
		}

		// helpers

		internal bool IsEmpty ()
		{
			// should we include internals ? or just publics ?
			return ((m_userQuota == 0) && (m_allowed == IsolatedStorageContainment.None));
		}
	}
}
