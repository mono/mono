//
// CustomBindingElement.cs
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
using System.Diagnostics;

namespace System.ServiceModel.Configuration
{
	public class CustomBindingElement
		 : NamedServiceModelExtensionCollectionElement<BindingElementExtensionElement>, ICollection<BindingElementExtensionElement>, IEnumerable<BindingElementExtensionElement>, IEnumerable, IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		public CustomBindingElement () {
		}

		public CustomBindingElement (string name) {
			Name = name;
		}

		// Properties

		[ConfigurationProperty ("closeTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:01:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan CloseTimeout {
			get { return (TimeSpan) base ["closeTimeout"]; }
			set { base ["closeTimeout"] = value; }
		}

		[ConfigurationProperty ("openTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:01:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan OpenTimeout {
			get { return (TimeSpan) base ["openTimeout"]; }
			set { base ["openTimeout"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("closeTimeout", typeof (TimeSpan), "00:01:00", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("openTimeout", typeof (TimeSpan), "00:01:00", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("receiveTimeout", typeof (TimeSpan), "00:10:00", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("sendTimeout", typeof (TimeSpan), "00:01:00", null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("receiveTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:10:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan ReceiveTimeout {
			get { return (TimeSpan) base ["receiveTimeout"]; }
			set { base ["receiveTimeout"] = value; }
		}

		[ConfigurationProperty ("sendTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:01:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan SendTimeout {
			get { return (TimeSpan) base ["sendTimeout"]; }
			set { base ["sendTimeout"] = value; }
		}

		[MonoTODO ("what to reject?")]
		public override void Add (BindingElementExtensionElement element)
		{
			base.Add (element);
		}

		[MonoTODO ("what to reject?")]
		public override bool CanAdd (BindingElementExtensionElement element)
		{
			return true;
		}

		public void ApplyConfiguration (Binding binding)
		{
			OnApplyConfiguration (binding);
		}

		[MonoTODO ("implement using EvaluationContext")]
		internal override BindingElementExtensionElement DeserializeExtensionElement (string elementName, XmlReader reader) {
			//ExtensionElementCollection extensions = ((ExtensionsSection) EvaluationContext.GetSection ("system.serviceModel/extensions")).BindingElementExtensions;
			ExtensionElementCollection extensions = ConfigUtil.ExtensionsSection.BindingElementExtensions;

			ExtensionElement extension = extensions [elementName];
			if (extension == null)
				throw new ConfigurationErrorsException ("Invalid element in configuration. The extension name '" + reader.LocalName + "' is not registered in the collection at system.serviceModel/extensions/bindingElementExtensions");

			BindingElementExtensionElement element = (BindingElementExtensionElement) Activator.CreateInstance (Type.GetType (extension.Type));
			element.DeserializeElementInternal (reader, false);
			return element;
		}

		protected void OnApplyConfiguration (Binding binding)
		{
			if (binding == null)
				throw new ArgumentNullException ("binding");
			var b = (CustomBinding) binding;
			b.CloseTimeout = CloseTimeout;
			b.OpenTimeout = OpenTimeout;
			b.ReceiveTimeout = ReceiveTimeout;
			b.SendTimeout = SendTimeout;

			foreach (var be in this)
				b.Elements.Add (be.CreateBindingElement ());
		}

		internal void InitializeFrom (Binding binding)
		{
			CloseTimeout = binding.CloseTimeout;
			OpenTimeout = binding.OpenTimeout;
			ReceiveTimeout = binding.ReceiveTimeout;
			SendTimeout = binding.SendTimeout;
		}
	}

}
