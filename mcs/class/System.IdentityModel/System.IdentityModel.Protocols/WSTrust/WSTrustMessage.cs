using System;
using System.IdentityModel;

namespace System.IdentityModel.Protocols.WSTrust
{
	public abstract class WSTrustMessage : OpenObject
	{
		public bool AllowPostdating { get; set; }
		public EndpointReference AppliesTo { get; set; }
		public string AuthenticationType { get; set; }
		public BinaryExchange BinaryExchange { get; set; }
		public string CanonicalizationAlgorithm { get; set; }
		public string Context { get; set; }
		public string EncryptionAlgorithm { get; set; }
		public string EncryptWith { get; set; }
		public Entropy Entropy { get; set; }
		public int? KeySizeInBits { get; set; }
		public string KeyType { get; set; }
		public string KeyWrapAlgorithm { get; set; }
		public Lifetime Lifetime { get; set; }
		public string ReplyTo { get; set; }
		public string RequestType { get; set; }
		public string SignatureAlgorithm { get; set; }
		public string SignWith { get; set; }
		public string TokenType { get; set; }
		public UseKey UseKey { get; set; }
	}
}