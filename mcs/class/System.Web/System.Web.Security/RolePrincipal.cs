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
using System.Security.Cryptography;
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

			if (cookieProtection == CookieProtection.None)
				return GetBase64FromBytes (ticket.GetBuffer (), 0, (int) ticket.Position);
			
			if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Validation) {

				byte [] hashBytes = null;
				byte [] validationBytes = MachineKeySectionUtils.ValidationKeyBytes (MachineConfig);
				writer.Write (validationBytes);

				switch (MachineConfig.Validation) {
					case MachineKeyValidation.MD5:
						hashBytes = MD5.Create ().ComputeHash (ticket.GetBuffer (), 0, (int) ticket.Position);
						break;

					case MachineKeyValidation.TripleDES:
					case MachineKeyValidation.SHA1:
						hashBytes = SHA1.Create ().ComputeHash (ticket.GetBuffer (), 0, (int) ticket.Position);
						break;
				}

				writer.Seek (-validationBytes.Length, SeekOrigin.Current);
				writer.Write (hashBytes);
			}

			byte [] ticketBytes = null;
			if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Encryption) {
				ICryptoTransform enc;
				enc = TripleDES.Create ().CreateEncryptor (MachineKeySectionUtils.DecryptionKey192Bits (MachineConfig),
									   InitVector);
				ticketBytes = enc.TransformFinalBlock (ticket.GetBuffer (), 0, (int) ticket.Position);
			}

			if (ticketBytes == null)
				return GetBase64FromBytes (ticket.GetBuffer (), 0, (int) ticket.Position);
			else
				return GetBase64FromBytes (ticketBytes, 0, ticketBytes.Length);
		}

		void DecryptTicket (string encryptedTicket)
		{
			if (encryptedTicket == null || encryptedTicket == String.Empty)
				throw new ArgumentException ("Invalid encrypted ticket", "encryptedTicket");

			byte [] ticketBytes = GetBytesFromBase64 (encryptedTicket);
			byte [] decryptedTicketBytes = null;

			CookieProtection cookieProtection = RoleManagerConfig.CookieProtection;
			if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Encryption) {
				ICryptoTransform decryptor;
				decryptor = TripleDES.Create ().CreateDecryptor (
					MachineKeySectionUtils.DecryptionKey192Bits (MachineConfig),
					InitVector);
				decryptedTicketBytes = decryptor.TransformFinalBlock (ticketBytes, 0, ticketBytes.Length);
			}
			else
				decryptedTicketBytes = ticketBytes;

			if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Validation) {
				byte [] validationBytes = MachineKeySectionUtils.ValidationKeyBytes (MachineConfig);
				byte [] rolesWithValidationBytes = null;
				byte [] tmpValidation = null;

				int hashSize = (MachineConfig.Validation == MachineKeyValidation.MD5) ? 16 : 20; //md5 is 16 bytes, sha1 is 20 bytes

				rolesWithValidationBytes = new byte [decryptedTicketBytes.Length - hashSize + validationBytes.Length];

				Buffer.BlockCopy (decryptedTicketBytes, 0, rolesWithValidationBytes, 0, decryptedTicketBytes.Length - hashSize);
				Buffer.BlockCopy (validationBytes, 0, rolesWithValidationBytes, decryptedTicketBytes.Length - hashSize, validationBytes.Length);

				switch (MachineConfig.Validation) {
					case MachineKeyValidation.MD5:
						tmpValidation = MD5.Create ().ComputeHash (rolesWithValidationBytes);
						break;

					case MachineKeyValidation.TripleDES:
					case MachineKeyValidation.SHA1:
						tmpValidation = SHA1.Create ().ComputeHash (rolesWithValidationBytes);
						break;
				}
				for (int i = 0; i < tmpValidation.Length; i++) {
					if (i >= decryptedTicketBytes.Length ||
						tmpValidation [i] != decryptedTicketBytes [i + decryptedTicketBytes.Length - hashSize])
						throw new HttpException ("ticket validation failed");
				}
			}

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

		byte [] InitVector
		{
			get { return new byte [] { 1, 2, 3, 4, 5, 6, 7, 8 }; }
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


