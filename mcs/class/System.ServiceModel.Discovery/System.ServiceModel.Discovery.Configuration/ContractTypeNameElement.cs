//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if NET_4_0
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
			get { return (string) base [name]; }
			set { base [name] = value; }
		}

		[ConfigurationProperty ("namespace", DefaultValue = "http://tempuri.org/", Options = ConfigurationPropertyOptions.IsKey)]
		public string Namespace {
			get { return (string) base [@namespace]; }
			set { base [@namespace] = value; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
