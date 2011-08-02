//
// System.ServiceModel.MessageHeader.cs
//
// Author: Duncan Mak (duncan@novell.com)
//	   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public abstract class MessageHeader : MessageHeaderInfo
	{
		static readonly XmlWriterSettings writer_settings;

		static MessageHeader ()
		{
			writer_settings = new XmlWriterSettings ();
			writer_settings.OmitXmlDeclaration = true;
			writer_settings.Indent = true;
		}

		protected MessageHeader () {}

		static string default_actor = String.Empty;
		static bool default_is_ref = false;
		static bool default_must_understand = false;
		static bool default_relay = false;

		public static MessageHeader CreateHeader (string name, string ns, object value)
		{
			return CreateHeader (name, ns, value, default_must_understand);
		}

		public static MessageHeader CreateHeader (string name, string ns, object value, bool must_understand)
		{
			return CreateHeader (name, ns, value, must_understand, default_actor);
		}

		public static MessageHeader CreateHeader (string name, string ns, object value, XmlObjectSerializer formatter)
		{
			return CreateHeader (name, ns, value, formatter, default_must_understand, 
					     default_actor, default_relay);
		}

		public static MessageHeader CreateHeader (string name, string ns, object value, 
						   bool must_understand, string actor)
		{
			return CreateHeader (name, ns, value, must_understand, actor, default_relay);
		}

		public static MessageHeader CreateHeader (string name, string ns, object value, XmlObjectSerializer formatter, 
						   bool must_understand)
		{
			return CreateHeader (name, ns, value, formatter, must_understand, default_actor, default_relay);
		}
		
		public static MessageHeader CreateHeader (string name, string ns, object value, 
						   bool must_understand, string actor, bool relay)
		{
			return CreateHeader (name, ns, value, new DataContractSerializer (value.GetType ()),
					must_understand, actor, relay);
		}

		public static MessageHeader CreateHeader (string name, string ns, object value, XmlObjectSerializer formatter,
						   bool must_understand, string actor)
		{
			return CreateHeader (name, ns, value, formatter, must_understand, actor, default_relay);
		}
		
		public static MessageHeader CreateHeader (string name, string ns, object value, XmlObjectSerializer formatter,
						   bool must_understand, string actor, bool relay)
		{
			// FIXME: how to get IsReferenceParameter ?
			return new DefaultMessageHeader (name, ns, value, formatter, default_is_ref, must_understand, actor, relay);
		}

		public virtual bool IsMessageVersionSupported (MessageVersion version)
		{
			if (version.Envelope == EnvelopeVersion.Soap12)
				if (Actor == EnvelopeVersion.Soap11.NextDestinationActorValue)
					return false;

			if (version.Envelope == EnvelopeVersion.Soap11)
				if (Actor == EnvelopeVersion.Soap12.NextDestinationActorValue ||
				    Actor == EnvelopeVersion.Soap12UltimateReceiver)
					return false;

			// by default, it's always supported
			return true;
		}

		protected abstract void OnWriteHeaderContents (XmlDictionaryWriter writer, MessageVersion version);

		protected virtual void OnWriteStartHeader (XmlDictionaryWriter writer, MessageVersion version)
		{
			var dic = Constants.SoapDictionary;
			XmlDictionaryString name, ns;
			var prefix = Prefix ?? (Namespace.Length > 0 ? writer.LookupPrefix (Namespace) : String.Empty);
			if (dic.TryLookup (Name, out name) && dic.TryLookup (Namespace, out ns))
				writer.WriteStartElement (prefix, name, ns);
			else
				writer.WriteStartElement (prefix, this.Name, this.Namespace);
			WriteHeaderAttributes (writer, version);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			XmlWriter w = XmlWriter.Create (sb, writer_settings);

			WriteHeader (w, MessageVersion.Default);
			w.Close ();

			return sb.ToString ();
		}

		public void WriteHeader (XmlDictionaryWriter writer, MessageVersion version)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer is null.");

			if (version == null)
				throw new ArgumentNullException ("version is null.");

			if (version.Envelope == EnvelopeVersion.None)
				return;

			WriteStartHeader (writer, version);
			WriteHeaderContents (writer, version);

			writer.WriteEndElement ();
		}

		public void WriteHeader (XmlWriter writer, MessageVersion version)
		{
			WriteHeader (XmlDictionaryWriter.CreateDictionaryWriter (writer), version);
		}

		protected void WriteHeaderAttributes (XmlDictionaryWriter writer, MessageVersion version)
		{
			var dic = Constants.SoapDictionary;
			if (Id != null)
				writer.WriteAttributeString ("u", dic.Add ("Id"), dic.Add (Constants.WsuNamespace), Id);
			if (!String.IsNullOrEmpty (Actor)) {
				if (version.Envelope == EnvelopeVersion.Soap11) 
					writer.WriteAttributeString ("s", dic.Add ("actor"), dic.Add (version.Envelope.Namespace), Actor);

				if (version.Envelope == EnvelopeVersion.Soap12) 
					writer.WriteAttributeString ("s", dic.Add ("role"), dic.Add (version.Envelope.Namespace), Actor);
			}

			// mustUnderstand is the same across SOAP 1.1 and 1.2
			if (MustUnderstand == true)
				writer.WriteAttributeString ("s", dic.Add ("mustUnderstand"), dic.Add (version.Envelope.Namespace), "1");

			// relay is only available on SOAP 1.2
			if (Relay == true && version.Envelope == EnvelopeVersion.Soap12)
				writer.WriteAttributeString ("s", dic.Add ("relay"), dic.Add (version.Envelope.Namespace), "true");
		}

		public void WriteHeaderContents (XmlDictionaryWriter writer, MessageVersion version)
		{
			this.OnWriteHeaderContents (writer, version);
		}

		public void WriteStartHeader (XmlDictionaryWriter writer, MessageVersion version)
		{
			this.OnWriteStartHeader (writer, version);
		}

		public override string Actor { get { return default_actor; }}

		public override bool IsReferenceParameter { get { return default_is_ref; }}

		public override bool MustUnderstand { get { return default_must_understand; }}

		public override bool Relay { get { return default_relay; }}

		internal class XmlMessageHeader : MessageHeader
		{
			bool is_ref, must_understand, relay;
			string actor;
#if NET_2_1
			string body;
#else
			// This is required to completely clone body xml that 
			// does not introduce additional xmlns declarations that
			// blocks canonicalized copy of the input XML.
			XmlDocument body;
#endif
			string local_name;
			string namespace_uri;

			public XmlMessageHeader (XmlReader reader, MessageVersion version)
			{
				var soapNS = version.Envelope.Namespace;
				var addrNS = version.Addressing.Namespace;
				Prefix = reader.Prefix;
				Id = reader.GetAttribute ("Id", Constants.WsuNamespace);

				string s = reader.GetAttribute ("relay", soapNS);
				relay = s != null ? XmlConvert.ToBoolean (s) : false;
				s = reader.GetAttribute ("mustUnderstand", soapNS);
				must_understand = s != null ? XmlConvert.ToBoolean (s) : false;
				actor = reader.GetAttribute ("actor", soapNS) ?? String.Empty;

				s = reader.GetAttribute ("IsReferenceParameter", addrNS);
				is_ref = s != null ? XmlConvert.ToBoolean (s) : false;

				local_name = reader.LocalName;
				namespace_uri = reader.NamespaceURI;
#if NET_2_1
				body = reader.ReadOuterXml ();
#else
				body = new XmlDocument ();
				var w = body.CreateNavigator ().AppendChild ();
				w.WriteNode (reader, false);
				w.Close ();
#endif
			}

			public XmlReader CreateReader ()
			{
#if NET_2_1
				var reader = XmlReader.Create (new StringReader (body));
#else
				var reader = new XmlNodeReader (body);
#endif
				reader.MoveToContent ();
				return reader;
			}

			protected override void OnWriteHeaderContents (
				XmlDictionaryWriter writer, MessageVersion version)
			{
				var r = CreateReader ();
				r.MoveToContent ();
				if (r.IsEmptyElement)
					return; // write nothing
				for (r.Read (); r.NodeType != XmlNodeType.EndElement;)
					writer.WriteNode (r, false);
			}

			public override string Actor { get { return actor; }}

			public override bool IsReferenceParameter { get { return is_ref; }}

			public override bool MustUnderstand { get { return must_understand; }}

			public override string Name { get { return local_name; }}

			public override string Namespace { get { return namespace_uri; }}

			public override bool Relay { get { return relay; }}
		}

		internal class DefaultMessageHeader : MessageHeader
		{
			string actor, name, ns;
			object value;
			XmlObjectSerializer formatter;
			bool is_ref, must_understand, relay;
			
			internal DefaultMessageHeader (string name, string ns, object value, XmlObjectSerializer formatter, 
						       bool isReferenceParameter,
						       bool mustUnderstand, string actor, bool relay)
			{
				this.name = name;
				this.ns = ns;
				this.value = value;
				this.formatter = formatter;
				this.is_ref = isReferenceParameter;
				this.must_understand = mustUnderstand;
				this.actor = actor ?? String.Empty;
				this.relay = relay;
			}

			protected override void OnWriteHeaderContents (XmlDictionaryWriter writer,
								       MessageVersion version)
			{
				// FIXME: it's a nasty workaround just to avoid UniqueId output as a string, for bug #577139.
				if (Value is UniqueId)
					writer.WriteValue ((UniqueId) Value);
				else
					this.formatter.WriteObjectContent (writer, value);
			}

			public object Value { get { return value; } }

			public override string Actor { get { return actor; }}

			public override bool IsReferenceParameter { get { return is_ref; }}

			public override bool MustUnderstand { get { return must_understand; }}

			public override string Name { get { return name; }}

			public override string Namespace { get { return ns; }}

			public override bool Relay { get { return relay; }}
		}
	}
}
