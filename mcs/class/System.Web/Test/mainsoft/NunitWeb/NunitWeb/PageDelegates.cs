#if NET_2_0
using System;

namespace NunitWeb
{
	[Serializable]
	public struct PageDelegates
	{
		public Helper.AnyMethod MyHandlerCallback;
		public Helper.AnyMethodInPage LoadComplete;
		public Helper.AnyMethodInPage PreInit;
		public Helper.AnyMethodInPage PreLoad;
		public Helper.AnyMethodInPage PreRenderComplete;
		public Helper.AnyMethodInPage InitComplete;
		public Helper.AnyMethodInPage SaveStateComplete;
		public Helper.AnyMethodInPage CommitTransaction;
		public Helper.AnyMethodInPage AbortTransaction;
		public Helper.AnyMethodInPage Error;
		public Helper.AnyMethodInPage Disposed;
		public Helper.AnyMethodInPage DataBinding;
		public Helper.AnyMethodInPage Init;
		public Helper.AnyMethodInPage Load;
		public Helper.AnyMethodInPage PreRender;
		public Helper.AnyMethodInPage Unload;
		public object Param;
	}
}
#endif
