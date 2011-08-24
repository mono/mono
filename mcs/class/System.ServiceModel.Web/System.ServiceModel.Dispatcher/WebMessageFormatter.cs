//
// WebMessageFormatter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008,2009 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin, Inc (http://xamarin.com)
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

#if NET_2_1
using XmlObjectSerializer = System.Object;
#endif

namespace System.ServiceModel.Dispatcher
{
	// This set of classes is to work as message formatters for 
	// WebHttpBehavior. There are couple of aspects to differentiate
	// implementations:
	// - request/reply and client/server
	//   by WebMessageFormatter hierarchy
	//   - WebClientMessageFormatter - for client
	//     - RequestClientFormatter - for request
	//     - ReplyClientFormatter - for response
	//   - WebDispatchMessageFormatter - for server
	//     - RequestDispatchFormatter - for request
	//     - ReplyDispatchFormatter - for response
	//
	// FIXME: below items need more work
	// - HTTP method differences
	//  - GET (WebGet)
	//  - POST (other way)
	// - output format: Stream, JSON, XML ...

	internal abstract class WebMessageFormatter
	{
		OperationDescription operation;
		ServiceEndpoint endpoint;
		QueryStringConverter converter;
		WebHttpBehavior behavior;
		UriTemplate template;
		WebAttributeInfo info = null;

		public WebMessageFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
		{
			this.operation = operation;
			this.endpoint = endpoint;
			this.converter = converter;
			this.behavior = behavior;
			ApplyWebAttribute ();
#if !NET_2_1
			// This is a hack for WebScriptEnablingBehavior
			var jqc = converter as JsonQueryStringConverter;
			if (jqc != null)
				BodyName = jqc.CustomWrapperName;
#endif
		}

		void ApplyWebAttribute ()
		{
			MethodInfo mi = operation.SyncMethod ?? operation.BeginMethod;

			object [] atts = mi.GetCustomAttributes (typeof (WebGetAttribute), false);
			if (atts.Length > 0)
				info = ((WebGetAttribute) atts [0]).Info;
			atts = mi.GetCustomAttributes (typeof (WebInvokeAttribute), false);
			if (atts.Length > 0)
				info = ((WebInvokeAttribute) atts [0]).Info;
			if (info == null)
				info = new WebAttributeInfo ();

			template = info.BuildUriTemplate (Operation, GetMessageDescription (MessageDirection.Input));
		}

		public string BodyName { get; set; }

		public WebHttpBehavior Behavior {
			get { return behavior; }
		}

		public WebAttributeInfo Info {
			get { return info; }
		}

		public WebMessageBodyStyle BodyStyle {
			get { return info.IsBodyStyleSetExplicitly ? info.BodyStyle : behavior.DefaultBodyStyle; }
		}

		public bool IsRequestBodyWrapped {
			get {
				switch (BodyStyle) {
				case WebMessageBodyStyle.Wrapped:
				case WebMessageBodyStyle.WrappedRequest:
					return true;
				}
				return BodyName != null;
			}
		}

		public bool IsResponseBodyWrapped {
			get {
				switch (BodyStyle) {
				case WebMessageBodyStyle.Wrapped:
				case WebMessageBodyStyle.WrappedResponse:
					return true;
				}
				return BodyName != null;
			}
		}

		public OperationDescription Operation {
			get { return operation; }
		}

		public QueryStringConverter Converter {
			get { return converter; }
		}

		public ServiceEndpoint Endpoint {
			get { return endpoint; }
		}

		public UriTemplate UriTemplate {
			get { return template; }
		}

		protected WebContentFormat ToContentFormat (WebMessageFormat src, object result)
		{
			if (result is Stream)
				return WebContentFormat.Raw;
			switch (src) {
			case WebMessageFormat.Xml:
				return WebContentFormat.Xml;
			case WebMessageFormat.Json:
				return WebContentFormat.Json;
			}
			throw new SystemException ("INTERNAL ERROR: should not happen");
		}

		protected string GetMediaTypeString (WebContentFormat fmt)
		{
			switch (fmt) {
			case WebContentFormat.Raw:
				return "application/octet-stream";
			case WebContentFormat.Json:
				return "application/json";
			case WebContentFormat.Xml:
			default:
				return "application/xml";
			}
		}

