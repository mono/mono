// 
// System.Web.NoParamsInvoker - proxy used to invoke wired up events without parameters
//				as if they had parameters.
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

namespace System.Web
{
	delegate void NoParamsDelegate ();
	class NoParamsInvoker
	{
		EventHandler faked;
		NoParamsDelegate real;

		public NoParamsInvoker (object o, string method)
		{
			 real = (NoParamsDelegate) Delegate.CreateDelegate (
						typeof (NoParamsDelegate), o, method);
			 faked = new EventHandler (InvokeNoParams);
		}

		void InvokeNoParams (object o, EventArgs args)
		{
			real ();
		}

		public EventHandler FakeDelegate {
			get { return faked; }
		}
	}
}

