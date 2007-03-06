//
// System.Web.Configuration.BackendProviderInfo
//
// Authors:
//      Marek Habersack <grendello@gmail.com>
//
// (C) 2007 Marek Habersack
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_2_0

using System;
using System.ComponentModel;
using System.Configuration;
using System.Web.Configuration;

namespace Mainsoft.Web.Configuration
{
	public sealed class BackendProviderInfo: ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty invariantProp;
		static ConfigurationProperty schemaBuilderTypeProp;
		static ConfigurationProperty parameterPlaceholderCharProp;
		static ConfigurationProperty parametersArePositionalProp;
		
		static BackendProviderInfo ()
		{
			nameProp = new ConfigurationProperty ("name", typeof (string), null,
							      TypeDescriptor.GetConverter (typeof (string)),
							      new StringValidator (1),
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			invariantProp = new ConfigurationProperty ("invariant", typeof (string), null,
								   TypeDescriptor.GetConverter (typeof (string)),
								   new StringValidator (1),
								   ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			schemaBuilderTypeProp = new ConfigurationProperty ("schemaBuilderType", typeof (string), null,
									   TypeDescriptor.GetConverter (typeof (string)),
									   new StringValidator (1),
									   ConfigurationPropertyOptions.IsRequired);

			parameterPlaceholderCharProp = new ConfigurationProperty ("parameterPlaceholderChar", typeof (string), "?",
										  TypeDescriptor.GetConverter (typeof (string)),
										  new StringValidator (1, 1),
										  ConfigurationPropertyOptions.IsRequired);

			parametersArePositionalProp = new ConfigurationProperty ("parametersArePositional", typeof (bool), false);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (nameProp);
			properties.Add (invariantProp);
			properties.Add (schemaBuilderTypeProp);
			properties.Add (parameterPlaceholderCharProp);
			properties.Add (parametersArePositionalProp);
		}
		
		internal BackendProviderInfo ()
		{
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Name {
			get { return (string) base [nameProp]; }
			set { base [nameProp] = value; }
		}
		
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("invariant", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Invariant {
			get { return (string) base [invariantProp]; }
			set { base [invariantProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("schemaBuilderType", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string SchemaBuilderType {
			get { return (string) base [schemaBuilderTypeProp]; }
			set { base [schemaBuilderTypeProp] = value; }
		}

		[StringValidator (MinLength = 1, MaxLength = 1)]
		[ConfigurationProperty ("parameterPlaceholderChar", DefaultValue = "?", Options = ConfigurationPropertyOptions.IsRequired)]
		public string ParameterPlaceholderChar {
			get { return (string) base [parameterPlaceholderCharProp]; }
			set { base [parameterPlaceholderCharProp] = value; }
		}

		[ConfigurationProperty ("parametersArePositional", DefaultValue = "True", Options = ConfigurationPropertyOptions.IsRequired)]
		public bool ParametersArePositional {
			get { return (bool) base [parametersArePositionalProp]; }
			set { base [parametersArePositionalProp] = value; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
