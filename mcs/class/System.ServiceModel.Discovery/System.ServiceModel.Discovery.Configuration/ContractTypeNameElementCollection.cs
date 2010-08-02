using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	[ConfigurationCollection (typeof(ContractTypeNameElement))]
	public sealed class ContractTypeNameElementCollection : ServiceModelConfigurationElementCollection<ContractTypeNameElement>
	{
		public ContractTypeNameElementCollection ()
		{
		}
		
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ContractTypeNameElement) element).Name;
		}
	}
}

