namespace System.Net.Http
{
	public partial class HttpClient
	{
		static HttpMessageHandler CreateDefaultHandler () => new WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler ();
	}
}
