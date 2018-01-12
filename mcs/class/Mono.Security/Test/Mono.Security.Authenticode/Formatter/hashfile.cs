using System.IO;

public class hashfile
{
public static void Main(string[] args)
{
	string temp = args[0] + ".temp";
	File.Delete(temp);
	File.Copy(args[0], temp);
	var a = new Mono.Security.Authenticode.AuthenticodeFormatter();
	var b = a.Sign(temp);
	System.Console.WriteLine($"hashfile {temp}{b}", temp, b);
	
	// TODO dump the signature
	// TODO if hosting on windows, run signcode and compare
	// For now, we are just going after failing cases anyway.
}
}
