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
