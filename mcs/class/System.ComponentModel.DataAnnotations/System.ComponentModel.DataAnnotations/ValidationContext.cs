//
// ValidationContext.cs
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. (http://novell.com)
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
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace System.ComponentModel.DataAnnotations
{
	public sealed class ValidationContext : IServiceProvider
	{
		public string DisplayName { get; set; }
		public IDictionary <object, object> Items { get; private set; }
		public string MemberName { get; set; }
		public object ObjectInstance { get; private set; }
		public Type ObjectType { get; private set; }
		public IServiceContainer ServiceContainer { get; private set; }
		
		public ValidationContext (object instance, IServiceProvider serviceProvider, IDictionary<object, object> items)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			
			ObjectInstance = instance;
			ObjectType = instance.GetType ();
			if (items != null)
				Items = new Dictionary <object, object> (items);
			else
				Items = new Dictionary <object, object> ();
			
			DisplayName = instance.GetType ().Name;

			// LAMESPEC: MSDN says vc.ServiceContainer should be initialized with the passed container if it implements 
			// the IServiceContainer interface - not the case, though.
			//
			// IServiceContainer container = serviceProvider as IServiceContainer;
			// if (container != null)
			// 	ServiceContainer = container;
			// else
			ServiceContainer = new ValidationContextServiceContainer ();
		}
		
#if NET_4_5
		public ValidationContext (object instance)
			: this (instance, null, null)
		{
		}
		
		public ValidationContext (object instance, IDictionary<object, object> items)
			: this (instance, null, items)
		{
		}

		// FIXME: According to MSDN, this should be defined in
		//        4.5, Silverlight and PCL
		[MonoTODO]
		public void InitializeServiceProvider (
			Func<Type, Object> serviceProvider)
		{
			throw new NotImplementedException ();
		}
#endif
		
		public object GetService (Type serviceType)
		{
			return ServiceContainer.GetService (serviceType);
		}

		sealed class ValidationContextServiceContainer : IServiceContainer
		{
			Dictionary <Type, object> services = new Dictionary <Type, object> ();
			
			public void AddService (Type serviceType, ServiceCreatorCallback callback, bool promote)
			{
				AddService (serviceType, (object)callback, promote);
			}

			public void AddService (Type serviceType, ServiceCreatorCallback callback)
			{
				AddService (serviceType, callback, false);
			}

			public void AddService (Type serviceType, object serviceInstance, bool promote)
			{
				if (serviceType == null)
					throw new ArgumentNullException ("serviceType");
				
				if (services.ContainsKey (serviceType))
					throw new ArgumentException (
						String.Format ("A service of type '{0}' already exists in the container.", serviceType)
					);

				services.Add (serviceType, serviceInstance);
			}

			public void AddService (Type serviceType, object serviceInstance)
			{
				AddService (serviceType, serviceInstance, false);
			}

			public void RemoveService (Type serviceType, bool promote)
			{
				if (serviceType == null)
					throw new ArgumentNullException ("serviceType");
				
				if (!services.ContainsKey (serviceType))
					return;

				services.Remove (serviceType);
			}

			public void RemoveService (Type serviceType)
			{
				RemoveService (serviceType, false);
			}

			public object GetService (Type serviceType)
			{
				if (serviceType == null)
					throw new ArgumentNullException ("serviceType");
				
				object o;
				if (!services.TryGetValue (serviceType, out o))
					return null;

				var cb = o as ServiceCreatorCallback;
				if (cb != null)
					return cb (this, serviceType);

				return o;
			}
		}
	}
}
