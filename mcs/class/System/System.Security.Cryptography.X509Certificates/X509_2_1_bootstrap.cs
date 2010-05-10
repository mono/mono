namespace System.Security.Cryptography.X509Certificates {

	public class X509Certificate2 : X509Certificate {
		public X509Certificate2 (byte[] rawData)
		{
		}
	}

	public class X509Chain {
		public bool Build (X509Certificate2 cert)
		{
			return false;
		}
	}
}

