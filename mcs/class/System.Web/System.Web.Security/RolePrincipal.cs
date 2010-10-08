//
// System.Web.Security.RolePrincipal
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Configuration;
using System.IO;
using System.Text;

namespace System.Web.Security {

	[Serializable]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_4_0
	public
#else
	public sealed
#endif
	class RolePrincipal : IPrincipal {

		IIdentity _identity;
		bool _listChanged;
		string[] _cachedArray;
		HybridDictionary _cachedRoles;
		readonly string _providerName;

		int _version = 1;
		string _cookiePath;
		DateTime _issueDate;
		DateTime _expireDate;


		public RolePrincipal (IIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			
			this._identity = identity;
			this._cookiePath = RoleManagerConfig.CookiePath;
			this._issueDate = DateTime.Now;
			this._expireDate = _issueDate.Add (RoleManagerConfig.CookieTimeout);
		}

		public RolePrincipal (IIdentity identity, string encryptedTicket)
			: this (identity)
		{
			DecryptTicket (encryptedTicket);
		}

		public RolePrincipal (string providerName, IIdentity identity)
			: this (identity)
		{
			if (providerName == null)
				throw new ArgumentNullException ("providerName");

			this._providerName = providerName;
		}

		public RolePrincipal (string providerName, IIdentity identity, string encryptedTicket)
			: this (providerName, identity)
		{
			DecryptTicket (encryptedTicket);
		}

		public string [] GetRoles ()
		{
			if (!_identity.IsAuthenticated)
				return new string[0];

			if (!IsRoleListCached || Expired) {
				_cachedArray = Provider.GetRolesForUser (_identity.Name);
				_cachedRoles = new HybridDictionary (true);

				foreach (string r in _cachedArray)
					_cachedRoles.Add(r, r);

				_listChanged = true;
			}

			return _cachedArray;
		}
		
		public bool IsInRole (string role)
		{
			if (!_identity.IsAuthenticated)
				return false;

			GetRoles ();

			return _cachedRoles [role] != null;
		}
		
		public string ToEncryptedTicket ()
		{
			string roles = string.Join (",", GetRoles ());
			string cookiePath = RoleManagerConfig.CookiePath;
			int approxTicketLen = roles.Length + cookiePath.Length + 64;

			if (_cachedArray.Length > Roles.MaxCachedResults)
			       return null;

			MemoryStream ticket = new MemoryStream (approxTicketLen);
			BinaryWriter writer = new BinaryWriter (ticket);

			// version
			writer.Write (Version);
		
			// issue datetime
			DateTime issueDate = DateTime.Now;
			writer.Write (issueDate.Ticks);

			// expiration datetime
			writer.Write (_expireDate.Ticks);

			writer.Write (cookiePath);
			writer.Write (roles);

			CookieProtection cookieProtection = RoleManagerConfig.CookieProtection;

			byte[] ticket_data = ticket.GetBuffer ();
			if (cookieProtection == CookieProtection.All) {
				ticket_data = MachineKeySectionUtils.EncryptSign (MachineConfig, ticket_data);
			} else if (cookieProtection == CookieProtection.Encryption) {
				ticket_data = MachineKeySectionUtils.Encrypt (MachineConfig, ticket_data);
			} else if (cookieProtection == CookieProtection.Validation) {
				ticket_data = MachineKeySectionUtils.Sign (MachineConfig, ticket_data);
			}

			return GetBase64FromBytes (ticket_data, 0, ticket_data.Length);
		}

		void DecryptTicket (string encryptedTicket)
		{
			if (encryptedTicket == null || encryptedTicket == String.Empty)
				throw new ArgumentException ("Invalid encrypted ticket", "encryptedTicket");

			byte [] ticketBytes = GetBytesFromBase64 (encryptedTicket);
			byte [] decryptedTicketBytes = null;

			CookieProtection cookieProtection = RoleManagerConfig.CookieProtection;

			if (cookieProtection == CookieProtection.All) {
				decryptedTicketBytes = MachineKeySectionUtils.VerifyDecrypt (MachineConfig, ticketBytes);
			} else if (cookieProtection == CookieProtection.Encryption) {
				decryptedTicketBytes = MachineKeySectionUtils.Decrypt (MachineConfig, ticketBytes);
			} else if (cookieProtection == CookieProtection.Validation) {
				decryptedTicketBytes = MachineKeySectionUtils.Verify (MachineConfig, ticketBytes);
			}

			if (decryptedTicketBytes == null)
				throw new HttpException ("ticket validation failed");

			MemoryStream ticket = new MemoryStream (decryptedTicketBytes);
			BinaryReader reader = new BinaryReader (ticket);

			// version
			_version = reader.ReadInt32 ();

			// issued date
			_issueDate = new DateTime (reader.ReadInt64 ());

			// expire date
			_expireDate = new DateTime (reader.ReadInt64 ());

			// cookie path
			_cookiePath = reader.ReadString ();
			
			// roles
			string roles = reader.ReadString ();

			if (!Expired) {
				InitializeRoles (roles);
				//update ticket if less than half of CookieTimeout remaining.
				if (Roles.CookieSlidingExpiration){
					if (_expireDate-DateTime.Now < TimeSpan.FromTicks (RoleManagerConfig.CookieTimeout.Ticks/2))	{
						_issueDate = DateTime.Now;
						_expireDate = DateTime.Now.Add (RoleManagerConfig.CookieTimeout);
						SetDirty ();
					}
				}
			} else {
				// issue a new ticket
				_issueDate = DateTime.Now;
				_expireDate = _issueDate.Add (RoleManagerConfig.CookieTimeout);
			}
		}

		void InitializeRoles (string decryptedRoles)
		{
			_cachedArray = decryptedRoles.Split (',');
			_cachedRoles = new HybridDictionary (true);

			foreach (string r in _cachedArray)
				_cachedRoles.Add (r, r);
		}

		public bool CachedListChanged {
			get { return _listChanged; }
		}
		
		public string CookiePath {
			get { return _cookiePath; }
		}
		
		public bool Expired {
			get { return ExpireDate < DateTime.Now; }
		}
		
		public DateTime ExpireDate {
			get { return _expireDate; }
		}
		
		public IIdentity Identity {
			get { return _identity; }
		}
		
		public bool IsRoleListCached {
			get { return (_cachedRoles != null) && RoleManagerConfig.CacheRolesInCookie; }
		}
		
		public DateTime IssueDate {
			get { return _issueDate; }
		}
		
		public string ProviderName {
			get { return String.IsNullOrEmpty(_providerName) ? Provider.Name : _providerName; }
		}
		
		public int Version {
			get { return _version; }
		}

		RoleProvider Provider {
			get {
				if (String.IsNullOrEmpty (_providerName))
					return Roles.Provider;

				return Roles.Providers [_providerName];
			}
		}

		public void SetDirty ()
		{
			_listChanged = true;
			_cachedRoles = null;
			_cachedArray = null;
		}

		static string GetBase64FromBytes (byte [] bytes, int offset, int len)
		{
			return Convert.ToBase64String (bytes, offset, len);
		}

		static byte [] GetBytesFromBase64 (string base64String)
		{
			return Convert.FromBase64String (base64String);
		}

		RoleManagerSection RoleManagerConfig
		{
			get { return (RoleManagerSection) WebConfigurationManager.GetSection ("system.web/roleManager"); }
		}

		MachineKeySection MachineConfig
		{
			get { return (MachineKeySection) WebConfigurationManager.GetSection ("system.web/machineKey"); }
		}
	}
}


