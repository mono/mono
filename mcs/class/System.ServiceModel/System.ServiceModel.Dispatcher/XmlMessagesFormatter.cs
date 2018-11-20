//
// XmlMessagesFormatter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Eyal Alaluf <eyala@mainsoft.com>
//
// Copyright (C) 2005-2010 Novell, Inc.  http://www.novell.com
// Copyright (C) 2008 Mainsoft Co. http://www.mainsoft.com
// Copyright (C) 2011 Xamarin Inc. http://www.xamarin.com
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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Dispatcher
{
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
			m.XmlAttributes = partDesc.MemberInfo == null ? new XmlAttributes () : new XmlAttributes (partDesc.MemberInfo);
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

		protected override Dictionary<MessageHeaderDescription,object> MessageToHeaderObjects (MessageDescription md, Message message)
		{
			// FIXME: do we need header serializers?
			return null;
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
			partsMapping [0] = xmlImporter.ImportMembersMapping (desc.WrapperName, desc.WrapperNamespace, members, desc.WrapperName != null);
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
				if (writer.WriteState == WriteState.Element) {
					writer.WriteXmlnsAttribute ("xsi", XmlSchema.InstanceNamespace);
					writer.WriteXmlnsAttribute ("xsd", XmlSchema.Namespace);
				}

				serializer.Serialize (writer, body);
			}
		}
	}
}