		protected void CheckMessageVersion (MessageVersion messageVersion)
		{
			if (messageVersion == null)
				throw new ArgumentNullException ("messageVersion");

			if (!MessageVersion.None.Equals (messageVersion))
				throw new ArgumentException (String.Format ("Only MessageVersion.None is supported. {0} is not.", messageVersion));
		}

		protected MessageDescription GetMessageDescription (MessageDirection dir)
		{
			foreach (MessageDescription md in operation.Messages)
				if (md.Direction == dir)
					return md;
			throw new SystemException ("INTERNAL ERROR: no corresponding message description for the specified direction: " + dir);
		}

		protected XmlObjectSerializer GetSerializer (WebContentFormat msgfmt, bool isWrapped, MessagePartDescription part)
		{
			switch (msgfmt) {
			case WebContentFormat.Xml:
				if (xml_serializer == null)
					xml_serializer = isWrapped ? new DataContractSerializer (part.Type, part.Name, part.Namespace) : new DataContractSerializer (part.Type);
				return xml_serializer;
			case WebContentFormat.Json:
				// FIXME: after name argument they are hack
				if (json_serializer == null)
#if MOONLIGHT
					json_serializer = new DataContractJsonSerializer (part.Type);
#else
					json_serializer = isWrapped ? new DataContractJsonSerializer (part.Type, BodyName ?? part.Name, null, 0x100000, false, null, true) : new DataContractJsonSerializer (part.Type);
#endif
				return json_serializer;
			default:
				throw new NotImplementedException (msgfmt.ToString ());
			}
		}

		XmlObjectSerializer xml_serializer, json_serializer;

		protected object DeserializeObject (XmlObjectSerializer serializer, Message message, MessageDescription md, bool isWrapped, WebContentFormat fmt)
		{
			// FIXME: handle ref/out parameters

			var reader = message.GetReaderAtBodyContents ();

			if (isWrapped) {
				if (fmt == WebContentFormat.Json)
					reader.ReadStartElement ("root", String.Empty); // note that the wrapper name is passed to the serializer.
				else
					reader.ReadStartElement (md.Body.WrapperName, md.Body.WrapperNamespace);
			}

			var ret = ReadObjectBody (serializer, reader);

			if (isWrapped)
				reader.ReadEndElement ();

			return ret;
		}
		
		protected object ReadObjectBody (XmlObjectSerializer serializer, XmlReader reader)
		{
#if NET_2_1
			return (serializer is DataContractJsonSerializer) ?
				((DataContractJsonSerializer) serializer).ReadObject (reader) :
				((DataContractSerializer) serializer).ReadObject (reader, true);
#else
			return serializer.ReadObject (reader, true);
#endif
		}

		internal class RequestClientFormatter : WebClientMessageFormatter
		{
			public RequestClientFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public override object DeserializeReply (Message message, object [] parameters)
			{
				throw new NotSupportedException ();
			}
		}

		internal class ReplyClientFormatter : WebClientMessageFormatter
		{
			public ReplyClientFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public override Message SerializeRequest (MessageVersion messageVersion, object [] parameters)
			{
				throw new NotSupportedException ();
			}
		}

#if !NET_2_1
		internal class RequestDispatchFormatter : WebDispatchMessageFormatter
		{
			public RequestDispatchFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public override Message SerializeReply (MessageVersion messageVersion, object [] parameters, object result)
			{
				throw new NotSupportedException ();
			}
		}

		internal class ReplyDispatchFormatter : WebDispatchMessageFormatter
		{
			public ReplyDispatchFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public override void DeserializeRequest (Message message, object [] parameters)
			{
				throw new NotSupportedException ();
			}
		}
#endif

		internal abstract class WebClientMessageFormatter : WebMessageFormatter, IClientMessageFormatter
		{
			IClientMessageFormatter default_formatter;

