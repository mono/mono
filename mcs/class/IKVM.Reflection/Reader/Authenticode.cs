using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IKVM.Reflection.Reader
{
	static class Authenticode
	{
		internal static X509Certificate GetSignerCertificate(Stream stream)
		{
			throw new NotSupportedException ("mcs");
		}
	}
}
