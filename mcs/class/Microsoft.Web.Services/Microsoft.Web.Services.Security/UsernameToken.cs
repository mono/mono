//
// UsernameToken.cs: Handles WS-Security UsernameToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using Microsoft.Web.Services.Timestamp;

using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;

#if WSE2
using System.Collections;
#endif

namespace Microsoft.Web.Services.Security {

	// References:
	// a.	Section 4.1
	//	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnglobspec/html/ws-security.asp
	// b.	Web Services Security Addendum, Version 1.0, August 18, 2002
	//	http://msdn.microsoft.com/library/en-us/dnglobspec/html/ws-security-addendum.asp
	// c.	WS-Trust, Section 7
	//	http://www-106.ibm.com/developerworks/library/ws-trust/

#if WSE1
	public sealed class UsernameToken : SecurityToken {
#else
	public class UsernameToken : SecurityToken, IMutableSecurityToken {
#endif
		static private IPasswordProvider provider;

		private string username;
		private string password;
		private string digest;
		private PasswordOption option;
		private DateTime created;
		private Nonce nonce;

		static UsernameToken () 
		{
			provider = null;
		}

		public UsernameToken (XmlElement element) : base (element) 
		{
			// base will call the LoadXml (element)
		}

		public UsernameToken (string username, string password) 
			: this (username, password, PasswordOption.SendNone) {}

		public UsernameToken (string username, string password, PasswordOption passwordOption) 
		{
			if ((username == null) || (username == ""))
				throw new ArgumentNullException ("username");
			if ((password == null) || (password == ""))
				throw new ArgumentNullException ("password");

			this.username = username;
			this.password = password;
			option = passwordOption;
		}

		[MonoTODO ("where is the key derivation described ?")]
		public override AuthenticationKey AuthenticationKey {
			get { 
				if (nonce == null) {
					// LAMESPEC: undocumented exception
					throw new InvalidOperationException ("AuthenticationKey"); 
				}
				return null;
			}
		}

		public DateTime Created {
			get { return created; }
		}

		public override DecryptionKey DecryptionKey {
			get { throw new NotSupportedException ("DecryptionKey"); }
		}

		public override EncryptionKey EncryptionKey {
			get { throw new NotSupportedException ("EncryptionKey"); }
		}

		public byte[] Nonce {
			get { 
				if (nonce == null)
					return null;
				return nonce.GetValueBytes (); 
			}
		}

		public string Password {
			get { return password; }
		}

		public PasswordOption PasswordOption {
			get { 
				if (password == null)
					throw new ArgumentException ("no password");
				return option; 
			}
			set { 
				if (password == null)
					throw new ArgumentException ("no password");
				option = value; 
			}
		}

		[MonoTODO ("where is the key derivation described ?")]
		public override SignatureKey SignatureKey {
			get {
				if (password == null)
					throw new InvalidOperationException ("no password");
				// TODO: where is the key derivation described ?
				return null;
			}
		}

		public override bool SupportsDataEncryption {
			get { return false; }
		}

		public override bool SupportsDigitalSignature {
			get { return true; }
		}

		public string Username {
			get { return username; }
		}

		// Reference B, Section 6
		// Password_digest = SHA1 ( nonce + created + password )
		private string HashPassword (string password) 
		{
			byte[] n = nonce.GetValueBytes ();
			byte[] c = Encoding.UTF8.GetBytes (created.ToString (WSTimestamp.TimeFormat));
			byte[] p = Encoding.UTF8.GetBytes (password);
			byte[] toBeDigested = new byte [n.Length + c.Length + p.Length];
			Array.Copy (n, 0, toBeDigested, 0, n.Length);
			Array.Copy (c, 0, toBeDigested, n.Length, c.Length);
			Array.Copy (p, 0, toBeDigested, (n.Length + c.Length), p.Length);
			// protect password
			Array.Clear (p, 0, p.Length);
			SHA1 hash = SHA1.Create ();
			byte[] digest = hash.ComputeHash (toBeDigested);
			// protect password
			Array.Clear (toBeDigested, 0, toBeDigested.Length);
			return Convert.ToBase64String (digest);
		}

		public override XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.UsernameToken, WSSecurity.NamespaceURI);
			xel.SetAttribute ("xmlns:" + WSTimestamp.Prefix, WSTimestamp.NamespaceURI);
			xel.SetAttribute (WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI, Id);

			XmlElement xelUsername = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Username, WSSecurity.NamespaceURI);
			xelUsername.InnerText = username;
			xel.AppendChild (xelUsername);

			// Nonce and Created are required for hashing the password
			if (nonce == null)
				nonce = new Nonce ();
			// get creation time when serializing
			created = DateTime.UtcNow;