			protected WebClientMessageFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public virtual Message SerializeRequest (MessageVersion messageVersion, object [] parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (messageVersion);

				var c = new Dictionary<string,string> ();

				MessageDescription md = GetMessageDescription (MessageDirection.Input);

				Message ret;
				Uri to;
				object msgpart = null;

				if (info.Method == "GET") {
					if (parameters.Length != md.Body.Parts.Count)
						throw new ArgumentException (String.Format ("Parameter array length does not match the number of message '{0}' body parts: {1} expected, got {2}", Operation.Name, md.Body.Parts.Count, parameters.Length));

					for (int i = 0; i < parameters.Length; i++) {
						var p = md.Body.Parts [i];
						string name = p.Name.ToUpper (CultureInfo.InvariantCulture);
						if (UriTemplate.PathSegmentVariableNames.Contains (name) ||
						    UriTemplate.QueryValueVariableNames.Contains (name))
							c.Add (name, parameters [i] != null ? Converter.ConvertValueToString (parameters [i], parameters [i].GetType ()) : null);
						else {
							// FIXME: bind as a message part
							if (msgpart == null)
								msgpart = parameters [i];
							else
								throw new  NotImplementedException (String.Format ("More than one parameters including {0} that are not contained in the URI template {1} was found.", p.Name, UriTemplate));
						}
					}
					ret = Message.CreateMessage (messageVersion, (string) null, msgpart);
				} else {
					if (default_formatter == null)
						default_formatter = BaseMessagesFormatter.Create (Operation);
					ret = default_formatter.SerializeRequest (messageVersion, parameters);
				}

				to = UriTemplate.BindByName (Endpoint.Address.Uri, c);
				ret.Headers.To = to;

				var hp = new HttpRequestMessageProperty ();
				hp.Method = Info.Method;

				WebMessageFormat msgfmt = Info.IsResponseFormatSetExplicitly ? Info.ResponseFormat : Behavior.DefaultOutgoingResponseFormat;
				var contentFormat = ToContentFormat (msgfmt, msgpart);
				string mediaType = GetMediaTypeString (contentFormat);
				// FIXME: get encoding from somewhere
				hp.Headers ["Content-Type"] = mediaType + "; charset=utf-8";

#if !NET_2_1
				if (WebOperationContext.Current != null)
					WebOperationContext.Current.OutgoingRequest.Apply (hp);
#endif
				// FIXME: set hp.SuppressEntityBody for some cases.
				ret.Properties.Add (HttpRequestMessageProperty.Name, hp);

				var wp = new WebBodyFormatMessageProperty (ToContentFormat (Info.IsRequestFormatSetExplicitly ? Info.RequestFormat : Behavior.DefaultOutgoingRequestFormat, null));
				ret.Properties.Add (WebBodyFormatMessageProperty.Name, wp);

				return ret;
			}

			public virtual object DeserializeReply (Message message, object [] parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (message.Version);

				if (message.IsEmpty)
					return null; // empty message, could be returned by HttpReplyChannel.

				string pname = WebBodyFormatMessageProperty.Name;
				if (!message.Properties.ContainsKey (pname))
					throw new SystemException ("INTERNAL ERROR: it expects WebBodyFormatMessageProperty existence");
				var wp = (WebBodyFormatMessageProperty) message.Properties [pname];
				var fmt = wp != null ? wp.Format : WebContentFormat.Xml;

				var md = GetMessageDescription (MessageDirection.Output);
				var serializer = GetSerializer (wp.Format, IsResponseBodyWrapped, md.Body.ReturnValue);
				var ret = DeserializeObject (serializer, message, md, IsResponseBodyWrapped, fmt);

				return ret;
			}
		}

		internal class WrappedBodyWriter : BodyWriter
		{
			public WrappedBodyWriter (object value, XmlObjectSerializer serializer, string name, string ns, WebContentFormat fmt)
				: base (true)
			{
				this.name = name;
				this.ns = ns;
				this.value = value;
				this.serializer = serializer;
				this.fmt = fmt;
			}

			WebContentFormat fmt;
			string name, ns;
			object value;
			XmlObjectSerializer serializer;

#if !NET_2_1
			protected override BodyWriter OnCreateBufferedCopy (int maxBufferSize)
			{
				return new WrappedBodyWriter (value, serializer, name, ns, fmt);
			}
#endif

