//
// System.Net.Configuration.HttpCachePolicyElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
using System.Net.Cache;
using System.Xml;

namespace System.Net.Configuration 
{
	public sealed class HttpCachePolicyElement : ConfigurationElement
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty maximumAge = new ConfigurationProperty ("MaximumAge", typeof (TimeSpan), TimeSpan.MaxValue);
		static ConfigurationProperty maximumStale = new ConfigurationProperty ("MaximumStale", typeof (TimeSpan), TimeSpan.MinValue);
		static ConfigurationProperty minimumFresh = new ConfigurationProperty ("MinimumFresh", typeof (TimeSpan), TimeSpan.MinValue);
		static ConfigurationProperty policyLevel = new ConfigurationProperty ("PolicyLevel", typeof (HttpRequestCacheLevel), HttpRequestCacheLevel.Default);

		#endregion // Fields

		#region Constructors

		public HttpCachePolicyElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			properties.Add (maximumAge);
			properties.Add (maximumStale);
			properties.Add (minimumFresh);
			properties.Add (policyLevel);
		}

		#endregion // Constructors

		#region Properties

		public TimeSpan MaximumAge {
			get { return (TimeSpan) base [maximumAge]; }
			set { base [maximumAge] = value; }
		}

		public TimeSpan MaximumStale {
			get { return (TimeSpan) base [maximumStale]; }
			set { base [maximumStale] = value; }
		}

		public TimeSpan MinimumFresh {
			get { return (TimeSpan) base [minimumFresh]; }
			set { base [minimumFresh] = value; }
		}

		public HttpRequestCacheLevel PolicyLevel {
			get { return (HttpRequestCacheLevel) base [policyLevel]; }
			set { base [policyLevel] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected internal override void Deserialize (XmlReader reader, bool serializeCollectionKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Reset (ConfigurationElement parentElement, object context)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
