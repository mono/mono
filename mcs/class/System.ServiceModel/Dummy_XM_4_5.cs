using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Threading;

namespace System.ServiceModel
{
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