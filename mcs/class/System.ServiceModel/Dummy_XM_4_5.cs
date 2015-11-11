using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	public class InstanceContext
	{
		public InstanceContext (object dummy) {}
	}
}

namespace System.ServiceModel.PeerResolvers
{
	class Dummy {}
}

namespace System.ServiceModel.Configuration
{
	class Dummy {}
}
namespace System.ServiceModel.Channels.Http
{
}
namespace System.ServiceModel.Channels.NetTcp
{
}

namespace System.ServiceModel.Dispatcher
{
	public sealed class EndpointDispatcher
	{
		internal EndpointDispatcher ()
		{
		}
	}
}

namespace System.ServiceModel.Channels
{
	public static class UrlUtility {
		public static string UrlEncode (string s, Encoding e)
		{
			return System.Runtime.UrlUtility.UrlEncode (s, e);
		}

		public static string UrlDecode (string s, Encoding e)
		{
			return System.Runtime.UrlUtility.UrlDecode (s, e);
		}
	}
}

namespace System.ServiceModel.Description
{
	public interface IPolicyExportExtension {}
	public interface IPolicyImportExtension {}
	public interface IWsdlExportExtension {}
	public interface IWsdlImportExtension {}
}
namespace System.ServiceModel.Channels
{
	public interface ITransportTokenAssertionProvider {}
}