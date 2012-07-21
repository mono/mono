using System.Web;


namespace System.Web.Configuration
{
	public abstract class HttpCapabilitiesProvider
	{
		protected HttpCapabilitiesProvider()
		{

		}


		public abstract HttpBrowserCapabilities GetBrowserCapabilities (HttpRequest request);
	}
}


