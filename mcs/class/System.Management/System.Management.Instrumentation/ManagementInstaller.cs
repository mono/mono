//
// System.Management.Instrumentation.ManagementInstaller
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Configuration.Install;
using System.Collections;

namespace System.Management.Instrumentation
{
        public class ManagementInstaller : Installer {
		
		[MonoTODO]
		public ManagementInstaller()
		{
		}

		[MonoTODO]
		public override string HelpText {
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override void Commit (IDictionary savedState)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Install (IDictionary savedState)
	        {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Uninstall (IDictionary savedState)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ManagementInstaller()
		{
		}
	}
}
