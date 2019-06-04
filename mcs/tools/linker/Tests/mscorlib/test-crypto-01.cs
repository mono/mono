using System.Security.Cryptography;

class C
{
	public static int Main ()
	{
		Aes aes = Aes.Create ();
		if (aes == null)
			return 1;

		return 0;
	}
}