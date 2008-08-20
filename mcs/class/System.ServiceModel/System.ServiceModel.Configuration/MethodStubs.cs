using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

using ConfigurationType = System.Configuration.Configuration;

namespace System.ServiceModel.Configuration
{

// ChannelEndpointElementCollection
	public sealed partial class ChannelEndpointElementCollection
		 : ServiceModelEnhancedConfigurationElementCollection<ChannelEndpointElement>
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			ChannelEndpointElement el = (ChannelEndpointElement) element;
			return el.Name + ";" + el.Contract;
		}
	}

// ClaimTypeElementCollection
	public sealed partial class ClaimTypeElementCollection
		 : ServiceModelConfigurationElementCollection<ClaimTypeElement>,  ICollection,  IEnumerable
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ClaimTypeElement) element).ClaimType;
		}
	}

// ComContractElementCollection
	public sealed partial class ComContractElementCollection
		 : ServiceModelEnhancedConfigurationElementCollection<ComContractElement>
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ComContractElement) element).Name;
		}
	}

// ComMethodElementCollection
	public sealed partial class ComMethodElementCollection
		 : ServiceModelEnhancedConfigurationElementCollection<ComMethodElement>
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ComMethodElement) element).ExposedMethod;
		}
	}

// ComPersistableTypeElementCollection
	public sealed partial class ComPersistableTypeElementCollection
		 : ServiceModelEnhancedConfigurationElementCollection<ComPersistableTypeElement>
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			// FIXME: ID? anyways, cosmetic COM stuff...
			return ((ComPersistableTypeElement) element).Name;
		}
	}

// ComUdtElementCollection
	public sealed partial class ComUdtElementCollection
		 : ServiceModelEnhancedConfigurationElementCollection<ComUdtElement>
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			// FIXME: another property? anyways COM stuff...
			return ((ComUdtElement) element).Name;
		}
	}

// CustomBindingElementCollection
	public sealed partial class CustomBindingElementCollection
		 : ServiceModelEnhancedConfigurationElementCollection<CustomBindingElement>,  ICollection,  IEnumerable
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((CustomBindingElement) element).Name;
		}
	}

// IssuedTokenClientBehaviorsElementCollection
	public sealed partial class IssuedTokenClientBehaviorsElementCollection
		 : ServiceModelConfigurationElementCollection<IssuedTokenClientBehaviorsElement>
	{
		[MonoTODO]
		protected override object GetElementKey (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}
	}

// StandardBindingElementCollection
	public sealed partial class StandardBindingElementCollection<TBindingConfiguration>
		 : ServiceModelEnhancedConfigurationElementCollection<TBindingConfiguration>,  ICollection,  IEnumerable
		 where TBindingConfiguration : StandardBindingElement, new()
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((StandardBindingElement) element).Name;
		}
	}

// TransportConfigurationTypeElementCollection
	public sealed partial class TransportConfigurationTypeElementCollection
		 : ServiceModelConfigurationElementCollection<TransportConfigurationTypeElement>
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((TransportConfigurationTypeElement) element).Name;
		}
	}

// XPathMessageFilterElementCollection
	public sealed partial class XPathMessageFilterElementCollection
		 : ServiceModelConfigurationElementCollection<XPathMessageFilterElement>
	{
		[MonoTODO]
		protected override object GetElementKey (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}
	}
}
