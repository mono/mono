//
// System.ComponentModel.Design.Serialization.InstanceDescriptor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;
using System.Reflection;

namespace System.ComponentModel.Design.Serialization
{
	public sealed class InstanceDescriptor
	{
		[MonoTODO]
		public InstanceDescriptor (MemberInfo info, 
					   ICollection collection)
		{
		}

		[MonoTODO]
		public InstanceDescriptor(MemberInfo info, 
					  ICollection coolection, 
					  bool boolean)
		{
			throw new NotImplementedException();
		}

		public ICollection Arguments {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public bool IsComplete {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public MemberInfo MemberInfo {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public object Invoke()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~InstanceDescriptor()
		{
		}
	}
}
