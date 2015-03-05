using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System.IdentityModel
{
	// To my understanding, this class is used only when "FIPS" is enabled,
	// which we explicitly disable (as the implementation depends on Windows).
	class RijndaelCryptoServiceProvider : Rijndael
	{
		public override ICryptoTransform CreateDecryptor (byte [] rgbKey, byte [] rgbIV)
		{
			throw new NotImplementedException ();
		}

		public override ICryptoTransform CreateEncryptor (byte [] rgbKey, byte [] rgbIV)
		{
			throw new NotImplementedException ();
		}

		public override void GenerateIV ()
		{
			throw new NotImplementedException ();
		}

		public override void GenerateKey ()
		{
			throw new NotImplementedException ();
		}
	}
}

