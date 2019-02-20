using System.Diagnostics.Tracing;
using System.Net.Http;

namespace System.Net
{
	internal sealed partial class NetEventSource : EventSource
	{
		public static NetEventSource Log => throw new PlatformNotSupportedException ();

		public static new bool IsEnabled => false;

		public static void Associate (params object[] args)
		{
		}

		[NonEvent]
		public static void Error (params object[] args)
		{
		}

		public static void Enter (params object[] args)
		{

		}

		public static void Exit (params object[] args)
		{
		}

		public static void Info (params object[] args)
		{
		}

		[NonEvent]
		public static void UriBaseAddress (object obj, Uri baseAddress)
		{
		}

		[NonEvent]
		public static void ContentNull (object obj)
		{
		}

		[NonEvent]
		public static void ClientSendCompleted (HttpClient httpClient, HttpResponseMessage response, HttpRequestMessage request)
		{
		}

		public void HeadersInvalidValue (string name, string rawValue)
		{
		}

		public void HandlerMessage (int handlerId, int workerId, int requestId, string memberName, string message)
		{
		}
	}
}
