//
// System.Management.Instrumentation.Instance
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{
	[InstrumentationClass (InstrumentationType.Instance)]
	public abstract class Instance : IInstance {
		bool published;

		[MonoTODO]
		protected Instance ()
		{
		}

		[IgnoreMember]
		public bool Published {
			get { return published; }
			set { published = value; }
		}
	}
}
