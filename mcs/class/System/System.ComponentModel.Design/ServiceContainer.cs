//
// System.ComponentModel.Design.ServiceContainer.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

using System.Collections;

namespace System.ComponentModel.Design
{
	public sealed class ServiceContainer : IServiceContainer, IServiceProvider
	{

		private IServiceProvider parentProvider;
		private Hashtable services = new Hashtable ();

		public ServiceContainer()
			: this (null)
		{
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

		public void AddService (Type serviceType, 
					object serviceInstance,
					bool promote)
		{
			if (serviceType == null)
				throw new ArgumentNullException ("serviceType", "Cannot be null");
			if (promote)
				if (parentProvider != null)
					((IServiceContainer)parentProvider.GetService(typeof(IServiceContainer))).AddService (serviceType, serviceInstance, promote);
			if (services.Contains (serviceType)) {
					throw new ArgumentException (string.Format ("The service {0} already exists in the service container.", serviceType.ToString()));
			}
			services.Add (serviceType, serviceInstance);
		}

		public void AddService (Type serviceType,
					ServiceCreatorCallback callback,
					bool promote)
		{
			if (serviceType == null)
				throw new ArgumentNullException ("serviceType", "Cannot be null");
			if (promote)
				if (parentProvider != null)
					((IServiceContainer)parentProvider.GetService(typeof(IServiceContainer))).AddService (serviceType, callback, promote);
			if (services.Contains (serviceType)) {
					throw new ArgumentException (string.Format ("The service {0} already exists in the service container.", serviceType.ToString()));
			}
			services.Add (serviceType, callback);
		}

		public void RemoveService (Type serviceType)
		{
			RemoveService (serviceType, false);
		}

		public void RemoveService (Type serviceType, bool promote)
		{
			if (serviceType == null)
				throw new ArgumentNullException ("serviceType", "Cannot be null");
			if (promote)
				if (parentProvider != null)
					((IServiceContainer)parentProvider.GetService(typeof(IServiceContainer))).RemoveService (serviceType, promote);
			else
				services.Remove (serviceType);
		}

		public object GetService (Type serviceType)
		{
			object result = services[serviceType];
			if (result == null && parentProvider != null){
				result = parentProvider.GetService (serviceType);
			}
			if (result != null) {
				ServiceCreatorCallback	cb = result as ServiceCreatorCallback;
				if (cb != null) {
					result = cb (this, serviceType);
					services[serviceType] = result;
				}
				
			}
			return result;
		}
	}
}
