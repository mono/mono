//
// MessageFault.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2009 Novell, Inc.  http://www.novell.com
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

namespace System.ServiceModel.Channels
{
	public abstract class MessageFault
	{
		// type members

		public static MessageFault CreateFault (Message message, int maxBufferSize)
		{
			try {
				if (message.Version.Envelope == EnvelopeVersion.Soap11)
					return CreateFault11 (message, maxBufferSize);
				else // common to None and SOAP12
					return CreateFault12 (message, maxBufferSize);
			} catch (XmlException ex) {
				throw new CommunicationException ("Received an invalid SOAP Fault message", ex);
			}
		}

		static MessageFault CreateFault11 (Message message, int maxBufferSize)
		{
			FaultCode fc = null;
			FaultReason fr = null;
			object details = null;
			XmlDictionaryReader r = message.GetReaderAtBodyContents ();
			r.ReadStartElement ("Fault", message.Version.Envelope.Namespace);
			r.MoveToContent ();

			while (r.NodeType != XmlNodeType.EndElement) {
				switch (r.LocalName) {
				case "faultcode":
					fc = ReadFaultCode11 (r);
					break;
				case "faultstring":
					fr = new FaultReason (r.ReadElementContentAsString());
					break;
				case "detail":
					return new XmlReaderDetailMessageFault (message, r, fc, fr, null, null);
				case "faultactor":
				default:
					throw new NotImplementedException ();
				}
				r.MoveToContent ();
			}
			r.ReadEndElement ();

			if (fr == null)
				throw new XmlException ("Reason is missing in the Fault message");

			if (details == null)
				return CreateFault (fc, fr);
			return CreateFault (fc, fr, details);
		}

		static MessageFault CreateFault12 (Message message, int maxBufferSize)
		{
			FaultCode fc = null;
			FaultReason fr = null;
			string node = null;
			XmlDictionaryReader r = message.GetReaderAtBodyContents ();
			r.ReadStartElement ("Fault", message.Version.Envelope.Namespace);

			for (r.MoveToContent (); r.NodeType != XmlNodeType.EndElement; r.MoveToContent ()) {
				if (r.NamespaceURI != message.Version.Envelope.Namespace) {
					r.Skip ();
					continue;
				}
				switch (r.LocalName) {
				case "Code":
					fc = ReadFaultCode12 (r, message.Version.Envelope.Namespace);
					break;
				case "Reason":
					fr = ReadFaultReason12 (r, message.Version.Envelope.Namespace);
					break;
				case "Node":
					node = r.ReadElementContentAsString ();
					break;
				case "Role":
					r.Skip (); // no corresponding member to store.
					break;
				case "Detail":
					if (!r.IsEmptyElement)
						return new XmlReaderDetailMessageFault (message, r, fc, fr, null, node);
					r.Read ();
					break;
				default:
					throw new XmlException (String.Format ("Unexpected node {0} name {1}", r.NodeType, r.Name));
				}
			}

			if (fr == null)
				throw new XmlException ("Reason is missing in the Fault message");

			r.ReadEndElement ();

			return CreateFault (fc, fr, null, null, null, node);
		}

		static FaultCode ReadFaultCode11 (XmlDictionaryReader r)
		{
			FaultCode subcode = null;
			XmlQualifiedName value = XmlQualifiedName.Empty;

			if (r.IsEmptyElement)
				throw new ArgumentException ("Fault Code is mandatory in SOAP fault message.");

			r.ReadStartElement ("faultcode");
			r.MoveToContent ();
			while (r.NodeType != XmlNodeType.EndElement) {
				if (r.NodeType == XmlNodeType.Element)
					subcode = ReadFaultCode11 (r);
				else
					value = (XmlQualifiedName) r.ReadContentAs (typeof (XmlQualifiedName), r as IXmlNamespaceResolver);
				r.MoveToContent ();
			}
			r.ReadEndElement ();

			return new FaultCode (value.Name, value.Namespace, subcode);
		}

		static FaultCode ReadFaultCode12 (XmlDictionaryReader r, string ns)
		{
			FaultCode subcode = null;
			XmlQualifiedName value = XmlQualifiedName.Empty;

			if (r.IsEmptyElement)
				throw new ArgumentException ("either SubCode or Value element is mandatory in SOAP fault code.");

			r.ReadStartElement (); // could be either Code or SubCode
			r.MoveToContent ();
			while (r.NodeType != XmlNodeType.EndElement) {
				switch (r.LocalName) {
				case "Subcode":
					subcode = ReadFaultCode12 (r, ns);
					break;
				case "Value":
					value = (XmlQualifiedName) r.ReadElementContentAs (typeof (XmlQualifiedName), r as IXmlNamespaceResolver, "Value", ns);
					break;
				default:
					throw new ArgumentException (String.Format ("Unexpected Fault Code subelement: '{0}'", r.LocalName));
				}
				r.MoveToContent ();
			}
			r.ReadEndElement ();

			return new FaultCode (value.Name, value.Namespace, subcode);
		}

