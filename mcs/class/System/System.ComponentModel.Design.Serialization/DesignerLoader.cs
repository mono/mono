//
// System.ComponentModel.Design.Serialization.DesignerLoader
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel.Design.Serialization
{
	public abstract class DesignerLoader
	{
		[MonoTODO]
		protected DesignerLoader()
		{
		}

		public virtual bool Loading {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public abstract void BeginLoad (IDesignerLoaderHost host);
		public abstract void Dispose();

		[MonoTODO]
		public virtual void Flush()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~DesignerLoader()
		{
		}
	}
}
