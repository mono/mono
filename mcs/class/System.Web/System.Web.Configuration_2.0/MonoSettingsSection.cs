//
// System.Web.Configuration.CompilationSection
//
// Authors:
//      Marek Habersack (mhabersack@novell.com)
//
// (c) Copyright 2008 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration
{
	internal sealed class MonoSettingsSection : ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty compilersCompatibilityProp;
		static ConfigurationProperty useCompilersCompatibilityProp;
		static ConfigurationProperty verificationCompatibilityProp;
		
		static MonoSettingsSection ()
		{
			compilersCompatibilityProp = new ConfigurationProperty ("compilersCompatibility", typeof (CompilerCollection), null, null, PropertyHelper.DefaultValidator,
										ConfigurationPropertyOptions.None);
			useCompilersCompatibilityProp = new ConfigurationProperty ("useCompilersCompatibility", typeof (bool), true);
			verificationCompatibilityProp = new ConfigurationProperty ("verificationCompatibility", typeof (int), 0);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (compilersCompatibilityProp);
			properties.Add (useCompilersCompatibilityProp);
			properties.Add (verificationCompatibilityProp);
		}

		[ConfigurationProperty ("compilersCompatibility")]
                public CompilerCollection CompilersCompatibility {
                        get { return (CompilerCollection) base [compilersCompatibilityProp]; }
                }

		[ConfigurationProperty ("useCompilersCompatibility", DefaultValue = "True")]
                public bool UseCompilersCompatibility {
                        get { return (bool) base [useCompilersCompatibilityProp]; }
                        set { base [useCompilersCompatibilityProp] = value; }
                }

		[ConfigurationProperty ("verificationCompatibility", DefaultValue = "0")]
                public int VerificationCompatibility {
                        get { return (int) base [verificationCompatibilityProp]; }
                        set { base [verificationCompatibilityProp] = value; }
                }
		
		protected internal override ConfigurationPropertyCollection Properties {
                        get { return properties; }
                }
	}
}
#endif
