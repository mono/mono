// System.Web.Configuration.CompilerProviderOptionsCollection.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc (http://www.novell.com)
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

#if CONFIGURATION_DEP
using System;
using System.Configuration;

namespace System.CodeDom.Compiler
{
	internal sealed class CompilerProviderOption : ConfigurationElement
	{
		static ConfigurationProperty nameProp;
		static ConfigurationProperty valueProp;
		static ConfigurationPropertyCollection properties;
		
		static CompilerProviderOption ()
		{
			nameProp = new ConfigurationProperty ("name", typeof (string), "",
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			valueProp = new ConfigurationProperty ("value", typeof (string), "",
							       ConfigurationPropertyOptions.IsRequired);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (nameProp);
			properties.Add (valueProp);
		}

		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [nameProp]; }
			set { base [nameProp] = value; }
		}

		[ConfigurationProperty ("value", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Value {
			get { return (string) base [valueProp]; }
			set { base [valueProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
#endif
