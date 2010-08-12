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
	public sealed class FindCriteriaElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty types, duration, extensions, max_results, scope_match_by, scopes;
		
		static FindCriteriaElement ()
		{
			types = new ConfigurationProperty ("types", typeof (ContractTypeNameElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			duration = new ConfigurationProperty ("duration", typeof (TimeSpan), "00:00:20", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			extensions = new ConfigurationProperty ("extensions", typeof (XmlElementElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			max_results = new ConfigurationProperty ("maxResults", typeof (TimeSpan), "00:00:20", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			scope_match_by = new ConfigurationProperty ("scopeMatchBy", typeof (Uri), null, null, null, ConfigurationPropertyOptions.None);
			scopes = new ConfigurationProperty ("scopes", typeof (ScopeElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (types);
			properties.Add (duration);
			properties.Add (extensions);
			properties.Add (max_results);
			properties.Add (scope_match_by);
			properties.Add (scopes);
		}

		public FindCriteriaElement ()
		{
		}

		[ConfigurationProperty ("types")]
		public ContractTypeNameElementCollection ContractTypeNames {
			get { return (ContractTypeNameElementCollection) base [types]; }
		}
		
		[ConfigurationProperty ("duration", DefaultValue = "00:00:20")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan Duration {
			get { return (TimeSpan) base [duration]; }
			set { base [duration] = value; }
		}
		
		[ConfigurationProperty ("extensions")]
		public XmlElementElementCollection Extensions {
			get { return (XmlElementElementCollection) base [extensions]; }
		}
		
		[ConfigurationProperty ("maxResults", DefaultValue = 0)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int MaxResults  {
			get { return (int) base [max_results]; }
			set { base [max_results] = value; }
		}
		
		[ConfigurationProperty ("scopeMatchBy")]
		[TypeConverter (typeof (UriTypeConverter))]
		public Uri ScopeMatchBy {
			get { return (Uri) base [scope_match_by]; }
			set { base [scope_match_by] = value; }
		}
		
		[ConfigurationProperty ("scopes")]
		public ScopeElementCollection Scopes {
			get { return (ScopeElementCollection) base [scopes]; }
		}
	}
}

#endif