		static FaultReason ReadFaultReason12 (XmlDictionaryReader r, string ns)
		{
			List<FaultReasonText> l = new List<FaultReasonText> ();
			if (r.IsEmptyElement)
				throw new ArgumentException ("One or more Text element is mandatory in SOAP fault reason text.");

			r.ReadStartElement ("Reason", ns);
			for (r.MoveToContent ();
			     r.NodeType != XmlNodeType.EndElement;
			     r.MoveToContent ()) {
				string lang = r.GetAttribute ("lang", "http://www.w3.org/XML/1998/namespace");
				if (lang == null)
					throw new XmlException ("xml:lang is mandatory on fault reason Text");
				l.Add (new FaultReasonText (r.ReadElementContentAsString ("Text", ns), lang));
			}
			r.ReadEndElement ();

			return new FaultReason (l);
		}

		public static MessageFault CreateFault (FaultCode code,
			string reason)
		{
			return CreateFault (code, new FaultReason (reason));
		}

		public static MessageFault CreateFault (FaultCode code,
			FaultReason reason)
		{
			return new SimpleMessageFault (code, reason,
				 false, null, null, null, null);
		}

		public static MessageFault CreateFault (FaultCode code,
			FaultReason reason, object detail)
		{
			return new SimpleMessageFault (code, reason,
				true, detail, new DataContractSerializer (detail.GetType ()), null, null);
		}

		public static MessageFault CreateFault (FaultCode code,
			FaultReason reason, object detail,
			XmlObjectSerializer formatter)
		{
			return new SimpleMessageFault (code, reason, true,
				detail, formatter, String.Empty, String.Empty);
		}

		public static MessageFault CreateFault (FaultCode code,
			FaultReason reason, object detail,
			XmlObjectSerializer formatter, string actor)
		{
			return new SimpleMessageFault (code, reason,
				true, detail, formatter, actor, String.Empty);
		}

		public static MessageFault CreateFault (FaultCode code,
			FaultReason reason, object detail,
			XmlObjectSerializer formatter, string actor, string node)
		{
			return new SimpleMessageFault (code, reason,
				true, detail, formatter, actor, node);
		}

		// pretty simple implementation class
		internal abstract class BaseMessageFault : MessageFault
		{
			string actor, node;
			FaultCode code;
			FaultReason reason;

			protected BaseMessageFault (FaultCode code, FaultReason reason, string actor, string node)
			{
				this.code = code;
				this.reason = reason;
				this.actor = actor;
				this.node = node;
			}

			public override string Actor {
				get { return actor; }
			}

			public override FaultCode Code {
				get { return code; }
			}

			public override string Node {
				get { return node; }
			}

			public override FaultReason Reason {
				get { return reason; }
			}
		}

		internal class SimpleMessageFault : BaseMessageFault
		{
			bool has_detail;
			object detail;
			XmlObjectSerializer formatter;

			public SimpleMessageFault (FaultCode code,
				FaultReason reason, bool has_detail,
				object detail, XmlObjectSerializer formatter,
				string actor, string node)
				: this (code, reason, detail, formatter, actor, node)
			{
				this.has_detail = has_detail;
			}

			public SimpleMessageFault (FaultCode code,
				FaultReason reason,
				object detail, XmlObjectSerializer formatter,
				string actor, string node)
				: base (code, reason, actor, node)
			{
				if (code == null)
					throw new ArgumentNullException ("code");
				if (reason == null)
					throw new ArgumentNullException ("reason");

				this.detail = detail;
				this.formatter = formatter;
			}

			public override bool HasDetail {
				// it is not simply "detail != null" since
				// null detail could become <ms:anyType xsi:nil="true" />
				get { return has_detail; }
			}

			protected override void OnWriteDetailContents (XmlDictionaryWriter writer)
			{
				if (formatter == null && detail != null)
					formatter = new DataContractSerializer (detail.GetType ());
				if (formatter != null)
					formatter.WriteObject (writer, detail);
				else
					throw new InvalidOperationException ("There is no fault detail to write");
			}

			public object Detail {
				get { return detail; }
			}
		}

		class XmlReaderDetailMessageFault : BaseMessageFault
		{
			XmlDictionaryReader reader;
			bool consumed;

			public XmlReaderDetailMessageFault (Message message, XmlDictionaryReader reader, FaultCode code, FaultReason reason, string actor, string node)
				: base (code, reason, actor, node)
			{
				this.reader = reader;
			}

			void Consume ()
			{
				if (consumed)
					throw new InvalidOperationException ("The fault detail content is already consumed");
				consumed = true;
				reader.ReadStartElement (); // consume the wrapper
				reader.MoveToContent ();
			}

			public override bool HasDetail {
				get { return true; }
			}

			protected override XmlDictionaryReader OnGetReaderAtDetailContents ()
			{
				Consume ();
				return reader;
			}

