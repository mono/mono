//
// System.Net.Configuration.RequestCachingSection.cs
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
	public sealed class RequestCachingSection : ConfigurationSection
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty defaultFtpCachePolicyProp;
		static ConfigurationProperty defaultHttpCachePolicyProp;
		static ConfigurationProperty defaultPolicyLevelProp;
		static ConfigurationProperty disableAllCachingProp;
		static ConfigurationProperty isPrivateCacheProp;
		static ConfigurationProperty unspecifiedMaximumAgeProp;

		#endregion // Fields

		#region Constructors

		static RequestCachingSection ()
		{
			defaultFtpCachePolicyProp = new ConfigurationProperty ("defaultFtpCachePolicy", typeof (FtpCachePolicyElement));
			defaultHttpCachePolicyProp = new ConfigurationProperty ("defaultHttpCachePolicy", typeof (HttpCachePolicyElement));
			defaultPolicyLevelProp = new ConfigurationProperty ("defaultPolicyLevel", typeof (RequestCacheLevel), RequestCacheLevel.BypassCache);
			disableAllCachingProp = new ConfigurationProperty ("disableAllCaching", typeof (bool), false);
			isPrivateCacheProp = new ConfigurationProperty ("isPrivateCache", typeof (bool), true);
			unspecifiedMaximumAgeProp = new ConfigurationProperty ("unspecifiedMaximumAge", typeof (TimeSpan), new TimeSpan (1, 0, 0, 0));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (defaultFtpCachePolicyProp);
			properties.Add (defaultHttpCachePolicyProp);
			properties.Add (defaultPolicyLevelProp);
			properties.Add (disableAllCachingProp);
			properties.Add (isPrivateCacheProp);
			properties.Add (unspecifiedMaximumAgeProp);
		}

		public RequestCachingSection ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("defaultFtpCachePolicy")]
		public FtpCachePolicyElement DefaultFtpCachePolicy {
			get { return (FtpCachePolicyElement) base [defaultFtpCachePolicyProp]; }
		}

		[ConfigurationProperty ("defaultHttpCachePolicy")]
		public HttpCachePolicyElement DefaultHttpCachePolicy {
			get { return (HttpCachePolicyElement) base [defaultHttpCachePolicyProp]; }
		}

		[ConfigurationProperty ("defaultPolicyLevel", DefaultValue = "BypassCache")]
		public RequestCacheLevel DefaultPolicyLevel {
			get { return (RequestCacheLevel) base [defaultPolicyLevelProp]; }
			set { base [defaultPolicyLevelProp] = value; }
		}

		[ConfigurationProperty ("disableAllCaching", DefaultValue = "False")]
		public bool DisableAllCaching {
			get { return (bool) base [disableAllCachingProp]; }
			set { base [disableAllCachingProp] = value; }
		}

		[ConfigurationProperty ("isPrivateCache", DefaultValue = "True")]
		public bool IsPrivateCache {
			get { return (bool) base [isPrivateCacheProp]; }
			set { base [isPrivateCacheProp] = value; }
		}

		[ConfigurationProperty ("unspecifiedMaximumAge", DefaultValue = "1.00:00:00")]
		public TimeSpan UnspecifiedMaximumAge {
			get { return (TimeSpan) base [unspecifiedMaximumAgeProp]; }
			set { base [unspecifiedMaximumAgeProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void PostDeserialize ()
		{
			base.PostDeserialize ();
		}

		[MonoTODO]
		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			base.DeserializeElement (reader, serializeCollectionKey);
		}

		#endregion // Methods
	}
}

#endif
