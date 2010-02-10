//
// Message.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml.XPath;

namespace System.ServiceModel.Channels
{
	public abstract class Message : IDisposable
	{
		bool disposed;
		string body_id;

		protected Message () {
			State = MessageState.Created;
		}

		public abstract MessageHeaders Headers { get; }

		internal string BodyId {
			get { return body_id; }
			set { body_id = value; }
		}

		public virtual bool IsEmpty {
			get { return false; }
		}

		public virtual bool IsFault {
			get { return false; }
		}

		public abstract MessageProperties Properties { get; }

		MessageState ___state;
		public MessageState State {
			get { return ___state; }
			private set { ___state = value;	}
		}

		public abstract MessageVersion Version { get; }

		protected bool IsDisposed {
			get { return disposed; }
		}

		public void Close ()
		{
			if (!disposed)
				OnClose ();
			State = MessageState.Closed;
			disposed = true;
		}

		public MessageBuffer CreateBufferedCopy (int maxBufferSize)
		{
			if (State != MessageState.Created)
				throw new InvalidOperationException (String.Format ("The message is already at {0} state", State));
			try {
				return OnCreateBufferedCopy (maxBufferSize);
			} finally {
				State = MessageState.Copied;
			}
		}

		void IDisposable.Dispose ()
		{
			Close ();
		}

		public T GetBody<T> ()
		{
			return GetBody<T> (new DataContractSerializer (typeof (T)));
		}

		public T GetBody<T> (XmlObjectSerializer xmlFormatter)
		{
			return (T) xmlFormatter.ReadObject (GetReaderAtBodyContents ());
		}

		public string GetBodyAttribute (string localName, string ns)
		{
			return OnGetBodyAttribute (localName, ns);
		}

		public XmlDictionaryReader GetReaderAtBodyContents ()
		{
			return OnGetReaderAtBodyContents ();
		}

		public override string ToString ()
		{
			MessageState tempState = State;
			try {

				StringWriter sw = new StringWriter ();
				XmlWriterSettings settings = new XmlWriterSettings ();
				settings.Indent = true;
				settings.OmitXmlDeclaration = true;
				using (XmlWriter w = XmlWriter.Create (sw, settings)) {
					WriteMessage (w);
				}
				return sw.ToString ();
			}
			finally {
				State = tempState;
			}
		}

		void WriteXsiNil (XmlDictionaryWriter writer)
		{
			var dic = Constants.SoapDictionary;
			writer.WriteStartElement ("z", dic.Add ("anyType"), dic.Add (Constants.MSSerialization));
			writer.WriteAttributeString ("i", dic.Add ("nil"), dic.Add ("http://www.w3.org/2001/XMLSchema-instance"), "true");
			writer.WriteEndElement ();
		}

		public void WriteBody (XmlDictionaryWriter writer)
		{
			if (Version.Envelope != EnvelopeVersion.None)
				WriteStartBody (writer);
			WriteBodyContents (writer);
			if (Version.Envelope != EnvelopeVersion.None)
				writer.WriteEndElement ();
		}