			protected override void OnWriteDetailContents (XmlDictionaryWriter writer)
			{
				if (!HasDetail)
					throw new InvalidOperationException ("There is no fault detail to write");
				Consume ();
				while (reader.NodeType != XmlNodeType.EndElement)
					writer.WriteNode (reader, false);
			}
		}

		// instance members

		protected MessageFault ()
		{
		}

		[MonoTODO ("is this true?")]
		public virtual string Actor {
			get { return String.Empty; }
		}

		public abstract FaultCode Code { get; }

		public abstract bool HasDetail { get; }

		[MonoTODO ("is this true?")]
		public virtual string Node {
			get { return String.Empty; }
		}

		public abstract FaultReason Reason { get; }

		public T GetDetail<T> ()
		{
			return GetDetail<T> (new DataContractSerializer (typeof (T)));
		}

		public T GetDetail<T> (XmlObjectSerializer formatter)
		{
			if (!HasDetail)
				throw new InvalidOperationException ("This message does not have details.");

			return (T) formatter.ReadObject (GetReaderAtDetailContents ());
		}

		public XmlDictionaryReader GetReaderAtDetailContents ()
		{
			return OnGetReaderAtDetailContents ();
		}

		public void WriteTo (XmlDictionaryWriter writer,
			EnvelopeVersion version)
		{
			writer.WriteStartElement ("Fault", version.Namespace);
			WriteFaultCode (writer, version, Code, false);
			WriteReason (writer, version);
			if (HasDetail)
				OnWriteDetail (writer, version);
			writer.WriteEndElement ();
		}

		private void WriteFaultCode (XmlDictionaryWriter writer, 
			EnvelopeVersion version, FaultCode code, bool sub)
		{
			if (version == EnvelopeVersion.Soap11) {
				writer.WriteStartElement ("", "faultcode", String.Empty);
				if (code.Namespace.Length > 0 && String.IsNullOrEmpty (writer.LookupPrefix (code.Namespace)))
					writer.WriteXmlnsAttribute ("a", code.Namespace);
				writer.WriteQualifiedName (code.Name, code.Namespace);
				writer.WriteEndElement ();
			} else { // Soap12
				writer.WriteStartElement (sub ? "Subcode" : "Code", version.Namespace);
				writer.WriteStartElement ("Value", version.Namespace);
				if (code.Namespace.Length > 0 && String.IsNullOrEmpty (writer.LookupPrefix (code.Namespace)))
					writer.WriteXmlnsAttribute ("a", code.Namespace);
				writer.WriteQualifiedName (code.Name, code.Namespace);
				writer.WriteEndElement ();
				if (code.SubCode != null)
					WriteFaultCode (writer, version, code.SubCode, true);
				writer.WriteEndElement ();
			}
		}

		private void WriteReason (XmlDictionaryWriter writer, 
			EnvelopeVersion version)
		{
			if (version == EnvelopeVersion.Soap11) {
				foreach (FaultReasonText t in Reason.Translations) {
					writer.WriteStartElement ("", "faultstring", String.Empty);
					if (t.XmlLang != null)
						writer.WriteAttributeString ("xml", "lang", null, t.XmlLang);
					writer.WriteString (t.Text);
					writer.WriteEndElement ();
				}
			} else { // Soap12
				writer.WriteStartElement ("Reason", version.Namespace);
				foreach (FaultReasonText t in Reason.Translations) {
					writer.WriteStartElement ("Text", version.Namespace);
					if (t.XmlLang != null)
						writer.WriteAttributeString ("xml", "lang", null, t.XmlLang);
					writer.WriteString (t.Text);
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();
			}
		}

		public void WriteTo (XmlWriter writer, EnvelopeVersion version)
		{
			WriteTo (XmlDictionaryWriter.CreateDictionaryWriter (
				writer), version);
		}

		protected virtual XmlDictionaryReader OnGetReaderAtDetailContents ()
		{
			if (!HasDetail)
				throw new InvalidOperationException ("There is no fault detail to read");
			MemoryStream ms = new MemoryStream ();
			using (XmlDictionaryWriter dw =
				XmlDictionaryWriter.CreateDictionaryWriter (
					XmlWriter.Create (ms))) {
				OnWriteDetailContents (dw);
			}
			ms.Seek (0, SeekOrigin.Begin);
			return XmlDictionaryReader.CreateDictionaryReader (
				XmlReader.Create (ms));
		}

		protected virtual void OnWriteDetail (XmlDictionaryWriter writer, EnvelopeVersion version)
		{
			OnWriteStartDetail (writer, version);
			OnWriteDetailContents (writer);
			writer.WriteEndElement ();
		}

		protected virtual void OnWriteStartDetail (XmlDictionaryWriter writer, EnvelopeVersion version)
		{
			if (version == EnvelopeVersion.Soap11)
				writer.WriteStartElement ("detail", String.Empty);
			else // Soap12
				writer.WriteStartElement ("Detail", version.Namespace);
		}

		protected abstract void OnWriteDetailContents (XmlDictionaryWriter writer);
	}
}
