//
// Mono.Messaging.RabbitMQ
//
// Authors:
//	  Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
//

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
using System.Collections;
using System.IO;
using System.Text;

using Mono.Messaging;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Mono.Messaging.RabbitMQ {

	public class MessageFactory {
		
		private static readonly string SENDER_VERSION_KEY = "SenderVersion";
		private static readonly string SOURCE_MACHINE_KEY = "SourceMachine";
		private static readonly string BODY_TYPE_KEY = "BodyType";
		private static readonly string ACKNOWLEDGE_TYPE_KEY = "AcknowledgeType";
		private static readonly string ADMINISTRATION_QUEUE_KEY = "AdministrationQueue";
		private static readonly string APP_SPECIFIC_KEY = "AppSpecific";
		private static readonly string AUTHENTICATION_PROVIDER_NAME_KEY = "AuthenticationProviderName";
		private static readonly string AUTHENTICATION_PROVIDER_TYPE_KEY = "AuthenticationProviderType";
		private static readonly string CONNECTOR_TYPE_KEY = "ConnectorType";
		private static readonly string DESTINATION_SYMMETRIC_KEY_KEY = "DestinationSymmetricKey";
		private static readonly string DIGITAL_SIGNATURE_KEY = "DigitalSignature";
		private static readonly string ENCRYPTION_ALGORITHM_KEY = "EncryptionAlgorithm";
		private static readonly string EXTENSION_KEY = "Extension";
		private static readonly string HASH_ALGORITHM_KEY = "HashAlgorithm";
		private static readonly string LABEL_KEY = "Label";
		private static readonly string SENDER_CERTIFICATE_KEY = "SenderCertificate";
		private static readonly string TIME_TO_BE_RECEIVED_KEY = "TimeToBeReceived";
		private static readonly string TIME_TO_REACH_QUEUE_KEY = "TimeToReachQueue";
		private static readonly string USE_AUTHENTICATION_KEY = "UseAuthentication";
		private static readonly string USE_DEAD_LETTER_QUEUE_KEY = "UseDeadLetterQueue";
		private static readonly string USE_ENCRYPTION_KEY = "UseEncryption";
		private static readonly string TRANSACTION_ID_KEY = "TrandactionId";
		
		private static readonly int PERSISTENT_DELIVERY_MODE = 2;
		
		private readonly RabbitMQMessagingProvider provider;
		
		public MessageFactory (RabbitMQMessagingProvider provider)
		{
			this.provider = provider;
		}
		
		public IMessageBuilder WriteMessage (IModel ch, IMessage msg)
		{
			BasicMessageBuilder mb = new BasicMessageBuilder (ch);
			mb.Properties.MessageId = msg.Id;
			if (msg.CorrelationId != null)
				mb.Properties.CorrelationId = msg.CorrelationId;
			// TODO: Change to DateTime.UtcNow??
			mb.Properties.Timestamp = MessageFactory.DateTimeToAmqpTimestamp (DateTime.UtcNow);
			Hashtable headers = new Hashtable ();
			
			headers[SENDER_VERSION_KEY] = msg.SenderVersion;
			headers[SOURCE_MACHINE_KEY] = (string) System.Environment.MachineName;
			headers[BODY_TYPE_KEY] = msg.BodyType;
			headers[ACKNOWLEDGE_TYPE_KEY] = (int) msg.AcknowledgeType;
			if (msg.AdministrationQueue != null)
				headers[ADMINISTRATION_QUEUE_KEY] = msg.AdministrationQueue.QRef.ToString ();
			headers[APP_SPECIFIC_KEY] = msg.AppSpecific;
			headers[AUTHENTICATION_PROVIDER_NAME_KEY] = msg.AuthenticationProviderName;
			headers[AUTHENTICATION_PROVIDER_TYPE_KEY] = (int) msg.AuthenticationProviderType;
			headers[CONNECTOR_TYPE_KEY] = msg.ConnectorType.ToString ();
			headers[DESTINATION_SYMMETRIC_KEY_KEY] = msg.DestinationSymmetricKey;
			headers[DIGITAL_SIGNATURE_KEY] = msg.DigitalSignature;
			headers[ENCRYPTION_ALGORITHM_KEY] = (int) msg.EncryptionAlgorithm;
			headers[EXTENSION_KEY] = msg.Extension;
			headers[HASH_ALGORITHM_KEY] = (int) msg.HashAlgorithm;
			SetValue (headers, LABEL_KEY, msg.Label);
			mb.Properties.Priority = (byte) (int) msg.Priority;
			mb.Properties.SetPersistent (msg.Recoverable);
			if (msg.ResponseQueue != null)
				mb.Properties.ReplyTo = msg.ResponseQueue.QRef.ToString ();
			headers[SENDER_CERTIFICATE_KEY] = msg.SenderCertificate;
			headers[TIME_TO_BE_RECEIVED_KEY] = msg.TimeToBeReceived.Ticks;
			headers[TIME_TO_REACH_QUEUE_KEY] = msg.TimeToReachQueue.Ticks;
			SetValue (headers, TRANSACTION_ID_KEY, msg.TransactionId);
			headers[USE_AUTHENTICATION_KEY] = msg.UseAuthentication;
			headers[USE_DEAD_LETTER_QUEUE_KEY] = msg.UseDeadLetterQueue;
			headers[USE_ENCRYPTION_KEY] = msg.UseEncryption;
			
			mb.Properties.Headers = headers;
			Stream s = msg.BodyStream;
			s.Seek (0, SeekOrigin.Begin);
			byte[] buf = new byte[s.Length];			
			msg.BodyStream.Read (buf, 0, buf.Length);
			mb.BodyStream.Write (buf, 0, buf.Length);
			return mb;
		}
		
		private static void SetValue (Hashtable headers, string name, object val)
		{
			if (val != null)
				headers[name] = val;
		}
		
		public IMessage ReadMessage (QueueReference destination, BasicDeliverEventArgs result)
		{
			/*
			if (destination == null)
				throw new ArgumentException ("destination must not be null");
			if (result == null)
				throw new ArgumentException ("result must not be null");
			*/
			MessageBase msg = new MessageBase ();
			Stream s = new MemoryStream ();
			s.Write (result.Body, 0, result.Body.Length);
			DateTime arrivedTime = DateTime.Now;
			IDictionary headers = result.BasicProperties.Headers;
			long senderVersion = (long) headers[SENDER_VERSION_KEY];
			string sourceMachine = GetString (headers, SOURCE_MACHINE_KEY);
			DateTime sentTime = AmqpTimestampToDateTime (result.BasicProperties.Timestamp);
			string transactionId = GetString (headers, TRANSACTION_ID_KEY);
			msg.SetDeliveryInfo (Acknowledgment.None,
			                     arrivedTime,
			                     new RabbitMQMessageQueue (provider,
			                                               destination,
			                                               true),
			                     result.BasicProperties.MessageId,
			                     MessageType.Normal,
			                     new byte[0],
			                     senderVersion,
			                     sentTime,
			                     sourceMachine,
			                     transactionId);
			msg.CorrelationId = result.BasicProperties.CorrelationId;
			msg.BodyStream = s;
			msg.BodyType = (int) result.BasicProperties.Headers[BODY_TYPE_KEY];
			msg.AcknowledgeType = (AcknowledgeTypes) 
				Enum.ToObject (typeof (AcknowledgeTypes), 
				               headers[ACKNOWLEDGE_TYPE_KEY]);
			string adminQueuePath = GetString (headers, ADMINISTRATION_QUEUE_KEY);
			if (adminQueuePath != null) {
				QueueReference qRef = QueueReference.Parse (adminQueuePath);
				msg.AdministrationQueue = new RabbitMQMessageQueue (provider,
				                                                    qRef,
				                                                    true);
			}
			msg.AppSpecific = (int) headers[APP_SPECIFIC_KEY];
			msg.AuthenticationProviderName = GetString (headers, AUTHENTICATION_PROVIDER_NAME_KEY);
			msg.AuthenticationProviderType = (CryptographicProviderType) Enum.ToObject (typeof (CryptographicProviderType), headers[AUTHENTICATION_PROVIDER_TYPE_KEY]);
			string connectorType = GetString (headers, CONNECTOR_TYPE_KEY);
			msg.ConnectorType = new Guid(connectorType);
			msg.DestinationSymmetricKey = (byte[]) headers[DESTINATION_SYMMETRIC_KEY_KEY];
			msg.DigitalSignature = (byte[]) headers[DIGITAL_SIGNATURE_KEY];
			msg.EncryptionAlgorithm = (EncryptionAlgorithm) Enum.ToObject (typeof (EncryptionAlgorithm), headers[ENCRYPTION_ALGORITHM_KEY]);
			msg.Extension = (byte[]) headers[EXTENSION_KEY];
			msg.HashAlgorithm = (HashAlgorithm) Enum.ToObject (typeof (HashAlgorithm), headers[HASH_ALGORITHM_KEY]);
			msg.Label = GetString (headers, LABEL_KEY);
			msg.Priority = (MessagePriority) Enum.ToObject (typeof (MessagePriority), result.BasicProperties.Priority);
			msg.Recoverable = result.BasicProperties.DeliveryMode == PERSISTENT_DELIVERY_MODE;
			if (result.BasicProperties.ReplyTo != null)
				msg.ResponseQueue = new RabbitMQMessageQueue (provider, QueueReference.Parse (result.BasicProperties.ReplyTo), true);
			msg.SenderCertificate = (byte[]) headers[SENDER_CERTIFICATE_KEY];
			msg.TimeToBeReceived = new TimeSpan((long) headers[TIME_TO_BE_RECEIVED_KEY]);
			msg.TimeToReachQueue = new TimeSpan((long) headers[TIME_TO_REACH_QUEUE_KEY]);
			msg.UseAuthentication = (bool) headers[USE_AUTHENTICATION_KEY];
			msg.UseDeadLetterQueue = (bool) headers[USE_DEAD_LETTER_QUEUE_KEY];
			msg.UseEncryption = (bool) headers[USE_ENCRYPTION_KEY];
			
			return msg;
		}
		
		public static string GetString (IDictionary properties, String key)
		{
			byte[] b = (byte[]) properties[key];
			if (b == null)
				return null;
			
			return Encoding.UTF8.GetString (b);
		}
		
		public static AmqpTimestamp DateTimeToAmqpTimestamp (DateTime t)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan ts = t.ToUniversalTime () - epoch;
			return new AmqpTimestamp((long) ts.TotalSeconds);
		}
		
		public static DateTime AmqpTimestampToDateTime (AmqpTimestamp ats)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return epoch.AddSeconds (ats.UnixTime).ToLocalTime ();
		}

		public static int TimeSpanToMillis (TimeSpan timespan)
		{
			if (timespan == TimeSpan.MaxValue)
				return -1;
			else
				return (int) timespan.TotalMilliseconds;
		}
	}
}
