namespace System.ServiceModel.Security
{
	public enum X509CertificateValidationMode
	{
		None = 0,
		PeerTrust = 1,
		ChainTrust = 2,
		PeerOrChainTrust = 3,
		Custom = 4,
	}
}