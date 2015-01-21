using System;
using System.Net;

namespace System.ServiceModel.Channels
{
	// SL-only interface.
	public interface IHttpCookieContainerManager
	{
		CookieContainer CookieContainer { get; set; }
	}
}
