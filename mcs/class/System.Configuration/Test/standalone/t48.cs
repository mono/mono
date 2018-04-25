using System;
using System.Collections;

// Bugzilla #39669

namespace TestConfigSection
{
	class Test
	{
		public static int Main (string[] args)
		{
			Hashtable testCustomSection = (Hashtable)System.Configuration.ConfigurationManager.GetSection ("TestCustomSection");
			string proxyServer = (string)testCustomSection["ProxyServer"];

			if (proxyServer == null)
				throw new Exception("Custom section value is null");

			if (proxyServer != "server.example.com")
				throw new Exception("Custom section value is incorrect");

			return 0;
		}
	}
}
