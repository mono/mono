//
// System.Web.Configuration.CompilerCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
extern alias PrebuiltSystem;
using TypeDescriptor = PrebuiltSystem.System.ComponentModel.TypeDescriptor;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace System.CodeDom.Compiler
{
	internal sealed class Compiler : ConfigurationElement
	{
		static ConfigurationProperty compilerOptionsProp;
		static ConfigurationProperty extensionProp;
		static ConfigurationProperty languageProp;
		static ConfigurationProperty typeProp;
		static ConfigurationProperty warningLevelProp;
		static ConfigurationProperty providerOptionsProp;
		
		static ConfigurationPropertyCollection properties;

		static Compiler ()
		{
			compilerOptionsProp = new ConfigurationProperty("compilerOptions", typeof (string), "");
			extensionProp = new ConfigurationProperty("extension", typeof (string), "");
			languageProp = new ConfigurationProperty("language", typeof (string), "", ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			typeProp = new ConfigurationProperty("type", typeof (string), "", ConfigurationPropertyOptions.IsRequired);
			warningLevelProp = new ConfigurationProperty("warningLevel", typeof (int), 0,
								     TypeDescriptor.GetConverter (typeof (int)),
								     new IntegerValidator (0, 4),
								     ConfigurationPropertyOptions.None);
			providerOptionsProp = new ConfigurationProperty ("", typeof (CompilerProviderOptionsCollection), null, null, null,
									 ConfigurationPropertyOptions.IsDefaultCollection);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (compilerOptionsProp);
			properties.Add (extensionProp);
			properties.Add (languageProp);
			properties.Add (typeProp);
			properties.Add (warningLevelProp);
			properties.Add (providerOptionsProp);
		}

		internal Compiler ()
		{
		}

		public Compiler (string compilerOptions, string extension, string language, string type, int warningLevel)
		{
			this.CompilerOptions = compilerOptions;
			this.Extension = extension;
			this.Language = language;
			this.Type = type;
			this.WarningLevel = warningLevel;
		}

		[ConfigurationProperty ("compilerOptions", DefaultValue = "")]
		public string CompilerOptions {
			get { return (string) base[compilerOptionsProp]; }
			internal set { base[compilerOptionsProp] = value; }
		}

		[ConfigurationProperty ("extension", DefaultValue = "")]
		public string Extension {
			get { return (string) base[extensionProp]; }
			internal set { base[extensionProp] = value; }
		}

		[ConfigurationProperty ("language", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Language {
			get { return (string) base[languageProp]; }
			internal set { base[languageProp] = value; }
		}

		[ConfigurationProperty ("type", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Type {
			get { return (string) base[typeProp]; }
			internal set { base[typeProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = 4)]
		[ConfigurationProperty ("warningLevel", DefaultValue = "0")]
		public int WarningLevel {
			get { return (int) base[warningLevelProp]; }
			internal set { base[warningLevelProp] = value; }
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public CompilerProviderOptionsCollection ProviderOptions {
			get { return (CompilerProviderOptionsCollection) base [providerOptionsProp]; }
			internal set { base [providerOptionsProp] = value; }
		}

		public Dictionary <string, string> ProviderOptionsDictionary {
			get { return ProviderOptions.ProviderOptions; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
#endif
