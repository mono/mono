//
// System.Management.Instrumentation.InstrumentedAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class InstrumentedAttribute : Attribute {
		
		[MonoTODO]
		public InstrumentedAttribute()
		{
		}

		[MonoTODO]
		public InstrumentedAttribute (string namespaceName) : this(namespaceName, null)
		{
		}

		[MonoTODO]
		public InstrumentedAttribute (string namespaceName, string securityDescriptor)
		{
		}

		public string NamespaceName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public string SecurityDescriptor {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}
	}
}
