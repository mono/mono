//
// System.Security.Permissions.IsolatedStoragePermission.cs
//
// Piers Haken <piersh@friskit.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;
using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[Serializable]
	public abstract class IsolatedStoragePermission : CodeAccessPermission, IUnrestrictedPermission
	{
		internal long m_userQuota;
		internal long m_machineQuota;
		internal long m_expirationDays;
		internal bool m_permanentData;
		internal IsolatedStorageContainment m_allowed;

		public IsolatedStoragePermission (PermissionState state)
		{
			if (state == PermissionState.None)
			{
				m_userQuota = 0;
				m_machineQuota = 0;
				m_expirationDays = 0;
				m_permanentData = false;
				m_allowed = IsolatedStorageContainment.None;
			}
			else if (state == PermissionState.Unrestricted)
			{
				m_userQuota = Int64.MaxValue;
				m_machineQuota = Int64.MaxValue;
				m_expirationDays = Int64.MaxValue ;
				m_permanentData = true;
				m_allowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			}
			else
			{
				throw new ArgumentException("Invalid Permission state");
			}
		}

		public long UserQuota
		{
			set { m_userQuota = value; }
			get { return m_userQuota; }
		}


		public IsolatedStorageContainment UsageAllowed
		{
			set { m_allowed = value; }
			get { return m_allowed; }
		}


		public bool IsUnrestricted ()
		{
			return IsolatedStorageContainment.UnrestrictedIsolatedStorage == m_allowed;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			if (m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage)
				e.AddAttribute ("Unrestricted", "true");

			else if (m_allowed == IsolatedStorageContainment.None)
				e.AddAttribute ("Allowed", "None");
			
			return e;
		}


		public override void FromXml (SecurityElement esd)
		{
			if (esd == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (esd.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (esd.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));
			
			if (esd.Attribute ("Unrestricted") == "true")
				m_allowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;

			else if (esd.Attribute ("Allowed") == "None")
				m_allowed = IsolatedStorageContainment.None;
		}
	}
}





