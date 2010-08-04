//
// WebMessageEncodingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public sealed partial class WebMessageEncodingElement
		 : BindingElementExtensionElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty binding_element_type;
		static ConfigurationProperty max_read_pool_size;
		static ConfigurationProperty max_write_pool_size;
		static ConfigurationProperty reader_quotas;
		static ConfigurationProperty write_encoding;
		static ConfigurationProperty web_content_type_mapper_type;

		static WebMessageEncodingElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			binding_element_type = new ConfigurationProperty ("",
				typeof (Type), null, new TypeConverter (), null,
				ConfigurationPropertyOptions.None);

			max_read_pool_size = new ConfigurationProperty ("maxReadPoolSize",
				typeof (int), "64", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_write_pool_size = new ConfigurationProperty ("maxWritePoolSize",
				typeof (int), "16", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			reader_quotas = new ConfigurationProperty ("readerQuotas",
				typeof (XmlDictionaryReaderQuotasElement), null, null/* FIXME: get converter for XmlDictionaryReaderQuotasElement*/, null,
				ConfigurationPropertyOptions.None);

			write_encoding = new ConfigurationProperty ("writeEncoding",
				typeof (Encoding), "utf-8", null/* FIXME: get converter for Encoding*/, null,
				ConfigurationPropertyOptions.None);

			web_content_type_mapper_type = new ConfigurationProperty ("",
				typeof (string), null, null /* FIXME: supply */, null,
				ConfigurationPropertyOptions.None);

			properties.Add (binding_element_type);
			properties.Add (max_read_pool_size);
			properties.Add (max_write_pool_size);
			properties.Add (reader_quotas);
			properties.Add (write_encoding);
			properties.Add (web_content_type_mapper_type);
		}

		public WebMessageEncodingElement ()
		{
		}


		// Properties

		public override Type BindingElementType {
			get { return (Type) base [binding_element_type]; }
		}

		[ConfigurationProperty ("maxReadPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "64")]
		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxReadPoolSize {
			get { return (int) base [max_read_pool_size]; }
			set { base [max_read_pool_size] = value; }
		}

		[ConfigurationProperty ("maxWritePoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "16")]
		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxWritePoolSize {
			get { return (int) base [max_write_pool_size]; }
			set { base [max_write_pool_size] = value; }
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) base [reader_quotas]; }
		}

		[ConfigurationProperty ("writeEncoding",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "utf-8")]
		[TypeConverter ()]
		public Encoding WriteEncoding {
			get { return (Encoding) base [write_encoding]; }
			set { base [write_encoding] = value; }
		}

		[ConfigurationProperty ("webContentTypeMapperType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue)]
		public string WebContentTypeMapperType {
			get { return (string) base [web_content_type_mapper_type]; }
			set { base [web_content_type_mapper_type] = value; }
		}

		protected internal override BindingElement CreateBindingElement ()
		{
			var be = new WebMessageEncodingBindingElement ();
			ApplyConfiguration (be);
			return be;
		}
		
		public override void ApplyConfiguration (BindingElement bindingElement)
		{
			base.ApplyConfiguration (bindingElement);
			var b = (WebMessageEncodingBindingElement) bindingElement;
			b.ContentTypeMapper = (WebContentTypeMapper) Activator.CreateInstance (Type.GetType (WebContentTypeMapperType), true);
			b.MaxReadPoolSize = MaxReadPoolSize;
			b.MaxWritePoolSize = MaxWritePoolSize;
			b.WriteEncoding = WriteEncoding;
			ReaderQuotas.ApplyConfiguration (b.ReaderQuotas);
		}

		public override void CopyFrom (ServiceModelExtensionElement from)
		{
			base.CopyFrom (from);
			var c = (WebMessageEncodingElement) from;
			MaxReadPoolSize = c.MaxReadPoolSize;
			MaxWritePoolSize = c.MaxWritePoolSize;
			ReaderQuotas.CopyFrom (c.ReaderQuotas);
			WriteEncoding = c.WriteEncoding;
		}
	}
	
	static class Extensions
	{
		public static void ApplyConfiguration (this XmlDictionaryReaderQuotasElement e, XmlDictionaryReaderQuotas q)
		{
			q.MaxArrayLength = e.MaxArrayLength;
			q.MaxBytesPerRead = e.MaxBytesPerRead;
			q.MaxDepth = e.MaxDepth;
			q.MaxNameTableCharCount = e.MaxNameTableCharCount;
			q.MaxStringContentLength = e.MaxStringContentLength;
		}

		public static void CopyFrom (this XmlDictionaryReaderQuotasElement e, XmlDictionaryReaderQuotasElement o)
		{
			e.MaxArrayLength = o.MaxArrayLength;
			e.MaxBytesPerRead = o.MaxBytesPerRead;
			e.MaxDepth = o.MaxDepth;
			e.MaxNameTableCharCount = o.MaxNameTableCharCount;
			e.MaxStringContentLength = o.MaxStringContentLength;
		}
	}
}