		public void WriteBody (XmlWriter writer)
		{
			WriteBody (XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public void WriteBodyContents (XmlDictionaryWriter writer)
		{
			if (!IsEmpty)
				OnWriteBodyContents (writer);
			else if (Version.Envelope == EnvelopeVersion.None)
				WriteXsiNil (writer);
			State = MessageState.Written;
		}

		public void WriteMessage (XmlDictionaryWriter writer)
		{
			if (State != MessageState.Created)
				throw new InvalidOperationException (String.Format ("The message is already at {0} state", State));

			OnWriteMessage (writer);
		}

		public void WriteMessage (XmlWriter writer)
		{
			WriteMessage (XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public void WriteStartBody (XmlDictionaryWriter writer)
		{
			if (State != MessageState.Created)
				throw new InvalidOperationException (String.Format ("The message is already at {0} state", State));

			OnWriteStartBody (writer);
		}

		public void WriteStartBody (XmlWriter writer)
		{
			WriteStartBody (
				XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public void WriteStartEnvelope (XmlDictionaryWriter writer)
		{
			if (State != MessageState.Created)
				throw new InvalidOperationException (String.Format ("The message is already at {0} state", State));

			OnWriteStartEnvelope (writer);
		}

		[MonoTODO]
		protected virtual void OnBodyToString (
			XmlDictionaryWriter writer)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnClose ()
		{
		}

		[MonoTODO ("use maxBufferSize")]
		protected virtual MessageBuffer OnCreateBufferedCopy (
			int maxBufferSize)
		{
			var s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			s.ConformanceLevel = ConformanceLevel.Auto;
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw, s)))
				WriteBodyContents (w);
			var headers = new MessageHeaders (Headers);
			var props = new MessageProperties (Properties);
			return new DefaultMessageBuffer (maxBufferSize, headers, props, new XmlReaderBodyWriter (sw.ToString ()), false);
		}

		protected virtual string OnGetBodyAttribute (
			string localName, string ns)
		{
			// other than XmlReaderMessage it cannot return anything
			return null;
		}

		protected virtual XmlDictionaryReader OnGetReaderAtBodyContents ()
		{
			var ws = new XmlWriterSettings ();
			ws.ConformanceLevel = ConformanceLevel.Auto;
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter body = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw, ws))) {
				WriteBodyContents (body);
			}

			var rs = new XmlReaderSettings ();
			rs.ConformanceLevel = ConformanceLevel.Auto;
			return XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (sw.ToString ()), rs));
		}

		protected abstract void OnWriteBodyContents (
			XmlDictionaryWriter writer);

		protected virtual void OnWriteMessage (
			XmlDictionaryWriter writer)
		{
			if (Version.Envelope != EnvelopeVersion.None) {
				WriteStartEnvelope (writer);
				if (Headers.Count > 0) {
					OnWriteStartHeaders (writer);
					for (int i = 0, count = Headers.Count; i < count; i++)
						Headers.WriteHeader (i, writer);
					writer.WriteEndElement ();
				}
			}
			WriteBody (writer);
			if (Version.Envelope != EnvelopeVersion.None)
				writer.WriteEndElement ();
		}

		protected virtual void OnWriteStartBody (
			XmlDictionaryWriter writer)
		{
			var dic = Constants.SoapDictionary;
			writer.WriteStartElement ("s", dic.Add ("Body"), dic.Add (Version.Envelope.Namespace));
			if (BodyId != null)
				writer.WriteAttributeString ("u", dic.Add ("Id"), dic.Add (Constants.WsuNamespace), BodyId);
		}

		protected virtual void OnWriteStartEnvelope (
			XmlDictionaryWriter writer)
		{
			var dic = Constants.SoapDictionary;
			writer.WriteStartElement ("s", dic.Add ("Envelope"), dic.Add (Version.Envelope.Namespace));
			if (Headers.Action != null && Version.Addressing.Namespace != MessageVersion.None.Addressing.Namespace)
				writer.WriteXmlnsAttribute ("a", dic.Add (Version.Addressing.Namespace));
			foreach (MessageHeaderInfo h in Headers)
				if (h.Id != null) {
					writer.WriteXmlnsAttribute ("u", dic.Add (Constants.WsuNamespace));
					break;
				}
		}

		protected virtual void OnWriteStartHeaders (
			XmlDictionaryWriter writer)
		{
			var dic = Constants.SoapDictionary;
			writer.WriteStartElement ("s", dic.Add ("Header"), dic.Add (Version.Envelope.Namespace));
		}

		#region factory methods

		//  1) fault -> 4
		//  2) action -> 5
		//  3) fault, action -> 10
		//  4) version, fault -> 10
		//  5) version, action -> EmptyMessage
		//  6) action, body -> 12
		//  7) action, xmlReader -> 8
		//  8) action, reader -> 16
		// 10) version, fault, action -> 20
		// 11) version, action, body -> 14
		// 12) action, body, formatter -> 14
		// 13) version, action, body -> 14
		// 14) version, action, body, formatter -> 20
		// 15) version, action, xmlReader -> 16
		// 16) version, action, reader -> 20
		// 17) xmlReader, maxSizeOfHeaders, version -> 18
		// 18) reader, maxSizeOfHeaders, version -> ForwardingMessage
		// 19) action, bodyWriter -> 20
		// 20) version, action, bodyWriter -> SimpleMessage

		public static Message CreateMessage (MessageVersion version,
			FaultCode code, string reason, string action)
		{
			MessageFault fault = MessageFault.CreateFault (code, reason);
			return CreateMessage (version, fault, action);
		}

		public static Message CreateMessage (MessageVersion version,
			FaultCode code, string reason, object detail,
			string action)
		{
			MessageFault fault = MessageFault.CreateFault (
				code, new FaultReason (reason), detail);
			return CreateMessage (version, fault, action);
		}

		public static Message CreateMessage (MessageVersion version,
			MessageFault fault, string action)
		{
			return new SimpleMessage (version, action,
				new MessageFaultBodyWriter (fault, version), true);
		}

		public static Message CreateMessage (MessageVersion version,
			string action, object body)
		{
			return body == null ?
				CreateMessage (version, action) :
				CreateMessage (version, action, body, new DataContractSerializer (body.GetType ()));
		}

		public static Message CreateMessage (MessageVersion version,
			string action, object body, XmlObjectSerializer xmlFormatter)
		{
			return body == null ?
				CreateMessage (version, action) :
				CreateMessage (
					version, action,
					new XmlObjectSerializerBodyWriter (body, xmlFormatter));
		}

		public static Message CreateMessage (MessageVersion version,
			string action, XmlReader body)
		{
			return CreateMessage (version, action,
				XmlDictionaryReader.CreateDictionaryReader (body));
		}

		public static Message CreateMessage (MessageVersion version,
			string action, XmlDictionaryReader body)
		{
			return CreateMessage (version, action,
				new XmlReaderBodyWriter (body));
		}

		public static Message CreateMessage (XmlReader envelopeReader,
			int maxSizeOfHeaders, MessageVersion version)
		{
			return CreateMessage (
				XmlDictionaryReader.CreateDictionaryReader (envelopeReader),
				maxSizeOfHeaders,
				version);
		}

		// Core implementations of CreateMessage.

		public static Message CreateMessage (MessageVersion version,
			string action, BodyWriter body)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			if (body == null)
				throw new ArgumentNullException ("body");
			return new SimpleMessage (version, action, body, false);
		}

		public static Message CreateMessage (MessageVersion version,
			string action)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			return new EmptyMessage (version, action);
		}

		public static Message CreateMessage (
			XmlDictionaryReader envelopeReader,
			int maxSizeOfHeaders,
			MessageVersion version)
		{
			if (envelopeReader == null)
				throw new ArgumentNullException ("envelopeReader");
			if (version == null)
				throw new ArgumentNullException ("version");
			return new XmlReaderMessage (version,
				envelopeReader, maxSizeOfHeaders);
		}

		#endregion
	}
}
