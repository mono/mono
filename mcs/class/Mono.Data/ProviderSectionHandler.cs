//
// Mono.Data.ProviderSectionHandler
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//  
//
// Copyright (C) Brian Ritchie, 2002
// 
//
using System;
using System.Xml;
using System.Configuration;

namespace Mono.Data
{
	public class ProviderSectionHandler : IConfigurationSectionHandler
	{
		public virtual object Create(object parent,object configContext,XmlNode section)
		{
			ProviderCollection providers=new ProviderCollection();
			foreach (XmlElement ProviderNode in section.ChildNodes)
			{
				Provider provider=new Provider(
					GetStringValue(ProviderNode,"name",true),
					GetStringValue(ProviderNode,"connection",true),
					GetStringValue(ProviderNode,"adapter",true),
					GetStringValue(ProviderNode,"command",true),
					GetStringValue(ProviderNode,"assembly",true));
				providers.Add(provider);
			}
			return providers;
		}

		private string GetStringValue(XmlNode _node, string _attribute, bool required)
		{
			XmlNode a = _node.Attributes.RemoveNamedItem(_attribute);
			if(a==null)
			{
				if (required)
					throw new ConfigurationException("Attribute required: " + _attribute);
				else
					return null;
			}
			return a.Value;		
		}
	}
}
