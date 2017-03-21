//
// TextMessageEncodingElement.cs
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
	public sealed class TextMessageEncodingElement
		 : BindingElementExtensionElement
	{
		public TextMessageEncodingElement () {
		}


		// Properties

		public override Type BindingElementType {
			get { return typeof (TextMessageEncodingBindingElement); }
		}

		[ConfigurationProperty ("maxReadPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "64")]
		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxReadPoolSize {
			get { return (int) base ["maxReadPoolSize"]; }
			set { base ["maxReadPoolSize"] = value; }
		}

		[ConfigurationProperty ("maxWritePoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "16")]
		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxWritePoolSize {
			get { return (int) base ["maxWritePoolSize"]; }
			set { base ["maxWritePoolSize"] = value; }
		}

		[ConfigurationProperty ("messageVersion",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Soap12WSAddressing10")]
		[TypeConverter (typeof (MessageVersionConverter))]
		public MessageVersion MessageVersion {
			get { return (MessageVersion) base ["messageVersion"]; }
			set { base ["messageVersion"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) base ["readerQuotas"]; }
		}

		[ConfigurationProperty ("writeEncoding",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "utf-8")]
		[TypeConverter (typeof (EncodingConverter))]
		public Encoding WriteEncoding {
			get { return (Encoding) base ["writeEncoding"]; }
			set { base ["writeEncoding"] = value; }
		}


		protected internal override BindingElement CreateBindingElement ()
		{
			return new TextMessageEncodingBindingElement ();
		}


		public override void ApplyConfiguration (BindingElement bindingElement)
		{
			var b = (TextMessageEncodingBindingElement) bindingElement;
			b.MaxReadPoolSize = MaxReadPoolSize;
			b.MaxWritePoolSize = MaxWritePoolSize;
			b.MessageVersion = MessageVersion;
			b.WriteEncoding = WriteEncoding;

			ReaderQuotas.ApplyConfiguration (b.ReaderQuotas);
		}

		public override void CopyFrom (ServiceModelExtensionElement from)
		{
			var b = (TextMessageEncodingElement) from;
			MaxReadPoolSize = b.MaxReadPoolSize;
			MaxWritePoolSize = b.MaxWritePoolSize;
			MessageVersion = b.MessageVersion;
			WriteEncoding = b.WriteEncoding;

			ReaderQuotas.CopyFrom (b.ReaderQuotas);
		}

		protected internal override void InitializeFrom (BindingElement bindingElement)
		{
			var b = (TextMessageEncodingBindingElement) bindingElement;
			MaxReadPoolSize = b.MaxReadPoolSize;
			MaxWritePoolSize = b.MaxWritePoolSize;
			MessageVersion = b.MessageVersion;
			WriteEncoding = b.WriteEncoding;

			ReaderQuotas.InitializeFrom (b.ReaderQuotas);
		}
	}

}
