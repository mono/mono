//
// System.ComponentModel.Design.Serialization.DesignerLoader.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel.Design.Serialization
{
	// This class is merely an interface with no implementation needed
	public abstract class DesignerLoader
	{

		protected DesignerLoader()
		{
		}

		public virtual bool Loading {
			get { return false; }
		}

		public abstract void BeginLoad (IDesignerLoaderHost host);
		public abstract void Dispose();

		public virtual void Flush()
		{
		}
	}
}
