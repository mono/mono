#if !XAMMAC_4_5
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Threading;

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
	public class InstanceContext : CommunicationObject, IExtensibleObject<InstanceContext>
	{
		protected internal override TimeSpan DefaultCloseTimeout
		{
			get { throw new NotImplementedException (); }
		}

		protected internal override TimeSpan DefaultOpenTimeout
		{
			get { throw new NotImplementedException (); }
		}

		IExtensionCollection<InstanceContext> IExtensibleObject<InstanceContext>.Extensions
		{
			get { throw new NotImplementedException (); }
		}

		public SynchronizationContext SynchronizationContext {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public InstanceContext (object implementation)
		{
			throw new NotImplementedException ();
		}

		public object GetServiceInstance (Message message)
		{
			throw new NotImplementedException ();
		}

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClosed ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnFaulted ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpened ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpening ()
		{
			throw new NotImplementedException ();
		}
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
	
	[FriendAccessAllowed]
	internal interface IDispatchOperation
	{
		bool DeserializeRequest { get; set; }
		IDispatchMessageFormatter Formatter { get; set; }
		string Name { get; }
		bool SerializeReply { get; set; }
	}
}
namespace System.ServiceModel.Channels
{
	public interface ITransportTokenAssertionProvider {}
	public static class UrlUtility {
		public static string UrlEncode (string s, Encoding e)
		{
			return s;
		}

		public static string UrlDecode (string s, Encoding e)
		{
			return s;
		}
	}
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
namespace System.ServiceModel
{
	// introduced for silverlight sdk compatibility
	internal interface IDuplexHelper { }

	[FriendAccessAllowed ()]
	internal class DiagnosticUtility
	{
		[FriendAccessAllowed ()]
		internal class ExceptionUtility
		{
			public static Exception ThrowHelperArgument (string message) { throw new NotImplementedException (); }
			
			public static Exception ThrowHelperArgument (string paramName, string message) { throw new NotImplementedException (); }
			
			public static Exception ThrowHelperArgumentNull (string arg)
			{
				return new ArgumentNullException (arg);
			}

			[FriendAccessAllowed]
			internal static Exception ThrowHelperCallback (Exception e) { throw new NotImplementedException (); }
			
			[FriendAccessAllowed]
			internal static Exception ThrowHelperCallback (string message, Exception innerException) { throw new NotImplementedException (); }
			
			public static Exception ThrowHelperError (Exception error)
			{
				return error;
			}
			
			[FriendAccessAllowed]
			internal static Exception ThrowHelperFatal (string message, Exception innerException) { throw new NotImplementedException (); }
			
			[FriendAccessAllowed]
			internal static Exception ThrowHelperInternal (bool fatal) { throw new NotImplementedException (); }
			
			public static Exception ThrowHelperWarning (Exception e) { throw new NotImplementedException (); }
		}
	}
}

namespace System.ServiceModel.Dispatcher
{
	public sealed class EndpointDispatcher
	{
		internal EndpointDispatcher ()
		{
		}
	}

	internal class FaultFormatter : IClientFaultFormatter
	{
		internal FaultFormatter (Type[] detailTypes) { throw new NotImplementedException (); }
		internal FaultFormatter (SynchronizedCollection<FaultContractInfo> faultContractInfoCollection) { throw new NotImplementedException (); }
		protected virtual FaultException CreateFaultException (MessageFault messageFault, string action) { throw new NotImplementedException (); }
		protected FaultException CreateFaultException (MessageFault messageFault, string action, object detailObj, Type detailType, XmlDictionaryReader detailReader) { throw new NotImplementedException (); }
		public FaultException Deserialize (MessageFault messageFault, string action) { throw new NotImplementedException (); }
		protected virtual XmlObjectSerializer GetSerializer (Type detailType, string faultExceptionAction, out string action) { throw new NotImplementedException (); }
	}

	internal interface IClientFaultFormatter
	{
		FaultException Deserialize (MessageFault messageFault, string action);
	}
}
namespace System.ServiceModel.Security
{
	class Dummy {}
}
#if !MOBILE
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
#endif
#endif