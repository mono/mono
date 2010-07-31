using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	[ConfigurationCollection (typeof (ScopeElement))]
	public sealed class ScopeElementCollection : ServiceModelConfigurationElementCollection<ScopeElement>
	{
		public ScopeElementCollection ()
		{
		}
	}
}

