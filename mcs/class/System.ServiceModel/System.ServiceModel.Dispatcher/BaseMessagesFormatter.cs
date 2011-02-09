//
// DefaultMessageOperationFormatter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Eyal Alaluf <eyala@mainsoft.com>
//
// Copyright (C) 2005-2010 Novell, Inc.  http://www.novell.com
// Copyright (C) 2008 Mainsoft Co. http://www.mainsoft.com
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
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.ServiceModel.Dispatcher
{
	// This type is introduced for moonlight compatibility.
	internal class OperationFormatter
		: IDispatchMessageFormatter, IClientMessageFormatter
	{
		BaseMessagesFormatter impl;
		string operation_name;

		public OperationFormatter (OperationDescription od, bool isRpc, bool isEncoded)
		{
			Validate (od, isRpc, isEncoded);

			impl = BaseMessagesFormatter.Create (od);

			operation_name = od.Name;
		}

		public string OperationName {
			get { return operation_name; }
		}

		public bool IsValidReturnValue (MessagePartDescription part)
		{
			return part != null && part.Type != typeof (void);
		}

		void Validate (OperationDescription od, bool isRpc, bool isEncoded)
		{
			bool hasParameter = false, hasVoid = false;
			foreach (var md in od.Messages) {
				if (md.IsTypedMessage || md.IsUntypedMessage) {
					if (isRpc && !isEncoded)
						throw new InvalidOperationException ("Message with action {0} is either strongly-typed or untyped, but defined as RPC and encoded.");
					if (hasParameter && !md.IsVoid)
						throw new InvalidOperationException (String.Format ("Operation '{0}' contains a message with parameters. Strongly-typed or untyped message can be paired only with strongly-typed, untyped or void message.", od.Name));
					if (isRpc && hasVoid)
						throw new InvalidOperationException (String.Format ("This operation '{0}' is defined as RPC and contains a message with void, which is not allowed.", od.Name));
				} else {
					hasParameter |= !md.IsVoid;
					hasVoid |= md.IsVoid;
				}
			}
		}

		public object DeserializeReply (Message message, object [] parameters)
		{
			return impl.DeserializeReply (message, parameters);
		}

		public Message SerializeRequest (MessageVersion messageVersion, object [] parameters)
		{
			return impl.SerializeRequest (messageVersion, parameters);
		}

		public void DeserializeRequest (Message message, object [] parameters)
		{
			impl.DeserializeRequest (message, parameters);
		}

		public Message SerializeReply (MessageVersion messageVersion, object [] parameters, object result)
		{
			return impl.SerializeReply (messageVersion, parameters, result);
		}
	}

	internal abstract class BaseMessagesFormatter
		: IDispatchMessageFormatter, IClientMessageFormatter
	{
		MessageDescriptionCollection messages;
		bool isAsync;
		ParameterInfo [] requestMethodParams;
		ParameterInfo [] replyMethodParams;
		List<Type> operation_known_types = new List<Type> ();

		public BaseMessagesFormatter (MessageDescriptionCollection messages)
		{
			this.messages = messages;
		}

		public BaseMessagesFormatter (OperationDescription desc)
			: this (desc.Messages)
		{
			if (desc.SyncMethod != null)
			{
				isAsync = false;
				requestMethodParams = replyMethodParams = desc.SyncMethod.GetParameters ();
				return;
			}
			isAsync = true;
			ParameterInfo [] methodParams = desc.BeginMethod.GetParameters ();
			requestMethodParams = new ParameterInfo [methodParams.Length - 2];
			Array.Copy (methodParams, requestMethodParams, requestMethodParams.Length);
			methodParams = desc.EndMethod.GetParameters ();
			replyMethodParams = new ParameterInfo [methodParams.Length - 1];
			Array.Copy (methodParams, replyMethodParams, replyMethodParams.Length);
			operation_known_types.AddRange (desc.KnownTypes);
		}

		public static BaseMessagesFormatter Create (OperationDescription desc)
		{
			MethodInfo attrProvider = desc.SyncMethod ?? desc.BeginMethod;
			object [] attrs;
#if !NET_2_1
			attrs = attrProvider.GetCustomAttributes (typeof (XmlSerializerFormatAttribute), false);
			if (attrs != null && attrs.Length > 0)
				return new XmlMessagesFormatter (desc, (XmlSerializerFormatAttribute) attrs [0]);
#endif

			attrs = attrProvider.GetCustomAttributes (typeof (DataContractFormatAttribute), false);
			DataContractFormatAttribute dataAttr = null;
			if (attrs != null && attrs.Length > 0)
				dataAttr = (DataContractFormatAttribute) attrs [0];
			return new DataContractMessagesFormatter (desc, dataAttr);
		}

		public IEnumerable<Type> OperationKnownTypes {
			get { return operation_known_types; }
		}

		protected abstract Message PartsToMessage (
			MessageDescription md, MessageVersion version, string action, object [] parts);
		protected abstract object [] MessageToParts (MessageDescription md, Message message);

		public Message SerializeRequest (
			MessageVersion version, object [] parameters)
		{
			MessageDescription md = null;
			foreach (MessageDescription mdi in messages)
				if (mdi.IsRequest)
					md = mdi;

			object [] parts = CreatePartsArray (md.Body);
			if (md.MessageType != null)
				MessageObjectToParts (md, parameters [0], parts);
			else {
				int index = 0;
				foreach (ParameterInfo pi in requestMethodParams)
					if (!pi.IsOut)
						parts [index++] = parameters [pi.Position];
			}
			return PartsToMessage (md, version, md.Action, parts);
		}

		public Message SerializeReply (
			MessageVersion version, object [] parameters, object result)
		{
			// use_response_converter

			MessageDescription md = null;
			foreach (MessageDescription mdi in messages)
				if (!mdi.IsRequest)
					md = mdi;

			object [] parts = CreatePartsArray (md.Body);
			if (md.MessageType != null)
				MessageObjectToParts (md, result, parts);
			else {
				if (HasReturnValue (md.Body))
					parts [0] = result;
				int index = ParamsOffset (md.Body);
				int paramsIdx = 0;
				foreach (ParameterInfo pi in replyMethodParams)
					if (pi.IsOut || pi.ParameterType.IsByRef)
				parts [index++] = parameters [paramsIdx++];
			}
			string action = version.Addressing == AddressingVersion.None ? null : md.Action;
			return PartsToMessage (md, version, action, parts);
		}

		public void DeserializeRequest (Message message, object [] parameters)
		{
			string action = message.Headers.Action;
			MessageDescription md = messages.Find (action);
			if (md == null)
				throw new ActionNotSupportedException (String.Format ("Action '{0}' is not supported by this operation.", action));

			object [] parts = MessageToParts (md, message);
			if (md.MessageType != null) {
#if NET_2_1
				parameters [0] = Activator.CreateInstance (md.MessageType);
#else
				parameters [0] = Activator.CreateInstance (md.MessageType, true);
#endif
				PartsToMessageObject (md, parts, parameters [0]);
			}
			else
			{
				int index = 0;
				foreach (ParameterInfo pi in requestMethodParams)
					if (!pi.IsOut) {
						parameters [index] = parts [index];
						index++;
					}
			}
		}

		public object DeserializeReply (Message message, object [] parameters)
		{
			MessageDescription md = null;
			foreach (MessageDescription mdi in messages)
				if (!mdi.IsRequest)
					md = mdi;

			object [] parts = MessageToParts (md, message);
			if (md.MessageType != null) {
#if NET_2_1
				object msgObject = Activator.CreateInstance (md.MessageType);
#else
				object msgObject = Activator.CreateInstance (md.MessageType, true);
#endif
				PartsToMessageObject (md, parts, msgObject);
				return msgObject;
			}
			else {
				int index = ParamsOffset (md.Body);
				foreach (ParameterInfo pi in requestMethodParams)
					if (pi.IsOut || pi.ParameterType.IsByRef)
						parameters [pi.Position] = parts [index++];
				return HasReturnValue (md.Body) ? parts [0] : null;
			}
		}

		void PartsToMessageObject (MessageDescription md, object [] parts, object msgObject)
		{
			foreach (MessagePartDescription partDesc in md.Body.Parts)
				if (partDesc.MemberInfo is FieldInfo)
					((FieldInfo) partDesc.MemberInfo).SetValue (msgObject, parts [partDesc.Index]);
				else
					((PropertyInfo) partDesc.MemberInfo).SetValue (msgObject, parts [partDesc.Index], null);
		}

		void MessageObjectToParts (MessageDescription md, object msgObject, object [] parts)
		{
			foreach (MessagePartDescription partDesc in md.Body.Parts)
				if (partDesc.MemberInfo is FieldInfo)
					parts [partDesc.Index] = ((FieldInfo) partDesc.MemberInfo).GetValue (msgObject);
				else
					parts [partDesc.Index] = ((PropertyInfo) partDesc.MemberInfo).GetValue (msgObject, null);
		}

		internal static bool HasReturnValue (MessageBodyDescription desc)
		{
			return desc.ReturnValue != null && desc.ReturnValue.Type != typeof (void);
		}

		protected static int ParamsOffset (MessageBodyDescription desc)
		{
			return HasReturnValue (desc) ? 1 : 0;
		}

		protected static object [] CreatePartsArray (MessageBodyDescription desc)
		{
			if (HasReturnValue (desc))
				return new object [desc.Parts.Count + 1];
			return new object [desc.Parts.Count];
		}
	}

#if !NET_2_1
	class XmlMessagesFormatter : BaseMessagesFormatter
	{
		XmlSerializerFormatAttribute attr;
		Dictionary<MessageBodyDescription,XmlSerializer> bodySerializers
			= new Dictionary<MessageBodyDescription,XmlSerializer> ();

		public XmlMessagesFormatter (OperationDescription desc, XmlSerializerFormatAttribute attr)
			: base (desc)
		{
			this.attr = attr;
		}

		public XmlMessagesFormatter (MessageDescriptionCollection messages, XmlSerializerFormatAttribute attr)
			: base (messages)
		{
			this.attr = attr;
		}

		private XmlReflectionMember CreateReflectionMember (MessagePartDescription partDesc, bool isReturnValue)
		{
			XmlReflectionMember m = new XmlReflectionMember ();
			m.IsReturnValue = isReturnValue;
			m.MemberName = partDesc.Name;
			m.MemberType = partDesc.Type;
			return m;
		}

		protected override Message PartsToMessage (
			MessageDescription md, MessageVersion version, string action, object [] parts)
		{
			return Message.CreateMessage (version, action, new XmlBodyWriter (GetSerializer (md.Body), parts));
		}

		protected override object [] MessageToParts (MessageDescription md, Message message)
		{
			if (message.IsEmpty)
				return null;
				
			XmlDictionaryReader r = message.GetReaderAtBodyContents ();
			return (object []) GetSerializer (md.Body).Deserialize (r);
		}

		XmlSerializer GetSerializer (MessageBodyDescription desc)
		{
			if (bodySerializers.ContainsKey (desc))
				return bodySerializers [desc];

			int count = desc.Parts.Count + (HasReturnValue (desc) ? 1 : 0);
			XmlReflectionMember [] members = new XmlReflectionMember [count];

			int ind = 0;
			if (HasReturnValue (desc))
				members [ind++] = CreateReflectionMember (desc.ReturnValue, true);

			foreach (MessagePartDescription partDesc in desc.Parts)
				members [ind++] = CreateReflectionMember (partDesc, false);

			XmlReflectionImporter xmlImporter = new XmlReflectionImporter ();
			// Register known types into xmlImporter.
			foreach (var type in OperationKnownTypes)
				xmlImporter.IncludeType (type);
			XmlMembersMapping [] partsMapping = new XmlMembersMapping [1];
			partsMapping [0] = xmlImporter.ImportMembersMapping (desc.WrapperName, desc.WrapperNamespace, members, true);
			bodySerializers [desc] = XmlSerializer.FromMappings (partsMapping) [0];
			return bodySerializers [desc];
		}

		class XmlBodyWriter : BodyWriter
		{
			XmlSerializer serializer;
			object body;

			public XmlBodyWriter (XmlSerializer serializer, object parts)
				: base (false)
			{
				this.serializer = serializer;
				this.body = parts;
			}

			protected override BodyWriter OnCreateBufferedCopy (int maxBufferSize)
			{
				return new XmlBodyWriter (serializer, body);
			}

			protected override void OnWriteBodyContents (XmlDictionaryWriter writer)
			{
				serializer.Serialize (writer, body);
			}
		}
	}
#endif

	class DataContractMessagesFormatter : BaseMessagesFormatter
	{
		DataContractFormatAttribute attr;

		public DataContractMessagesFormatter (OperationDescription desc, DataContractFormatAttribute attr)
			: base (desc)
		{
			this.attr = attr;
		}

		public DataContractMessagesFormatter (MessageDescriptionCollection messages, DataContractFormatAttribute attr)
			: base (messages)
		{
			this.attr = attr;
		}

		Dictionary<MessagePartDescription, XmlObjectSerializer> serializers
			= new Dictionary<MessagePartDescription,XmlObjectSerializer> ();

		protected override Message PartsToMessage (
			MessageDescription md, MessageVersion version, string action, object [] parts)
		{
			return Message.CreateMessage (version, action, new DataContractBodyWriter (md.Body, this, parts));
		}

		protected override object [] MessageToParts (
			MessageDescription md, Message message)
		{
			if (message.IsEmpty)
				return null;

			int offset = ParamsOffset (md.Body);
			object [] parts = CreatePartsArray (md.Body);

			XmlDictionaryReader r = message.GetReaderAtBodyContents ();
			if (md.Body.WrapperName != null)
				r.ReadStartElement (md.Body.WrapperName, md.Body.WrapperNamespace);

			for (r.MoveToContent (); r.NodeType == XmlNodeType.Element; r.MoveToContent ()) {
				XmlQualifiedName key = new XmlQualifiedName (r.LocalName, r.NamespaceURI);
				MessagePartDescription rv = md.Body.ReturnValue;
				if (rv != null && rv.Name == key.Name && rv.Namespace == key.Namespace)
					parts [0] = GetSerializer (md.Body.ReturnValue).ReadObject (r);
				else if (md.Body.Parts.Contains (key)) {
					MessagePartDescription p = md.Body.Parts [key];
					parts [p.Index + offset] = GetSerializer (p).ReadObject (r);
				}
				else // Skip unknown elements
					r.Skip ();
			}

			if (md.Body.WrapperName != null && !r.EOF)
				r.ReadEndElement ();

			return parts;
		}

		XmlObjectSerializer GetSerializer (MessagePartDescription partDesc)
		{
			if (!serializers.ContainsKey (partDesc))
				serializers [partDesc] = new DataContractSerializer (
					partDesc.Type, partDesc.Name, partDesc.Namespace, OperationKnownTypes);
			return serializers [partDesc];
		}

		class DataContractBodyWriter : BodyWriter
		{
			MessageBodyDescription desc;
			object [] parts;
			DataContractMessagesFormatter parent;

			public DataContractBodyWriter (MessageBodyDescription desc, DataContractMessagesFormatter parent, object [] parts)
				: base (false)
			{
				this.desc = desc;
				this.parent = parent;
				this.parts = parts;
			}

			protected override void OnWriteBodyContents (XmlDictionaryWriter writer)
			{
				int offset = HasReturnValue (desc) ? 1 : 0;
				if (desc.WrapperName != null)
					writer.WriteStartElement (desc.WrapperName, desc.WrapperNamespace);
				if (HasReturnValue (desc))
					WriteMessagePart (writer, desc, desc.ReturnValue, parts [0]);
				foreach (MessagePartDescription partDesc in desc.Parts)
					WriteMessagePart (writer, desc, partDesc, parts [partDesc.Index + offset]);
				if (desc.WrapperName != null)
					writer.WriteEndElement ();
			}

			void WriteMessagePart (
				XmlDictionaryWriter writer, MessageBodyDescription desc, MessagePartDescription partDesc, object obj)
			{
				parent.GetSerializer (partDesc).WriteObject (writer, obj);
			}
		}
	}
}
