//
// System.ComponentModel.Design.InheritanceService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Reflection;

namespace System.ComponentModel.Design
{
	public class InheritanceService : IInheritanceService, IDisposable
	{
		[MonoTODO]
		public InheritanceService()
		{
		}

		[MonoTODO]
		public void AddInheritedComponents (IComponent component,
						    IContainer container)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void AddInheritedComponents (Type type,
							       IComponent component,
							       IContainer container)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public InheritanceAttribute GetInheritanceAttribute (IComponent component)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual bool IgnoreInheritedMember (MemberInfo member,
							      IComponent component)
		{
			throw new NotImplementedException();
		}


		[MonoTODO]
		~InheritanceService()
		{
		}

	}
}
