using System;
using System.Configuration;

class T1
{
	static void Main(string[] args)
	{
		try {
			Console.WriteLine ("DefaultProvider = {0}", ProtectedConfiguration.DefaultProvider);
			RsaProtectedConfigurationProvider rsa = (RsaProtectedConfigurationProvider)ProtectedConfiguration.Providers [ProtectedConfiguration.DefaultProvider];
			Console.WriteLine (rsa.Name);

			Console.WriteLine ("cspProviderName = '{0}'", rsa.CspProviderName == null ? "(null)" : rsa.CspProviderName);
			Console.WriteLine ("keyContainerName = '{0}'", rsa.KeyContainerName == null ? "(null)" : rsa.KeyContainerName);
			Console.WriteLine ("useMachineContainer = '{0}'", rsa.UseMachineContainer);
			Console.WriteLine ("useOAEP = '{0}'", rsa.UseOAEP);
		}
		catch (Exception e)
		{
			Console.WriteLine ("Exception raised: {0}", e);
		}
	}
}
