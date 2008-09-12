//
// System.Runtime.Remoting.Channels.Http.HttpRemotingHandlerFactory
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		
		void ConfigureHttpChannel (HttpContext context)
		{
			lock (GetType())
			{
				if (webConfigLoaded) return;

				string appConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
				if (File.Exists (appConfig))
					RemotingConfiguration.Configure (appConfig);
							
				// Look for a channel that wants to receive http request
				IChannelReceiverHook chook = null;
				foreach (IChannel channel in ChannelServices.RegisteredChannels)
				{
					chook = channel as IChannelReceiverHook;
					if (chook == null) continue;
					
					if (chook.ChannelScheme != "http")
						throw new RemotingException ("Only http channels are allowed when hosting remoting objects in a web server");
					
					if (!chook.WantsToListen) 
					{
						chook = null;
						continue;
					}
					
					//found chook
					break;
				}

				if (chook == null)
				{
					HttpChannel chan = new HttpChannel();
					ChannelServices.RegisterChannel(chan);
					chook = chan;
				}
					
				// Register the uri for the channel. The channel uri includes the scheme, the
				// host and the application path
					
				string channelUrl = context.Request.Url.GetLeftPart(UriPartial.Authority);
				channelUrl += context.Request.ApplicationPath;
				chook.AddHookChannelUri (channelUrl);
				
				transportSink = new HttpServerTransportSink (chook.ChannelSinkChain);
				webConfigLoaded = true;
			}
		}

		public void ReleaseHandler (IHttpHandler handler)
		{
		}
	}
}