			if (option != PasswordOption.SendNone) {
				XmlElement xelPassword = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Password, WSSecurity.NamespaceURI);
				switch (option) {
					case PasswordOption.SendHashed:
						// no WSSecurity.NamespaceURI because it would add a "wsse" before "Type"
						xelPassword.SetAttribute (WSSecurity.AttributeNames.Type, WSSecurity.Prefix + ":PasswordDigest");
						xelPassword.InnerText = HashPassword (password);
						break;
					case PasswordOption.SendPlainText:
						// no WSSecurity.NamespaceURI because it would add a "wsse" before "Type"
						xelPassword.SetAttribute (WSSecurity.AttributeNames.Type, WSSecurity.Prefix + ":PasswordText");
						xelPassword.InnerText = password;
						break;
				}
				xel.AppendChild (xelPassword);
			}
			xel.AppendChild (nonce.GetXml (document));
			XmlElement xelCreated = document.CreateElement (WSTimestamp.Prefix, WSTimestamp.ElementNames.Created, WSTimestamp.NamespaceURI);
			xelCreated.InnerText = created.ToString (WSTimestamp.TimeFormat);
			xel.AppendChild (xelCreated);
			return xel;
		}

		public override void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			if ((element.LocalName != WSSecurity.ElementNames.UsernameToken) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");

			// retrieve Id
			XmlAttribute xaId = element.Attributes [WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI];
			if (xaId != null) {
				Id = xaId.InnerText;
			}
			// retreive Username
			XmlNodeList xnl = element.GetElementsByTagName (WSSecurity.ElementNames.Username, WSSecurity.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				username = xnl [0].InnerText;
			}
			// retreive Password (if present), PasswordDigest (if present) or none and set PasswordOption
			xnl = element.GetElementsByTagName (WSSecurity.ElementNames.Password, WSSecurity.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				XmlAttribute pwdType = xnl [0].Attributes [WSSecurity.AttributeNames.Type];
				if (pwdType != null) {
					string s = pwdType.InnerText;
					if (s.EndsWith (":PasswordDigest")) {
						option = PasswordOption.SendHashed;
						digest = xnl [0].InnerText;
					}
					else if (s.EndsWith (":PasswordText")) {
						option = PasswordOption.SendPlainText;
						password = xnl [0].InnerText;
					}
					else
						throw new Exception ("TODO");
				}
			}
			else
				option = PasswordOption.SendNone;
			// retreive Nonce
			xnl = element.GetElementsByTagName (WSSecurity.ElementNames.Nonce, WSSecurity.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				XmlElement xel = (XmlElement) xnl [0];
				if (nonce == null)
					nonce = new Nonce (xel);
				else
					nonce.LoadXml (xel);
			}
			// retreive Created
			xnl = element.GetElementsByTagName (WSTimestamp.ElementNames.Created, WSTimestamp.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				created = DateTime.ParseExact (xnl [0].InnerText, WSTimestamp.TimeFormat, null);
			}

			if (provider == null)
				throw new ConfigurationException (Locale.GetText ("No PasswordProvider configured"));

			string providerPassword = provider.GetPassword (this);
			switch (option) {
				case PasswordOption.SendNone:
					break;
				case PasswordOption.SendHashed:
					if (digest != HashPassword (providerPassword))
						throw new SecurityFault (Locale.GetText ("bad password"), null);
					break;
				case PasswordOption.SendPlainText:
					if (providerPassword != password)
						throw new SecurityFault (Locale.GetText ("bad password"), null);
					break;
			}
		}

#if WSE1
		public override void Verify () {}
#else
		public IList AnyElements {
			get { return null; }
		}

		public SecurityToken Clone ()
		{
			return new UsernameToken (username, password, option); 
		}

		[MonoTODO ("need to compare results with WSE2")]
		public override int GetHashCode () 
		{
			return username.GetHashCode ();
		}

		[MonoTODO ("need to compare results with WSE2")]
		public override bool Equals (SecurityToken token) 
		{
			if (token is UsernameToken) {
				UsernameToken t = token as UsernameToken;
				if ((t.Username == username) && (t.PasswordOption == option)) {
					// 1st case - we have a textual password (SendPlainText)
					if (password != null)
						return (t.Password == password);
					// we may not have the actual password if we're created
					// with the UsernameToken(XmlElement) constructor.
					// TODO: compare hashed password
					// however we can't compare two UsernameToken coming
					// both from UsernameToken(XmlElement) constructor
					return false;
				}
			}
			return false;
		}

		public override bool IsCurrent {
			get { return false; }
		}
#endif
	}
}
