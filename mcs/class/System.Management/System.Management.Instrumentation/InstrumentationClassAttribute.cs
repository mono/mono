//
// System.Management.Instrumentation.InstrumentationClassAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | 
			AttributeTargets.Struct)]
        public class InstrumentationClassAttribute : Attribute {
		
		[MonoTODO]
		public InstrumentationClassAttribute()
		{
		}

		public InstrumentationType InstrumentationType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public string ManagedBaseClassName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~InstrumentationClassAttribute()
		{
		}
	}
}
