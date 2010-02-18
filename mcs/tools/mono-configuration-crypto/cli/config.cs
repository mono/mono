using System;

namespace MonoConfigurationCrypto
{
	sealed class Config
	{
		public const string DefaultContainerName = "MonoFrameworkConfigurationKey";
		public const uint DefaultKeySize = 1024;
		public const string DefaultApplicationPhysicalPath = ".";
		public const string DefaultConfigFileName = "Web.config";
		
		public string ApplicationPhysicalPath {
			get;
			set;
		}
		
		public string ConfigSectionName {
			get;
			set;
		}

		public string ConfigFileName {
			get;
			set;
		}
		
		public string FileName {
			get;
			set;
		}
		
		public bool UseMachinePath {
			get;
			set;
		}

		public bool ShowHelp {
			get;
			set;
		}

		public string ContainerName {
			get;
			set;
		}

		public uint KeySize {
			get;
			set;
		}

		public bool Verbose {
			get;
			set;
		}
		
		public Config ()
		{
			ApplicationPhysicalPath = DefaultApplicationPhysicalPath;
			ContainerName = DefaultContainerName;
			ConfigFileName = DefaultContainerName;
			KeySize = DefaultKeySize;
		}
	}
}
