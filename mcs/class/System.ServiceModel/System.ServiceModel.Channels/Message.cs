//
// Message.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006,2010 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace System.ServiceModel.Channels
{
	public abstract class Message : IDisposable
	{
		bool disposed;
		string body_id;
		Message copied_message;
		string string_cache;

		protected Message () {
			State = MessageState.Created;
		}

		public abstract MessageHeaders Headers { get; }

		public virtual bool IsEmpty {
			get { return false; }
		}

		public virtual bool IsFault {
			get { return false; }
		}

		public abstract MessageProperties Properties { get; }

		public MessageState State { get; private set; }

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

			if (copied_message != null)
				return copied_message.CreateBufferedCopy (maxBufferSize);

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
			return OnGetBody<T> (GetReaderAtBodyContents ());
		}

		public T GetBody<T> (XmlObjectSerializer serializer)
		{
			// FIXME: Somehow use OnGetBody() here as well?
			return (T)serializer.ReadObject (GetReaderAtBodyContents ());
		}

		protected virtual T OnGetBody<T> (XmlDictionaryReader reader)
		{
			var xmlFormatter = new DataContractSerializer (typeof (T));
			return (T)xmlFormatter.ReadObject (reader);
		}

		public string GetBodyAttribute (string localName, string ns)
		{
			return OnGetBodyAttribute (localName, ns);
		}

		public XmlDictionaryReader GetReaderAtBodyContents ()
		{
			if (copied_message != null)
				return copied_message.GetReaderAtBodyContents ();

			return OnGetReaderAtBodyContents ();
		}

		public override string ToString ()
		{
			if (string_cache != null)
				return string_cache;

			StringWriter sw = new StringWriter ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.Indent = true;
			settings.OmitXmlDeclaration = true;

			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				OnBodyToString (XmlDictionaryWriter.CreateDictionaryWriter (w));
			}
			string_cache = sw.ToString ();
			return string_cache;
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
			if (!IsEmpty) {
				if (copied_message != null)
					copied_message.WriteBodyContents (writer);
				else
					OnWriteBodyContents (writer);
			}
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

		protected virtual void OnBodyToString (
			XmlDictionaryWriter writer)
		{
			MessageState tempState = State;
			try {
				var mb = CreateBufferedCopy (int.MaxValue);
				copied_message = mb.CreateMessage ();
				var msg = mb.CreateMessage ();
				msg.WriteMessage (writer);
			}
			finally {
				State = tempState;
			}
		}

		protected virtual void OnClose ()
		{
		}

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
			return new DefaultMessageBuffer (maxBufferSize, headers, props, new XmlReaderBodyWriter (sw.ToString (), maxBufferSize, null), false, new AttributeCollection ());
		}

		protected virtual string OnGetBodyAttribute (
			string localName, string ns)
		{
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

			var nt = new NameTable ();
			var nsmgr = new XmlNamespaceManager (nt);
			nsmgr.AddNamespace ("s", Version.Envelope.Namespace);
			nsmgr.AddNamespace ("a", Version.Addressing.Namespace);
			var pc = new XmlParserContext (nt, nsmgr, null, XmlSpace.None);
			
			var rs = new XmlReaderSettings ();
			rs.ConformanceLevel = ConformanceLevel.Auto;
			
			return XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (sw.ToString ()), rs, pc));
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
		}

		protected virtual void OnWriteStartEnvelope (
			XmlDictionaryWriter writer)
		{
			var dic = Constants.SoapDictionary;
			writer.WriteStartElement ("s", dic.Add ("Envelope"), dic.Add (Version.Envelope.Namespace));
			if (Headers.Action != null && Version.Addressing.Namespace != MessageVersion.None.Addressing.Namespace)
				writer.WriteXmlnsAttribute ("a", dic.Add (Version.Addressing.Namespace));
			foreach (MessageHeaderInfo h in Headers)
				if (h.Id != null && writer.LookupPrefix (Constants.WsuNamespace) != "u") {
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

		// 1) version, code, reason, action -> 3
		// 2) version, code, reason, detail, action -> 3
		// 3) version, fault, action -> SimpleMessage
		// 4) version, action, body -> 10 or 5
		// 5) version, action, body, formatter -> 10 or 9
		// 6) version, action, xmlReader -> 7
		// 7) version, action, reader -> 9
		// 8) xmlReader, maxSizeOfHeaders, version -> 11
		// 9) version, action, body -> SimpleMessage
		// 10) version, action -> EmptyMessage
		// 11) reader, maxSizeOfHeaders, version -> XmlReaderMessage

		// 1)
		public static Message CreateMessage (MessageVersion version,
			FaultCode faultCode, string reason, string action)
		{
			MessageFault fault = MessageFault.CreateFault (faultCode, reason);
			return CreateMessage (version, fault, action);
		}

		// 2)
		public static Message CreateMessage (MessageVersion version,
			FaultCode faultCode, string reason, object detail,
			string action)
		{
			MessageFault fault = MessageFault.CreateFault (
				faultCode, new FaultReason (reason), detail);
			return CreateMessage (version, fault, action);
		}

		// 3)
		public static Message CreateMessage (MessageVersion version,
			MessageFault fault, string action)
		{
			return new SimpleMessage (version, action,
				new MessageFaultBodyWriter (fault, version), true, empty_attributes);
		}

		// 4)
		public static Message CreateMessage (MessageVersion version,
			string action, object body)
		{
			return body == null ?
				CreateMessage (version, action) :
				CreateMessage (version, action, body, new DataContractSerializer (body.GetType ()));
		}

		// 5)
		public static Message CreateMessage (MessageVersion version,
			string action, object body, XmlObjectSerializer serializer)
		{
			return body == null ?
				CreateMessage (version, action) :
				CreateMessage (
					version, action,
					new XmlObjectSerializerBodyWriter (body, serializer));
		}

		// 6)
		public static Message CreateMessage (MessageVersion version,
			string action, XmlReader body)
		{
			return CreateMessage (version, action,
				XmlDictionaryReader.CreateDictionaryReader (body));
		}

		// 7)
		public static Message CreateMessage (MessageVersion version,
			string action, XmlDictionaryReader body)
		{
			return CreateMessage (version, action,
				new XmlReaderBodyWriter (body));
		}

		// 8)
		public static Message CreateMessage (XmlReader envelopeReader,
			int maxSizeOfHeaders, MessageVersion version)
		{
			return CreateMessage (
				XmlDictionaryReader.CreateDictionaryReader (envelopeReader),
				maxSizeOfHeaders,
				version);
		}

		// Core implementations of CreateMessage.

		static readonly AttributeCollection empty_attributes = new AttributeCollection ();

		// 9)
		public static Message CreateMessage (MessageVersion version,
			string action, BodyWriter body)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			if (body == null)
				throw new ArgumentNullException ("body");
			return new SimpleMessage (version, action, body, false, empty_attributes);
		}

		// 10)
		public static Message CreateMessage (MessageVersion version,
			string action)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			return new EmptyMessage (version, action);
		}

		// 11)
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
