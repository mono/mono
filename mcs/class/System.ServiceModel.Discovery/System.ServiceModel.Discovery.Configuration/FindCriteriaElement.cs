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
using System.Xml;
using System.Xml.Linq;

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
		public Uri ScopeMatchBy {
			get { return (Uri) base [scope_match_by]; }
			set { base [scope_match_by] = value; }
		}
		
		[ConfigurationProperty ("scopes")]
		public ScopeElementCollection Scopes {
			get { return (ScopeElementCollection) base [scopes]; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		internal FindCriteria CreateInstance ()
		{
			var fc = new FindCriteria ();
			foreach (ContractTypeNameElement ctn in ContractTypeNames)
				fc.ContractTypeNames.Add (new XmlQualifiedName (ctn.Name, ctn.Namespace));
			fc.Duration = Duration;
			foreach (XmlElementElement ext in Extensions)
				fc.Extensions.Add (XElement.Load (new XmlNodeReader (ext.XmlElement)));
			fc.MaxResults = MaxResults;
			fc.ScopeMatchBy = ScopeMatchBy;
			foreach (ScopeElement scope in Scopes)
				fc.Scopes.Add (scope.Scope);
			return fc;
		}

		internal void CopyFrom (FindCriteriaElement other)
		{
			foreach (ContractTypeNameElement ctn in other.ContractTypeNames)
				ContractTypeNames.Add (new ContractTypeNameElement () { Name = ctn.Name, Namespace = ctn.Namespace });
			Duration = other.Duration;
			foreach (XmlElementElement ext in other.Extensions)
				Extensions.Add (new XmlElementElement () { XmlElement = (XmlElement) ext.XmlElement.CloneNode (true) });
			MaxResults = other.MaxResults;
			ScopeMatchBy = other.ScopeMatchBy;
			foreach (ScopeElement scope in other.Scopes)
				Scopes.Add (new ScopeElement () { Scope = scope.Scope });
		}

		internal void InitializeFrom (FindCriteria fc)
		{
			foreach (var ctn in fc.ContractTypeNames)
				ContractTypeNames.Add (new ContractTypeNameElement () { Name = ctn.Name, Namespace = ctn.Namespace});
			Duration = fc.Duration;
			var doc = new XmlDocument ();
			foreach (var ext in fc.Extensions) {
				var xr = ext.CreateReader ();
				xr.MoveToContent ();
				Extensions.Add (new XmlElementElement () { XmlElement = (XmlElement) doc.ReadNode (xr) });
			}
			MaxResults = fc.MaxResults;
			ScopeMatchBy = fc.ScopeMatchBy;
			foreach (var scope in fc.Scopes)
				Scopes.Add (new ScopeElement () { Scope = scope});
		}
	}
}

#endif
