namespace System.Net.Test.Common
{
	public static partial class Configuration
	{
		public static partial class Http
		{
			public static readonly object[][] EchoServers = { new object[] { RemoteEchoServer }, new object[] { SecureRemoteEchoServer } };
			public static readonly object[][] VerifyUploadServers = { new object[] { RemoteVerifyUploadServer }, new object[] { SecureRemoteVerifyUploadServer } };
			public static readonly object[][] CompressedServers = { new object[] { RemoteDeflateServer }, new object[] { RemoteGZipServer } };
			public static readonly object[][] Http2Servers = { new object[] { new Uri ("https://" + Http2Host) } };
			public static readonly object[][] Http2NoPushServers = { new object[] { new Uri ("https://" + Http2NoPushHost) } };
		}
	}
}
