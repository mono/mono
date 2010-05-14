using System;
using System.Net;
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public class HttpCookieContainerBindingElement : BindingElement
	{
		HttpCookieContainerManager manager;

		public HttpCookieContainerBindingElement ()
		{
			manager = new HttpCookieContainerManager ();
		}

		protected HttpCookieContainerBindingElement (HttpCookieContainerBindingElement elementToBeCloned)
		{
			if (elementToBeCloned == null)
				throw new ArgumentNullException ("elementToBeCloned");

			manager = new HttpCookieContainerManager (elementToBeCloned.manager);
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			//context.RemainingBindingElements.Add (this);
			return base.BuildChannelFactory<TChannel> (context);
		}

		public override BindingElement Clone ()
		{
			return new HttpCookieContainerBindingElement (this);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (manager is T)
				return (T) (object) manager;
			return context.GetInnerProperty<T> ();
		}
	}

	class HttpCookieContainerManager : IHttpCookieContainerManager
	{
		public HttpCookieContainerManager ()
		{
			CookieContainer = new CookieContainer ();
		}

		public HttpCookieContainerManager (HttpCookieContainerManager original)
		{
			CookieContainer = original.CookieContainer;
		}

		public CookieContainer CookieContainer { get; set; }
	}
}
