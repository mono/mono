//
// XmlDictionaryReaderQuotasElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	[MonoTODO]
	public sealed partial class XmlDictionaryReaderQuotasElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty max_array_length;
		static ConfigurationProperty max_bytes_per_read;
		static ConfigurationProperty max_depth;
		static ConfigurationProperty max_name_table_char_count;
		static ConfigurationProperty max_string_content_length;

		static XmlDictionaryReaderQuotasElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			max_array_length = new ConfigurationProperty ("maxArrayLength",
				typeof (int), "0", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_bytes_per_read = new ConfigurationProperty ("maxBytesPerRead",
				typeof (int), "0", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_depth = new ConfigurationProperty ("maxDepth",
				typeof (int), "0", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_name_table_char_count = new ConfigurationProperty ("maxNameTableCharCount",
				typeof (int), "0", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_string_content_length = new ConfigurationProperty ("maxStringContentLength",
				typeof (int), "0", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (max_array_length);
			properties.Add (max_bytes_per_read);
			properties.Add (max_depth);
			properties.Add (max_name_table_char_count);
			properties.Add (max_string_content_length);
		}

		public XmlDictionaryReaderQuotasElement ()
		{
		}


		// Properties

		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxArrayLength",
			 DefaultValue = "0",
			 Options = ConfigurationPropertyOptions.None)]
		public int MaxArrayLength {
			get { return (int) base [max_array_length]; }
			set { base [max_array_length] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBytesPerRead",
			 DefaultValue = "0",
			 Options = ConfigurationPropertyOptions.None)]
		public int MaxBytesPerRead {
			get { return (int) base [max_bytes_per_read]; }
			set { base [max_bytes_per_read] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxDepth",
			 DefaultValue = "0",
			 Options = ConfigurationPropertyOptions.None)]
		public int MaxDepth {
			get { return (int) base [max_depth]; }
			set { base [max_depth] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxNameTableCharCount",
			 DefaultValue = "0",
			 Options = ConfigurationPropertyOptions.None)]
		public int MaxNameTableCharCount {
			get { return (int) base [max_name_table_char_count]; }
			set { base [max_name_table_char_count] = value; }
		}

		[ConfigurationProperty ("maxStringContentLength",
			 DefaultValue = "0",
			 Options = ConfigurationPropertyOptions.None)]
		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxStringContentLength {
			get { return (int) base [max_string_content_length]; }
			set { base [max_string_content_length] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		internal XmlDictionaryReaderQuotas Create ()
		{
			var q =  new XmlDictionaryReaderQuotas ();
			if (MaxArrayLength > 0)
				q.MaxArrayLength = MaxArrayLength;
			if (MaxBytesPerRead > 0)
				q.MaxBytesPerRead = MaxBytesPerRead;
			if (MaxDepth > 0)
				q.MaxDepth = MaxDepth;
			if (MaxNameTableCharCount > 0)
				q.MaxNameTableCharCount = MaxNameTableCharCount;
			if (MaxStringContentLength > 0)
				q.MaxStringContentLength = MaxStringContentLength;
			return q;
		}
	}

}
