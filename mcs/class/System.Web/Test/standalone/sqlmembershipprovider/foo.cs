
using System.Security.Cryptography;
using System.Text;
using System;

public class foo {
		static byte ToHexValue (char c, bool high)
		{
			byte v;
			if (c >= '0' && c <= '9')
				v = (byte) (c - '0');
			else if (c >= 'a' && c <= 'f')
				v = (byte) (c - 'a' + 10);
			else if (c >= 'A' && c <= 'F')
				v = (byte) (c - 'A' + 10);
			else
				throw new ArgumentException ("Invalid hex character");

			if (high)
				v <<= 4;

			return v;
		}
		
		internal static byte [] GetBytes (string key, int len)
		{
			byte [] result = new byte [len / 2];
			for (int i = 0; i < len; i += 2)
				result [i / 2] = (byte) (ToHexValue (key [i], true) + ToHexValue (key [i + 1], false));

			return result;
		}


  static void decrypt () {
    string ans = "NVG3kJkWiFzKHh2UAEDiaY4kvYYUk+z4gEdlv2rZudA=";
    string key = "1E14FC86752772F5DB58B99764D0168106D336563D77CCBA";
    string salt = "xXsrnq4n1jebmRiC/Ty46g==";

    byte[] key_bytes = GetBytes (key, key.Length);
    byte[] ans_bytes = Convert.FromBase64String (ans);
    byte[] salt_bytes = Convert.FromBase64String (salt);

    Console.WriteLine ("encrypted = {0} long, salt = {1} long", ans_bytes.Length, salt_bytes.Length);

    SymmetricAlgorithm alg = Rijndael.Create ();

    ICryptoTransform decryptor = alg.CreateDecryptor (key_bytes, salt_bytes);

    byte[] rv = decryptor.TransformFinalBlock (ans_bytes, 0, ans_bytes.Length);

    Console.WriteLine ("decryption result = {0}", Convert.ToBase64String (rv));

    Console.WriteLine (Encoding.Unicode.GetString (rv));
  }

  static void encrypt () {
    string ans = "green";
    string key = "1E14FC86752772F5DB58B99764D0168106D336563D77CCBA";
    string salt = "xXsrnq4n1jebmRiC/Ty46g==";

    byte[] key_bytes = GetBytes (key, key.Length);
    byte[] ans_bytes = Encoding.Unicode.GetBytes (ans);
    byte[] salt_bytes = Convert.FromBase64String (salt);

    byte[] buf = new byte[ans_bytes.Length + salt_bytes.Length];

    /*Array.Copy (salt_bytes, 0, buf, 0, salt_bytes.Length);*/
    Array.Copy (ans_bytes, 0, buf, salt_bytes.Length, ans_bytes.Length);

    Console.WriteLine ("before encryption {0}", Convert.ToBase64String (buf));

    SymmetricAlgorithm alg = Rijndael.Create ();

    ICryptoTransform encryptor = alg.CreateEncryptor (key_bytes, salt_bytes);

    Console.WriteLine (Convert.ToBase64String (encryptor.TransformFinalBlock (buf, 0, buf.Length)));
  }

  public static void Main (string[] args) {
	decrypt();
	encrypt();
  }
}