			protected override void OnWriteBodyContents (XmlDictionaryWriter writer)
			{
				switch (fmt) {
				case WebContentFormat.Raw:
					WriteRawContents (writer);
					break;
				case WebContentFormat.Json:
					WriteJsonBodyContents (writer);
					break;
				case WebContentFormat.Xml:
					WriteXmlBodyContents (writer);
					break;
				}
			}
			
			void WriteRawContents (XmlDictionaryWriter writer)
			{
				throw new NotSupportedException ("Some unsupported sequence of writing operation occured. It is likely a missing feature.");
			}
			
			void WriteJsonBodyContents (XmlDictionaryWriter writer)
			{
				if (name != null) {
					writer.WriteStartElement ("root");
					writer.WriteAttributeString ("type", "object");
				}
				WriteObject (serializer, writer, value);
				if (name != null)
					writer.WriteEndElement ();
			}

			void WriteXmlBodyContents (XmlDictionaryWriter writer)
			{
				if (name != null)
					writer.WriteStartElement (name, ns);
				WriteObject (serializer, writer, value);
				if (name != null)
					writer.WriteEndElement ();
			}

			void WriteObject (XmlObjectSerializer serializer, XmlDictionaryWriter writer, object value)
			{
#if NET_2_1
					if (serializer is DataContractJsonSerializer)
						((DataContractJsonSerializer) serializer).WriteObject (writer, value);
					else
						((DataContractSerializer) serializer).WriteObject (writer, value);
#else
					serializer.WriteObject (writer, value);
#endif
			}
		}

#if !NET_2_1
		internal abstract class WebDispatchMessageFormatter : WebMessageFormatter, IDispatchMessageFormatter
		{
			protected WebDispatchMessageFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public virtual Message SerializeReply (MessageVersion messageVersion, object [] parameters, object result)
			{
				try {
					return SerializeReplyCore (messageVersion, parameters, result);
				} finally {
					if (WebOperationContext.Current != null)
						OperationContext.Current.Extensions.Remove (WebOperationContext.Current);
				}
			}

			Message SerializeReplyCore (MessageVersion messageVersion, object [] parameters, object result)
			{
				// parameters could be null.
				// result could be null. For Raw output, it becomes no output.

				CheckMessageVersion (messageVersion);

				MessageDescription md = GetMessageDescription (MessageDirection.Output);

				// FIXME: use them.
				// var dcob = Operation.Behaviors.Find<DataContractSerializerOperationBehavior> ();
				// XmlObjectSerializer xos = dcob.CreateSerializer (result.GetType (), md.Body.WrapperName, md.Body.WrapperNamespace, null);
				// var xsob = Operation.Behaviors.Find<XmlSerializerOperationBehavior> ();
				// XmlSerializer [] serializers = XmlSerializer.FromMappings (xsob.GetXmlMappings ().ToArray ());

				WebMessageFormat msgfmt = Info.IsResponseFormatSetExplicitly ? Info.ResponseFormat : Behavior.DefaultOutgoingResponseFormat;

				XmlObjectSerializer serializer = null;

				// FIXME: serialize ref/out parameters as well.

				string name = null, ns = null;

				switch (msgfmt) {
				case WebMessageFormat.Xml:
					serializer = GetSerializer (WebContentFormat.Xml, IsResponseBodyWrapped, md.Body.ReturnValue);
					name = IsResponseBodyWrapped ? md.Body.WrapperName : null;
					ns = IsResponseBodyWrapped ? md.Body.WrapperNamespace : null;
					break;
				case WebMessageFormat.Json:
					serializer = GetSerializer (WebContentFormat.Json, IsResponseBodyWrapped, md.Body.ReturnValue);
					name = IsResponseBodyWrapped ? (BodyName ?? md.Body.ReturnValue.Name) : null;
					ns = String.Empty;
					break;
				}

				var contentFormat = ToContentFormat (msgfmt, result);
				string mediaType = GetMediaTypeString (contentFormat);
				Message ret = contentFormat == WebContentFormat.Raw ? new RawMessage ((Stream) result) : Message.CreateMessage (MessageVersion.None, null, new WrappedBodyWriter (result, serializer, name, ns, contentFormat));

				// Message properties

				var hp = new HttpResponseMessageProperty ();
				// FIXME: get encoding from somewhere
				hp.Headers ["Content-Type"] = mediaType + "; charset=utf-8";

				// apply user-customized HTTP results via WebOperationContext.
				if (WebOperationContext.Current != null) // this formatter must be available outside ServiceHost.
					WebOperationContext.Current.OutgoingResponse.Apply (hp);

				// FIXME: fill some properties if required.
				ret.Properties.Add (HttpResponseMessageProperty.Name, hp);

				var wp = new WebBodyFormatMessageProperty (contentFormat);
				ret.Properties.Add (WebBodyFormatMessageProperty.Name, wp);

				return ret;
			}

