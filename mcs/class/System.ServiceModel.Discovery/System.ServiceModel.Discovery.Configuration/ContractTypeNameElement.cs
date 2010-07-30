using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class ContractTypeNameElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty name, @namespace;
		
		static ContractTypeNameElement ()
		{
			name = new ConfigurationProperty ("name", typeof (string), null, null, new StringValidator (0), ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			@namespace = new ConfigurationProperty ("namespace", typeof (string), "http://tempuri.org/", null, null, ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (name);
			properties.Add (@namespace);
		}
		
		public ContractTypeNameElement ()
		{
		}
		
		public ContractTypeNameElement (string name, string ns)
		{
			Name = name;
			Namespace = ns;
		}
		
		[ConfigurationProperty ("name", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		[StringValidator (MinLength = 0)]
		public string Name {
			get { return (string) Properties [name]; }
			set { Properties [name] = value; }
		}

		[ConfigurationProperty ("namespace", DefaultValue = "http://tempuri.org/", Options = ConfigurationPropertyOptions.IsKey)]
		public string Namespace {
			get { return (string) Properties [@namespace]; }
			set { Properties [@namespace] = value; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

