//
// WebOperationContext.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

#if NET_2_1 // Note that moonlight System.ServiceModel.Web.dll does not contain this class.
using IncomingWebRequestContext = System.Object;
using OutgoingWebResponseContext = System.Object;
#else
using System.Runtime.Serialization.Json;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Serialization;
#endif

namespace System.ServiceModel.Web
{
	public class WebOperationContext
#if !NET_2_1
	 : IExtension<OperationContext>
#endif
	{
#if !NET_2_1
		public static WebOperationContext Current {
			get {
				if (OperationContext.Current == null)
					return null;
				var ret = OperationContext.Current.Extensions.Find<WebOperationContext> ();
				if (ret == null) {
					ret = new WebOperationContext (OperationContext.Current);
					OperationContext.Current.Extensions.Add (ret);
				}
				return ret;
			}
		}
#endif

		IncomingWebRequestContext incoming_request;
		IncomingWebResponseContext incoming_response;
		OutgoingWebRequestContext outgoing_request;
		OutgoingWebResponseContext outgoing_response;

		public WebOperationContext (OperationContext operation)
		{
			if (operation == null)
				throw new ArgumentNullException ("operation");

			outgoing_request = new OutgoingWebRequestContext ();
			incoming_response = new IncomingWebResponseContext (operation);
#if !NET_2_1
			incoming_request = new IncomingWebRequestContext (operation);
			outgoing_response = new OutgoingWebResponseContext ();
#endif
		}

#if !NET_2_1
		public IncomingWebRequestContext IncomingRequest {
			get { return incoming_request; }
		}
#endif

		public IncomingWebResponseContext IncomingResponse {
			get { return incoming_response; }
		}

		public OutgoingWebRequestContext OutgoingRequest {
			get { return outgoing_request; }
		}

#if !NET_2_1
		public OutgoingWebResponseContext OutgoingResponse {
			get { return outgoing_response; }
		}
#endif

		public void Attach (OperationContext context)
		{
			// do nothing
		}

		public void Detach (OperationContext context)
		{
			// do nothing
		}

#if NET_4_0 && !MOBILE
		static readonly XmlWriterSettings settings = new XmlWriterSettings () { OmitXmlDeclaration = true, Indent = false };
		XmlSerializer document_serializer, feed_serializer, item_serializer;

		Message CreateAtom10Response<T> (T obj, ref XmlSerializer serializer)
		{
			if (serializer == null)
				serializer = new XmlSerializer (typeof (T));
			var ms = new MemoryStream ();
			using (var xw = XmlWriter.Create (ms, settings))
				serializer.Serialize (xw, obj);
			ms.Position = 0;
			return Message.CreateMessage (MessageVersion.None, null, XmlReader.Create (ms));
		}

		public Message CreateAtom10Response (ServiceDocument document)
		{
			return CreateAtom10Response<AtomPub10ServiceDocumentFormatter> (new AtomPub10ServiceDocumentFormatter (document), ref document_serializer);
		}

		public Message CreateAtom10Response (SyndicationFeed feed)
		{
			return CreateAtom10Response<Atom10FeedFormatter> (new Atom10FeedFormatter (feed), ref feed_serializer);
		}

		public Message CreateAtom10Response (SyndicationItem item)
		{
			return CreateAtom10Response<Atom10ItemFormatter> (new Atom10ItemFormatter (item), ref item_serializer);
		}

		public Message CreateJsonResponse<T> (T instance)
		{
			return CreateJsonResponse<T> (instance, new DataContractJsonSerializer (typeof (T)));
		}

		public Message CreateJsonResponse<T> (T instance, DataContractJsonSerializer serializer)
		{
			return Message.CreateMessage (MessageVersion.None, null, instance, serializer);
		}
#endif
	}
}
