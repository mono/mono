//
// UsernameToken.cs: Handles WS-Security UsernameToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using Microsoft.Web.Services.Timestamp;
using System;
using System.Security.Cryptography;
//using System.Security.Cryptography.Xml;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Security {

	// References:
	// a.	Section 4.1
	//	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnglobspec/html/ws-security.asp
	// b.	...
	//	
	public sealed class UsernameToken : SecurityToken {

		private string username;
		private string password;
		private PasswordOption option;
		private DateTime created;
		private Nonce nonce;

		public UsernameToken (XmlElement element) : base (element)
		{
		}

		public UsernameToken (string username, string password) 
			: this (username, password, PasswordOption.SendNone)
		{
		}

		public UsernameToken (string username, string password, PasswordOption passwordOption) 
		{
			if ((username == null) || (username == ""))
				throw new ArgumentNullException ("username");
			if (password == null)
				throw new ArgumentNullException ("password");

			this.username = username;
			this.password = password;
			option = passwordOption;
		}

		public override AuthenticationKey AuthenticationKey {
			get { throw new SoapHeaderException ("AuthenticationKey", null); }
		}

		// TODO
		public DateTime Created {
			get { return DateTime.UtcNow; }
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

		public override SignatureKey SignatureKey {
			get {
				if (password == null)
					throw new InvalidOperationException ("no password");
				return null;
			}
		}

		public override bool SupportsDataEncryption {
			get { return false; }
		}

		public override bool SupportsDigitalSignature {
			get { return (password != null); }
		}

		public string Username {
			get { return username; }
		}

		private void Zeroize (byte[] array) 
		{
			for (int i=0; i < array.Length; i++)
				array[i] = 0;
		}

		// Reference B, Section 6
		// Password_digest = SHA1 ( nonce + created + password )
		private string HashPassword (string password) 
		{
			nonce = new Nonce ();
			byte[] n = nonce.GetValueBytes ();
			created = DateTime.UtcNow;
			byte[] c = Encoding.UTF8.GetBytes (created.ToString (WSTimestamp.TimeFormat));
			byte[] p = Encoding.UTF8.GetBytes (password);
			byte[] toBeDigested = new byte [n.Length + c.Length + p.Length];
			Array.Copy (n, 0, toBeDigested, 0, n.Length);
			Array.Copy (c, 0, toBeDigested, n.Length, c.Length);
			Array.Copy (p, 0, toBeDigested, (n.Length + c.Length), p.Length);
			// protect password
			Zeroize (p);
			SHA1 hash = SHA1.Create ();
			byte[] digest = hash.ComputeHash (toBeDigested);
			// protect password
			Zeroize (toBeDigested);
			return Convert.ToBase64String (digest);
		}

		public override XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.UsernameToken, WSSecurity.NamespaceURI);

			XmlElement xelUsername = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Username, WSSecurity.NamespaceURI);
			xelUsername.InnerText = username;
			xel.AppendChild (xelUsername);

			XmlElement xelPassword = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Password, WSSecurity.NamespaceURI);
			switch (option) {
				case PasswordOption.SendNone:
					// we don't Append as we don't send the password
					break;
				case PasswordOption.SendHashed:
					xelPassword.SetAttribute (WSSecurity.AttributeNames.Type, WSSecurity.NamespaceURI, "PasswordDigest");
					xelPassword.InnerText = HashPassword (password);
					xel.AppendChild (xelPassword);
					xel.AppendChild (nonce.GetXml (document));
					// xel.AppendChild (xelCreated);
					break;
				case PasswordOption.SendPlainText:
					xelPassword.SetAttribute (WSSecurity.AttributeNames.Type, WSSecurity.NamespaceURI, "PasswordText");
					xelPassword.InnerText = password;
					xel.AppendChild (xelPassword);
					break;
			}
			return xel;
		}

		public override void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != WSSecurity.ElementNames.UsernameToken) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
		}

		public override void Verify () {}
	}
}
