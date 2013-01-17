//
// System.Net.Configuration.HttpCachePolicyElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) Tim Coleman, 2004
// (C) 2004,2005 Novell, Inc. (http://www.novell.com)
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
using System.Net.Cache;
using System.Xml;

namespace System.Net.Configuration 
{
	public sealed class HttpCachePolicyElement : ConfigurationElement
	{
		#region Fields

		static ConfigurationProperty maximumAgeProp;
		static ConfigurationProperty maximumStaleProp;
		static ConfigurationProperty minimumFreshProp;
		static ConfigurationProperty policyLevelProp;
		static ConfigurationPropertyCollection properties;

		#endregion // Fields

		#region Constructors

		static HttpCachePolicyElement ()
		{
			maximumAgeProp = new ConfigurationProperty ("maximumAge", typeof (TimeSpan), TimeSpan.MaxValue);
			maximumStaleProp = new ConfigurationProperty ("maximumStale", typeof (TimeSpan), TimeSpan.MinValue);
			minimumFreshProp = new ConfigurationProperty ("minimumFresh", typeof (TimeSpan), TimeSpan.MinValue);
			policyLevelProp = new ConfigurationProperty ("policyLevel", typeof (HttpRequestCacheLevel),
								     HttpRequestCacheLevel.Default, ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (maximumAgeProp);
			properties.Add (maximumStaleProp);
			properties.Add (minimumFreshProp);
			properties.Add (policyLevelProp);
		}

		public HttpCachePolicyElement ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("maximumAge", DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan MaximumAge {
			get { return (TimeSpan) base [maximumAgeProp]; }
			set { base [maximumAgeProp] = value; }
		}

		[ConfigurationProperty ("maximumStale", DefaultValue = "-10675199.02:48:05.4775808")]
		public TimeSpan MaximumStale {
			get { return (TimeSpan) base [maximumStaleProp]; }
			set { base [maximumStaleProp] = value; }
		}

		[ConfigurationProperty ("minimumFresh", DefaultValue = "-10675199.02:48:05.4775808")]
		public TimeSpan MinimumFresh {
			get { return (TimeSpan) base [minimumFreshProp]; }
			set { base [minimumFreshProp] = value; }
		}

		[ConfigurationProperty ("policyLevel", DefaultValue = "Default", Options = ConfigurationPropertyOptions.IsRequired)]
		public HttpRequestCacheLevel PolicyLevel {
			get { return (HttpRequestCacheLevel) base [policyLevelProp]; }
			set { base [policyLevelProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Reset (ConfigurationElement parentElement)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
