//
// System.ComponentModel.Design.ServiceContainer.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// (C) 2006 Ivan N. Zlatev

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

namespace System.ComponentModel.Design
{
	public class ServiceContainer : IServiceContainer, IServiceProvider, IDisposable
	{
		private IServiceProvider parentProvider;
		private Hashtable services;
		private bool _disposed;
		
		public ServiceContainer()
			: this (null)
		{
		}

		private Hashtable Services {
			get {
				if (services == null)
					services = new Hashtable ();
				return services;
			}
		}

		public ServiceContainer (IServiceProvider parentProvider)
		{
			this.parentProvider = parentProvider;
		}

		public void AddService (Type serviceType, object serviceInstance)
		{
			AddService (serviceType, serviceInstance, false);
		}

		public void AddService (Type serviceType, ServiceCreatorCallback callback)
		{
			AddService (serviceType, callback, false);
		}

		public virtual void AddService (Type serviceType,
					object serviceInstance,
					bool promote)
		{
			if (promote && parentProvider != null) {
				IServiceContainer container = (IServiceContainer)
					parentProvider.GetService (typeof (IServiceContainer));
				container.AddService (serviceType, serviceInstance, promote);
				return;
			}

			if (serviceType == null)
				throw new ArgumentNullException ("serviceType");
			if (serviceInstance == null)
				throw new ArgumentNullException ("serviceInstance");
			if (Services.Contains (serviceType))
				throw new ArgumentException (string.Format (
					"The service {0} already exists in the service container.",
					serviceType.ToString ()), "serviceType");
			Services.Add (serviceType, serviceInstance);
		}

		public virtual
		void AddService (Type serviceType,
					ServiceCreatorCallback callback,
					bool promote)
		{
			if (promote && parentProvider != null) {
				IServiceContainer container = (IServiceContainer)
					parentProvider.GetService (typeof (IServiceContainer));
				container.AddService (serviceType, callback, promote);
				return;
			}

			if (serviceType == null)
				throw new ArgumentNullException ("serviceType");
			if (callback == null)
				throw new ArgumentNullException ("callback");
			if (Services.Contains (serviceType))
				throw new ArgumentException (string.Format (
					"The service {0} already exists in the service container.",
					serviceType.ToString ()), "serviceType");
			Services.Add (serviceType, callback);
		}

		public void RemoveService (Type serviceType)
		{
			RemoveService (serviceType, false);
		}

		public virtual void RemoveService (Type serviceType, bool promote)
		{
			if (promote && parentProvider != null) {
				IServiceContainer container = (IServiceContainer)
					parentProvider.GetService (typeof (IServiceContainer));
				container.RemoveService (serviceType, promote);
				return;
			}

			if (serviceType == null)
				throw new ArgumentNullException ("serviceType");
			Services.Remove (serviceType);
		}

		public virtual
		object GetService (Type serviceType)
		{
			object result = null;

			Type[] defaultServices = this.DefaultServices;
			for (int i=0; i < defaultServices.Length; i++) {
				if (defaultServices[i] == serviceType) {
					result = this;
					break;
				}
			}

			if (result == null)
				result = Services [serviceType];
			if (result == null && parentProvider != null)
				result = parentProvider.GetService (serviceType);
			if (result != null) {
				ServiceCreatorCallback cb = result as ServiceCreatorCallback;
				if (cb != null) {
					result = cb (this, serviceType);
					Services [serviceType] = result;
				}
			}
			return result;
		}

		protected virtual
		Type [] DefaultServices {
			get {
				return new Type [] { typeof (IServiceContainer), typeof (ServiceContainer)};
			}
		}

		public void Dispose ()
		{
			this.Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
					if (services != null) {
						foreach (object obj in services) {
							if (obj is IDisposable) {
								((IDisposable) obj).Dispose ();
							}
						}
						services = null;
					}
				}
				_disposed = true;
			}
		}
	}
}