			public virtual void DeserializeRequest (Message message, object [] parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (message.Version);

				IncomingWebRequestContext iwc = null;
				if (OperationContext.Current != null) {
					OperationContext.Current.Extensions.Add (new WebOperationContext (OperationContext.Current));
					iwc = WebOperationContext.Current.IncomingRequest;
				}
				
				var wp = message.Properties [WebBodyFormatMessageProperty.Name] as WebBodyFormatMessageProperty;
				var fmt = wp != null ? wp.Format : WebContentFormat.Xml;

				Uri to = message.Headers.To;
				UriTemplateMatch match = to == null ? null : UriTemplate.Match (Endpoint.Address.Uri, to);
				if (match != null && iwc != null)
					iwc.UriTemplateMatch = match;

				MessageDescription md = GetMessageDescription (MessageDirection.Input);

				for (int i = 0; i < parameters.Length; i++) {
					var p = md.Body.Parts [i];
					string name = p.Name.ToUpperInvariant ();
					if (fmt == WebContentFormat.Raw && p.Type == typeof (Stream)) {
						var rmsg = (RawMessage) message;
						parameters [i] = rmsg.Stream;
					} else {
						var str = match.BoundVariables [name];
						if (str != null)
							parameters [i] = Converter.ConvertStringToValue (str, p.Type);
						else {
							var serializer = GetSerializer (fmt, IsRequestBodyWrapped, p);
							parameters [i] = DeserializeObject (serializer, message, md, IsRequestBodyWrapped, fmt);
						}
					}
				}
			}
		}
#endif

		internal class RawMessage : Message
		{
			public RawMessage (Stream stream)
			{
				this.Stream = stream;
				headers = new MessageHeaders (MessageVersion.None);
				properties = new MessageProperties ();
			}
		
			public override MessageVersion Version {
				get { return MessageVersion.None; }
			}
		
			MessageHeaders headers;

			public override MessageHeaders Headers {
				get { return headers; }
			}
		
			MessageProperties properties;

			public override MessageProperties Properties {
				get { return properties; }
			}

			public Stream Stream { get; private set; }

			protected override void OnWriteBodyContents (XmlDictionaryWriter writer)
			{
				writer.WriteString ("-- message body is raw binary --");
			}

			protected override MessageBuffer OnCreateBufferedCopy (int maxBufferSize)
			{
				var ms = Stream as MemoryStream;
				if (ms == null) {
					ms = new MemoryStream ();
#if NET_4_0 || NET_2_1
					Stream.CopyTo (ms);
#else
					byte [] tmp = new byte [0x1000];
					int size;
					do {
						size = Stream.Read (tmp, 0, tmp.Length);
						ms.Write (tmp, 0, size);
					} while (size > 0);
#endif
					this.Stream = ms;
				}
				return new RawMessageBuffer (ms.ToArray (), headers, properties);
			}
		}
		
		internal class RawMessageBuffer : MessageBuffer
		{
			byte [] buffer;
			MessageHeaders headers;
			MessageProperties properties;

			public RawMessageBuffer (byte [] buffer, MessageHeaders headers, MessageProperties properties)
			{
				this.buffer = buffer;
				this.headers = new MessageHeaders (headers);
				this.properties = new MessageProperties (properties);
			}
			
			public override int BufferSize {
				get { return buffer.Length; }
			}
			
			public override void Close ()
			{
			}
			
			public override Message CreateMessage ()
			{
				var msg = new RawMessage (new MemoryStream (buffer));
				msg.Headers.CopyHeadersFrom (headers);
				msg.Properties.CopyProperties (properties);
				return msg;
			}
		}
	}
}
