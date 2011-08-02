//
// System.Web.Configuration.OutputCacheSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Configuration;

namespace System.Web.Configuration
{

	public sealed class OutputCacheSection : ConfigurationSection
	{
		static ConfigurationProperty enableFragmentCacheProp;
		static ConfigurationProperty enableOutputCacheProp;
		static ConfigurationProperty omitVaryStarProp;
		static ConfigurationProperty sendCacheControlHeaderProp;
		static ConfigurationProperty enableKernelCacheForVaryByStarProp;
#if NET_4_0
		static ConfigurationProperty providersProp;
		static ConfigurationProperty defaultProviderNameProp;
#endif
		
		static ConfigurationPropertyCollection properties;

		static OutputCacheSection ()
		{
			enableFragmentCacheProp = new ConfigurationProperty ("enableFragmentCache", typeof (bool), true);
			enableOutputCacheProp = new ConfigurationProperty ("enableOutputCache", typeof (bool), true);
			omitVaryStarProp = new ConfigurationProperty ("omitVaryStar", typeof (bool), false);
			sendCacheControlHeaderProp = new ConfigurationProperty ("sendCacheControlHeader", typeof (bool), true);
			enableKernelCacheForVaryByStarProp = new ConfigurationProperty ("enableKernelCacheForVaryByStar", typeof (bool), false);
#if NET_4_0
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection));
			defaultProviderNameProp = new ConfigurationProperty ("defaultProvider", typeof (string), "AspNetInternalProvider");
#endif
			
			properties = new ConfigurationPropertyCollection ();

			properties.Add (enableFragmentCacheProp);
			properties.Add (enableOutputCacheProp);
			properties.Add (omitVaryStarProp);
			properties.Add (sendCacheControlHeaderProp);
			properties.Add (enableKernelCacheForVaryByStarProp);
#if NET_4_0
			properties.Add (providersProp);
			properties.Add (defaultProviderNameProp);
#endif
		}

		[ConfigurationProperty ("enableFragmentCache", DefaultValue = "True")]
		public bool EnableFragmentCache {
			get { return (bool) base [enableFragmentCacheProp];}
			set { base[enableFragmentCacheProp] = value; }
		}

		[ConfigurationProperty ("enableOutputCache", DefaultValue = "True")]
		public bool EnableOutputCache {
			get { return (bool) base [enableOutputCacheProp];}
			set { base[enableOutputCacheProp] = value; }
		}

		[ConfigurationProperty ("enableKernelCacheForVaryByStar", DefaultValue = "False")]
		public bool EnableKernelCacheForVaryByStar {
			get { return (bool) base [enableKernelCacheForVaryByStarProp]; }
			set { base [enableKernelCacheForVaryByStarProp] = value; }
		}
		
		[ConfigurationProperty ("omitVaryStar", DefaultValue = "False")]
		public bool OmitVaryStar {
			get { return (bool) base [omitVaryStarProp];}
			set { base[omitVaryStarProp] = value; }
		}

		[ConfigurationProperty ("sendCacheControlHeader", DefaultValue = "True")]
		public bool SendCacheControlHeader {
			get { return (bool) base [sendCacheControlHeaderProp];}
			set { base[sendCacheControlHeaderProp] = value; }
		}

#if NET_4_0
		[StringValidatorAttribute(MinLength = 1)]
		[ConfigurationPropertyAttribute("defaultProvider", DefaultValue = "AspNetInternalProvider")]
		public string DefaultProviderName {
			get { return base [defaultProviderNameProp] as string; }
			set { base [defaultProviderNameProp] = value; }
		}
		
		[ConfigurationPropertyAttribute("providers")]
		public ProviderSettingsCollection Providers {
			get { return base [providersProp] as ProviderSettingsCollection; }
		}
#endif
		
		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}


