using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.CompilerServices
{
	// introduced for silverlight sdk compatibility
	internal class FriendAccessAllowedAttribute : Attribute
	{
		public FriendAccessAllowedAttribute ()
		{
		}
	}
}

namespace System.ServiceModel
{
	public class EndpointIdentity {}
	public class InstanceContext
	{
		public InstanceContext (object dummy) {}
	}
	// introduced for silverlight sdk compatibility
	internal class OperationFormatStyleHelper
	{
		public static bool IsDefined (OperationFormatStyle style)
		{
			switch (style) {
			case OperationFormatStyle.Document:
			case OperationFormatStyle.Rpc:
				return true;
			}
			return false;
		}
	}
}
namespace System.ServiceModel.Channels
{
	public interface ITransportTokenAssertionProvider {}
}
namespace System.ServiceModel.Channels.Http
{
}
namespace System.ServiceModel.Channels.Security
{
}
namespace System.ServiceModel.Configuration
{
	class Dummy {}
}
namespace System.ServiceModel.Description
{
	public interface IPolicyExportExtension {}
	public interface IPolicyImportExtension {}
	public interface IWsdlExportExtension {}
	public interface IWsdlImportExtension {}

	// introduced for silverlight sdk compatibility
	internal class ServiceReflector
	{
		public static T GetSingleAttribute<T> (ICustomAttributeProvider p, Type [] types)
		{
			T ret = default (T);
			foreach (Type t in types) {
				foreach (object att in p.GetCustomAttributes (t, false)) {
					if (att is T) {
						if (ret != null)
							throw new InvalidOperationException (String.Format ("More than one {0} attributes are found in the argument types", typeof (T)));
						ret = (T) att;
					}
				}
			}
			return ret;
		}
	}
}
namespace System.ServiceModel.DiagnosticUtility
{
	// introduced for silverlight sdk compatibility
	internal class ExceptionUtility
	{
		public static Exception ThrowHelperError (Exception error)
		{
			return error;
		}

		public static Exception ThrowHelperArgumentNull (string arg)
		{
			return new ArgumentNullException (arg);
		}
	}
}
namespace System.ServiceModel.Dispatcher
{
	public class EndpointDispatcher
	{
		internal EndpointDispatcher ()
		{
		}
	}
}
namespace System.ServiceModel.Security
{
	class Dummy {}
}
namespace System.Net.Security
{
	public enum ProtectionLevel {None}
}
namespace System.Xml.Serialization
{
	public class XmlTypeMapping {}
}
namespace System.Xml.XPath
{
	class Dummy {}
}
namespace Mono.Xml.XPath
{
	class Dummy {}
}

