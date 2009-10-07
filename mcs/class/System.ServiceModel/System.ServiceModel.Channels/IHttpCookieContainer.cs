using System;
using System.Net;

namespace System.ServiceModel.Channels
{
	// SL-only interface.
	public interface IHttpCookieContainer
	{
		CookieContainer CookieContainer { get; set; }
	}
}
