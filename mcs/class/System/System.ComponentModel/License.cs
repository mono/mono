//
// System.ComponentModel.License
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public abstract class License : IDisposable
	{
		[MonoTODO]
		protected License()
		{
		}

		public abstract string LicenseKey { get; }
		public abstract void Dispose();

		[MonoTODO]
		~License()
		{
		}
	}
}
