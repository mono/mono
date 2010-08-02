using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;


namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class ScopeElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty scope;
		
		static ScopeElement ()
		{
			scope = new ConfigurationProperty ("scopes", typeof (Uri), null, null, new CallbackValidator (typeof (ScopeElement), null/*FIXME: fill it*/), ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (scope);
		}

		public ScopeElement ()
		{
		}
		
		[MonoTODO]
		[ConfigurationProperty ("scope", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		[CallbackValidator (CallbackMethodName = "ScopeValidatorCallback", Type = typeof (ScopeElement))]
		public Uri Scope {
			get { return (Uri) base [scope]; }
			set { base [scope] = value; }
		}
	}
}

