//
// System.ComponentModel.Design.ServiceContainer
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design
{
	public sealed class ServiceContainer : IServiceContainer,
                                               IServiceProvider
	{
		[MonoTODO]
		public ServiceContainer()
		{
		}

		[MonoTODO]
		public ServiceContainer (IServiceProvider parentProvider)
		{
		}

		[MonoTODO]
		public void AddService (Type serviceType,
					object serviceInstance)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void AddService (Type serviceType,
					ServiceCreatorCallback callback)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void AddService (Type serviceType, 
					object serviceInstance,
					bool promote)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void AddService (Type serviceType,
					ServiceCreatorCallback callback,
					bool promote)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void RemoveService (Type serviceType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void RemoveService (Type serviceType,
					   bool promote)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public object GetService (Type serviceType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ServiceContainer()
		{
		}
	}
}
