namespace System.IdentityModel.Tokens
{
	public class EncryptingCredentials
	{
		public string Algorithm { get; set; }
		public SecurityKey SecurityKey { get; set; }
		public SecurityKeyIdentifier SecurityKeyIdentifier { get; set; }

		public EncryptingCredentials ()
		{ }

		public EncryptingCredentials (SecurityKey key, SecurityKeyIdentifier keyIdentifier, string algorithm) {
			SecurityKey = key;
			SecurityKeyIdentifier = keyIdentifier;
			Algorithm = algorithm;
		}
	}
}