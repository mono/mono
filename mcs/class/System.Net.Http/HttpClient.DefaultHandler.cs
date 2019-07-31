namespace System.Net.Http
{
	partial class HttpClient
	{
		static HttpMessageHandler CreateDefaultHandler()
		{
			return new HttpClientHandler();
		}
	}
}
