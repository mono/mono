//
// MexInstanceContextProvider.cs
//
// Author:
//	Ankit Jain <jankit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Xml;
using System.IO;

namespace System.ServiceModel.Dispatcher
{
	internal class MexInstanceContextProvider : IInstanceContextProvider
	{
		InstanceContext ctx;

		public MexInstanceContextProvider (ServiceHostBase service_host)
		{
			foreach (IServiceBehavior beh in service_host.Description.Behaviors) {
				ServiceMetadataBehavior mex_beh = beh as ServiceMetadataBehavior;
				if (mex_beh == null)
					continue;

				MetadataExchange mex_instance = new MetadataExchange (mex_beh);
				ctx = new InstanceContext (mex_instance);
				break;
			}
			//if (ctx == null)
		}
		
		public InstanceContext GetExistingInstanceContext (Message message, IContextChannel channel)
		{
			if (message.Headers.Action != "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get")
				return null;

			return ctx;
		}

		public void InitializeInstanceContext (InstanceContext instanceContext, Message message, IContextChannel channel)
		{
		}

		public bool IsIdle (InstanceContext instanceContext)
		{
			throw new NotImplementedException ();
		}

		public void NotifyIdle (InstanceContextIdleCallback callback, InstanceContext instanceContext)
		{
			throw new NotImplementedException ();
		}
	}
	
	class MetadataExchange : IMetadataExchange
	{
		ServiceMetadataBehavior beh;
		
		public MetadataExchange (ServiceMetadataBehavior beh)
		{
			this.beh = beh;
		}

		public Message Get (Message request)
		{
			UniqueId id = request.Headers.MessageId;

			MemoryStream ms = new MemoryStream ();
			XmlWriterSettings xws = new XmlWriterSettings ();
			xws.OmitXmlDeclaration = true;
			
			using (XmlWriter xw = XmlWriter.Create (ms, xws))
				beh.MetadataExporter.GetGeneratedMetadata ().WriteTo (xw);

			ms.Seek (0, SeekOrigin.Begin);
			XmlReader xr = XmlReader.Create (ms);

			Message ret = Message.CreateMessage (request.Version,
				"http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse",
				xr);
			ret.Headers.RelatesTo = id;

			return ret;
		}

		public IAsyncResult BeginGet (Message request, AsyncCallback cb, object state)
		{
			throw new NotImplementedException ();
		}

		public Message EndGet (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
