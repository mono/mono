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
        public abstract class Instance : IInstance {
		bool published;

		public bool Published {
			get { return published; }
			set { published = value; }
		}

		[MonoTODO]
		protected Instance()
		{

		}

		[MonoTODO]
		~Instance()
		{
		}
	}
}
