using System;
using System.Configuration;

class T1
{
	static void Main(string[] args)
	{
		try {
			Console.WriteLine ("DefaultProvider = {0}", ProtectedConfiguration.DefaultProvider);
			foreach (ProtectedConfigurationProvider pc in ProtectedConfiguration.Providers) {
				Console.WriteLine (pc.Name);
				if (pc is RsaProtectedConfigurationProvider) {
					RsaProtectedConfigurationProvider rsa = (RsaProtectedConfigurationProvider)pc;

					Console.WriteLine ("keyContainerName = {0}", rsa.KeyContainerName);
					Console.WriteLine ("useMachineContainer = {0}", rsa.UseMachineContainer);
				}
			}
		}
		catch (Exception e)
		{
			Console.WriteLine ("Exception raised: {0}", e.Message);
		}
	}
}
