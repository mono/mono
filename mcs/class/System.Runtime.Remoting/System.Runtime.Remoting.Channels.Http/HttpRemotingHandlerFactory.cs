//
// System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Web;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.Http 
{
	public class HttpRemotingHandlerFactory : IHttpHandlerFactory
	{
		static bool webConfigLoaded = false;
		static HttpServerTransportSink transportSink = null;
		
		public HttpRemotingHandlerFactory ()
		{
		}

		public IHttpHandler GetHandler (HttpContext context,
						string verb,
						string url,
						string filePath)
		{
			if (!webConfigLoaded)
				ConfigureHttpChannel (context);
			
			return new HttpRemotingHandler (transportSink);
		}
		
		public void ConfigureHttpChannel (HttpContext context)
		{
			lock (GetType())
			{
				if (webConfigLoaded) return;
				
				// Read the configuration file
				
				string webconfig = Path.Combine (context.Request.PhysicalApplicationPath, "web.config");
				RemotingConfiguration.Configure (webconfig);
				
				// Look for a channel that wants to receive http request
				
				foreach (IChannel channel in ChannelServices.RegisteredChannels)
				{
					IChannelReceiverHook chook = channel as IChannelReceiverHook;
					if (chook == null) continue;
					
					if (chook.ChannelScheme != "http")
						throw new RemotingException ("Only http channels are allowed when hosting remoting objects in a web server");
					
					if (!chook.WantsToListen) continue;
					
					// Register the uri for the channel. The channel uri includes the scheme, the
					// host and the application path
					
					string channelUrl = context.Request.Url.GetLeftPart(UriPartial.Authority);
					channelUrl += context.Request.ApplicationPath;
					chook.AddHookChannelUri (channelUrl);
					
					transportSink = new HttpServerTransportSink (chook.ChannelSinkChain);
				}
				webConfigLoaded = true;
			}
		}

		public void ReleaseHandler (IHttpHandler handler)
		{
		}
	}
}
