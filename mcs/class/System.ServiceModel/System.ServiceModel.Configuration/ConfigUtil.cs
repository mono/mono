//
// ConfigUtil.cs
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
using System.Configuration;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Configuration;

namespace System.ServiceModel.Configuration
{
	internal static class ConfigUtil
	{
		static object GetSection (string name)
		{
			if (ServiceHostingEnvironment.InAspNet)
				return WebConfigurationManager.GetSection (name);
			else
				return ConfigurationManager.GetSection (name);
		}

		public static BindingsSection BindingsSection {
			get {
				return (BindingsSection) GetSection ("system.serviceModel/bindings");
			}
		}

		public static ClientSection ClientSection {
			get { return (ClientSection) GetSection ("system.serviceModel/client"); }
		}

		public static ServicesSection ServicesSection {
			get { return (ServicesSection) GetSection ("system.serviceModel/services"); }
		}

		public static BehaviorsSection BehaviorsSection {
			get { return (BehaviorsSection) GetSection ("system.serviceModel/behaviors"); }
		}

		public static ExtensionsSection ExtensionsSection {
			get { return (ExtensionsSection) GetSection ("system.serviceModel/extensions"); }
		}

		public static Binding CreateBinding (string binding, string bindingConfiguration)
		{
			BindingCollectionElement section = ConfigUtil.BindingsSection [binding];
			if (section == null)
				throw new ArgumentException (String.Format ("binding section for {0} was not found.", binding));

			Binding b = (Binding) Activator.CreateInstance (section.BindingType, new object [0]);

			foreach (IBindingConfigurationElement el in section.ConfiguredBindings)
				if (el.Name == bindingConfiguration)
					el.ApplyConfiguration (b);

			return b;
		}
	}
}
