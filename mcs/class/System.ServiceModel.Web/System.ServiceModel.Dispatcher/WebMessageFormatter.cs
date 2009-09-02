//
// WebMessageFormatter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008,2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Description
{
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

		public WebHttpBehavior Behavior {
			get { return behavior; }
		}

		public WebAttributeInfo Info {
			get { return info; }
		}

		public WebMessageBodyStyle BodyStyle {
			get { return info.IsBodyStyleSetExplicitly ? info.BodyStyle : behavior.DefaultBodyStyle; }
		}

		public bool IsResponseBodyWrapped {
			get {
				switch (BodyStyle) {
				case WebMessageBodyStyle.Wrapped:
				case WebMessageBodyStyle.WrappedResponse:
					return true;
				}
				return false;
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

		protected WebContentFormat ToContentFormat (WebMessageFormat src)
		{
			switch (src) {
			case WebMessageFormat.Xml:
				return WebContentFormat.Xml;
			case WebMessageFormat.Json:
				return WebContentFormat.Json;
			}
			throw new SystemException ("INTERNAL ERROR: should not happen");
		}

		protected void CheckMessageVersion (MessageVersion messageVersion)
		{
			if (messageVersion == null)
				throw new ArgumentNullException ("messageVersion");

			if (!MessageVersion.None.Equals (messageVersion))
				throw new ArgumentException ("Only MessageVersion.None is supported");
		}

		protected MessageDescription GetMessageDescription (MessageDirection dir)
		{
			foreach (MessageDescription md in operation.Messages)
				if (md.Direction == dir)
					return md;
			throw new SystemException ("INTERNAL ERROR: no corresponding message description for the specified direction: " + dir);
		}

		protected XmlObjectSerializer GetSerializer (WebContentFormat msgfmt)
		{
			switch (msgfmt) {
			case WebContentFormat.Xml:
				return GetSerializer (ref xml_serializer, t => new DataContractSerializer (t));
				break;
			case WebContentFormat.Json:
				return GetSerializer (ref json_serializer, t => new DataContractJsonSerializer (t));
				break;
			default:
				throw new NotImplementedException ();
			}
		}

		XmlObjectSerializer xml_serializer, json_serializer;

		XmlObjectSerializer GetSerializer (ref XmlObjectSerializer serializer, Func<Type,XmlObjectSerializer> f)
		{
			if (serializer == null) {
				MessageDescription md = GetMessageDescription (MessageDirection.Output);
				serializer = f (md.Body.ReturnValue.Type);
			}
			return serializer;
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

		internal class RequestDispatchFormatter : WebDispatchMessageFormatter
		{
			public RequestDispatchFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}
		}

		internal class ReplyDispatchFormatter : WebDispatchMessageFormatter
		{
			public ReplyDispatchFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}
		}

		internal abstract class WebClientMessageFormatter : WebMessageFormatter, IClientMessageFormatter
		{
			protected WebClientMessageFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public virtual Message SerializeRequest (MessageVersion messageVersion, object [] parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (messageVersion);

				var c = new NameValueCollection ();

				MessageDescription md = GetMessageDescription (MessageDirection.Input);

				if (parameters.Length != md.Body.Parts.Count)
					throw new ArgumentException ("Parameter array length does not match the number of message body parts");

				for (int i = 0; i < parameters.Length; i++) {
					var p = md.Body.Parts [i];
					string name = p.Name.ToUpperInvariant ();
					if (UriTemplate.PathSegmentVariableNames.Contains (name) ||
					    UriTemplate.QueryValueVariableNames.Contains (name))
						c.Add (name, parameters [i] != null ? Converter.ConvertValueToString (parameters [i], parameters [i].GetType ()) : null);
					else
						// FIXME: bind as a message part
						throw new NotImplementedException (String.Format ("parameter {0} is not contained in the URI template {1} {2} {3}", p.Name, UriTemplate, UriTemplate.PathSegmentVariableNames.Count, UriTemplate.QueryValueVariableNames.Count));
				}

				Uri to = UriTemplate.BindByName (Endpoint.Address.Uri, c);

				Message ret = Message.CreateMessage (messageVersion, (string) null);
				ret.Headers.To = to;

				var hp = new HttpRequestMessageProperty ();
				hp.Method = Info.Method;

				// FIXME: isn't it always null?
				if (WebOperationContext.Current != null)
					WebOperationContext.Current.OutgoingRequest.Apply (hp);
				// FIXME: set hp.SuppressEntityBody for some cases.
				ret.Properties.Add (HttpRequestMessageProperty.Name, hp);

				var wp = new WebBodyFormatMessageProperty (ToContentFormat (Info.IsRequestFormatSetExplicitly ? Info.RequestFormat : Behavior.DefaultOutgoingRequestFormat));
				ret.Properties.Add (WebBodyFormatMessageProperty.Name, wp);

				return ret;
			}

			public virtual object DeserializeReply (Message message, object [] parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (message.Version);

				string pname = WebBodyFormatMessageProperty.Name;
				if (!message.Properties.ContainsKey (pname))
					throw new SystemException ("INTERNAL ERROR: it expects WebBodyFormatMessageProperty existence");
				var wp = (WebBodyFormatMessageProperty) message.Properties [pname];

				var serializer = GetSerializer (wp.Format);

				// FIXME: handle ref/out parameters

				var md = GetMessageDescription (MessageDirection.Output);

				var reader = message.GetReaderAtBodyContents ();

				if (IsResponseBodyWrapped && md.Body.WrapperName != null)
					reader.ReadStartElement (md.Body.WrapperName, md.Body.WrapperNamespace);

				var ret = serializer.ReadObject (reader, false);

				if (IsResponseBodyWrapped && md.Body.WrapperName != null)
					reader.ReadEndElement ();

				return ret;
			}
		}

		internal abstract class WebDispatchMessageFormatter : WebMessageFormatter, IDispatchMessageFormatter
		{
			protected WebDispatchMessageFormatter (OperationDescription operation, ServiceEndpoint endpoint, QueryStringConverter converter, WebHttpBehavior behavior)
				: base (operation, endpoint, converter, behavior)
			{
			}

			public Message SerializeReply (MessageVersion messageVersion, object [] parameters, object result)
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
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (messageVersion);

				MessageDescription md = GetMessageDescription (MessageDirection.Output);

				// FIXME: use them.
				// var dcob = Operation.Behaviors.Find<DataContractSerializerOperationBehavior> ();
				// XmlObjectSerializer xos = dcob.CreateSerializer (result.GetType (), md.Body.WrapperName, md.Body.WrapperNamespace, null);
				// var xsob = Operation.Behaviors.Find<XmlSerializerOperationBehavior> ();
				// XmlSerializer [] serializers = XmlSerializer.FromMappings (xsob.GetXmlMappings ().ToArray ());

				WebMessageFormat msgfmt = Info.IsResponseFormatSetExplicitly ? Info.ResponseFormat : Behavior.DefaultOutgoingResponseFormat;

				string mediaType = null;
				XmlObjectSerializer serializer = null;
				switch (msgfmt) {
				case WebMessageFormat.Xml:
					serializer = GetSerializer (WebContentFormat.Xml);
					mediaType = "application/xml";
					break;
				case WebMessageFormat.Json:
					serializer = GetSerializer (WebContentFormat.Json);
					mediaType = "application/json";
					break;
				}

				// FIXME: serialize ref/out parameters as well.

				Message ret = Message.CreateMessage (MessageVersion.None, null, result, serializer);

				// Message properties

				var hp = new HttpResponseMessageProperty ();
				// FIXME: get encoding from somewhere
				hp.Headers ["Content-Type"] = mediaType + "; charset=utf-8";

				// apply user-customized HTTP results via WebOperationContext.
				WebOperationContext.Current.OutgoingResponse.Apply (hp);

				// FIXME: fill some properties if required.
				ret.Properties.Add (HttpResponseMessageProperty.Name, hp);

				var wp = new WebBodyFormatMessageProperty (ToContentFormat (msgfmt));
				ret.Properties.Add (WebBodyFormatMessageProperty.Name, wp);

				return ret;
			}

			public void DeserializeRequest (Message message, object [] parameters)
			{
				if (parameters == null)
					throw new ArgumentNullException ("parameters");
				CheckMessageVersion (message.Version);

				OperationContext.Current.Extensions.Add (new WebOperationContext (OperationContext.Current));

				IncomingWebRequestContext iwc = WebOperationContext.Current.IncomingRequest;

				Uri to = message.Headers.To;
				UriTemplateMatch match = UriTemplate.Match (Endpoint.Address.Uri, to);
				if (match == null)
					// not sure if it could happen
					throw new SystemException (String.Format ("INTERNAL ERROR: UriTemplate does not match with the request: {0} / {1}", UriTemplate, to));
				iwc.UriTemplateMatch = match;

				MessageDescription md = GetMessageDescription (MessageDirection.Input);

				for (int i = 0; i < parameters.Length; i++) {
					var p = md.Body.Parts [i];
					string name = p.Name.ToUpperInvariant ();
					parameters [i] = match.BoundVariables [name];
				}
			}
		}
	}
}
