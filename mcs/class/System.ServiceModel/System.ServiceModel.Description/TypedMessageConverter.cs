//
// TypedMessageConverter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Description
{
	internal class DefaultTypedMessageConverter : TypedMessageConverter
	{
		IClientMessageFormatter formatter;

		public DefaultTypedMessageConverter (IClientMessageFormatter formatter)
		{
			this.formatter = formatter;
		}

		public override object FromMessage (Message message)
		{
			return formatter.DeserializeReply (message, null);
		}

		public override Message ToMessage (object typedMessage)
		{
			return ToMessage (typedMessage, MessageVersion.Default);
		}

		public override Message ToMessage (
			object typedMessage, MessageVersion version)
		{
			return formatter.SerializeRequest (version, new object [] {typedMessage});
		}
	}

	public abstract class TypedMessageConverter
	{
		internal const string TempUri = "http://tempuri.org/";

		protected TypedMessageConverter ()
		{
		}

		public static TypedMessageConverter Create (
			Type type, string action)
		{
			return Create (type, action, TempUri);
		}

		public static TypedMessageConverter Create (
			Type type, string action,
			string defaultNamespace)
		{
			return Create (type, action, defaultNamespace, (DataContractFormatAttribute)null);
		}

		public static TypedMessageConverter Create (
			Type type, string action,
			DataContractFormatAttribute formatterAttribute)
		{
			return Create (type, action, TempUri, formatterAttribute);
		}

		public static TypedMessageConverter Create (
			Type type,
			string action, string defaultNamespace,
			DataContractFormatAttribute formatterAttribute)
		{
			return new DefaultTypedMessageConverter (
				new DataContractMessagesFormatter (
					MessageContractToMessagesDescription (type, defaultNamespace, action),
					formatterAttribute));
		}

		public static TypedMessageConverter Create (
			Type type, string action,
			XmlSerializerFormatAttribute formatterAttribute)
		{
			return Create (type, action, TempUri, formatterAttribute);
		}

		public static TypedMessageConverter Create (
			Type type, string action, string defaultNamespace,
			XmlSerializerFormatAttribute formatterAttribute)
		{
			return new DefaultTypedMessageConverter (
				new XmlMessagesFormatter (
					MessageContractToMessagesDescription (type, defaultNamespace, action),
					formatterAttribute));
		}

		public abstract object FromMessage (Message message);

		public abstract Message ToMessage (object typedMessage);

		public abstract Message ToMessage (object typedMessage, MessageVersion version);

		static MessageDescriptionCollection MessageContractToMessagesDescription (
			Type src, string defaultNamespace, string action)
		{
			MessageContractAttribute mca =
				ContractDescriptionGenerator.GetMessageContractAttribute (src);

			if (mca == null)
				throw new ArgumentException (String.Format ("Type {0} and its ancestor types do not have MessageContract attribute.", src));

			MessageDescriptionCollection messages = new MessageDescriptionCollection ();
			// FIXME: not sure if isDirectionInput arguments are ignorable (and can be dummy) here...
			messages.Add (ContractDescriptionGenerator.CreateMessageDescription (src, defaultNamespace, action, true, false, mca));
			messages.Add (ContractDescriptionGenerator.CreateMessageDescription (src, defaultNamespace, action, false, false, mca));
			return messages;
		}
	}
}
