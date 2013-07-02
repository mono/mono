//
// StandardBindingCollectionElement.cs
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

using SysConfig = System.Configuration.Configuration;

namespace System.ServiceModel.Configuration
{
	public class StandardBindingCollectionElement<TStandardBinding,TBindingConfiguration>
		 : BindingCollectionElement
		where TStandardBinding : Binding
		where TBindingConfiguration : StandardBindingElement, new()
	{
		ConfigurationPropertyCollection _properties;

		// Properties

		[ConfigurationProperty ("",
			 Options = ConfigurationPropertyOptions.IsDefaultCollection,
			IsDefaultCollection = true)]
		public StandardBindingElementCollection<TBindingConfiguration> Bindings {
			get { return (StandardBindingElementCollection<TBindingConfiguration>) this [String.Empty]; }
		}

		public override ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings {
			get {
				List<IBindingConfigurationElement> list = new List<IBindingConfigurationElement> ();
				StandardBindingElementCollection<TBindingConfiguration> bindings = Bindings;
				for (int i = 0; i < bindings.Count; i++)
					list.Add (bindings [i]);
				return new ReadOnlyCollection<IBindingConfigurationElement> (list);
			}
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = new ConfigurationPropertyCollection ();
					_properties.Add (new ConfigurationProperty (String.Empty, typeof (StandardBindingElementCollection<TBindingConfiguration>), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
				}
				return _properties;
			}
		}

		public override Type BindingType {
			get { return typeof (TStandardBinding); }
		}


		public override bool ContainsKey (string name)
		{
			return Bindings.ContainsKey (name);
		}

		protected internal override Binding GetDefault ()
		{
			return (Binding) Activator.CreateInstance (BindingType, new object [0]);
		}

		protected internal override bool TryAdd (string name, Binding binding, SysConfig config)
		{
			if (!binding.GetType ().Equals (typeof (TStandardBinding)))
				return false;

			var element = new TBindingConfiguration ();
			element.Name = name;
			element.InitializeFrom (binding);
			Bindings.Add (element);
			return true;
		}
	}

}
